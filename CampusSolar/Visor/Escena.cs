using System.Numerics;
using CampusSolar;
using Raylib_cs;

internal static class Escena
{
    static readonly Color Suelo = new(220, 207, 177, 255);
    static readonly Color Oscuro = new(78, 90, 103, 255);
    static readonly Color Pared = new(177, 104, 73, 255);
    static readonly Color Madera = new(115, 83, 52, 255);
    static readonly Color Rayo = new(226, 151, 30, 255);
    static Color Mezclar(Color a, Color b, float t) => new(
        (int)MathF.Round(a.R + (b.R-a.R)*t), (int)MathF.Round(a.G + (b.G-a.G)*t),
        (int)MathF.Round(a.B + (b.B-a.B)*t), 255);
    static float Signo(Cuadro c) => c.Datos.Resultado.DireccionSombra == "oeste" ? 1 : -1;
    static (Vector3 cima, Vector3 punta) Coordenadas(Cuadro c)
    {
        float x = (float)c.Patio.ParedXM;
        return (new(x,(float)c.Entrada.AlturaParedM,0),
                new(x + Signo(c)*(float)c.Datos.Resultado.LongitudSombraM,0,0));
    }
    public static void Vista2D(Cuadro c, float t)
    {
        // Matriz mundo -> pantalla: 32 px/m en ambos ejes; invierte Y solo para dibujar.
        var m = Matrix3x2.CreateScale(32,-32) * Matrix3x2.CreateTranslation(540,468);
        Vector2 Pant(Vector3 v) => Vector2.Transform(new Vector2(v.X,v.Y),m);
        var (cima,punta)=Coordenadas(c);
        var a=Pant(cima); var b=Pant(punta); int x0=(int)a.X;
        var mesa=Pant(new Vector3((float)c.Patio.MesaXM,0,0));
        Raylib.DrawRectangle(0,468,1080,85,Suelo);
        var sombra=Mezclar(Suelo,Oscuro,(float)c.Datos.Visual.ContrasteSombra);
        Raylib.DrawRectangle((int)MathF.Min(a.X,b.X),468,(int)MathF.Abs(a.X-b.X),13,sombra);
        Raylib.DrawLineV(a,b,Rayo);
        var viajero=Vector2.Lerp(a,b,t); // Indicador didáctico, no velocidad física de la luz.
        Raylib.DrawCircleV(viajero,4,Rayo);
        var sol=a+Vector2.Normalize(a-b)*72;
        Raylib.DrawCircleV(sol,15,Color.Gold);
        Raylib.DrawLineV(sol,a,Rayo);
        Raylib.DrawRectangle(x0-4,(int)a.Y,8,468-(int)a.Y,Pared);
        Raylib.DrawRectangle((int)mesa.X-23,439,46,6,Madera);
        Raylib.DrawRectangle((int)mesa.X-18,445,4,23,Madera);
        Raylib.DrawRectangle((int)mesa.X+14,445,4,23,Madera);
        Raylib.DrawCircle((int)mesa.X,468,3,Color.DarkBlue);
        Raylib.DrawText("mesa: +4 m",(int)mesa.X-28,407,16,Color.DarkBlue);
        Raylib.DrawText("Este (-x)",40,515,18,Color.DarkGray);
        Raylib.DrawText("Oeste (+x)",926,515,18,Color.DarkGray);
        Raylib.DrawText("0 m",x0-12,492,15,Color.DarkGray);
    }

    public static void Vista3D(Cuadro c,float t,bool ortografica,bool iluminar)
    {
        var camara = new Camera3D {
            Position = new Vector3(10,11,23), Target = new Vector3(0,2,0), Up = Vector3.UnitY,
            FovY = ortografica ? 16f : 40f,
            Projection = ortografica ? CameraProjection.Orthographic : CameraProjection.Perspective
        };
        float contraste=(float)c.Datos.Visual.ContrasteSombra;
        var (cima,punta)=Coordenadas(c);
        float angulo=(float)c.Datos.Resultado.ElevacionSolGrados*MathF.PI/180;
        // L va de la pared hacia el Sol: su componente horizontal es opuesta a la sombra.
        Vector3 L=new(-Signo(c)*MathF.Cos(angulo),MathF.Sin(angulo),0);
        float paredX=(float)c.Patio.ParedXM, mesaX=(float)c.Patio.MesaXM;

        Raylib.BeginMode3D(camara);
        Raylib.DrawPlane(Vector3.Zero,new Vector2(30,10),Suelo);
        // La geometría proyectada proviene de la API (no es un shadow map).
        Raylib.DrawPlane(new Vector3((paredX+punta.X)/2,.012f,0),
            new Vector2(MathF.Abs(punta.X-paredX),3),Mezclar(Suelo,Oscuro,contraste));
        Caja(new(paredX,cima.Y/2,0),new(.12f,cima.Y,3),Pared,L,contraste,iluminar);
        float luzMesa=c.Datos.Resultado.MesaEnSombra ? 0 : contraste;
        Caja(new(mesaX,1,0),new(1.5f,.13f,1),Madera,L,luzMesa,iluminar);
        foreach(float dx in new[]{-.58f,.58f}) foreach(float dz in new[]{-.38f,.38f})
            Caja(new(mesaX+dx,.48f,dz),new(.1f,.96f,.1f),Madera,L,luzMesa,iluminar);
        Raylib.DrawLine3D(cima,punta,Rayo);
        Raylib.DrawSphere(Vector3.Lerp(cima,punta,t),.09f,Rayo);
        Raylib.DrawSphere(cima+L*2,.24f,Color.Gold);
        Raylib.EndMode3D();
        Raylib.DrawText(iluminar?"Luz ambiental + difusa por cara":"Colores planos (sin sombreado por cara)",20,115,18,Color.DarkGray);
    }
    // La caja nace en coordenadas locales. La matriz escala y luego coloca sus ocho vértices.
    static void Caja(Vector3 centro,Vector3 tam,Color material,Vector3 L,float directa,bool luz)
    {
        var modelo=Matrix4x4.CreateScale(tam)*Matrix4x4.CreateTranslation(centro);
        Vector3 X=Vector3.UnitX, Y=Vector3.UnitY, Z=Vector3.UnitZ;
        void Cara(Vector3 n,Vector3 u,Vector3 v)
        {
            // Caras alineadas a los ejes: escalar y trasladar no cambia sus normales unitarias.
            float intensidad=luz ? .42f+.58f*directa*MathF.Max(0,Vector3.Dot(n,L)) : 1f;
            Color tinta=Mezclar(Color.Black,material,intensidad);
            Vector3 V(float a,float b)=>Vector3.Transform(.5f*(n+a*u+b*v),modelo);
            var p=V(-1,-1); var q=V(1,-1); var r=V(1,1); var s=V(-1,1);
            Raylib.DrawTriangle3D(p,q,r,tinta); Raylib.DrawTriangle3D(p,r,s,tinta);
        }
        Cara(X,Y,Z); Cara(-X,Z,Y); Cara(Y,Z,X); Cara(-Y,X,Z); Cara(Z,X,Y); Cara(-Z,Y,X);
    }
}
