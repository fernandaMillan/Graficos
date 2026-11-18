# Guía docente · Práctica 3

**Idea central:** automática, manual y cámara usan la misma matriz de rotación. Cambia cómo se obtiene o actualiza el ángulo. La palma abierta funciona como un botón que se sostiene, no como un medidor de orientación de la mano.

## Demostración sugerida

1. **10 min · Traslación.** Pide predecir p=(1,0) con T=(2,1). Pulsa E y compara (3,1) con el resultado en pantalla. Señala que la figura se desplaza sin girar.
2. **15 min · Tiempo y control manual.** Compara Giro automático y Giro manual en 90° exactos con E. Detén el giro automático y contrasta las matrices: deben ser iguales al mismo ángulo. Reanuda con P; en manual el ángulo solo cambia al usar el control.
3. **10 min · Cámara.** Coloca navegador y Raylib lado a lado. Activa la cámara, muestra una mano abierta y retírala. Pregunta: «¿Cambió la fórmula o solo la condición que deja avanzar el ángulo?». Evita convertirlo en una actividad de entrenamiento de modelos.
4. **15 min · Papel y puesta en común.** Deja que completen la tabla y expliquen el caso a 180°. Si hay tiempo, usa Pivote y Orden como extensión.
5. **20 min · Cambio de código y evidencia.** Modifican `RapidezAutomatica`, prueban y explican el resultado. Pueden entregar la reflexión al cierre.

## Respuestas de referencia

| Caso con p=(1,0) | Resultado |
|---|---|
| T=(2,1) | (3,1) |
| R=90° sobre el origen | (0,1) |
| R=180° sobre el origen | (−1,0) |
| R=90° sobre C=(2,0) | (2,−1). El pivote C queda fijo. |
| T·R, con T=(3,0), R=90° | (3,1). Primero gira. |
| R·T con los mismos valores | (0,4). Primero traslada. |

Al retirar la mano, el ángulo conserva su valor; no se reinicia. En automático, una vuelta ideal tarda 6 s a 60 grados/s y 12 s a 30 grados/s. Una pausa o una interrupción larga puede alargar el tiempo visual. Volver a la figura original en cada cuadro evita acumular transformaciones por accidente.

## Prueba antes de clase

1. Ejecuta `dotnet build`, `dotnet run -- --probar` y `dotnet run -- --probar-puente`. La segunda comprueba resultados y la tercera prueba el puente sin webcam.
2. Ejecuta la aplicación y verifica seis pestañas. En Traslación presiona E. En Orden presiona E y luego O: deben alternar (3,1) y (0,4).
3. En Mano, verifica la secuencia **palma abierta → gira; sin palma abierta → se detiene; al cerrar la página → se detiene**. Repite con la iluminación del aula. Si el permiso o internet falla, continúa con el modo automático y muestra la condición de activación en el código.
