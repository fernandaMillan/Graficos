using System;
using System.Globalization;
using System.Numerics;
using Raylib_cs;

// Ejemplo docente independiente. No sustituye el laboratorio de las cuatro prácticas.
internal static class Program
{
    private const float DtManual = 0.5f;
    private const float Escala = 60f;
    private static readonly Vector2 Origen = new(74, 562);
    private static readonly Color Tinta = new(26, 39, 53, 255);
    private static readonly Color Azul = new(29, 77, 117, 255);
    private static readonly Color Verde = new(0, 113, 94, 255);
    private static readonly Color Naranja = new(172, 81, 13, 255);
    private static readonly Color Borde = new(216, 225, 233, 255);

    private record struct Caso(string Nombre, Vector2 Inicio, Vector2 Meta);
    private record struct Paso(Vector2 Antes, Vector2 Meta, Vector2 D,
        float Distancia, Vector2 Direccion, float Rapidez, float Dt,
        float Nominal, float Aplicado, Vector2 Despues);

    private static readonly Caso[] Casos =
    {
        new("Diagonal 3-4-5", new(2, 2), new(5, 6)),
        new("Horizontal", new(2, 2), new(5, 2)),
        new("P y Q coinciden", new(2, 2), new(2, 2)),
        new("Llegada sin pasarse", new(4.9f, 6), new(5, 6))
    };

    [STAThread]
    private static int Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        if (Array.IndexOf(args, "--probar") >= 0) return Probar();

