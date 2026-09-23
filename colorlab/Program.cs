using Raylib_cs;
 
class Program
{
    const int Filas = 16;
    const int Columnas = 16;
    const int Celda = 28;
    const int InicioX = 330;
    const int InicioY = 70;
 
    static void Main()
    {
        Raylib.InitWindow(900, 650, "Practica 3 - Color Lab");
        Raylib.SetTargetFPS(60);
 
        string[] nombres = { "Blanco", "Rojo", "Azul", "Verde", "Amarillo", "Morado" };
        Color[] colores =
        {
            new Color(255, 255, 255, 255),
            new Color(220, 50, 50, 255),
            new Color(30, 80, 180, 255),
            new Color(40, 170, 90, 255),
            new Color(245, 200, 40, 255),
            new Color(140, 80, 180, 255)
        };
 
        int[,] dibujo = new int[Filas, Columnas];
        int colorSeleccionado = 1;
        int colorInicial = 1;
        int colorFinal = 2;
 
        while (!Raylib.WindowShouldClose())
        {
            int mouseX = Raylib.GetMouseX();
            int mouseY = Raylib.GetMouseY();
 
            for (int i = 0; i < colores.Length; i++)
            {
                int botonY = 85 + i * 63;
                if (Dentro(mouseX, mouseY, 25, botonY, 250, 52))
                {
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        colorSeleccionado = i;
                        colorInicial = i;
                    }
                    if (Raylib.IsMouseButtonPressed(MouseButton.Right))
                        colorFinal = i;
                }
            }
 
            bool dentroCuadricula = Dentro(mouseX, mouseY, InicioX, InicioY,
                                            Columnas * Celda, Filas * Celda);
            if (dentroCuadricula)
            {
                int columna = (mouseX - InicioX) / Celda;
                int fila = (mouseY - InicioY) / Celda;
 
                if (Raylib.IsMouseButtonDown(MouseButton.Left))
                    dibujo[fila, columna] = colorSeleccionado;
                if (Raylib.IsMouseButtonDown(MouseButton.Right))
                    dibujo[fila, columna] = 0;
            }
 
            if (Raylib.IsKeyPressed(KeyboardKey.C))
                Limpiar(dibujo);
 
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(242, 244, 247, 255));
 
            Raylib.DrawText("COLOR LAB", 25, 25, 28, Color.Black);
            Raylib.DrawText("Izquierdo: seleccionar/inicio", 25, 52, 14, Color.DarkGray);
            Raylib.DrawText("Derecho: final del degradado", 25, 67, 14, Color.DarkGray);
 
            for (int i = 0; i < colores.Length; i++)
            {
                int y = 85 + i * 63;
                Color fondo = i == colorSeleccionado
                    ? new Color(210, 225, 242, 255)
                    : Color.White;
 
                Raylib.DrawRectangle(25, y, 250, 52, fondo);
                Raylib.DrawRectangleLines(25, y, 250, 52,
                    i == colorSeleccionado ? Color.Blue : Color.LightGray);
                Raylib.DrawRectangle(34, y + 9, 34, 34, colores[i]);
                Raylib.DrawRectangleLines(34, y + 9, 34, 34, Color.Gray);
                Raylib.DrawText(nombres[i], 78, y + 7, 17, Color.Black);
                string datos = $"RGB({colores[i].R}, {colores[i].G}, {colores[i].B})  {ColorAHex(colores[i])}";
                Raylib.DrawText(datos, 78, y + 29, 12, Color.DarkGray);
            }
 
            Raylib.DrawText("Lienzo 16 x 16", InicioX, 40, 20, Color.Black);
            for (int fila = 0; fila < Filas; fila++)
            {
                for (int columna = 0; columna < Columnas; columna++)
                {
                    int x = InicioX + columna * Celda;
                    int y = InicioY + fila * Celda;
                    Raylib.DrawRectangle(x, y, Celda, Celda, colores[dibujo[fila, columna]]);
                    Raylib.DrawRectangleLines(x, y, Celda, Celda, new Color(190, 195, 202, 255));
                }
            }
 
            Raylib.DrawText("C: limpiar", InicioX, InicioY + Filas * Celda + 10, 16, Color.DarkGray);
 
            int gradX = 25;
            int gradY = 495;
            const int pasos = 8;
            Raylib.DrawText($"Degradado: {nombres[colorInicial]} a {nombres[colorFinal]}", gradX, gradY, 17, Color.Black);
            for (int i = 0; i < pasos; i++)
            {
                float t = i / (float)(pasos - 1);
                Color muestra = Interpolar(colores[colorInicial], colores[colorFinal], t);
                int x = gradX + i * 58;
                Raylib.DrawRectangle(x, gradY + 25, 52, 36, muestra);
                Raylib.DrawText($"{t:0.00}", x + 8, gradY + 65, 12, Color.DarkGray);
            }
 
            Color transparente = new Color((byte)colores[colorSeleccionado].R,
                                            (byte)colores[colorSeleccionado].G,
                                            (byte)colores[colorSeleccionado].B,
                                            (byte)120);
            int alphaX = 520;
            int alphaY = 525;
            Raylib.DrawText("Alpha 120", alphaX, alphaY - 25, 17, Color.Black);
            Raylib.DrawRectangle(alphaX, alphaY, 150, 70, Color.White);
            Raylib.DrawRectangle(alphaX + 150, alphaY, 150, 70, new Color(35, 38, 45, 255));
            Raylib.DrawRectangle(alphaX + 35, alphaY + 15, 80, 40, transparente);
            Raylib.DrawRectangle(alphaX + 185, alphaY + 15, 80, 40, transparente);
            Raylib.DrawText("claro", alphaX + 53, alphaY + 74, 13, Color.DarkGray);
            Raylib.DrawText("oscuro", alphaX + 198, alphaY + 74, 13, Color.DarkGray);
 
            Raylib.EndDrawing();
        }
 
        Raylib.CloseWindow();
    }
 
    static bool Dentro(int mx, int my, int x, int y, int ancho, int alto)
    {
        return mx >= x && mx < x + ancho && my >= y && my < y + alto;
    }
 
    static string ColorAHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
 
    static Color Interpolar(Color inicio, Color fin, float t)
    {
        byte r = (byte)(inicio.R + (fin.R - inicio.R) * t);
        byte g = (byte)(inicio.G + (fin.G - inicio.G) * t);
        byte b = (byte)(inicio.B + (fin.B - inicio.B) * t);
        return new Color(r, g, b, (byte)255);
    }
 
    static void Limpiar(int[,] dibujo)
    {
        for (int fila = 0; fila < Filas; fila++)
            for (int columna = 0; columna < Columnas; columna++)
                dibujo[fila, columna] = 0;
    }
}
