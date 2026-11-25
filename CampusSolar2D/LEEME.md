# Campus Solar 2D · Polígonos y trayectorias
**Pregunta de la demostración:** ¿en qué dirección se mueve el hexágono al llegar a la primera cresta? El marcador recorre una curva construida con los datos de la sombra. No representa el movimiento físico del Sol.
La pared y la mesa son rectángulos, el Sol es un octágono y el móvil es un hexágono. No se usan imágenes ni modelos 3D.

## Ejecutar en VS Code (Windows / PowerShell)
Requisitos: SDK de .NET 8 (o un SDK que compile net8.0 y runtime de .NET 8), Node.js 22 o 24. La primera restauración de Raylib-cs 8.0.0 necesita internet o el paquete ya almacenado. Extrae **todo** el ZIP y abre la carpeta **CampusSolar2D** (la que contiene Servidor y Visualizador). No ejecutes desde el ZIP.

**Terminal 1 — API incluida, sin instalar paquetes:**
```powershell
cd Servidor
npm start
```
Deja esta terminal abierta. En el navegador abre http://127.0.0.1:3000/api/health; debe aparecer `"estado": "ok"`.

**Terminal 2 — nueva terminal en CampusSolar2D:**
```powershell
dotnet run --project Visualizador
```
La aplicación lee **Datos/peticion.json**, llama a GET /api/health y GET /api/escenario, y envía la petición a POST /api/simular. El estado debe pasar a **API / en vivo**. Empieza en onda senoidal. No hay que crear un proyecto, instalar Raylib a mano ni copiar un JSON en el código.

**Si no tienes servidor o quieres demostrar el respaldo:** `dotnet run --project Visualizador -- --mock`. Verás los datos reales del archivo **Datos/mock-response.json** (un ejemplo estático, no datos actuales de una API). No hace falta Node para este modo; sí hace falta tener restaurado Raylib. Si el primer intento de conexión falla, el visor intenta el respaldo y lo identifica; si luego falla otra consulta, conserva la escena anterior e indica el error. M selecciona el respaldo explícitamente. Enter vuelve a consultar la API.

**Si ya publicaste esta API en Vercel:** antes de `dotnet run`, en la misma terminal de C#:
```powershell
$env:CAMPUS_API_URL = Read-Host "Pega la URL base HTTPS sin /api"
```
No cambia el código. Para volver al equipo local, usa `$env:CAMPUS_API_URL = "http://127.0.0.1:3000"`. El paquete no presupone una URL pública. En otro equipo, “localhost” es ese otro equipo.

## Qué muestran los datos
Caso de la hoja: hora 10, nubes 25 %, pared 5 m; pared en x=0, mesa en +4 m. El modelo es didáctico, no meteorológico. Oeste es +x. En C# se guarda por separado la selección **para enviar** y la respuesta **aplicada**: mover un control no inventa un resultado.
- `longitudSombraM` y `direccionSombra` controlan el extremo de la franja sobre el piso y el desplazamiento horizontal con signo de la trayectoria. Una matriz transforma metros a pantalla (32 px/m y eje Y invertido).
- `contrasteSombra` mezcla el color del suelo con el de la sombra; no cambia los metros ni la curva.
- `mesaEnSombra` indica si el centro de la mesa queda dentro de la sombra geométrica. Con nubes al 100 % el contraste es cero: se puede cumplir la condición geométrica sin ver una franja oscura.
- `elevacionSolGrados` orienta el rayo y el octágono. Las posiciones de pared y mesa se leen de GET /api/escenario. La altura de la pared procede de la petición aceptada.
- La animación usa tiempo local; no se consulta la red por cuadro. Cambiar hora, nubes o altura, **y pulsar Enter**, solicita otra simulación. Cambiar la fórmula solo mueve el mismo hexágono.

