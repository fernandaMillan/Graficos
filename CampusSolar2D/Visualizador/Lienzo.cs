using System.Numerics;
using Raylib_cs;
namespace CampusSolar;

internal static class Lienzo
{
    public const int Ancho=1120, Alto=720;
    public static readonly Color Fondo=new(239,246,247,255), Tinta=new(21,48,61,255);
    static readonly Color Azul=new(14,103,111,255), Menta=new(159,212,205,255),
        Suelo=new(226,218,196,255), Tierra=new(134,74,47,255),
        Ocre=new(228,158,47,255), Guion=new(161,184,190,255), Claro=new(235,245,242,255);
    const int Piso=494;
    const float Escala=32;
    static readonly Matrix3x2 Matriz=Matrix3x2.CreateScale(Escala,-Escala)*Matrix3x2.CreateTranslation(Ancho/2f,Piso);
    static Vector2 Px(Vector2 m) => Vector2.Transform(m,Matriz);
    static readonly string[] Tabs=["L   Recta","C   Circular","O   Senoidal","B   Parabola"];
    static readonly Ruta[] Rutas=[Ruta.Recta,Ruta.Circular,Ruta.Senoidal,Ruta.Parabola];
    public static Ruta? PestanaPulsada() {
        if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return null;
        int x=Raylib.GetMouseX(), y=Raylib.GetMouseY();
        for (int i=0;i<4;i++)
            if (x>=20+i*135 && x<20+i*135+126 && y>=79 && y<109) return Rutas[i];
        return null;
    }
    static void Texto(string s,int x,int y,int tam,Color c) => Raylib.DrawText(s,x,y,tam,c);
    static string Cabe(string s,int max,int tam) {
        if(Raylib.MeasureText(s,tam)<=max) return s;
        while(s.Length>0 && Raylib.MeasureText(s+"...",tam)>max) s=s[..^1];
        return s+"...";
    }
    static Color Mezcla(Color a,Color b,float t) => new(
        (int)MathF.Round(a.R+(b.R-a.R)*t),(int)MathF.Round(a.G+(b.G-a.G)*t),
        (int)MathF.Round(a.B+(b.B-a.B)*t),255);
    public static void DibujarEscena(Cuadro c,Trayecto g,Ruta ruta,float t,bool tangente)
    {
        var r=c.Datos.Resultado;
        float altura=(float)c.Entrada.AlturaParedM;
        Vector2 cima=Px(new(g.X0,altura));
        Vector2 extremo=Px(new(g.X0+g.Dx,0));
        Vector2 mesa=Px(new((float)c.Patio.MesaXM,0));
        // El rectángulo de sombra usa la longitud, el lado y el contraste de la respuesta real.
        Raylib.DrawRectangle(0,Piso,Ancho,55,Suelo);
        Raylib.DrawRectangle((int)MathF.Min(cima.X,extremo.X),Piso,
            Math.Max(1,(int)MathF.Round(MathF.Abs(g.Dx)*Escala)),13,
            Mezcla(Suelo,Tinta,(float)c.Datos.Visual.ContrasteSombra));
        for (int x=-16;x<=16;x+=2) {
            int px=(int)Px(new(x,0)).X;
            Raylib.DrawRectangle(px,Piso+15,1,6,Guion);
            Texto(x.ToString(),px-8,Piso+24,13,Tinta);
        }
        float rad=(float)r.ElevacionSolGrados*MathF.PI/180;
        Vector2 haciaSol=new(-MathF.Sign(g.Dx)*MathF.Cos(rad),MathF.Sin(rad));
        var sol=Px(new Vector2(g.X0,altura)+2*haciaSol);
        Raylib.DrawLineEx(sol,extremo,1.5f,Ocre);
        Raylib.DrawPoly(sol,8,15,22.5f,Ocre); // Sol octagonal, no una imagen.
        Raylib.DrawRectangle((int)cima.X-4,(int)cima.Y,8,Piso-(int)cima.Y,Tierra);
        Raylib.DrawRectangle((int)mesa.X-24,Piso-32,48,5,Tierra);
        Raylib.DrawRectangle((int)mesa.X-20,Piso-27,5,27,Tierra);
        Raylib.DrawRectangle((int)mesa.X+15,Piso-27,5,27,Tierra);
        Texto("Mesa",(int)mesa.X+30,Piso-37,15,Tinta);
        Texto("ESTE (-x)",24,461,14,Tinta); Texto("OESTE (+x)",995,461,14,Tinta);

        // Guía local de la trayectoria. La misma función genera guía y marcador.
        const int tramos=120;
        for(int i=0;i<tramos;i+=2)
            Raylib.DrawLineEx(Px(g.Punto(ruta,i/(float)tramos)),Px(g.Punto(ruta,(i+1)/(float)tramos)),2,Guion);
        var m=g.En(ruta,t);
        var p=Px(m.Posicion);
        if(tangente && m.Velocidad.LengthSquared()>.000001f) {
            // La pantalla invierte Y. Flecha normalizada: muestra dirección, no magnitud.
            var dir=Vector2.Normalize(new Vector2(m.Velocidad.X,-m.Velocidad.Y));
            var normal=new Vector2(-dir.Y,dir.X); var fin=p+dir*45;
            Raylib.DrawLineEx(p,fin,3,Azul);
            Raylib.DrawLineEx(fin,fin-dir*9+normal*5,3,Azul);
            Raylib.DrawLineEx(fin,fin-dir*9-normal*5,3,Azul);
        }
        Raylib.DrawPoly(p,6,10,0,Azul);
        Raylib.DrawPoly(p,4,3,45,Claro);
    }
    public static void Panel(Cuadro? c,Variables edit,Trayecto g,Ruta ruta,float t,
        string estado,string aviso,bool respaldo,bool pausa)
    {
        Raylib.DrawRectangle(0,0,Ancho,69,Tinta);
        Texto("CAMPUS SOLAR",20,12,24,Color.White);
        Texto("Laboratorio de trayectorias  /  Sistemas Vivos",20,43,15,Menta);
        Texto(Cabe(estado,190,19),764,13,19,Color.White);
        string fuente=c==null ? "Sin datos" : respaldo ? "Fuente: JSON local" : "Fuente: API";
        Texto(fuente,937,15,15,Menta);
        Texto(Cabe(aviso,700,14),395,43,14,Claro);
        for(int i=0;i<4;i++) {
            bool activo=Rutas[i]==ruta;
            Raylib.DrawRectangle(20+i*135,79,126,30,activo?Azul:Claro);
            Texto(Tabs[i],29+i*135,86,16,activo?Color.White:Tinta);
        }
        Texto("K: primera cresta    Espacio: pausa    T: tangente",587,86,16,Tinta);
        Raylib.DrawRectangle(0,549,Ancho,171,Color.RayWhite);
        Raylib.DrawRectangle(0,549,Ancho,2,Guion);
        if(c!=null) {
            var v=g.En(ruta,t);
            float red(float k)=>MathF.Abs(k)<.005f?0:k;
            string datos=$"Escena aplicada: {c.Entrada.HoraLocal}:00 | {c.Entrada.Nubosidad}% nubes | pared {c.Entrada.AlturaParedM:0.##} m | sombra {c.Datos.Resultado.LongitudSombraM:0.00} m al {c.Datos.Resultado.DireccionSombra} | contraste {c.Datos.Visual.ContrasteSombra:0.00} | mesa {(c.Datos.Resultado.MesaEnSombra?"en sombra":"al sol")}";
            Texto(Cabe(datos,1080,16),20,558,16,Tinta);
            string movimiento=$"t = {t:0.00} s  |  P = ({red(v.Posicion.X):0.00}, {red(v.Posicion.Y):0.00}) m  |  v = ({red(v.Velocidad.X):0.00}, {red(v.Velocidad.Y):0.00}) m/s  |  rapidez = {v.Velocidad.Length():0.00} m/s";
            Texto(movimiento,20,583,18,Azul);
            Texto($"{(pausa?"PAUSA":"EN MOVIMIENTO")}   |   A = {g.Amplitud:0.0} m    N = {g.Ondas}    T = {g.Periodo:0} s",20,609,15,Tinta);
        }
        Texto($"Para enviar: {edit.HoraLocal}:00  |  {edit.Nubosidad}% nubes  |  pared {edit.AlturaParedM:0.##} m     ENTER: aplicar     M: respaldo",20,635,17,Tinta);
        Texto("API: izquierda/derecha = hora   |   arriba/abajo = nubes   |   W/S = altura",20,662,15,Tinta);
        Texto("Local: A = amplitud/radio   |   N = ondas   |   F = periodo   |   R = inicio en pausa   |   Esc = salir",20,688,15,Tinta);
    }
}
