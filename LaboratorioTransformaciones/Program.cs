using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Raylib_cs;

internal static class Program
{
    private enum Modo { Traslacion, Automatico, Manual, Mano, Pivote, Orden }

    private const float RapidezAutomatica = 60f; // Cambia solo esta constante en el reto.
    private const float RapidezDeReferencia = 60f;
    private static readonly Vector2[] Original =
        [new(-1, -1), new(1, -1), new(1, 1), new(0, 2), new(-1, 1)];
    private static readonly Vector2 PuntoPrueba = new(1, 0);
    private static readonly Vector2 Pivote = new(2, 0);
    private static readonly string[] Tabs =
        ["1 Traslacion", "2 Giro automatico", "3 Giro manual", "4 Mano", "5 Pivote", "6 Orden"];

    private static Modo modo;
    private static float tx, ty, angulo;
    private static bool pausa, invertir, arrastrando;
    private static Camara? camara;
    private static string aviso = "";

    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Contains("--probar"))
        {
            ProbarMatrices();
            return;
        }
        if (args.Contains("--probar-puente"))
        {
            ProbarPuente().GetAwaiter().GetResult();
            return;
        }

        Raylib.InitWindow(1200, 710, "Practica 3 | Traslacion y rotacion");
        Raylib.SetTargetFPS(60);
        try
        {
            while (!Raylib.WindowShouldClose())
            {
                Actualizar();
                Dibujar(MatrizActual());
            }
        }
        finally
        {
            CerrarCamara();
            Raylib.CloseWindow();
        }
    }

    private static void Actualizar()
    {
        var mouse = Raylib.GetMousePosition();
        bool clic = Raylib.IsMouseButtonPressed(MouseButton.Left);
        for (int i = 0; i < Tabs.Length; i++)
        {
            int x = 20 + i * 194;
            if (clic && mouse.Y >= 58 && mouse.Y < 100 && mouse.X >= x && mouse.X < x + 184)
            {
                Modo siguiente = (Modo)i;
                if (siguiente != modo)
                {
                    if (modo == Modo.Mano) CerrarCamara();
                    modo = siguiente;
                    Reiniciar();
                }
                break;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.R)) Reiniciar();
        if ((modo is Modo.Automatico or Modo.Pivote) && Raylib.IsKeyPressed(KeyboardKey.P))
            pausa = !pausa;
        if (modo == Modo.Orden && Raylib.IsKeyPressed(KeyboardKey.O)) invertir = !invertir;
        if (modo != Modo.Mano && Raylib.IsKeyPressed(KeyboardKey.E))
        {
            // Casos exactos para comparar con papel, sin depender del timing.
            if (modo == Modo.Traslacion) (tx, ty) = (2f, 1f);
            else
            {
                angulo = 90f;
                pausa = modo is Modo.Automatico or Modo.Pivote;
            }
        }
        if (modo == Modo.Mano && Raylib.IsKeyPressed(KeyboardKey.C)) AbrirCamara();

        float dt = MathF.Min(Raylib.GetFrameTime(), 0.1f);
        if (modo == Modo.Traslacion)
        {
            float dx = (Raylib.IsKeyDown(KeyboardKey.Right) ? 1 : 0)
                     - (Raylib.IsKeyDown(KeyboardKey.Left) ? 1 : 0);
            float dy = (Raylib.IsKeyDown(KeyboardKey.Up) ? 1 : 0)
                     - (Raylib.IsKeyDown(KeyboardKey.Down) ? 1 : 0);
            tx = Math.Clamp(tx + dx * 2f * dt, -5f, 5f);
            ty = Math.Clamp(ty + dy * 2f * dt, -3.5f, 3.5f);
        }
        else if (modo is Modo.Manual or Modo.Orden)
        {
            if (Raylib.IsKeyDown(KeyboardKey.Left)) angulo -= RapidezDeReferencia * dt;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) angulo += RapidezDeReferencia * dt;
            if (clic && mouse.Y >= 622 && mouse.Y <= 655 && mouse.X >= 69 && mouse.X <= 731)
                arrastrando = true;
            if (!Raylib.IsMouseButtonDown(MouseButton.Left)) arrastrando = false;
            if (arrastrando) angulo = Math.Clamp((mouse.X - 80f) / 640f, 0f, 1f) * 359f;
        }
        else if (!pausa && modo == Modo.Automatico) angulo += RapidezAutomatica * dt;
        else if (!pausa && modo == Modo.Pivote) angulo += RapidezDeReferencia * dt;
        else if (modo == Modo.Mano && camara?.PalmaAbierta == true)
            angulo += RapidezDeReferencia * dt; // El gesto habilita el giro. No cambia la matriz.

        if (angulo is < 0f or >= 360f) angulo = (angulo % 360f + 360f) % 360f;
    }

    private static void AbrirCamara()
    {
        try
        {
            camara ??= new Camara();
            Console.WriteLine($"Control de la mano: {camara.Direccion}");
            using var navegador = Process.Start(new ProcessStartInfo(camara.Direccion)
            {
                UseShellExecute = true
            });
            aviso = "Activa la camara en Chrome o Edge.";
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            aviso = "No se abrio la pagina. Revisa la terminal.";
        }
    }

    private static void CerrarCamara()
    {
        try { camara?.Dispose(); }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); }
        finally { camara = null; aviso = ""; }
    }

    private static void Reiniciar()
    {
        tx = 0f;
        ty = 0f;
        angulo = 0f;
        pausa = invertir = arrastrando = false;
    }

    // Convención: vectores columna, Y positiva hacia arriba en el mundo.
    private static float[,] T(float x, float y) => new float[,] { { 1, 0, x }, { 0, 1, y }, { 0, 0, 1 } };
    private static float[,] R(float grados)
    {
        float radianes = grados * MathF.PI / 180f;
        float c = MathF.Cos(radianes), s = MathF.Sin(radianes);
        return new float[,] { { c, -s, 0 }, { s, c, 0 }, { 0, 0, 1 } };
    }

    private static float[,] Por(float[,] a, float[,] b)
    {
        float[,] c = new float[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                for (int k = 0; k < 3; k++)
                    c[i, j] += a[i, k] * b[k, j];
        return c;
    }

    private static Vector2 Aplicar(float[,] m, Vector2 p) =>
        new(m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2],
            m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2]);

    private static float[,] Alrededor(float grados, Vector2 c) =>
        Por(T(c.X, c.Y), Por(R(grados), T(-c.X, -c.Y)));

    private static float[,] MatrizActual() => modo switch
    {
        Modo.Traslacion => T(tx, ty),
        Modo.Pivote => Alrededor(angulo, Pivote),
        Modo.Orden => invertir ? Por(R(angulo), T(3, 0)) : Por(T(3, 0), R(angulo)),
        _ => R(angulo)
    };

    private static Vector2 Pantalla(Vector2 p) => new(400 + 40 * p.X, 365 - 40 * p.Y);

    private static void Dibujar(float[,] matriz)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.RayWhite);
        Raylib.DrawText("Practica 3   Traslacion y rotacion", 20, 16, 27, Color.DarkBlue);
        for (int i = 0; i < Tabs.Length; i++)
        {
            bool activo = i == (int)modo;
            Raylib.DrawRectangle(20 + i * 194, 58, 184, 42, activo ? Color.DarkBlue : Color.LightGray);
            Raylib.DrawText(Tabs[i], 30 + i * 194, 71, 17, activo ? Color.White : Color.Black);
        }

        for (int x = -8; x <= 8; x++)
            Raylib.DrawLineV(Pantalla(new(x, -6)), Pantalla(new(x, 6)), x == 0 ? Color.Gray : Color.LightGray);
        for (int y = -6; y <= 6; y++)
            Raylib.DrawLineV(Pantalla(new(-8, y)), Pantalla(new(8, y)), y == 0 ? Color.Gray : Color.LightGray);
        Raylib.DrawText("+X", 720, 375, 16, Color.Gray);
        Raylib.DrawText("+Y", 410, 125, 16, Color.Gray);

        Poligono(T(0, 0), Color.Gray, 2);
        Poligono(matriz, Color.DarkBlue, 4);
        Vector2 resultado = Aplicar(matriz, PuntoPrueba);
        Raylib.DrawCircleV(Pantalla(PuntoPrueba), 5, Color.Gray);
        Raylib.DrawCircleV(Pantalla(resultado), 6, Color.Orange);
        Raylib.DrawCircleV(Pantalla(Vector2.Zero), 3, Color.Gray);
        if (modo == Modo.Pivote)
        {
            var p = Pantalla(Pivote);
            Raylib.DrawCircleV(p, 6, Color.Red);
            Raylib.DrawText($"C=({Pivote.X:G}, {Pivote.Y:G})", (int)p.X + 10, (int)p.Y + 10, 17, Color.Red);
        }

        string entrada = modo switch
        {
            Modo.Traslacion => "Flechas: mueve en X y Y",
            Modo.Automatico => $"Tiempo: {RapidezAutomatica:G} grados/s",
            Modo.Manual => "Slider o flechas: elige el angulo",
            Modo.Mano => "C: abre el control de la camara",
            Modo.Pivote => "Tiempo: giro sobre un pivote fijo",
            _ => "O: compara los dos ordenes"
        };
        Raylib.DrawText(entrada, 765, 126, 20, Color.DarkBlue);
        string parametro = modo == Modo.Traslacion
            ? $"tx={tx:F2}   ty={ty:F2}"
            : $"Angulo: {angulo:F1} grados";
        Raylib.DrawText(parametro, 765, 163, 22, Color.Black);
        string caso = modo switch
        {
            Modo.Traslacion => "E carga T=(2,1)",
            Modo.Mano => "La palma abierta solo habilita el giro",
            Modo.Orden => invertir ? "R*T: traslada, luego gira" : "T*R: gira, luego traslada",
            _ => "E carga 90 grados"
        };
        Raylib.DrawText(caso, 765, 204, 18, Color.DarkGray);
        Raylib.DrawText("Matriz aplicada (vectores columna)", 765, 246, 20, Color.DarkBlue);
        for (int i = 0; i < 3; i++)
        {
            string fila = string.Join("   ", Enumerable.Range(0, 3).Select(j => Formato(matriz[i, j])));
            Raylib.DrawText($"[   {fila}   ]", 765, 280 + i * 29, 19, Color.Black);
        }
        Raylib.DrawText("Punto de prueba p = (1,0)", 765, 396, 19, Color.DarkGray);
        Raylib.DrawText($"M*p = ({Formato(resultado.X)}, {Formato(resultado.Y)})",
                        765, 431, 22, Color.DarkBlue);
        Raylib.DrawText("Gris: original / azul: resultado", 765, 477, 18, Color.DarkGray);
        Raylib.DrawText("Punto naranja: imagen de p", 765, 503, 18, Color.DarkGray);
        if (pausa) Raylib.DrawText("Pausa activa. P reanuda.", 765, 552, 19, Color.DarkBlue);
        if (modo == Modo.Mano)
        {
            bool giro = camara?.PalmaAbierta == true;
            string estado = giro ? "Palma abierta: girando"
                : camara?.TieneSenal == true ? "Sin palma abierta: detenido" : "Sin senal de mano: detenido";
            Raylib.DrawText(estado, 765, 550, 20, giro ? Color.DarkGreen : Color.DarkGray);
            Raylib.DrawText(aviso, 765, 579, 17, Color.DarkGray);
        }
        if (modo is Modo.Manual or Modo.Orden)
        {
            Raylib.DrawText("Angulo manual: 0 a 359 grados   |   E: 90 grados exactos", 80, 608, 17, Color.DarkGray);
            Raylib.DrawRectangle(80, 635, 640, 5, Color.Gray);
            Raylib.DrawCircle(80 + (int)(640 * MathF.Min(angulo, 359f) / 359f), 637, 9, Color.DarkBlue);
        }
        Raylib.DrawText("R reinicia   |   P pausa tiempo y pivote   |   ESC sale   |   40 px = 1 unidad",
                        20, 674, 18, Color.DarkGray);
        Raylib.EndDrawing();
    }

    private static string Formato(float x) =>
        (MathF.Abs(x) < 0.005f ? 0f : x).ToString("0.00", CultureInfo.InvariantCulture);

    private static void Poligono(float[,] m, Color color, float grosor)
    {
        // Siempre parte de la figura original; no acumula las matrices cuadro a cuadro.
        for (int i = 0; i < Original.Length; i++)
            Raylib.DrawLineEx(Pantalla(Aplicar(m, Original[i])),
                              Pantalla(Aplicar(m, Original[(i + 1) % Original.Length])),
                              grosor, color);
    }

    private static void ProbarMatrices()
    {
        static void Igual(Vector2 a, Vector2 b)
        {
            if (Vector2.Distance(a, b) > 0.001f) throw new Exception($"Se esperaba {b}; obtenido {a}.");
        }
        Igual(Aplicar(T(2, 1), PuntoPrueba), new(3, 1));
        Igual(Aplicar(R(90), PuntoPrueba), new(0, 1));
        Igual(Aplicar(R(180), PuntoPrueba), new(-1, 0));
        Igual(Aplicar(Alrededor(90, Pivote), PuntoPrueba), new(2, -1));
        Igual(Aplicar(Alrededor(90, Pivote), Pivote), Pivote);
        Igual(Aplicar(Por(T(3, 0), R(90)), PuntoPrueba), new(3, 1));
        Igual(Aplicar(Por(R(90), T(3, 0)), PuntoPrueba), new(0, 4));
        Igual(Pantalla(new(0, 1)), new(400, 325));
        Console.WriteLine("OK: traslacion, rotacion, pivote, orden y conversion a pantalla.");
    }

    private static async Task ProbarPuente()
    {
        using var puente = new Camara();
        using var cliente = new HttpClient();
        string pagina = await cliente.GetStringAsync(puente.Direccion);
        if (!pagina.Contains("Activar cámara")) throw new Exception("No se recibio camara.html.");
        cliente.DefaultRequestHeaders.Add("Origin", puente.Direccion);
        using var si = await cliente.PostAsync(puente.Direccion + "/gesto", new StringContent("1"));
        si.EnsureSuccessStatusCode();
        if (!puente.TieneSenal || !puente.PalmaAbierta) throw new Exception("El puente no activo el giro.");
        using var no = await cliente.PostAsync(puente.Direccion + "/gesto", new StringContent("0"));
        no.EnsureSuccessStatusCode();
        if (puente.PalmaAbierta) throw new Exception("El puente no detuvo el giro.");
        using var otra = await cliente.PostAsync(puente.Direccion + "/gesto", new StringContent("1"));
        otra.EnsureSuccessStatusCode();
        await Task.Delay(1000);
        if (puente.TieneSenal || puente.PalmaAbierta) throw new Exception("La senal no vencio.");
        Console.WriteLine("OK: pagina local, inicio y parada del giro y perdida de senal. No prueba la camara.");
    }
}
