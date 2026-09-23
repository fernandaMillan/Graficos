using Raylib_cs;

class Program
{
    const int VentanaAncho = 940, VentanaAlto = 690;
    const int LienzoX = 360, LienzoY = 115, LienzoAncho = 500, LienzoAlto = 250;
    const int SliderX = 430, SliderY = 420, SliderAncho = 360;

    static void Main()
    {
        Raylib.InitWindow(VentanaAncho, VentanaAlto, "Practica 3 - Color Lab");
        Raylib.SetTargetFPS(60);

        // Sustituye estos colores por los cinco extraidos de tu imagen.
        string[] nombres = { "Rojo coral", "Azul profundo", "Verde hoja", "Amarillo sol", "Morado" };
        Color[] colores =
        {
            new Color(220, 50, 50, 255),
            new Color(30, 80, 180, 255),
            new Color(40, 170, 90, 255),
            new Color(245, 200, 40, 255),
            new Color(140, 80, 180, 255)
        };

        int seleccionado = 0, colorLienzo = 0, colorA = 0, colorB = 1, alpha = 255;

        while (!Raylib.WindowShouldClose())
        {
            int mouseX = Raylib.GetMouseX(), mouseY = Raylib.GetMouseY();

            for (int i = 0; i < colores.Length; i++)
            {
                int y = 120 + i * 70;
                if (!EstaDentro(mouseX, mouseY, 25, y, 285, 58)) continue;

                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    seleccionado = i;
                    colorA = i;
                }
                if (Raylib.IsMouseButtonPressed(MouseButton.Right)) colorB = i;
            }

            if (EstaDentro(mouseX, mouseY, LienzoX, LienzoY, LienzoAncho, LienzoAlto)
                && Raylib.IsMouseButtonPressed(MouseButton.Left))
                colorLienzo = seleccionado;

            if (EstaDentro(mouseX, mouseY, SliderX - 8, SliderY - 14, SliderAncho + 16, 28)
                && Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                float proporcion = Math.Clamp((mouseX - SliderX) / (float)SliderAncho, 0f, 1f);
                alpha = (int)Math.Round(proporcion * 255);
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(242, 244, 247, 255));
            DibujarTitulo();
            DibujarPaleta(nombres, colores, seleccionado, colorA, colorB);
            DibujarLienzo(colores[colorLienzo], alpha);
            DibujarSlider(alpha);
            DibujarDegradado(nombres[colorA], nombres[colorB], colores[colorA], colores[colorB]);
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }

    static void DibujarTitulo()
    {
        Raylib.DrawText("COLOR LAB", 25, 25, 30, Color.Black);
        Raylib.DrawText("Selecciona un color y haz clic en el rectangulo para pintarlo.", 25, 63, 17, Color.DarkGray);
    }

    static void DibujarPaleta(string[] nombres, Color[] colores, int seleccionado, int colorA, int colorB)
    {
        Raylib.DrawText("Paleta de cinco colores", 25, 93, 18, Color.Black);
        for (int i = 0; i < colores.Length; i++)
        {
            int y = 120 + i * 70;
            Color fondo = i == seleccionado ? new Color(216, 232, 249, 255) : Color.White;
            Color borde = i == seleccionado ? new Color(36, 93, 145, 255) : new Color(190, 195, 202, 255);

            Raylib.DrawRectangle(25, y, 285, 58, fondo);
            Raylib.DrawRectangleLines(25, y, 285, 58, borde);
            Raylib.DrawRectangle(35, y + 12, 34, 34, colores[i]);
            Raylib.DrawRectangleLines(35, y + 12, 34, 34, Color.Gray);
            Raylib.DrawText(nombres[i], 80, y + 8, 17, Color.Black);
            Raylib.DrawText($"RGB({colores[i].R}, {colores[i].G}, {colores[i].B})  {ColorAHex(colores[i])}", 80, y + 33, 13, Color.DarkGray);
            if (i == colorA) Raylib.DrawText("A", 283, y + 7, 15, new Color(36, 93, 145, 255));
            if (i == colorB) Raylib.DrawText("B", 283, y + 34, 15, new Color(145, 55, 55, 255));
        }
        Raylib.DrawText("Izquierdo: seleccionar color / A", 25, 480, 14, Color.DarkGray);
        Raylib.DrawText("Derecho: elegir color B", 25, 501, 14, Color.DarkGray);
    }

