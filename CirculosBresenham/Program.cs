using System.Numerics;
using Raylib_cs;

namespace CirculosBresenham;

internal static class Program
{
    private const int AnchoVentana = 900;
    private const int AltoVentana = 650;
    private const int PasoCuadricula = 25;

    [STAThread]
    public static void Main()
    {
        bool esperandoRadio = false;
        bool circuloListo = false;
        bool mostrarCuadricula = true;

        Vector2 centro = Vector2.Zero;
        int radio = 0;

        Raylib.InitWindow(
            AnchoVentana,
            AltoVentana,
            "Practica 1 | Circulos con Bresenham"
        );

        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            Vector2 mouse = Raylib.GetMousePosition();

            // C limpia el estado y permite comenzar nuevamente.
            if (Raylib.IsKeyPressed(KeyboardKey.C))
            {
                esperandoRadio = false;
                circuloListo = false;
                centro = Vector2.Zero;
                radio = 0;
            }

            // G muestra u oculta la cuadrícula.
            if (Raylib.IsKeyPressed(KeyboardKey.G))
            {
                mostrarCuadricula = !mostrarCuadricula;
            }

            // Primer clic: centro. Segundo clic: punto del borde.
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (!esperandoRadio)
                {
                    centro = mouse;
                    radio = 0;
                    circuloListo = false;
                    esperandoRadio = true;
                }
                else
                {
                    radio = CalcularRadio(centro, mouse);
                    circuloListo = true;
                    esperandoRadio = false;
                }
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);

            if (mostrarCuadricula)
            {
                DibujarCuadricula(AnchoVentana, AltoVentana, PasoCuadricula);
            }

            // La vista previa sí puede usar la función de Raylib.
            // El círculo confirmado se dibuja con Bresenham y DrawPixel.
            if (esperandoRadio)
            {
                int radioTemporal = CalcularRadio(centro, mouse);

                Raylib.DrawCircleLines(
                    (int)MathF.Round(centro.X),
                    (int)MathF.Round(centro.Y),
                    radioTemporal,
                    Color.Gray
                );

                Raylib.DrawLine(
                    (int)MathF.Round(centro.X),
                    (int)MathF.Round(centro.Y),
                    (int)MathF.Round(mouse.X),
                    (int)MathF.Round(mouse.Y),
                    Color.Orange
                );

                Raylib.DrawText(
                    $"Vista previa | radio = {radioTemporal}",
                    20,
                    92,
                    20,
                    Color.Gray
                );
            }

            if (circuloListo)
            {
                DibujarCirculoBresenham(
                    (int)MathF.Round(centro.X),
                    (int)MathF.Round(centro.Y),
                    radio,
                    Color.DarkBlue
                );

                Raylib.DrawText(
                    $"Circulo confirmado | radio = {radio}",
                    20,
                    92,
                    20,
                    Color.DarkBlue
                );
            }

            if (esperandoRadio || circuloListo)
            {
                Raylib.DrawCircle(
                    (int)MathF.Round(centro.X),
                    (int)MathF.Round(centro.Y),
                    4,
                    Color.Orange
                );

                Raylib.DrawText(
                    $"Centro: ({MathF.Round(centro.X)}, {MathF.Round(centro.Y)})",
                    20,
                    120,
                    18,
                    Color.DarkGray
                );
            }

            DibujarInterfaz(esperandoRadio, circuloListo, mostrarCuadricula);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    /// <summary>
    /// Calcula la distancia euclidiana entre el centro y el punto del borde.
    /// El resultado se redondea porque Bresenham trabaja con un radio entero.
    /// </summary>
    private static int CalcularRadio(Vector2 centro, Vector2 puntoBorde)
    {
        Vector2 diferencia = puntoBorde - centro;

        float distancia = MathF.Sqrt(
            diferencia.X * diferencia.X +
            diferencia.Y * diferencia.Y
        );

        return (int)MathF.Round(distancia);
    }

    /// <summary>
    /// Rasteriza un círculo mediante la variante entera de Bresenham:
    /// e = 3 - 2r.
    /// </summary>
    private static void DibujarCirculoBresenham(
        int xc,
        int yc,
        int radio,
        Color color
    )
    {
        int x = 0;
        int y = radio;
        int e = 3 - 2 * radio;

        // x = y todavía pertenece al octante; por eso se usa <=.
        while (x <= y)
        {
            DibujarOchoPuntos(xc, yc, x, y, color);

            if (e < 0)
            {
                // El píxel horizontal sigue suficientemente cerca.
                // Conservamos y.
                e += 4 * x + 6;
            }
            else
            {
                // Debemos acercarnos al eje: disminuye y.
                e += 4 * (x - y) + 10;
                y--;
            }

            x++;
        }
    }

    /// <summary>
    /// Refleja el punto local (x,y) sobre los ejes y las diagonales.
    /// Así, una muestra del octante genera hasta ocho píxeles.
    /// </summary>
    private static void DibujarOchoPuntos(
        int xc,
        int yc,
        int x,
        int y,
        Color color
    )
    {
        Raylib.DrawPixel(xc + x, yc + y, color);
        Raylib.DrawPixel(xc - x, yc + y, color);
        Raylib.DrawPixel(xc + x, yc - y, color);
        Raylib.DrawPixel(xc - x, yc - y, color);

        Raylib.DrawPixel(xc + y, yc + x, color);
        Raylib.DrawPixel(xc - y, yc + x, color);
        Raylib.DrawPixel(xc + y, yc - x, color);
        Raylib.DrawPixel(xc - y, yc - x, color);
    }

    private static void DibujarCuadricula(int ancho, int alto, int paso)
    {
        for (int x = 0; x <= ancho; x += paso)
        {
            Raylib.DrawLine(x, 0, x, alto, Color.LightGray);
        }

        for (int y = 0; y <= alto; y += paso)
        {
            Raylib.DrawLine(0, y, ancho, y, Color.LightGray);
        }
    }

    private static void DibujarInterfaz(
        bool esperandoRadio,
        bool circuloListo,
        bool mostrarCuadricula
    )
    {
        string estado = esperandoRadio
            ? "Estado: selecciona el punto del borde"
            : circuloListo
                ? "Estado: circulo confirmado; haz clic para iniciar otro"
                : "Estado: selecciona el centro";

        Raylib.DrawRectangle(0, 0, AnchoVentana, 78, Color.RayWhite);

        Raylib.DrawText(
            "Circulos: punto medio y Bresenham",
            20,
            14,
            25,
            Color.DarkBlue
        );

        Raylib.DrawText(
            "Clic 1: centro | Clic 2: radio | C: limpiar | G: cuadricula | ESC: salir",
            20,
            45,
            17,
            Color.DarkGray
        );

        Raylib.DrawText(
            estado,
            20,
            AltoVentana - 32,
            18,
            Color.DarkBlue
        );

        Raylib.DrawText(
            mostrarCuadricula ? "Cuadricula: visible" : "Cuadricula: oculta",
            AnchoVentana - 195,
            AltoVentana - 32,
            17,
            Color.Gray
        );
    }
}
