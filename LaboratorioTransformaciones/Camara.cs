using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// El navegador procesa el video; el servidor local recibe exclusivamente 1 o 0.
internal sealed class Camara : IDisposable
{
    private readonly WebApplication app;
    private long ultimaPalma = -1;
    private long ultimaSenal = -1;
    public string Direccion { get; }
    public bool TieneSenal => EsReciente(Interlocked.Read(ref ultimaSenal));
    public bool PalmaAbierta => EsReciente(Interlocked.Read(ref ultimaPalma));

    public Camara()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = AppContext.BaseDirectory
        });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(o =>
        {
            o.Listen(IPAddress.Loopback, 0); // Puerto disponible, solo en este equipo.
            o.Limits.MaxRequestBodySize = 16;
            o.AddServerHeader = false;
        });
        app = builder.Build();
        string pagina = Path.Combine(AppContext.BaseDirectory, "camara.html");
        app.MapGet("/", () => Results.File(pagina, "text/html; charset=utf-8"));
        app.MapPost("/gesto", Recibir);

        try
        {
            app.StartAsync().GetAwaiter().GetResult();
            Direccion = app.Urls.Single().TrimEnd('/');
        }
        catch
        {
            app.DisposeAsync().AsTask().GetAwaiter().GetResult();
            throw;
        }
    }

    private async Task<IResult> Recibir(HttpRequest request)
    {
        if (!string.Equals(request.Headers["Origin"].ToString(), Direccion, StringComparison.Ordinal))
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        if (request.ContentLength != 1)
            return Results.BadRequest();

        var dato = new byte[1];
        int leidos = await request.Body.ReadAsync(dato.AsMemory(), request.HttpContext.RequestAborted);
        if (leidos != 1 || (dato[0] != (byte)'0' && dato[0] != (byte)'1'))
            return Results.BadRequest();

        long ahora = Environment.TickCount64;
        Interlocked.Exchange(ref ultimaPalma, dato[0] == (byte)'1' ? ahora : -1);
        Interlocked.Exchange(ref ultimaSenal, ahora);
        return Results.NoContent();
    }

    private static bool EsReciente(long marca) =>
        marca >= 0 && Environment.TickCount64 - marca < 900;

    public void Dispose()
    {
        using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        try { app.StopAsync(limite.Token).GetAwaiter().GetResult(); }
        finally { app.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
    }
}
