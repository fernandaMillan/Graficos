# Campus Solar · Laboratorio del problema 8
¿Cómo cambia la sombra de una pared y alcanza o no el centro de una mesa a 4 m al oeste? Los ángulos y horas son un modelo escolar de Mexicali. La API no proporciona el clima real. Primero suministra datos y reglas; cada estudiante de tercero consume su API y crea su propia interpretación gráfica.

## Guía de arranque (VS Code en Windows)
Extrae todo el ZIP. Abre **CampusSolar**, la carpeta que contiene Servidor, Datos, Postman, Consola y Visor. Para las ventanas de terminal siguientes usa Terminal → Nueva terminal. Cada terminal debe empezar en esa carpeta. Necesitas Node.js (22 o 24 para el servidor local), el SDK de .NET 8, Postman de escritorio y, la primera vez, internet para restaurar Raylib-cs 8.0.0. El despliegue usa Node 22.x. No hacen falta credenciales para el ejercicio local.

### 1. Antes de la computadora
Dibuja la pared en 0 y la mesa a +4 m. Oeste es +x. A las 10:00, con la pared de 5 m y el Sol a 45°, la sombra llega a +5 m. Cambia únicamente la hora a 14:00 y predice dónde termina. Es la misma longitud pero hacia el este: termina en −5 m. La mesa queda al sol.
Separa lo que *elige* el usuario (10, 25, 5) de lo que *averigua* (longitud, lado, cobertura, contraste). Abre Datos/peticion.json y Datos/mock-response.json. Los dos son texto; el segundo es una respuesta guardada que no cambia mágicamente al editar los controles. Al serializar un objeto se obtiene JSON; al deserializarlo se recuperan valores que el programa puede utilizar.

### 2. ¿GET o POST?
El método describe qué se pide hacer; no hay tipos JSON exclusivos de GET o POST. GET recupera información. Para indicar un filtro usamos la URL, no un body: /api/escenario?horaLocal=14. El servidor lee "14" como texto, valida el catálogo y lo convierte. Ambos métodos pueden DEVOLVER JSON. POST /api/simular recibe el objeto de Datos/peticion.json como body de tipo application/json y devuelve un cálculo; no crea registros. JSON representa objetos, listas, cadenas, números, booleanos y null, pero el contrato decide cuáles acepta: el número 10 es una hora válida; la cadena "10" se rechaza en este POST.

### 3. Construcción y arranque en Node (terminal A)
```powershell
node --version
cd Servidor
npm test
npm start
```
Deja la terminal abierta; prueba http://127.0.0.1:3000/api/health. La respuesta debe decir "estado": "ok". No hay dependencias que instalar para el servidor local; también puedes arrancarlo con `node local.mjs`.
Para construirlo paso a paso, abre en este orden:
1. **api/health.mjs**: GET mínimo, una respuesta JSON con el estado.
2. **modelo.mjs**: objeto escenario, cinco horas y función simular. Compara la longitud sin redondear, luego presenta dos decimales solo al mostrarla. Cambia la nubosidad y localiza la única fórmula que debe verse afectada.
3. **api/escenario.mjs**: GET para la configuración; con horaLocal en la URL devuelve además la fila de la tabla solar.
4. **api/simular.mjs**: comprueba Content-Type, lee await request.json(), llama a simular y devuelve Response.json(...). El error de rango produce HTTP 400, no un resultado con ceros.
5. **local.mjs** conecta esas tres funciones a un puerto local. Vercel ejecuta los archivos de api/; no publiques un proceso escuchando un puerto ni los proyectos .NET.

### 4. Postman
Abre Postman de escritorio → Import → Postman/CampusSolar.postman_collection.json. La variable de colección baseUrl ya vale http://127.0.0.1:3000. Pulsa Send en cada solicitud y revisa el status, Body y Test Results. Puedes ejecutar la colección completa. Si prefieres empezar a mano:
- GET {{baseUrl}}/api/health (sin body): 200, estado ok.
- GET {{baseUrl}}/api/escenario (sin body): 200, horas permitidas y mesaXM = 4.
- Duplica ese GET. En Params pon horaLocal = 14. Predice y confirma referenciaSolar.direccionSombra = "este".
- POST {{baseUrl}}/api/simular → Body → raw → JSON. Copia Datos/peticion.json. Confirma el encabezado Content-Type: application/json. Send → 200. Luego cambia solo una variable y vuelve a enviar. Usa los ejemplos 03–09 de la colección.

| Cambio (desde 10 h, 25 %, 5 m) | Resultado |
|---|---|
| Ninguno | 5 m oeste; true; contraste 0.75 |
| Solo 12 h | ≈2.89 m oeste; false; contraste 0.75 |
| Solo 14 h | 5 m este; false; contraste 0.75 |
| Solo 80 % | 5 m oeste; true; contraste 0.20 |
| Solo altura 4 m | 4 m oeste; true (incluye el límite) |
| Solo nubosidad 120 | 400, error sobre nubosidad |
| Solo hora "10" con comillas | 400, error de tipo |
Conserva una captura de la petición y otra que muestre un error con su causa.