        int indice = 0;
        float rapidez = 2f;
        Vector2 posicion = Casos[0].Inicio;
        Paso? ultimo = null;
        bool animar = false;
        Raylib.InitWindow(1160, 730, "Sistemas Vivos | Practica 1 - Vectores");
        Raylib.SetTargetFPS(60);
        try
        {
            while (!Raylib.WindowShouldClose())
            {
                int elegido = Raylib.IsKeyPressed(KeyboardKey.One) ? 0 :
                    Raylib.IsKeyPressed(KeyboardKey.Two) ? 1 :
                    Raylib.IsKeyPressed(KeyboardKey.Three) ? 2 :
                    Raylib.IsKeyPressed(KeyboardKey.Four) ? 3 : -1;
                if (elegido >= 0 || Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    if (elegido >= 0) indice = elegido;
                    posicion = Casos[indice].Inicio;
                    rapidez = 2;
                    ultimo = null;
                    animar = false;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.Up))
                    rapidez = Math.Min(5f, rapidez + 1);
                if (Raylib.IsKeyPressed(KeyboardKey.Down))
                    rapidez = Math.Max(1f, rapidez - 1);

                Caso caso = Casos[indice];
                if (Raylib.IsKeyPressed(KeyboardKey.Enter)) animar = !animar;
                bool manual = Raylib.IsKeyPressed(KeyboardKey.Space);
                if (manual) animar = false;
                if (manual || animar)
                {
                    float dt = manual ? DtManual : Raylib.GetFrameTime();
                    Paso muestra = CalcularPaso(posicion, caso.Meta, rapidez, dt);
                    posicion = muestra.Despues;
                    ultimo = muestra;
                    if (posicion == caso.Meta) animar = false;
                }
                Dibujar(caso, indice, posicion, rapidez, animar, ultimo);
            }
        }
        finally { Raylib.CloseWindow(); }
        return 0;
    }

    // Única función de movimiento: también la ejecutan las pruebas de consola.
    private static Paso CalcularPaso(Vector2 p, Vector2 q, float rapidez, float dt)
    {
        Vector2 d = q - p;                       // 1. De P a Q.
        float distancia = d.Length();            // 2. Longitud de d.
        float nominal = rapidez * dt;            // 3. Avance sin límite.
        Vector2 direccion = distancia == 0 ? Vector2.Zero : d / distancia;
        float aplicado = MathF.Min(nominal, distancia);
        // Si el paso cubre lo que falta, se asigna Q exactamente.
        Vector2 nuevo = aplicado >= distancia ? q : p + direccion * aplicado;
        return new(p, q, d, distancia, direccion, rapidez, dt, nominal, aplicado, nuevo);
    }

    private static Vector2 Pantalla(Vector2 p) =>
        new(Origen.X + Escala * p.X, Origen.Y - Escala * p.Y);

    private static string Par(Vector2 p) => $"({p.X:0.###}, {p.Y:0.###})";

    private static void Flecha(Vector2 a, Vector2 b, Color color)
    {
        Vector2 diferencia = b - a;
        float largo = diferencia.Length();
        if (largo < 1) return;
        Vector2 eje = diferencia / largo;
        Vector2 perpendicular = new(-eje.Y, eje.X);
        float punta = MathF.Min(13, largo * 0.35f);
        Vector2 basePunta = b - eje * punta;
        Raylib.DrawLineEx(a, b, 3f, color);
        Raylib.DrawLineEx(b, basePunta + perpendicular * punta * 0.45f, 3f, color);
        Raylib.DrawLineEx(b, basePunta - perpendicular * punta * 0.45f, 3f, color);
    }

    private static void Dibujar(Caso caso, int indice, Vector2 posicion, float rapidez,
        bool animar, Paso? ultimo)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.RayWhite);
        Raylib.DrawText("Practica 1   Vectores y movimiento", 35, 26, 30, Tinta);
        Raylib.DrawText($"Caso {indice + 1}: {caso.Nombre}", 36, 69, 22, Azul);
        Raylib.DrawText("Plano de mundo", 36, 109, 20, Tinta);

        for (int x = 0; x <= 8; x++)
        {
            Vector2 a = Pantalla(new(x, 0));
            Raylib.DrawLineEx(a, Pantalla(new(x, 7)), x == 0 ? 2 : 1, x == 0 ? Tinta : Borde);
            Raylib.DrawText(x.ToString(), (int)a.X - 5, 572, 15, Tinta);
        }
        for (int y = 0; y <= 7; y++)
        {
            Vector2 a = Pantalla(new(0, y));
            Raylib.DrawLineEx(a, Pantalla(new(8, y)), y == 0 ? 2 : 1, y == 0 ? Tinta : Borde);
            if (y > 0) Raylib.DrawText(y.ToString(), 51, (int)a.Y - 7, 15, Tinta);
        }
        Raylib.DrawText("x", 564, 553, 18, Tinta);
        Raylib.DrawText("y", 65, 125, 18, Tinta);

        // Antes de mover, P es el agente. Después se conserva la P del último cálculo.
        Vector2 p = Pantalla(ultimo?.Antes ?? posicion);
        Vector2 q = Pantalla(caso.Meta);
        Vector2 esquina = new(q.X, p.Y);
        Raylib.DrawLineEx(p, esquina, 2, Borde);
        Raylib.DrawLineEx(esquina, q, 2, Borde);
        Flecha(p, q, Azul);
        if (ultimo is Paso avance) Flecha(p, Pantalla(avance.Despues), Verde);
        Raylib.DrawCircleLines((int)p.X, (int)p.Y, 5, Azul);
        Raylib.DrawCircleLines((int)q.X, (int)q.Y, 11, Naranja);
        Raylib.DrawCircleV(Pantalla(posicion), 5, Verde);
        Raylib.DrawText("Azul: de P a Q   Verde: avance   Aro naranja: meta", 36, 610, 18, Tinta);

        Raylib.DrawRectangle(600, 109, 525, 519, Color.White);
        Raylib.DrawRectangleLinesEx(new Rectangle(600, 109, 525, 519), 1, Borde);
        Raylib.DrawText("Estado actual", 623, 132, 21, Azul);
        Raylib.DrawText($"Agente {Par(posicion)}     Meta {Par(caso.Meta)}", 623, 166, 20, Tinta);
        Raylib.DrawText($"Rapidez: {rapidez:0} u/s     {(animar ? "En animacion" : "En pausa")}", 623, 199, 18, Tinta);
        Raylib.DrawLine(623, 232, 1102, 232, Borde);
        Raylib.DrawText("Ultimo paso aplicado", 623, 251, 21, Azul);
        if (ultimo is Paso s)
        {
            string dir = s.Distancia == 0 ? "no se normaliza" : Par(s.Direccion);
            string[] filas =
            {
                $"P antes = {Par(s.Antes)}",
                $"Desplazamiento Q - P = {Par(s.D)}",
                $"Distancia restante = {s.Distancia:0.###} u",
                $"Direccion unitaria = {dir}",
                $"v = {s.Rapidez:0} u/s     dt = {s.Dt:0.####} s",
                $"Paso nominal (v x dt) = {s.Nominal:0.###} u",
                $"Paso aplicado (minimo) = {s.Aplicado:0.###} u",
                $"P nueva = {Par(s.Despues)}"
            };
            for (int i = 0; i < filas.Length; i++)
                Raylib.DrawText(filas[i], 623, 291 + 34 * i, 19, i == 7 ? Verde : Tinta);
        }
        else
        {
            Raylib.DrawText("Predice el resultado en papel.", 623, 295, 20, Tinta);
            Raylib.DrawText("Pulsa ESPACIO para revelar un paso.", 623, 334, 20, Tinta);
        }
        Raylib.DrawText("1-4 Cambiar caso   |   ESPACIO Paso de 0.5 s   |   ENTER Animar / pausar", 36, 662, 19, Tinta);
        Raylib.DrawText("Flechas arriba / abajo: rapidez   |   R Reiniciar caso   |   ESC Salir", 36, 693, 18, Tinta);
        Raylib.EndDrawing();
    }

    private static int Probar()
    {
        Vector2[] esperados = { new(2.6f, 2.8f), new(3, 2), new(2, 2), new(5, 6) };
        float[] pasos = { 1, 1, 0, 0.1f };
        int fallos = 0;
        for (int i = 0; i < Casos.Length; i++)
        {
            Caso c = Casos[i];
            Paso s = CalcularPaso(c.Inicio, c.Meta, 2, DtManual);
            bool ok = Vector2.Distance(s.Despues, esperados[i]) < 0.0001f &&
                MathF.Abs(s.Aplicado - pasos[i]) < 0.0001f &&
                (s.Distancia == 0 || MathF.Abs(s.Direccion.Length() - 1f) < 0.0001f);
            Console.WriteLine($"{(ok ? "OK" : "FALLO")} | {c.Nombre}: {Par(s.Despues)}");
            if (!ok) fallos++;
        }
        return fallos == 0 ? 0 : 1;
    }
}
