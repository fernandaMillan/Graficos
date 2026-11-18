# Práctica 3 · Laboratorio de traslación y rotación

Demostración lista para explicar en clase y práctica para estudiantes. C# con .NET 8 y Raylib-cs. Cada pestaña muestra la figura original en gris, la transformada en azul, la matriz aplicada y el resultado del punto de prueba (1,0), marcado en naranja.

## Inicio rápido

1. Descomprime el ZIP y abre la carpeta LaboratorioTransformaciones en VS Code.
2. Con el SDK de .NET 8 instalado, abre la terminal en esa carpeta y ejecuta `dotnet run`.
3. Haz clic en una pestaña. En las primeras tres puedes empezar sin cámara. La primera ejecución necesita conexión para restaurar Raylib-cs desde NuGet.

| Pestaña | Cómo probarla | Qué mostrar |
|---|---|---|
| 1 Traslación | Flechas. Pulsa **E** para cargar (2,1). | El punto (1,0) llega a (3,1), sin cambiar la orientación. |
| 2 Giro automático | R inicia desde cero. P pausa y reanuda. **E** carga 90° y pausa. | El ángulo avanza con el tiempo, aunque nadie toque un control. |
| 3 Giro manual | Flechas izquierda/derecha o slider. **E** carga 90° exactos. | El usuario elige el ángulo; al dejar de mover el control, el giro se detiene. |
| 4 Mano | **C** abre la página de cámara. Actívala y muestra **una palma abierta**. | Solo con la palma abierta detectada cambia el ángulo. Retira la mano: conserva el último ángulo. |
| 5 Pivote (opcional) | P pausa. **E** carga 90° y pausa. | El pivote rojo (2,0) queda fijo; (1,0) llega a (2,−1). |
| 6 Orden (opcional) | **E** carga 90° y **O** alterna T·R y R·T. | (1,0) llega a (3,1) o a (0,4), según el orden. |

**R** reinicia la pestaña. **ESC** cierra Raylib. Los atajos requieren que la ventana de Raylib tenga el foco.

## Cámara: un gesto, la palma abierta

1. En la pestaña **Mano**, pulsa **C**. Si el navegador no se abre, copia en Chrome o Edge la URL local que aparece en la terminal.
2. Haz clic en **Activar cámara** y concede permiso. Coloca el navegador al lado de Raylib; si ocultas la página, la cámara se apaga.
3. Con buena luz, abre una mano con los dedos extendidos. El navegador muestra «Palma abierta: giro activado». Al retirar la mano o usar otra pose, deja de girar. **Ningún otro gesto tiene una acción asignada.**
4. Pulsa **Detener cámara** para finalizar. Salir de la pestaña Mano también cierra el puente local. Al perder la señal, Raylib deja de girar en menos de un segundo desde la última señal válida.

El navegador analiza manos con MediaPipe y solo acepta la clase `Open_Palm` (confianza mínima 0.65). La página no muestra, graba ni envía video al programa; Raylib recibe únicamente un 1 o un 0 por una conexión local. No usa detección facial, seguimiento corporal ni micrófono. **No tienes que agregar un archivo de modelo**: requiere internet para cargar el modelo y la biblioteca de reconocimiento. Las otras pestañas no necesitan cámara ni conexión una vez restaurado el proyecto.

## Trabajo en clase

- `HOJA_DE_TRABAJO.md`: instrucciones y evidencias para estudiantes.
- `GUIA_DOCENTE.md`: secuencia de demostración, respuestas y prueba antes de clase.
- `Program.cs`: matrices homogéneas, dibujo y controles. Cambia `RapidezAutomatica` en el reto.
- `Camara.cs` y `camara.html`: puente local y detección. Los estudiantes no necesitan modificarlos.

`dotnet run -- --probar` revisa los casos de matrices en consola. `dotnet run -- --probar-puente` revisa la conexión local y la pérdida de señal sin abrir la cámara. Para confirmar el funcionamiento de la webcam haz la prueba de la guía docente.

Referencia: [MediaPipe Gesture Recognizer](https://ai.google.dev/edge/mediapipe/solutions/vision/gesture_recognizer/web_js).
