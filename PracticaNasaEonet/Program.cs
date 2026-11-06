using System.Numerics;
using System.Text.Json;
using Raylib_cs;

internal static class Program
{
    private const string Url =
        "https://eonet.gsfc.nasa.gov/api/v3/events?category=wildfires&status=all&limit=10";
    private const int Izquierda = 40, Arriba = 80, Ancho = 720, Alto = 360;
    private const float Radio = 9f;
    private static readonly Vector2 Centro = new(400, 260);
    private static readonly Color Naranja = new(194, 65, 12, 255);
    private static readonly Color Azul = new(29, 78, 137, 255);
    private static readonly float[,] Matriz =
    {
        { 2,  0, 400 },
        { 0, -2, 260 },
        { 0,  0,   1 }
    };

    private sealed record Evento(string Titulo, bool Cerrado, DateTimeOffset Fecha,
                                 float Longitud, float Latitud, Vector2 Pantalla);
    private sealed record Datos(List<Evento> Eventos, string Origen, string Aviso);

    [STAThread]
    private static int Main(string[] args)
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, "eventos.json");
        if (args.Contains("--verificar")) return Verificar(ruta);

        // La consulta se hace una vez. Raylib inicia y dibuja en el mismo hilo.
        bool local = args.Contains("--local");
        Console.WriteLine(local ? "Leyendo eventos.json..." : "Consultando NASA EONET...");
        Datos datos = CargarDatosAsync(ruta, local).GetAwaiter().GetResult();

        Raylib.InitWindow(1100, 560, "NASA EONET | Practica de graficos");
        Raylib.SetTargetFPS(60);
        Evento? seleccionado = null;
        try
        {
            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    seleccionado = BuscarEvento(datos.Eventos, Raylib.GetMousePosition());
                Dibujar(datos, seleccionado);
            }
        }
        finally { Raylib.CloseWindow(); }
        return 0;
    }

    private static async Task<Datos> CargarDatosAsync(string ruta, bool local)
    {
        string aviso = "";
        if (!local)
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
                string json = await http.GetStringAsync(Url);
                return new(LeerEventos(json), "NASA EONET", "");
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
            {
                Console.Error.WriteLine(ex.Message);
                aviso = "No se pudo usar la respuesta de NASA. Se intento leer el respaldo.";
            }
        }
        try
        {
            return new(LeerEventos(await File.ReadAllTextAsync(ruta)), "Archivo local", aviso);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Console.Error.WriteLine(ex.Message);
            return new([], "Sin datos", "No se pudo leer eventos.json o su formato es invalido.");
        }
    }

    private static List<Evento> LeerEventos(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (!Campo(doc.RootElement, "events", JsonValueKind.Array, out var eventos))
            throw new JsonException("Falta el arreglo events.");

        List<Evento> resultado = [];
        foreach (var e in eventos.EnumerateArray())
        {
            if (!Campo(e, "geometry", JsonValueKind.Array, out var geometrias))
                continue;
            string titulo = Campo(e, "title", JsonValueKind.String, out var t)
                ? t.GetString() ?? "Sin titulo" : "Sin titulo";
            bool cerrado = Campo(e, "closed", JsonValueKind.String, out var cierre)
                && !string.IsNullOrWhiteSpace(cierre.GetString());
            Evento? reciente = null;
            foreach (var g in geometrias.EnumerateArray())
            {
                // Se omiten polígonos y puntos incompletos.
                if (!Campo(g, "type", JsonValueKind.String, out var tipo) || tipo.GetString() != "Point"
                    || !Campo(g, "date", JsonValueKind.String, out var fechaJson)
                    || !fechaJson.TryGetDateTimeOffset(out var fecha)
                    || !Campo(g, "coordinates", JsonValueKind.Array, out var xy) || xy.GetArrayLength() < 2
                    || xy[0].ValueKind != JsonValueKind.Number || xy[1].ValueKind != JsonValueKind.Number
                    || !xy[0].TryGetSingle(out float lon) || !xy[1].TryGetSingle(out float lat)
                    || !float.IsFinite(lon) || !float.IsFinite(lat)
                    || lon is < -180 or > 180 || lat is < -90 or > 90)
                    continue;
                if (reciente is null || fecha > reciente.Fecha)
                    reciente = new(titulo, cerrado, fecha, lon, lat, Proyectar(lon, lat));
            }
            if (reciente is not null) resultado.Add(reciente);
        }
        return resultado;
    }

    private static bool Campo(JsonElement obj, string nombre, JsonValueKind tipo, out JsonElement valor)
    {
        valor = default;
        return obj.ValueKind == JsonValueKind.Object && obj.TryGetProperty(nombre, out valor)
            && valor.ValueKind == tipo;
    }

    // M * [longitud, latitud, 1] en coordenadas homogéneas.
    private static Vector2 Proyectar(float lon, float lat) =>
        new(Matriz[0, 0] * lon + Matriz[0, 1] * lat + Matriz[0, 2],
            Matriz[1, 0] * lon + Matriz[1, 1] * lat + Matriz[1, 2]);

    private static Evento? BuscarEvento(List<Evento> eventos, Vector2 mouse)
    {
        Evento? mejor = null;
        float limite = Radio * Radio;
        foreach (var e in eventos)
        {
            float distancia = Vector2.DistanceSquared(mouse, e.Pantalla);
            if (distancia > limite || (mejor is not null && distancia == limite)) continue;
            mejor = e;
            limite = distancia;
        }
        return mejor;
    }

    private static void Dibujar(Datos datos, Evento? seleccionado)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.RayWhite);
        Raylib.DrawText("NASA EONET: mapa de incendios", 40, 18, 26, Color.DarkBlue);
        Raylib.DrawText($"Origen: {datos.Origen}  |  Eventos con punto: {datos.Eventos.Count}",
            40, 51, 17, Color.DarkGray);
        Raylib.DrawRectangle(Izquierda, Arriba, Ancho, Alto, Color.White);
        for (int x = Izquierda; x <= Izquierda + Ancho; x += 60)
            Raylib.DrawLine(x, Arriba, x, Arriba + Alto, x == (int)Centro.X ? Color.Gray : Color.LightGray);
        for (int y = Arriba; y <= Arriba + Alto; y += 60)
            Raylib.DrawLine(Izquierda, y, Izquierda + Ancho, y, y == (int)Centro.Y ? Color.Gray : Color.LightGray);
        Raylib.DrawRectangleLines(Izquierda, Arriba, Ancho, Alto, Color.Gray);
        Raylib.DrawCircleV(Centro, 3f, Color.DarkGray);
        if (seleccionado is not null) DibujarVector(seleccionado.Pantalla - Centro);
        foreach (var e in datos.Eventos)
        {
            if (e == seleccionado)
                Raylib.DrawCircleLines((int)e.Pantalla.X, (int)e.Pantalla.Y, Radio + 4, Color.Black);
            Marcador(e.Pantalla, e.Cerrado);
        }
        Marcador(new(50, 469), false);
        Raylib.DrawText("Abierto", 66, 460, 18, Color.DarkGray);
        Marcador(new(170, 469), true);
        Raylib.DrawText("Cerrado", 186, 460, 18, Color.DarkGray);
        Raylib.DrawText("El color representa el estado del registro.", 300, 460, 17, Color.DarkGray);
        if (datos.Aviso.Length > 0) Raylib.DrawText(datos.Aviso, 40, 502, 16, Color.DarkGray);
        Raylib.DrawText("Detalle del evento", 790, 94, 24, Color.DarkBlue);
        if (seleccionado is null)
        {
            Raylib.DrawText(datos.Eventos.Count == 0 ? "Sin eventos para mostrar" : "Haz clic en un marcador.",
                790, 140, 18, Color.DarkGray);
        }
        else
        {
            var e = seleccionado;
            Vector2 d = e.Pantalla - Centro;
            string titulo = e.Titulo.Replace('\n', ' ').Replace('\r', ' ');
            while (Raylib.MeasureText(titulo + "...", 19) > 275 && titulo.Length > 0)
                titulo = titulo[..^1];
            if (titulo.Length < e.Titulo.Length) titulo += "...";
            Raylib.DrawText(titulo, 790, 138, 19, Color.Black);
            Raylib.DrawText($"Estado: {(e.Cerrado ? "cerrado" : "abierto")}", 790, 175, 17, Color.DarkGray);
            Raylib.DrawText($"Fecha del punto: {e.Fecha.UtcDateTime:yyyy-MM-dd} UTC", 790, 204, 17, Color.DarkGray);
            Raylib.DrawText($"Lon: {e.Longitud:F2}  Lat: {e.Latitud:F2}", 790, 233, 17, Color.DarkGray);
            Raylib.DrawText($"Pantalla: ({e.Pantalla.X:F1}, {e.Pantalla.Y:F1}) px", 790, 280, 17, Color.DarkGray);
            Raylib.DrawText($"Vector: ({d.X:F1}, {d.Y:F1}) px", 790, 309, 17, Color.DarkGray);
        }
        Raylib.EndDrawing();
    }

    private static void Marcador(Vector2 p, bool cerrado)
    {
        if (cerrado) Raylib.DrawCircleLines((int)p.X, (int)p.Y, Radio, Azul);
        else Raylib.DrawCircleV(p, Radio, Naranja);
    }

    private static void DibujarVector(Vector2 d)
    {
        if (d.LengthSquared() < 1f) return;
        Vector2 extremo = Centro + d;
        Vector2 u = Vector2.Normalize(d);
        Vector2 perpendicular = new(-u.Y, u.X);
        float punta = MathF.Min(18f, d.Length() / 2f);
        Vector2 basePunta = extremo - u * punta;
        Raylib.DrawLineEx(Centro, extremo, 2f, Color.DarkGray);
        Raylib.DrawLineEx(extremo, basePunta + perpendicular * (punta / 3f), 2f, Color.DarkGray);
        Raylib.DrawLineEx(extremo, basePunta - perpendicular * (punta / 3f), 2f, Color.DarkGray);
    }

    private static int Verificar(string ruta)
    {
        var eventos = LeerEventos(File.ReadAllText(ruta));
        bool ok = Proyectar(0, 0) == Centro
            && Proyectar(-180, 90) == new Vector2(40, 80)
            && Proyectar(180, -90) == new Vector2(760, 440)
            && eventos.Count == 2
            && eventos[0].Pantalla == new Vector2(220, 170) && !eventos[0].Cerrado
            && eventos[1].Pantalla == new Vector2(520, 320) && eventos[1].Cerrado
            && eventos[0].Pantalla - Centro == new Vector2(-180, -90)
            && BuscarEvento(eventos, new(220, 170)) == eventos[0]
            && BuscarEvento(eventos, Centro) is null;
        Console.WriteLine(ok ? "OK: JSON, matriz, vector y seleccion." : "Fallo en las comprobaciones.");
        return ok ? 0 : 1;
    }
}
