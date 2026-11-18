# Práctica 3 · Tres formas de controlar una transformación

**Nombre:** ____________________________________  **Fecha:** _______________

**Meta:** predecir la posición final de un punto, comprobarla en el laboratorio y explicar qué diferencia hay entre una matriz de rotación y la entrada que actualiza su ángulo.

Trabaja con el laboratorio ya programado. No tienes que implementar visión por computadora.

## 1. Primero en papel

Dibuja unos ejes con X a la derecha, Y hacia arriba y p=(1,0). Calcula y marca el nuevo punto antes de abrir cada ejemplo. Una rotación positiva es antihoraria en estos ejes.

| Caso | Predicción en papel | Resultado en Raylib | ¿Coincide? |
|---|---|---|---|
| T=(2,1) | | | |
| R=90° sobre el origen | | | |
| R=180° sobre el origen | | | |

## 2. Después en pantalla

1. **Traslación:** pulsa **E** para cargar (2,1) y comprueba la primera fila. Después mueve la figura con las flechas. ¿Qué se conserva al trasladarla?
2. **Giro automático:** presiona **R**, mira cómo cambia el ángulo sin mover ningún control y usa **P** para detenerlo. Pulsa **E** para comprobar el caso exacto a 90°.
3. **Giro manual:** pulsa **E** y compara la matriz con la de la pestaña anterior. Aproxima 180° con el slider o las flechas. Anota el ángulo real que muestra el programa antes de comparar. ¿Qué cambia entre las dos pestañas: la matriz o la forma de elegir el ángulo?
4. **Mano:** pulsa **C** en Raylib y **Activar cámara** en el navegador. Con ambas ventanas visibles, muestra una palma abierta y después retírala. Anota lo que pasa con el ángulo. Prueba sin mano y con otra pose: ninguna activa otro comando.

| Entrada en la pestaña Mano | ¿El ángulo cambia? | ¿Qué observé? |
|---|---|---|
| Palma abierta | | |
| Sin palma abierta | | |
| Cámara apagada | | |

Si no puedes usar la cámara, registra el motivo. Compara el giro automático al pulsar P con la regla «solo actualizo el ángulo si hay señal de palma». No grabes a nadie para entregar evidencia.

## 3. Modificación en C#

Cambia solo `RapidezAutomatica` de 60 a 30 grados por segundo. Antes de ejecutar, predice cuánto tarda una vuelta completa a cada velocidad. Comprueba que el modo **Mano** conserva sus 60 grados/s de referencia.

**Predicción:** ____________________________________________________________________

**Prueba y observación:** ______________________________________________________________

Explica qué trabajo hacen `R`, `Aplicar` y `Pantalla`. ¿Por qué se transforma la figura original en cada cuadro en vez de transformar la figura ya rotada?

_____________________________________________________________________________________

**Entrega:** la tabla en papel, una captura del caso a 90° mostrando la matriz y el punto transformado, y tu predicción y comprobación del cambio de velocidad.

## Extensión opcional

- En **Pivote**, usa **E**. Predice dónde queda (1,0) con giro de 90° alrededor de C=(2,0). ¿Qué coordenada no se mueve?
- En **Orden**, usa **E** para fijar 90°. Presiona **O** sin cambiar el ángulo y compara T·R y R·T con T=(3,0). Escribe por qué no dan el mismo resultado.
