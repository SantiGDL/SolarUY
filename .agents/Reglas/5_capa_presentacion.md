# Reglas Avanzadas: Capa de Presentación

Estas reglas se aplican a los componentes Blazor y sus archivos code-behind.

## 1. Responsabilidad de los Componentes

- Los componentes deben capturar eventos del usuario, invocar operaciones de Application y mostrar sus resultados.
- Pueden gestionar estado de presentación: carga, selección, filtros visuales, mensajes de error y apertura de modales.
- Evitar componentes que concentren responsabilidades de presentación, negocio y persistencia.

## 2. Separación de la Lógica de Negocio

- No implementar reglas de negocio ni cálculos de dominio dentro de componentes.
- Las reglas e invariantes pertenecen a Domain.
- Application coordina los casos de uso, utiliza el dominio y las abstracciones necesarias.
- Ejemplos que deben quedar fuera de la UI:
    - Determinar si un nodo puede asignarse a un parque.
    - Evaluar umbrales para generar alertas.
    - Calcular producción o indicadores del negocio.
- Si se detecta esta lógica en un componente, señalar el problema y proponer su traslado a la capa correspondiente.

## 3. Lógica de Presentación Permitida

- Se permite utilizar `if`, `switch` y `foreach` para renderizar y controlar la interfaz.
- Ejemplos:
    - Mostrar un indicador mientras se cargan datos.
    - Recorrer lecturas para construir una tabla.
    - Mostrar un mensaje cuando una lista está vacía.
    - Aplicar un color según el estado de una alerta ya determinada por el backend.
- Evaluar la responsabilidad del código, no prohibir estructuras del lenguaje por sí mismas.

## 4. Acceso a Datos y Validaciones

- Los componentes no deben acceder directamente al DbContext ni implementar consultas de persistencia.
- Deben invocar servicios o contratos de Application.
- Utilizar DTOs o modelos de presentación cuando corresponda, evitando exponer entidades con estado modificable directamente a formularios.
- La UI puede validar formatos y campos requeridos para ayudar al usuario.
- Las reglas de negocio deben comprobarse igualmente en Application o Domain, aunque exista validación en pantalla.

## 5. Actualizaciones en Tiempo Real

- Los componentes muestran las lecturas y alertas recibidas.
- La recepción y el procesamiento de telemetría deben delegarse a los servicios correspondientes.
- Gestionar las suscripciones de la UI y liberarlas cuando el componente se destruya.