### 5. Publicación en Vercel (una demostración docente)
Opcional con internet y tu cuenta. En una terminal NUEVA, desde CampusSolar:
```powershell
cd Servidor
npx vercel@latest login
npx vercel@latest --prod
```
Elige tu cuenta y crea un proyecto nuevo; directorio actual ./, preset Other si lo pide, sin framework de frontend ni comando de build personalizado. Copia el dominio de **producción** que devuelva Vercel, sin /api. En Postman edita la variable de colección baseUrl y pega ese origen; repite GET health y el POST de las 10 h. Si recibes HTML o un inicio de sesión en lugar de JSON, confirma que es el dominio de producción previsto y que tu cuenta permite ese acceso. No necesitas Supabase/Firebase: esta simulación no persiste datos. Si el despliegue no es posible, continúa en localhost; en otra computadora localhost NO apunta al equipo docente.
La publicación es un paso que realizas tú; el ZIP no contiene una URL pública preasignada.

### 6. Cliente de consola C# (terminal B, desde CampusSolar)
```powershell
dotnet --version
dotnet run --project Consola
dotnet run --project Consola -- 14 25 5
```
Sin argumentos usa 10, 25, 5. Orden de argumentos: hora, nubosidad, altura. El primer caso debe imprimir 5.00 m al oeste y mesa en sombra; en el segundo aparece este y mesa al sol. Si usas la nube, antes de ejecutar define en **cada terminal que ejecute un cliente**:
```powershell
$env:CAMPUS_API_URL = Read-Host "Pega el dominio HTTPS de Vercel, sin /api"
```
Para volver al servidor local en esa terminal: `$env:CAMPUS_API_URL = "http://127.0.0.1:3000"`.
Abre Comun/CampusApi.cs: Preparar hace GET de salud y escenario; Consultar usa PostAsJsonAsync y ReadFromJsonAsync. Un 400 no se deserializa como simulación válida. La consola imprime entradas, JSON recibido y lectura humana. Para probar sin red: `dotnet run --project Consola -- --mock`. Eso lee el archivo incluido; no sustituye la prueba de una API real.

### 7. Una respuesta, dos escenas
Desde CampusSolar: `dotnet run --project Visor`. Enter consulta; M permite trabajar con el respaldo fijo. Si modificas controles y no confirmas, el renglón de la escena sigue mostrando los últimos valores aplicados.
- Izq./der.: hora. Arriba/abajo: nubosidad. W/S: altura. **Enter:** enviar. **Tab:** alternar 2D y 3D. **O:** alternar cámara en 3D. **L:** activar o desactivar sombreado por cara en 3D. Esc: cerrar.
- **2D:** 32 píxeles por metro, +x al oeste. En el caso base, la sombra mide 160 px; la mesa está a 128 px de la pared. A las 14 h la punta cambia de lado. Una matriz transforma del mundo a la pantalla. El punto que recorre el rayo muestra interpolación con tiempo local; no representa la velocidad física de la luz.
- **3D:** el mismo resultado controla el sentido y tamaño de la sombra. Una matriz coloca cada pieza de la mesa; cada cara de la pared recibe luz según su normal. Ambiente + difusa de Lambert por cara. Pulsa L para comparar; el rectángulo que representa la sombra permanece porque proviene de la API. No hay shadow maps, Phong especular ni iluminación global. Si cambias únicamente de proyección, la sombra cambia de aspecto en pantalla, no de longitud en metros. Una vista 3D en monitor no equivale a realidad virtual o aumentada.
- Al fallar la red, la aplicación muestra **Error** y conserva la última escena válida. M elige **Respaldo** explícitamente. Enter vuelve a intentar. No hagas un POST dentro de cada cuadro.
- Las teclas están en Visor/Program.cs; la proyección y la iluminación están en Visor/Escena.cs.

### 8. Tarea individual (diseño, todavía no implementación)
En una hoja: escribe la pregunta que contestará tu escena; pega un JSON **de tu problemática**, real o de respaldo, e indica su origen. Elige tres variables modificables. Dibuja dos estados que difieran en una sola variable. Anota tres correspondencias propiedad → unidad/rango → regla visual y elige tres recursos de Gráficos, uno espacial. Indica qué cambia por tiempo local y cuándo envías otra petición. Predice un resultado que se pueda comprobar en pantalla y qué verá el usuario si se cae la API. 3D solo si ayuda a contestar tu pregunta.

## Si algo falla
- Puerto 3000 ocupado: en la terminal A, antes de arrancar, usa `$env:PORT="3001"`; cambia baseUrl en Postman y CAMPUS_API_URL en C# al mismo puerto.
- El navegador muestra error en simular: escribir esa URL en su barra hace GET. Para esta ruta usa POST en Postman.
- 415: elige raw → JSON. 400: revisa tipos/rangos y nombres. Error de conexión: verifica que siga abierta la terminal A y prueba health con la misma base.
- .NET no encuentra el proyecto: abre la terminal donde se ven Consola y Visor. La consola no requiere Raylib; el visor sí restaura NuGet. Si el laboratorio no permite gráficos, demuestra el valor en consola y pasa la escena a la cuadrícula en papel.
- Diagnóstico sin interfaz gráfica: `dotnet build Consola`, `dotnet build Visor`; desde Servidor, `npm test`.
