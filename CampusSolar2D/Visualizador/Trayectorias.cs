using System.Numerics;
namespace CampusSolar;

public enum Ruta { Recta, Circular, Senoidal, Parabola }
public readonly record struct Movimiento(Vector2 Posicion, Vector2 Velocidad);

// Todas las posiciones están en metros (Y hacia arriba) y el tiempo en segundos.
// dx es la longitud CON signo que obtuvimos de la API; x0 es la posición de la pared.
// El marcador ilustra una trayectoria; no simula el movimiento astronómico del Sol.
public readonly record struct Trayecto(float X0, float Dx, float YBase, float Amplitud, int Ondas, float Periodo)
{
    // Una sola definición geométrica para dibujar la guía y colocar el polígono.
    // u recorre de 0 a 1. La onda usa k radianes por unidad de u.
    public Vector2 Punto(Ruta ruta, float u)
    {
        float x = X0 + Dx * u;
        float k = 2 * MathF.PI * Ondas;
        return ruta switch
        {
            Ruta.Recta    => new(x, YBase),
            Ruta.Senoidal => new(x, YBase + Amplitud * MathF.Sin(k * u)),
            Ruta.Parabola => new(x, YBase + 4 * Amplitud * u * (1-u)),
            Ruta.Circular => new(X0 + Dx/2 + Amplitud*MathF.Cos(2*MathF.PI*u),
                                 YBase     + Amplitud*MathF.Sin(2*MathF.PI*u)),
            _ => throw new ArgumentOutOfRangeException(nameof(ruta))
        };
    }

    public Movimiento En(Ruta ruta, float segundos)
    {
        float omega = 2 * MathF.PI / Periodo;
        float theta = omega * segundos;
        if (ruta == Ruta.Circular)
        {
            // Radio A; una vuelta en T segundos. La velocidad siempre es tangente.
            var v = Amplitud * omega * new Vector2(-MathF.Sin(theta), MathF.Cos(theta));
            return new(Punto(ruta, segundos / Periodo), v);
        }

        // Ida y vuelta suave: u=0 al salir, 1 a mitad del periodo y 0 al regresar.
        float u  = (1 - MathF.Cos(theta)) / 2;
        float du = omega * MathF.Sin(theta) / 2; // du/dt
        float k = 2 * MathF.PI * Ondas;
        float dydu = ruta switch
        {
            Ruta.Recta    => 0,
            Ruta.Senoidal => Amplitud * k * MathF.Cos(k*u),
            Ruta.Parabola => 4 * Amplitud * (1 - 2*u),
            _ => throw new ArgumentOutOfRangeException(nameof(ruta))
        };
        // Regla de la cadena: v = dP/dt = (dP/du)(du/dt).
        return new(Punto(ruta, u), new Vector2(Dx * du, dydu * du));
    }

    // Primera cresta en el recorrido de ida: k*u=pi/2, por lo que u=1/(4*N).
    public float TiempoPrimeraCresta() =>
        Periodo * MathF.Acos(1 - 1f/(2*Ondas)) / (2*MathF.PI);
}
