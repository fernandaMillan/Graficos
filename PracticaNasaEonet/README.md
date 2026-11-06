# Práctica resuelta · NASA EONET

Proyecto Console App en C# (.NET 8 y Raylib-cs 8.0.0). La cuadrícula representa longitudes y latitudes; no incluye continentes ni muestra la propagación del fuego.

## Cómo ejecutarlo

1. Descomprime el ZIP. En Visual Studio Code, abre la carpeta `PracticaNasaEonet` (la que contiene el archivo .csproj).
2. Abre Terminal → New Terminal. Con el SDK de .NET 8 instalado, ejecuta `dotnet run`. La primera ejecución necesita acceso a NuGet para descargar Raylib-cs.
3. Haz clic en un círculo para ver sus datos y el vector desde el centro. Haz clic fuera para limpiar la selección.

## Demostración sin consultar NASA

Ejecuta `dotnet run -- --local`. Usa el archivo `eventos.json` incluido, con datos **simulados**, y muestra «Archivo local». Después de restaurar las dependencias puedes usar `dotnet run --no-restore -- --local` sin red.

| Registro | Longitud | Latitud | Pantalla | Marcador |
|---|---:|---:|---|---|
| Simulado A | −90° | 45° | (220, 170) | Naranja relleno |
| Simulado B | 60° | −30° | (520, 320) | Azul de contorno |

Con A seleccionado, el vector es (−180, −90) px. La flecha señala el punto seleccionado, no una trayectoria. Ejecuta `dotnet run -- --verificar` para comprobar el JSON incluido, la matriz, el vector y la selección sin abrir una ventana.

## Dónde está cada parte

- `CargarDatosAsync` hace un GET al iniciar. Si hay un error de red o JSON inválido, intenta leer el archivo local. Si la consulta válida no trae puntos, la ventana indica «Sin eventos para mostrar».
- `LeerEventos` conserva el punto válido más reciente de cada evento y omite polígonos.
- `Proyectar` aplica la matriz de la hoja: x = 2·longitud + 400, y = −2·latitud + 260.
- `DibujarVector` dibuja el desplazamiento desde el centro. `Marcador` aplica la leyenda por color y forma.

Referencia: [NASA EONET v3](https://eonet.gsfc.nasa.gov/docs/v3).
