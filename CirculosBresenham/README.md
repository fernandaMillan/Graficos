# Práctica 1 · Círculos con punto medio y Bresenham

Proyecto completo en **C#**, **.NET 8** y **Raylib-cs 8.0.0**.

El programa permite elegir el centro y el radio con dos clics. La vista previa usa `DrawCircleLines`; el círculo confirmado se rasteriza píxel por píxel con la variante de Bresenham `e = 3 - 2r`.

## Abrir y ejecutar

1. Descomprime el proyecto.
2. Abre la carpeta `CirculosBresenham` en Visual Studio Code.
3. Abre `Terminal → New Terminal`.
4. Ejecuta:

```powershell
dotnet restore
dotnet build
dotnet run
```

## Controles

| Acción | Control |
|---|---|
| Definir el centro | Primer clic izquierdo |
| Definir el punto del borde y confirmar el radio | Segundo clic izquierdo |
| Comenzar otro círculo | Clic izquierdo después de confirmar |
| Limpiar | `C` |
| Mostrar u ocultar la cuadrícula | `G` |
| Salir | `ESC` |

## Qué observar en el código

- `CalcularRadio`: aplica distancia euclidiana y redondea el radio.
- `DibujarCirculoBresenham`: calcula un octante usando enteros.
- `DibujarOchoPuntos`: usa la simetría para completar la circunferencia.
- `esperandoRadio` y `circuloListo`: conservan el estado entre frames.
- `DrawCircleLines`: solo genera la vista previa.
- `DrawPixel`: construye el resultado confirmado.

## Pruebas solicitadas

### Radio 0

Haz los dos clics en la misma posición. Debe aparecer un punto.

### Radio 1

Coloca el segundo punto a un píxel del centro. Deben aparecer los cuatro puntos cardinales.

### Radio 6

Selecciona un centro y coloca el segundo punto seis píxeles a la derecha. El octante esperado es:

```text
(0,6), (1,6), (2,6), (3,5), (4,4)
```

### Cerca del borde

Selecciona un centro cerca de una esquina. La parte que queda fuera no se verá, pero el programa debe continuar funcionando.

### Segundo clic vertical

Prueba la misma distancia hacia abajo o hacia arriba. El radio debe ser igual porque depende de la distancia, no de la dirección.

## Explicación breve

El algoritmo inicia en `(x, y) = (0, r)`. En cada iteración dibuja los ocho reflejos del punto actual. La variable `e` decide si `y` se conserva o disminuye:

```text
e < 0   → conservar y
e >= 0  → disminuir y
```

Después aumenta `x`. El ciclo termina cuando `x > y`, porque en ese momento ya terminó el primer octante.

## Error que conviene provocar

Mueve `y--` fuera del bloque `else` y ejecuta el programa. El círculo se deformará porque `y` disminuirá en todas las iteraciones. Después devuelve `y--` al bloque `else` y documenta intento, evidencia, causa, corrección y explicación.
