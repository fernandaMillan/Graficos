using System;
using System.Numerics;
using Raylib_cs;

internal static class Program
{
    // Variables que puede modificar el usuario
    private static int profundidad = 7;
    private static float apertura = 28f * MathF.PI / 180f;
    private static float factorEscala = 0.72f;
    private static int colorSeleccionado = 0;

    private static readonly Color[] Paleta =
    {
        Color.DarkGreen,
        Color.DarkBlue,
        Color.Maroon,
        Color.Purple
    };

    [STAThread]
    public static void Main()
    {
        const int anchoVentana = 1100;
        const int altoVentana = 720;

        Raylib.InitWindow(
            anchoVentana,
            altoVentana,
            "Recursion grafica con Raylib-cs"
        );

        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            ActualizarControles();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);

            DibujarInterfaz();

            Vector2 inicio = new(
                anchoVentana / 2f,
                altoVentana - 45f
            );

            float anguloInicial = -90f * MathF.PI / 180f;

            DibujarRama(
                inicio,
                150f,
                anguloInicial,
                profundidad
            );

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    private static void ActualizarControles()
    {
        // Aumentar o disminuir profundidad
        if (Raylib.IsKeyPressed(KeyboardKey.Up))
        {
            profundidad++;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Down))
        {
            profundidad--;
        }

        profundidad = Math.Clamp(profundidad, 1, 12);

        // Modificar la apertura de las ramas
        if (Raylib.IsKeyDown(KeyboardKey.Left))
        {
            apertura -= 0.01f;
        }

        if (Raylib.IsKeyDown(KeyboardKey.Right))
        {
            apertura += 0.01f;
        }

        apertura = Math.Clamp(
            apertura,
            5f * MathF.PI / 180f,
            75f * MathF.PI / 180f
        );

        // Modificar el factor de reducción
        if (Raylib.IsKeyDown(KeyboardKey.Q))
        {
            factorEscala -= 0.002f;
        }

        if (Raylib.IsKeyDown(KeyboardKey.E))
        {
            factorEscala += 0.002f;
        }

        factorEscala = Math.Clamp(
            factorEscala,
            0.50f,
            0.85f
        );

        // Cambiar color
        if (Raylib.IsKeyPressed(KeyboardKey.M))
        {
            colorSeleccionado++;

            if (colorSeleccionado >= Paleta.Length)
            {
                colorSeleccionado = 0;
            }
        }

        // Restablecer valores
        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            profundidad = 7;
            apertura = 28f * MathF.PI / 180f;
            factorEscala = 0.72f;
            colorSeleccionado = 0;
        }
    }

    private static void DibujarRama(
        Vector2 inicio,
        float longitud,
        float angulo,
        int nivel
    )
    {
        // Caso base: detiene la recursión
        if (nivel <= 0 || longitud < 3f)
        {
            return;
        }

        // Calcular el desplazamiento horizontal
        float desplazamientoX =
            MathF.Cos(angulo) * longitud;

        // Calcular el desplazamiento vertical
        float desplazamientoY =
            MathF.Sin(angulo) * longitud;

        // Calcular dónde termina la rama
        Vector2 fin = new(
            inicio.X + desplazamientoX,
            inicio.Y + desplazamientoY
        );

        // Las ramas superiores serán más delgadas
        float grosor = MathF.Max(1f, nivel * 0.85f);

        Raylib.DrawLineEx(
            inicio,
            fin,
            grosor,
            Paleta[colorSeleccionado]
        );

        // Rama izquierda
        DibujarRama(
            fin,
            longitud * factorEscala,
            angulo - apertura,
            nivel - 1
        );

        // Rama derecha
        DibujarRama(
            fin,
            longitud * factorEscala,
            angulo + apertura,
            nivel - 1
        );
    }

    private static void DibujarInterfaz()
    {
        float aperturaGrados =
            apertura * 180f / MathF.PI;

        int cantidadRamas =
            (1 << profundidad) - 1;

        Raylib.DrawText(
            "Arbol recursivo",
            25,
            20,
            30,
            Color.Black
        );

        Raylib.DrawText(
            $"Profundidad: {profundidad}",
            25,
            70,
            20,
            Color.DarkGray
        );

        Raylib.DrawText(
            $"Apertura: {aperturaGrados:F1} grados",
            25,
            100,
            20,
            Color.DarkGray
        );

        Raylib.DrawText(
            $"Factor de escala: {factorEscala:F2}",
            25,
            130,
            20,
            Color.DarkGray
        );

        Raylib.DrawText(
            $"Lineas esperadas: {cantidadRamas}",
            25,
            160,
            20,
            Color.DarkGray
        );

        Raylib.DrawText(
            "Flechas arriba/abajo: profundidad",
            25,
            215,
            18,
            Color.Gray
        );

        Raylib.DrawText(
            "Flechas izquierda/derecha: apertura",
            25,
            242,
            18,
            Color.Gray
        );

        Raylib.DrawText(
            "Q/E: escala de las ramas",
            25,
            269,
            18,
            Color.Gray
        );

        Raylib.DrawText(
            "M: cambiar color",
            25,
            296,
            18,
            Color.Gray
        );

        Raylib.DrawText(
            "R: restablecer valores",
            25,
            323,
            18,
            Color.Gray
        );

        Raylib.DrawText(
            "ESC: cerrar",
            25,
            350,
            18,
            Color.Gray
        );

        Raylib.DrawRectangle(
            25,
            390,
            45,
            45,
            Paleta[colorSeleccionado]
        );

        Raylib.DrawRectangleLines(
            25,
            390,
            45,
            45,
            Color.Black
        );
    }
}