using Raylib_cs;

namespace S07PixelLab;

internal static class Program
{
    public static void Main()
    {
        const int anchoVentana = 800;
        const int altoVentana = 600;

        Raylib.InitWindow(anchoVentana, altoVentana, "Pixel Lab 16x16");
        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);

            Raylib.DrawText("Pixel Lab 16x16", 20, 20, 24, Color.DarkBlue);
            Raylib.DrawText("ESC: cerrar", 20, 52, 18, Color.Gray);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}