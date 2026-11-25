using CampusSolar;
using System.Globalization;
using System.Text.Json;

try
{
    Cuadro c;
    if (args is ["--mock"])
    {
        c = CampusApi.LeerRespaldo();
        Console.WriteLine("RESPALDO LOCAL. Caso fijo de las 10 h; no hubo HTTP.");
    }
    else
    {
        if (args.Length is not (0 or 3)) throw new ArgumentException("Uso: sin argumentos, o bien HORA NUBOSIDAD ALTURA. Ej.: 14 25 5");
        var v = args.Length == 0 ? CampusApi.CasoBase :
            new Variables(int.Parse(args[0]), double.Parse(args[1], CultureInfo.InvariantCulture),
                double.Parse(args[2], CultureInfo.InvariantCulture));
        using var api = new CampusApi();
        var patio = await api.Preparar();
        Console.WriteLine($"GET health: ok | GET escenario: {patio.EscenarioId}, mesa en x={patio.MesaXM} m");
        Console.WriteLine("POST /api/simular  " + JsonSerializer.Serialize(new { escenario = patio.EscenarioId, variables = v }, CampusApi.Json));
        c = await api.Consultar(v);
        Console.WriteLine("DATOS DE LA API");
    }
    var r = c.Datos.Resultado;
    Console.WriteLine(JsonSerializer.Serialize(c.Datos, new JsonSerializerOptions(CampusApi.Json) { WriteIndented = true }));
    Console.WriteLine($"{c.Entrada.HoraLocal}:00 | {r.LongitudSombraM:F2} m al {r.DireccionSombra} | Mesa: {(r.MesaEnSombra ? "en sombra" : "al sol")} | Contraste: {c.Datos.Visual.ContrasteSombra:F2}");
}
catch (Exception e)
{
    Console.Error.WriteLine("ERROR: " + e.Message);
    Console.Error.WriteLine("Revisa la URL y las entradas. Para leer el archivo de respaldo: dotnet run --project Consola -- --mock");
    Environment.ExitCode = 1;
}
