using CampusSolar;
using Raylib_cs;

// El hilo de Raylib solo recoge resultados terminados. Ningún await ni espera de red al dibujar.
var seleccion = CampusApi.CasoBase;
Cuadro? visible = null;
Task<Cuadro>? pendiente = null;
string estado = "Inicio", aviso = "Leyendo la peticion inicial...";
bool respaldo = false, pausa = false, tangente = true;
Ruta ruta = Ruta.Senoidal;
int[] horas = [8,10,12,14,16];
float[] amplitudes = [.5f,1f,1.5f,2f], periodos = [4f,8f,12f];
int a = 1, n = 1, f = 1;
float t = 0;
CampusApi? api = null;

void MostrarRespaldo(string motivo)
{
    try {
        visible = CampusApi.LeerRespaldo();
        respaldo = true; estado = "Respaldo JSON"; aviso = motivo;
    } catch (Exception error) { estado = "Error"; aviso = error.Message; }
}
void Enviar()
{
    if (pendiente != null) return;
    try {
        api ??= new CampusApi();
        pendiente = api.Consultar(seleccion);
        estado = "Cargando"; aviso = "Esperando a Node.js; la ultima escena sigue animada.";
    } catch (Exception error) { estado = "Error"; aviso = error.Message; }
}

if (args.Contains("--mock")) MostrarRespaldo("Caso fijo de las 10 h. ENTER consulta la API.");
else {
    try { seleccion = CampusApi.LeerEntradaInicial(); Enviar(); }
    catch (Exception error) { MostrarRespaldo("Peticion inicial no valida. " + error.Message); }
}
Raylib.InitWindow(Lienzo.Ancho, Lienzo.Alto, "Campus Solar | Poligonos y trayectorias");
Raylib.SetTargetFPS(60);
try
{
    while (!Raylib.WindowShouldClose())
    {
        bool K(KeyboardKey key) => Raylib.IsKeyPressed(key);
        if (pendiente is { IsCompleted: true }) {
            try { visible = pendiente.GetAwaiter().GetResult(); estado = "API / en vivo";
                  respaldo = false; aviso = "Respuesta recibida. Cambia una sola entrada y compara."; }
            catch (Exception error) {
                if (visible == null) MostrarRespaldo("API no disponible: " + error.Message);
                else { estado = "Error"; aviso = "Escena previa conservada. " + error.Message; }
            }
            pendiente = null;
        }
        if (K(KeyboardKey.M) && pendiente == null) {
            MostrarRespaldo("Caso fijo: 10 h, nubes 25%, pared 5 m. ENTER reintenta en la API.");
            if (respaldo && visible != null) seleccion = visible.Entrada;
        }
        int i = Array.IndexOf(horas,seleccion.HoraLocal);
        if (K(KeyboardKey.Left))  seleccion = seleccion with { HoraLocal = horas[(i+4)%5] };
        if (K(KeyboardKey.Right)) seleccion = seleccion with { HoraLocal = horas[(i+1)%5] };
        if (K(KeyboardKey.Up))    seleccion = seleccion with { Nubosidad = Math.Min(100,seleccion.Nubosidad+5) };
        if (K(KeyboardKey.Down))  seleccion = seleccion with { Nubosidad = Math.Max(0,seleccion.Nubosidad-5) };
        if (K(KeyboardKey.W))     seleccion = seleccion with { AlturaParedM = Math.Min(8,seleccion.AlturaParedM+1) };
        if (K(KeyboardKey.S))     seleccion = seleccion with { AlturaParedM = Math.Max(1,seleccion.AlturaParedM-1) };
        if (K(KeyboardKey.Enter)) Enviar();

        // Estos controles NO hacen peticiones: solo cambian cómo interpreta C# la escena.
        Ruta? nueva = Lienzo.PestanaPulsada();
        if (K(KeyboardKey.L)) nueva = Ruta.Recta;
        if (K(KeyboardKey.C)) nueva = Ruta.Circular;
        if (K(KeyboardKey.O)) nueva = Ruta.Senoidal;
        if (K(KeyboardKey.B)) nueva = Ruta.Parabola;
        if (nueva.HasValue && ruta != nueva.Value) { ruta=nueva.Value; t=0; }
        if (K(KeyboardKey.A)) a=(a+1)%amplitudes.Length;
        if (K(KeyboardKey.N)) n=n%3+1;
        if (K(KeyboardKey.F)) {
            float fase=t/periodos[f]; f=(f+1)%periodos.Length; t=fase*periodos[f];
        }
        if (K(KeyboardKey.T)) tangente=!tangente;
        if (K(KeyboardKey.Space)) pausa=!pausa;
        if (K(KeyboardKey.R)) { t=0; pausa=true; }

        Trayecto camino = default;
        if (visible != null) {
            var r=visible.Datos.Resultado;
            float signo=r.DireccionSombra=="oeste" ? 1 : -1;
            camino=new((float)visible.Patio.ParedXM,signo*(float)r.LongitudSombraM,
                (float)visible.Entrada.AlturaParedM+1.8f,amplitudes[a],n,periodos[f]);
            if (K(KeyboardKey.K)) { ruta=Ruta.Senoidal; t=camino.TiempoPrimeraCresta(); pausa=true; }
            if (!pausa) t=(t+Math.Min(Raylib.GetFrameTime(),.1f))%periodos[f];
        }
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Lienzo.Fondo);
        if (visible != null) Lienzo.DibujarEscena(visible,camino,ruta,t,tangente);
        else Raylib.DrawText("Todavia no hay datos. ENTER = consultar, M = respaldo.",170,340,22,Lienzo.Tinta);
        Lienzo.Panel(visible,seleccion,camino,ruta,t,estado,aviso,respaldo,pausa);
        Raylib.EndDrawing();
    }
}
finally { Raylib.CloseWindow(); api?.Dispose(); }
