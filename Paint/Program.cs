using System.Numerics;
using Raylib_cs;

namespace PaintLapizMemoria;

enum Herramienta
{
    Lapiz,
    Borrador
}

readonly record struct Trazo(
    Vector2 Desde,
    Vector2 Hasta,
    Color Color,
    float Grosor
);

internal static class Program
{
    private const int AnchoVentana = 1200;
    private const int AltoVentana = 760;

    private static readonly Color FondoAplicacion = new(238, 243, 247, 255);
    private static readonly Color FondoPanel = new(21, 58, 91, 255);
    private static readonly Color TextoOscuro = new(23, 35, 45, 255);
    private static readonly Color TextoClaro = new(235, 244, 250, 255);
    private static readonly Color AzulInterfaz = new(46, 111, 149, 255);
    private static readonly Color VerdeInterfaz = new(42, 157, 143, 255);
    private static readonly Color RojoInterfaz = new(217, 93, 93, 255);
    private static readonly Color Borde = new(184, 200, 212, 255);
    private static readonly Color ColorLienzo = Color.RayWhite;

    public static void Main()
    {
        Rectangle lienzo = new(260, 70, 910, 650);

        Rectangle botonLapiz = new(20, 115, 95, 44);
        Rectangle botonBorrador = new(125, 115, 95, 44);
        Rectangle botonMenos = new(20, 474, 58, 42);
        Rectangle botonMas = new(87, 474, 58, 42);
        Rectangle botonLimpiar = new(20, 548, 200, 46);

        Color[] colores =
        {
            new(30, 35, 40, 255),
            new(46, 111, 149, 255),
            new(217, 93, 93, 255),
            new(42, 157, 143, 255),
            new(242, 193, 78, 255),
            new(132, 94, 194, 255)
        };

        string[] nombresColores =
        {
            "Negro",
            "Azul",
            "Rojo",
            "Verde",
            "Amarillo",
            "Morado"
        };

        Rectangle[] botonesColor = CrearBotonesColor();

        List<Trazo> trazos = new();
        Herramienta herramientaActual = Herramienta.Lapiz;
        int colorSeleccionado = 0;
        float grosorActual = 4;
        Vector2 mouseAnterior = Vector2.Zero;
        bool dibujando = false;

        Raylib.InitWindow(
            AnchoVentana,
            AltoVentana,
            "Paint | Lapiz y memoria de trazos"
        );

        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            Vector2 mouse = Raylib.GetMousePosition();

            // ---------------------------------------------------------
            // 1. LEER CONTROLES DE TECLADO
            // ---------------------------------------------------------
            if (Raylib.IsKeyPressed(KeyboardKey.L))
            {
                herramientaActual = Herramienta.Lapiz;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.B))
            {
                herramientaActual = Herramienta.Borrador;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Up))
            {
                grosorActual = MathF.Min(30, grosorActual + 1);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Down))
            {
                grosorActual = MathF.Max(1, grosorActual - 1);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.C))
            {
                trazos.Clear();
            }

            // ---------------------------------------------------------
            // 2. LEER BOTONES DE LA INTERFAZ
            // ---------------------------------------------------------
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (Raylib.CheckCollisionPointRec(mouse, botonLapiz))
                {
                    herramientaActual = Herramienta.Lapiz;
                }
                else if (Raylib.CheckCollisionPointRec(mouse, botonBorrador))
                {
                    herramientaActual = Herramienta.Borrador;
                }
                else if (Raylib.CheckCollisionPointRec(mouse, botonMenos))
                {
                    grosorActual = MathF.Max(1, grosorActual - 1);
                }
                else if (Raylib.CheckCollisionPointRec(mouse, botonMas))
                {
                    grosorActual = MathF.Min(30, grosorActual + 1);
                }
                else if (Raylib.CheckCollisionPointRec(mouse, botonLimpiar))
                {
                    trazos.Clear();
                }
                else
                {
                    for (int i = 0; i < botonesColor.Length; i++)
                    {
                        if (Raylib.CheckCollisionPointRec(mouse, botonesColor[i]))
                        {
                            colorSeleccionado = i;
                            herramientaActual = Herramienta.Lapiz;
                        }
                    }
                }

                // El gesto de dibujo solo comienza dentro del lienzo.
                if (Raylib.CheckCollisionPointRec(mouse, lienzo))
                {
                    dibujando = true;
                    mouseAnterior = mouse;
                }
            }

            // ---------------------------------------------------------
            // 3. CAPTURAR EL TRAZO CONTINUO
            // ---------------------------------------------------------
            if (dibujando && Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                Vector2 mouseActual = Raylib.GetMousePosition();

                if (Raylib.CheckCollisionPointRec(mouseActual, lienzo))
                {
                    float distancia = Vector2.Distance(
                        mouseAnterior,
                        mouseActual
                    );

                    if (distancia > 0)
                    {
                        Color colorTrazo =
                            herramientaActual == Herramienta.Borrador
                                ? ColorLienzo
                                : colores[colorSeleccionado];

                        trazos.Add(
                            new Trazo(
                                mouseAnterior,
                                mouseActual,
                                colorTrazo,
                                grosorActual
                            )
                        );

                        // El punto actual será el inicio del siguiente segmento.
                        mouseAnterior = mouseActual;
                    }
                }
                else
                {
                    // Si el mouse sale del lienzo, se termina el gesto.
                    dibujando = false;
                }
            }

            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                dibujando = false;
            }

            // ---------------------------------------------------------
            // 4. DIBUJAR EL FRAME
            // ---------------------------------------------------------
            Raylib.BeginDrawing();
            Raylib.ClearBackground(FondoAplicacion);

            DibujarEncabezado(lienzo, trazos.Count);
            DibujarPanel(
                botonLapiz,
                botonBorrador,
                botonMenos,
                botonMas,
                botonLimpiar,
                botonesColor,
                colores,
                nombresColores,
                herramientaActual,
                colorSeleccionado,
                grosorActual
            );

            Raylib.DrawRectangleRec(lienzo, ColorLienzo);

            // La lista se vuelve a dibujar completa en cada frame.
            Raylib.BeginScissorMode(
                (int)lienzo.X,
                (int)lienzo.Y,
                (int)lienzo.Width,
                (int)lienzo.Height
            );

            foreach (Trazo trazo in trazos)
            {
                Raylib.DrawLineEx(
                    trazo.Desde,
                    trazo.Hasta,
                    trazo.Grosor,
                    trazo.Color
                );
            }

            Raylib.EndScissorMode();
            Raylib.DrawRectangleLinesEx(lienzo, 2, Borde);

            // Vista previa del tamaño del lápiz o borrador.
            if (Raylib.CheckCollisionPointRec(mouse, lienzo))
            {
                Color colorVista =
                    herramientaActual == Herramienta.Borrador
                        ? Color.Gray
                        : colores[colorSeleccionado];

                Raylib.DrawCircleLines(
                    (int)mouse.X,
                    (int)mouse.Y,
                    MathF.Max(2, grosorActual / 2),
                    colorVista
                );
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    private static Rectangle[] CrearBotonesColor()
    {
        Rectangle[] botones = new Rectangle[6];

        for (int i = 0; i < botones.Length; i++)
        {
            int columna = i % 2;
            int fila = i / 2;

            botones[i] = new Rectangle(
                20 + columna * 105,
                268 + fila * 62,
                95,
                48
            );
        }

        return botones;
    }

    private static void DibujarEncabezado(
        Rectangle lienzo,
        int cantidadSegmentos
    )
    {
        Raylib.DrawText(
            "Paint con lapiz y memoria de trazos",
            (int)lienzo.X,
            20,
            28,
            TextoOscuro
        );

        string estado = $"Segmentos guardados: {cantidadSegmentos}";
        int anchoEstado = Raylib.MeasureText(estado, 18);

        Raylib.DrawText(
            estado,
            AnchoVentana - anchoEstado - 30,
            28,
            18,
            Color.DarkGray
        );
    }

    private static void DibujarPanel(
        Rectangle botonLapiz,
        Rectangle botonBorrador,
        Rectangle botonMenos,
        Rectangle botonMas,
        Rectangle botonLimpiar,
        Rectangle[] botonesColor,
        Color[] colores,
        string[] nombresColores,
        Herramienta herramientaActual,
        int colorSeleccionado,
        float grosorActual
    )
    {
        Raylib.DrawRectangle(0, 0, 240, AltoVentana, FondoPanel);

        Raylib.DrawText("HERRAMIENTA", 20, 28, 16, TextoClaro);
        Raylib.DrawText("Paint", 20, 52, 34, Color.White);

        DibujarBoton(
            botonLapiz,
            "Lapiz",
            herramientaActual == Herramienta.Lapiz,
            AzulInterfaz
        );

        DibujarBoton(
            botonBorrador,
            "Borrador",
            herramientaActual == Herramienta.Borrador,
            VerdeInterfaz
        );

        string herramienta =
            herramientaActual == Herramienta.Lapiz
                ? "L: lapiz activo"
                : "B: borrador activo";

        Raylib.DrawText(herramienta, 20, 178, 18, TextoClaro);

        Raylib.DrawText("COLORES", 20, 230, 16, TextoClaro);

        for (int i = 0; i < botonesColor.Length; i++)
        {
            Rectangle boton = botonesColor[i];
            Raylib.DrawRectangleRec(boton, colores[i]);

            if (i == colorSeleccionado)
            {
                Raylib.DrawRectangleLinesEx(boton, 4, Color.White);
            }
            else
            {
                Raylib.DrawRectangleLinesEx(boton, 1, Borde);
            }

            Color texto = i == 4 ? TextoOscuro : Color.White;
            int ancho = Raylib.MeasureText(nombresColores[i], 15);

            Raylib.DrawText(
                nombresColores[i],
                (int)(boton.X + boton.Width / 2 - ancho / 2),
                (int)(boton.Y + 16),
                15,
                texto
            );
        }

        Raylib.DrawText("GROSOR", 20, 444, 16, TextoClaro);
        DibujarBoton(botonMenos, "-", false, AzulInterfaz);
        DibujarBoton(botonMas, "+", false, AzulInterfaz);

        string grosor = $"{grosorActual:0} px";
        Raylib.DrawText(grosor, 158, 484, 20, Color.White);

        DibujarBoton(botonLimpiar, "Limpiar (C)", false, RojoInterfaz);

        Raylib.DrawText("ATAJOS", 20, 630, 16, TextoClaro);
        Raylib.DrawText("L  Lapiz", 20, 657, 17, TextoClaro);
        Raylib.DrawText("B  Borrador", 20, 681, 17, TextoClaro);
        Raylib.DrawText("Arriba/Abajo  Grosor", 20, 705, 17, TextoClaro);
        Raylib.DrawText("ESC  Salir", 20, 729, 17, TextoClaro);
    }

    private static void DibujarBoton(
        Rectangle area,
        string texto,
        bool activo,
        Color colorActivo
    )
    {
        Color relleno = activo
            ? colorActivo
            : new Color(43, 78, 105, 255);

        Raylib.DrawRectangleRec(area, relleno);
        Raylib.DrawRectangleLinesEx(
            area,
            activo ? 3 : 1,
            activo ? Color.White : Borde
        );

        int tamano = texto.Length <= 2 ? 24 : 17;
        int anchoTexto = Raylib.MeasureText(texto, tamano);

        Raylib.DrawText(
            texto,
            (int)(area.X + area.Width / 2 - anchoTexto / 2),
            (int)(area.Y + area.Height / 2 - tamano / 2),
            tamano,
            Color.White
        );
    }
}
