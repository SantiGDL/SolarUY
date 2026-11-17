# Legibilidad y simplicidad del código

Estas reglas se aplican al escribir, revisar o refactorizar código en todos
los módulos y capas de SolarUY.

## Condiciones

- Preferir condiciones directas. Si una condición combina varias comprobaciones difíciles de leer juntas, usar variables booleanas con nombres que expresen su significado.

## Comentarios

- Antes de un método, usar una frase breve para explicar qué hace cuando el nombre deja algún detalle importante afuera.
- Junto a una variable o condición, explicar el significado o el motivo cuando no surja del código.
- Antes de llamar a otro método, explicar por qué se llama en ese punto si el orden importa o si la llamada tiene un efecto que no se ve a simple vista.
- Mantener los comentarios cortos. Por ejemplo, `// Guardamos antes de notificar para que otros usuarios puedan consultar el cambio.`

## Operaciones asíncronas y cancelación

- Usar métodos normales para cálculos, validaciones y cambios de estado en memoria.
- Usar métodos asíncronos para operaciones que pueden quedar pendientes, como una consulta a la base de datos, una llamada de red o una espera de temporizador. Usar el sufijo `Async` según las convenciones de .NET y devolver directamente la tarea existente cuando el método solo delega la operación.
- Evitar `Task.Run` para envolver código que puede ejecutarse directamente.
- Si solo hace falta decidir si el ciclo continúa en la próxima vuelta, usar una bandera cuando sea suficiente. Usar `CancellationToken` cuando también haga falta interrumpir una espera en curso.
- Mantener una sola señal de cancelación por cada proceso que pueda detenerse de forma independiente. Su nombre debe indicar qué detiene.
