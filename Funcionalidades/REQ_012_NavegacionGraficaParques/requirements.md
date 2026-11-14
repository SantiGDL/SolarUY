# REQ_012 — Navegación y representación gráfica de parques, nodos y paneles

## Objetivo

Mejorar el mapa interactivo existente de SolarUy para representar visualmente la jerarquía **parque → nodos → paneles** y facilitar el acceso a las páginas de gestión mediante clics sobre los elementos gráficos.

La implementación debe priorizar la reutilización de funcionalidades existentes y minimizar el riesgo de regresiones.

Este documento define requisitos y no implica implementar los cambios.

## 1. Navegación gráfica

### RF-01 — Navegación desde parques

Al hacer clic en la tarjeta principal de un parque en `/admin/parques`, navegar a:

`/admin/parques/{parque.Id}/nodos`

- Utilizar el identificador real del parque.
- Reutilizar la misma ruta de «Gestionar nodos» de la tabla existente.
- Permitir la navegación incluso cuando el parque no tenga nodos.
- Sustituir el comportamiento actual de selección por navegación, conservando las funcionalidades visuales existentes.

### RF-02 — Navegación desde nodos

Al hacer clic en cualquier nodo representado gráficamente, navegar a:

`/admin/paneles`

- Todos los nodos utilizan el mismo destino.
- No transmitir el identificador del nodo mediante parámetros.
- No preseleccionar automáticamente parques ni nodos.
- El usuario seleccionará manualmente los datos necesarios en el formulario existente.
- La navegación del nodo debe ser independiente de la navegación del parque.

### RF-03 — Interacción y accesibilidad

- Conservar el diseño y la estructura actual del mapa.
- Mantener la visualización de nodos mediante hover y foco.
- Permitir navegación con mouse, teclado y pantalla táctil.
- Mantener indicadores de foco visibles.
- Actualizar los textos de ayuda para describir el nuevo comportamiento.
- Evitar que un clic active simultáneamente dos navegaciones.

## 2. Representación gráfica de paneles

### RF-04 — Visualización dentro de los nodos

Representar los paneles registrados mediante **pequeños rectángulos azules dentro de la tarjeta de cada nodo**, respetando su asociación real.

- Cada rectángulo representa visualmente un panel registrado.
- Los rectángulos no tienen interacción ni navegación propia.
- El clic sobre cualquier parte del nodo continúa navegando a `/admin/paneles`.
- Mantener el nombre y la ubicación del nodo visibles.
- Evitar modificaciones importantes en el tamaño y la disposición de las tarjetas.

### RF-05 — Cantidad de paneles

- Mostrar un máximo de 4 rectángulos por nodo para evitar saturación visual.
- Si existen más de 4 paneles, mostrar un indicador adicional `+N` con la cantidad restante.
- Un nodo sin paneles no muestra rectángulos.
- La cantidad representada debe corresponder a los paneles realmente registrados.
- Utilizar únicamente datos obtenidos mediante los servicios existentes.

### RF-06 — Actualización de datos

- Los paneles representados deben corresponder al nodo al que están asociados.
- Al regresar al mapa después de registrar paneles, la información debe reflejar los datos persistidos.
- Reutilizar el mecanismo de carga existente siempre que sea posible.
- No incorporar actualizaciones en tiempo real, SignalR adicional ni mecanismos nuevos de sincronización.

## 3. Restricciones técnicas

### RF-07 — Cambios mínimos

Priorizar modificaciones exclusivamente en:

- `SolarUy/Components/Pages/Parques.razor`
- `SolarUy/Components/Pages/Parques.razor.css`

No modificar los formularios ni el comportamiento interno de `Nodos.razor` y `Paneles.razor`.

No realizar cambios en:

- Dominio y entidades.
- Contratos de servicios de Aplicación.
- Infraestructura y persistencia.
- Base de datos y migraciones.
- Autenticación y autorización.
- Simulador, tareas y mediciones.

Reutilizar Blazor Interactive Server y los servicios existentes.

No incorporar nuevas dependencias, bibliotecas gráficas ni componentes innecesarios.

### RF-08 — Conservación del funcionamiento

- Evitar refactorizaciones y limpiezas de código ajenas al requisito.
- No eliminar estados o métodos existentes sin comprobar previamente sus dependencias.
- Mantener los estados de carga, error y mapa vacío.
- Conservar la navegación desde la tabla y el menú lateral.
- No modificar los procedimientos de registro, edición o eliminación.
- Navegar o visualizar elementos nunca debe crear ni modificar registros.

## 4. Alcance por etapas

### Etapa 1 — Navegación

1. Habilitar navegación al hacer clic en la tarjeta de un parque.
2. Habilitar navegación al hacer clic en un nodo.
3. Actualizar los textos de ayuda.
4. Verificar que la representación gráfica actual siga funcionando.

Esta etapa debe poder entregarse independientemente.

### Etapa 2 — Representación de paneles

1. Consultar los paneles registrados mediante los servicios existentes.
2. Asociar visualmente los paneles a su nodo correspondiente.
3. Mostrar los rectángulos azules y el indicador de excedentes.
4. Mantener la navegación de parques y nodos sin interferencias.

Si la representación requiere cambios significativos fuera de Presentación, documentar la limitación antes de ampliar el alcance.

## 5. Criterios de aceptación

### Etapa 1

1. Al hacer clic en Parque 1, si su ID es 1, se abre `/admin/parques/1/nodos`.
2. Cada parque navega utilizando su identificador correspondiente.
3. Un parque sin nodos también permite acceder a su gestión.
4. Al hacer clic en cualquier nodo se abre `/admin/paneles`.
5. Los nodos conservan su visualización gráfica mediante hover y foco.
6. Las navegaciones funcionan con mouse, teclado y pantalla táctil.
7. No se crean registros ni se modifican datos al navegar.
8. La tabla, el menú lateral y los formularios existentes mantienen su funcionamiento.

### Etapa 2

1. Cada nodo muestra hasta 4 rectángulos azules correspondientes a sus paneles registrados.
2. Si un nodo tiene más de 4 paneles, muestra `+N` con la cantidad restante.
3. Los nodos sin paneles mantienen su presentación sin rectángulos.
4. Los indicadores no alteran el comportamiento del clic sobre el nodo.
5. Los paneles se representan únicamente dentro del nodo al que pertenecen.
6. Al regresar al mapa después de registrar un panel, se reflejan los datos guardados.
7. La representación conserva el diseño y la disposición general del mapa.

## 6. Verificación

Al implementar cada etapa:

- Comprobar el funcionamiento con parques y nodos de diferentes identificadores.
- Probar nodos con cero, uno y múltiples paneles.
- Revisar el comportamiento con mouse, teclado y pantalla táctil.
- Verificar que no existan regresiones visuales ni funcionales.
- Ejecutar `dotnet build SolarUy.sln`.
- Ejecutar las pruebas existentes aplicables.
- Seguir `.agents/Reglas/6_flujo_trabajo_ia.md`.
- Ejecutar SonarQube conforme a `Docs/ReadmeSonarQube.md`, si está disponible.
- Documentar las verificaciones no realizadas y sus motivos.

**Principio general:** implementar la solución más sencilla que cumpla los requisitos, conservando al máximo el código y los comportamientos existentes.