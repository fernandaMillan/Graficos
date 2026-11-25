using CampusSolar;
using Raylib_cs;

// Un único hilo dibuja. La tarea de red solo produce datos; nunca llama a Raylib.
using var api = new CampusApi();
var seleccion = CampusApi.CasoBase;
Cuadro? aplicado = null;
Task<Cuadro>? pendiente = null;
string estado = "Inicio", aviso = "ENTER: consultar la API. M: abrir el ejemplo local.";
int[] horas = [8, 10, 12, 14, 16];
bool tresD = false, orto = false, luz = true;
float fase = 0;
Raylib.InitWindow(1080, 660, "Sistemas Vivos | Sombra en el campus");
Raylib.SetTargetFPS(60);
try
{
    while (!Raylib.WindowShouldClose())
    {
        bool tecla(KeyboardKey k) => Raylib.IsKeyPressed(k);
        int h = Array.IndexOf(horas, seleccion.HoraLocal);
        if (tecla(KeyboardKey.Right)) seleccion = seleccion with { HoraLocal = horas[(h+1)%5] };
        if (tecla(KeyboardKey.Left))  seleccion = seleccion with { HoraLocal = horas[(h+4)%5] };
        if (tecla(KeyboardKey.Up))   seleccion = seleccion with { Nubosidad = Math.Min(100, seleccion.Nubosidad+5) };
        if (tecla(KeyboardKey.Down)) seleccion = seleccion with { Nubosidad = Math.Max(0, seleccion.Nubosidad-5) };
        if (tecla(KeyboardKey.W)) seleccion = seleccion with { AlturaParedM = Math.Min(8, seleccion.AlturaParedM+1) };
        if (tecla(KeyboardKey.S)) seleccion = seleccion with { AlturaParedM = Math.Max(1, seleccion.AlturaParedM-1) };
        if (tecla(KeyboardKey.Tab)) tresD = !tresD;
        if (tecla(KeyboardKey.O)) orto = !orto;
        if (tecla(KeyboardKey.L)) luz = !luz;
        if (pendiente == null && tecla(KeyboardKey.Enter))
        {
            pendiente = api.Consultar(seleccion);
            estado = "Cargando"; aviso = "Esperando respuesta. La ultima escena sigue activa.";
        }
        if (pendiente is { IsCompleted: true })
        {
            try { aplicado = pendiente.GetAwaiter().GetResult(); estado = "API"; aviso = "Se recibio una respuesta valida."; }
            catch (Exception e) { estado = "Error"; aviso = e.Message + " | M: respaldo."; }
            pendiente = null;
        }
        if (pendiente == null && tecla(KeyboardKey.M))
        {
            try { aplicado = CampusApi.LeerRespaldo(); seleccion = aplicado.Entrada;
                estado = "Respaldo"; aviso = "mock-response.json: caso fijo de las 10 h. ENTER para volver a la API."; }
            catch(Exception e) { estado="Error"; aviso=e.Message; }
        }

        fase = (fase + Math.Min(Raylib.GetFrameTime(), 0.1f)*0.55f) % 1;
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(229, 240, 243, 255));
        if (aplicado != null)
        {
            if (tresD) Escena.Vista3D(aplicado, fase, orto, luz);
            else Escena.Vista2D(aplicado, fase);
        }
        else Raylib.DrawText("Sin datos: predice primero donde llegara la sombra.", 155,310,25,Color.DarkBlue);
        Raylib.DrawRectangle(0,0,1080,102,Color.RayWhite);
        Raylib.DrawText("EXPLANADA  /  " + (tresD ? (orto ? "3D ortografica" : "3D perspectiva") : "2D lateral") + "  /  " + estado,20,12,25,Color.DarkBlue);
        Raylib.DrawText($"Controles: {seleccion.HoraLocal}:00  |  Nubes {seleccion.Nubosidad}%  |  Pared {seleccion.AlturaParedM} m     ENTER = aplicar",20,45,19,Color.Black);
        Raylib.DrawText(aviso.Length>107 ? aviso[..104]+"..." : aviso,20,75,15,estado=="Error" ? Color.Maroon : Color.DarkGray);
        Raylib.DrawRectangle(0,554,1080,106,Color.RayWhite);
        if (aplicado != null) {
            var r = aplicado.Datos.Resultado;
            Raylib.DrawText($"Escena: {aplicado.Entrada.HoraLocal}:00 | {aplicado.Entrada.Nubosidad}% | pared {aplicado.Entrada.AlturaParedM} m | sombra {r.LongitudSombraM:0.00} m al {r.DireccionSombra} | contraste {aplicado.Datos.Visual.ContrasteSombra:0.00}",20,564,18,Color.DarkBlue);
            Raylib.DrawText("Centro de la mesa: " + (r.MesaEnSombra ? "EN SOMBRA" : "AL SOL"),20,588,19,Color.Black);
        }
        Raylib.DrawText("Izq/Der: hora   Arr/Ab: nubes   W/S: altura   M: respaldo   TAB: 2D/3D   O: proyeccion   L: luz 3D",20,628,17,Color.DarkGray);
        Raylib.EndDrawing();
    }
}
finally { Raylib.CloseWindow(); }
