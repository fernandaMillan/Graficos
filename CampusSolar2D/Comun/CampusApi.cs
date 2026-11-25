using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CampusSolar;

// Los nombres C# se traducen al camelCase del JSON mediante los valores web.
public sealed record Variables([property: JsonRequired] int HoraLocal,
    [property: JsonRequired] double Nubosidad, [property: JsonRequired] double AlturaParedM);
public sealed record PeticionInicial(string Escenario, Variables Variables);
public sealed record Resultado(
    [property: JsonRequired] double ElevacionSolGrados, [property: JsonRequired] string DireccionSombra,
    [property: JsonRequired] double LongitudSombraM, [property: JsonRequired] bool MesaEnSombra);
public sealed record Visual([property: JsonRequired] double ContrasteSombra);
public sealed record Respuesta(string Estado, Resultado Resultado, Visual Visual);
public sealed record Escenario(string EscenarioId, double ParedXM, double MesaXM);
public sealed record Cuadro(Variables Entrada, Respuesta Datos, Escenario Patio);

public sealed class CampusApi : IDisposable
{
    public static readonly Variables CasoBase = new(10, 25, 5);
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        { NumberHandling = JsonNumberHandling.Strict };
    private readonly HttpClient http;
    private Escenario? patio;
    public CampusApi()
    {
        string baseUrl = Environment.GetEnvironmentVariable("CAMPUS_API_URL") ?? "http://127.0.0.1:3000";
        http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(6) };
    }
    public static Variables LeerEntradaInicial()
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "peticion.json");
        var p = JsonSerializer.Deserialize<PeticionInicial>(File.ReadAllText(ruta), Json);
        var v = p?.Variables;
        if (p?.Escenario != "explanada_mexicali" || v is null ||
            v.HoraLocal is not (8 or 10 or 12 or 14 or 16) ||
            !double.IsFinite(v.Nubosidad) || v.Nubosidad is < 0 or > 100 ||
            !double.IsFinite(v.AlturaParedM) || v.AlturaParedM is < 1 or > 8)
            throw new InvalidDataException("Revisa escenario y variables en Datos/peticion.json.");
        return v;
    }
    public async Task<Escenario> Preparar()
    {
        if (patio != null) return patio;
        var salud = await http.GetFromJsonAsync<JsonElement>("api/health");
        if (!salud.TryGetProperty("estado", out var estado) || estado.GetString() != "ok")
            throw new InvalidDataException("La API no confirmó su estado.");
        var e = await http.GetFromJsonAsync<JsonElement>("api/escenario");
        if (!e.TryGetProperty("escenario", out var id) || id.GetString() != "explanada_mexicali" ||
            !e.TryGetProperty("paredXM", out var pared) || pared.ValueKind != JsonValueKind.Number ||
            !e.TryGetProperty("mesaXM", out var mesa) || mesa.ValueKind != JsonValueKind.Number)
            throw new InvalidDataException("Escenario incompleto o no reconocido.");
        return patio = new Escenario(id.GetString()!, pared.GetDouble(), mesa.GetDouble());
    }
    public async Task<Cuadro> Consultar(Variables entrada)
    {
        var e = await Preparar();  // GET solo la primera vez que se conecta.
        var peticion = new { escenario = e.EscenarioId, variables = entrada };
        using var r = await http.PostAsJsonAsync("api/simular", peticion);
        if (!r.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        var datos = await r.Content.ReadFromJsonAsync<Respuesta>(Json);
        return new Cuadro(entrada, Validar(datos), e);
    }
    public static Cuadro LeerRespaldo()
    {
        string contenido = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "mock-response.json"));
        var respuesta = Validar(JsonSerializer.Deserialize<Respuesta>(contenido, Json));
        // El mock va con ESTAS entradas y posiciones, no con los controles pendientes.
        return new Cuadro(CasoBase, respuesta, new Escenario("explanada_mexicali", 0, 4));
    }
    private static Respuesta Validar(Respuesta? d)
    {
        if (d?.Estado != "ok" || d.Resultado is null || d.Visual is null ||
            d.Resultado.DireccionSombra is not ("este" or "oeste") ||
            !double.IsFinite(d.Resultado.LongitudSombraM) || d.Resultado.LongitudSombraM < 0 ||
            d.Resultado.ElevacionSolGrados is not (30 or 45 or 60) ||
            !double.IsFinite(d.Visual.ContrasteSombra) || d.Visual.ContrasteSombra is < 0 or > 1)
            throw new InvalidDataException("La respuesta no cumple el contrato de la hoja.");
        return d;
    }
    public void Dispose() => http.Dispose();
}