    static void DibujarLienzo(Color color, int alpha)
    {
        Raylib.DrawText("Rectangulo para pintar", LienzoX, 86, 20, Color.Black);
        const int cuadro = 25;
        for (int fila = 0; fila < LienzoAlto / cuadro; fila++)
            for (int columna = 0; columna < LienzoAncho / cuadro; columna++)
            {
                Color fondo = (fila + columna) % 2 == 0 ? Color.White : new Color(210, 213, 218, 255);
                Raylib.DrawRectangle(LienzoX + columna * cuadro, LienzoY + fila * cuadro, cuadro, cuadro, fondo);
            }

        Color conAlpha = new Color(color.R, color.G, color.B, (byte)alpha);
        Raylib.DrawRectangle(LienzoX, LienzoY, LienzoAncho, LienzoAlto, conAlpha);
        Raylib.DrawRectangleLines(LienzoX, LienzoY, LienzoAncho, LienzoAlto, Color.DarkGray);
    }

    static void DibujarSlider(int alpha)
    {
        Raylib.DrawText("Transparencia (alpha)", LienzoX, 390, 18, Color.Black);
        Raylib.DrawText("0", SliderX - 25, SliderY - 8, 15, Color.DarkGray);
        Raylib.DrawText("255", SliderX + SliderAncho + 12, SliderY - 8, 15, Color.DarkGray);
        Raylib.DrawRectangle(SliderX, SliderY - 3, SliderAncho, 6, Color.LightGray);
        int posicion = SliderX + (int)(alpha / 255f * SliderAncho);
        Raylib.DrawCircle(posicion, SliderY, 11, new Color(36, 93, 145, 255));
        Raylib.DrawText($"Alpha: {alpha}", SliderX + 130, SliderY + 20, 17, Color.DarkGray);
    }

    static void DibujarDegradado(string nombreA, string nombreB, Color inicio, Color fin)
    {
        const int pasos = 8, inicioX = 350, inicioY = 535, ancho = 63, separacion = 5;
        Raylib.DrawText($"Degradado: {nombreA} -> {nombreB}", inicioX, inicioY - 30, 18, Color.Black);
        for (int i = 0; i < pasos; i++)
        {
            float t = i / (float)(pasos - 1);
            Color muestra = Interpolar(inicio, fin, t);
            int x = inicioX + i * (ancho + separacion);
            Raylib.DrawRectangle(x, inicioY, ancho, 48, muestra);
            Raylib.DrawRectangleLines(x, inicioY, ancho, 48, Color.Gray);
            Raylib.DrawText($"{t:0.00}", x + 15, inicioY + 54, 12, Color.DarkGray);
            Raylib.DrawText($"{muestra.R},{muestra.G},{muestra.B}", x + 2, inicioY + 72, 9, Color.DarkGray);
        }
    }

    static string ColorAHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    static Color Interpolar(Color inicio, Color fin, float t)
    {
        byte r = (byte)Math.Round(inicio.R + (fin.R - inicio.R) * t);
        byte g = (byte)Math.Round(inicio.G + (fin.G - inicio.G) * t);
        byte b = (byte)Math.Round(inicio.B + (fin.B - inicio.B) * t);
        return new Color(r, g, b, (byte)255);
    }

    static bool EstaDentro(int mouseX, int mouseY, int x, int y, int ancho, int alto) =>
        mouseX >= x && mouseX < x + ancho && mouseY >= y && mouseY < y + alto;
}
