using System.Numerics;
using CampusSolar;

// Sin paquetes gráficos. Ejecuta desde CampusSolar2D: dotnet run --project Comprobar
int total=0;
void Cerca(float actual,float esperado,float margen,string caso)
{
    if (!float.IsFinite(actual) || MathF.Abs(actual-esperado)>margen)
        throw new Exception($"{caso}: esperado {esperado}, obtenido {actual}");
    total++;
}
try {
    var c=CampusApi.LeerRespaldo();
    if(c.Datos.Resultado.MesaEnSombra!=true || c.Datos.Resultado.DireccionSombra!="oeste")
        throw new InvalidDataException("El mock no es el caso conocido de las 10 h.");
    Cerca((float)c.Datos.Resultado.LongitudSombraM,5,.00001f,"longitud del mock");
    _=CampusApi.LeerEntradaInicial();
    foreach(float dx in new[]{5f,-5f}) foreach(int ondas in new[]{1,2,3})
    {
        var g=new Trayecto(0,dx,6.8f,1,ondas,8);
        var cumbre=g.En(Ruta.Senoidal,g.TiempoPrimeraCresta());
        Cerca(cumbre.Posicion.Y,g.YBase+g.Amplitud,.00002f,"altura de primera cresta");
        Cerca(cumbre.Velocidad.Y,0,.00002f,"vy en cresta");
        if(cumbre.Velocidad.X*dx<=0) throw new Exception("En la cresta debe avanzar horizontalmente.");
        foreach(Ruta ruta in Enum.GetValues<Ruta>()) {
            var inicio=g.En(ruta,0);
            var final=g.En(ruta,g.Periodo);
            Cerca(Vector2.Distance(inicio.Posicion,final.Posicion),0,.00002f,"cierre del ciclo");
            foreach(float t in new[]{.53f,1.71f,3.1f,5.2f,7.3f}) {
                var m=g.En(ruta,t);
                const float dt=.002f;
                Vector2 estimada=(g.En(ruta,t+dt).Posicion-g.En(ruta,t-dt).Posicion)/(2*dt);
                Cerca(Vector2.Distance(m.Velocidad,estimada),0,.004f,$"derivada {ruta}");
                if(ruta==Ruta.Circular) {
                    var radio=m.Posicion-new Vector2(g.X0+g.Dx/2,g.YBase);
                    Cerca(radio.Length(),g.Amplitud,.00002f,"radio");
                    Cerca(Vector2.Dot(radio,m.Velocidad),0,.00002f,"radio perpendicular a v");
                }
            }
        }
    }
    if(args.Contains("--api")) {
        using var api=new CampusApi();
        var a=await api.Consultar(CampusApi.CasoBase);
        var b=await api.Consultar(new Variables(14,25,5));
        if(a.Datos!=c.Datos || b.Datos.Resultado.DireccionSombra!="este" || b.Datos.Resultado.MesaEnSombra)
            throw new Exception("La API no devolvió los dos casos de la hoja.");
        Console.WriteLine("OK: el POST de las 10 coincide con el archivo JSON; a las 14 la mesa queda al sol.");
    }
    Console.WriteLine($"OK: {total} comprobaciones numéricas de datos, posición y derivada.");
}
catch(Exception error) { Console.Error.WriteLine("ERROR: "+error.Message); Environment.ExitCode=1; }