## Controles
| Control | Acción |
|---|---|
| L / C / O / B, o clic en la pestaña | Recta / circular / senoidal / parábola |
| Espacio / R | Pausa o reanuda / regresa a t=0 y deja en pausa |
| T / K | Muestra u oculta la flecha tangente / pausa exactamente en la primera cresta del seno |
| A | Alterna 0.5, 1, 1.5 y 2 m: amplitud del seno, radio del círculo o altura de la parábola. No modifica la recta. |
| N | Alterna 1, 2 o 3 ondas completas por recorrido de ida. Solo afecta el seno. |
| F | Alterna periodos de 4, 8 o 12 s para el ciclo completo. No cambia la forma geométrica. |
| Izq./der.; arriba/abajo; W/S | Elegir hora; subir/bajar nubosidad; subir/bajar altura de la pared |
| Enter / M / Esc | Enviar selección / cargar respaldo fijo de las 10 h / salir |

## Del tema de vectores al cálculo
Abre **Visualizador/Trayectorias.cs**: `Punto` devuelve la posición en metros; `En` devuelve posición y velocidad en m/s. Para extenderlo con otra función f, cambia el par posición–derivada, no el servidor.
- Recta: P = (x₀ + dx·u, y₀). La dirección se invierte al volver.
- Circular: P = centro + A·(cos θ, sen θ), con θ = 2πt/T. La velocidad es tangente al círculo; su magnitud es 2πA/T.
- Senoidal: P = (x₀ + dx·u, y₀ + A·sen(2πN·u)). **A** es altura respecto a la línea media, no distancia de valle a cresta; **N** es el número de ondas durante la ida. **dx** viene de la longitud y el lado que devolvió Node.
- Parabólica: P = (x₀ + dx·u, y₀ + 4A·u·(1−u)). En u=0.5 se llega a y₀+A.
Para las tres rutas abiertas, u=(1−cos(2πt/T))/2: va de 0 a 1 y regresa a 0 sin saltar. Un periodo incluye ida y vuelta. El factor du/dt permite calcular la velocidad real por la regla de la cadena. En el seno:
```text
vx = dx · du/dt
vy = A · (2πN) · cos(2πN·u) · du/dt
rapidez = sqrt(vx² + vy²)
```
La flecha tiene largo fijo: muestra dirección. Su magnitud está en el panel como rapidez. Si v es cero no se dibuja la flecha. En pausa, el panel indica la velocidad matemática en el instante congelado.

## Tres experimentos para proyectar
1. **Primera cresta:** arranca con los valores iniciales y pulsa K. A=1 m, N=1 y T=8 s. En t≈1.33 s la posición es (1.25, 7.80) m y la velocidad ≈(1.70, 0.00) m/s. La derivada vertical es cero; el móvil todavía avanza a la derecha. Pulsa Espacio para continuar.
2. **Forma y velocidad no son lo mismo:** en O pulsa N hasta ver dos ondas; K localiza la nueva primera cresta. Cambia A y compara la altura. En C cambia F: el tiempo de una vuelta varía pero el radio no. En una órbita de radio 1 m y periodo 8 s, la rapidez es ≈0.79 m/s.
3. **Una sola variable de la API:** desde el caso base, elige 14 h sin cambiar 25 % ni 5 m y pulsa Enter. Al llegar la respuesta, la sombra sigue midiendo 5 m pero apunta al este; la mesa queda al sol. Pulsa K: la componente horizontal cambia de signo. La trayectoria circular conserva el sentido del giro y cambia de centro.

## Pruebas y archivos
```powershell
# Desde CampusSolar2D, primera terminal si no corre el servidor:
npm --prefix Servidor test
# Desde CampusSolar2D, con SDK .NET 8:
dotnet build Visualizador
dotnet run --project Comprobar
# Con Node activo, comprobar también HTTP desde C#, sin ventana:
dotnet run --project Comprobar -- --api
```
**Program.cs**: entradas, estados y reloj. **Lienzo.cs**: polígonos y conversión mundo/pantalla. **Trayectorias.cs**: funciones matemáticas y velocidades. **Comun/CampusApi.cs**: cliente HTTP y lectura de respaldo. **Servidor/modelo.mjs**: cálculo de la sombra. **Postman/**: colección de pruebas importable.
