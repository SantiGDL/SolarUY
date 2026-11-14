# REQ_012 — Especificación de navegación y representación gráfica

## Base y alcance

Diseño derivado del `requirements.md` actualizado de esta carpeta, siguiendo
`AGENTS.md`, los seis archivos de `.agents/Reglas/` y la guía local
`.agents/skills/sdd-generar-spec.md`. La referencia de esa guía a
`7_flujo_trabajo_ia.md` corresponde al archivo real `6_flujo_trabajo_ia.md`.

La etapa 1 entrega navegación de parques y nodos; la etapa 2 agrega la
representación de paneles. Ambas se limitan a `Parques.razor` y
`Parques.razor.css`, en `SolarUy/Components/Pages/`. Este documento define el
diseño y no ejecuta la implementación.

**Criterio de mínima intervención:** preservar el mapa, su distribución y la
posición de sus nodos. No hacer refactorizaciones ni limpiezas preventivas.
Implementar cada etapa por separado para poder comprobarla y revertirla sin
afectar la otra.

## Módulo Parques

### Dominio

No modificar entidades, relaciones ni comportamiento de negocio. La asociación
panel → nodo se toma de `PanelDto.NodoId`. Contar paneles y limitar su dibujo
son operaciones de presentación. Navegar o visualizar nunca modifica datos.
No se requieren nuevas pruebas de Dominio porque su comportamiento no cambia.

### Aplicación

Reutilizar los contratos y servicios actuales sin cambios. La pantalla conserva
`IParquesService` e `INodosService` y, en etapa 2, inyecta `IPanelesService`.
Su operación `ObtenerTodosAsync()` devuelve paneles registrados con `Id` y
`NodoId`; permite consultar a usuarios autenticados. No agregar endpoints,
contratos, consultas individuales por tarjeta ni operaciones de escritura.

### Infraestructura

Sin cambios en repositorios, DbContext, persistencia, base de datos o migraciones.
Los componentes no acceden directamente a estos elementos.

### Presentación — etapa 1: navegación (RF-01 a RF-03, RF-07 y RF-08)

1. En `Parques.razor`, sustituir el botón principal `.parque-map-marker` por
   un enlace con destino relativo construido con el ID real del parque:
   `/admin/parques/{parque.Id}/nodos`. Reutilizar la construcción de ruta de
   «Gestionar nodos» en la tabla. Conservar su contenido, clases y contador.
2. Cambiar el `href` de cada `.parque-map-node-chip` a `/admin/paneles`.
   Todos los nodos navegan exactamente al mismo destino, sin query string,
   fragmentos ni identificadores. No transferir contexto por otros mecanismos
   ni preseleccionar datos; se conserva la selección manual del formulario.
3. Mantener el enlace del parque y los enlaces de nodos como elementos separados
   dentro del artículo, sin enlaces anidados ni navegación en el contenedor.
   Así una activación no dispara dos destinos. La capa visual de los nodos
   no debe bloquear las zonas libres de la tarjeta del parque.
4. Antes de cambiar la selección, buscar referencias a `AlternarParque`,
   `parqueSeleccionadoId` e `is-selected`. Modificar únicamente lo indispensable
   para que el clic principal navegue. Conservar estados, métodos y estilos
   existentes siempre que no interfieran con la navegación ni generen efectos
   incorrectos. Retirar código solo si es indispensable y se ha comprobado
   que no sostiene otras funcionalidades; no realizar limpiezas preventivas.
   Conservar `:hover` y `:focus-within`, que permiten mostrar los nodos.
5. Adaptar únicamente los estilos necesarios para el nuevo enlace principal:
   conservar color, tamaño, disposición, foco visible y ausencia de subrayado.
   Mantener las reglas móviles y de movimiento reducido. La activación mediante
   Enter o toque debe navegar sin necesitar una selección previa.
6. Actualizar ayuda y nombres accesibles para explicar ambos destinos. El nombre
   accesible del nodo debe identificarlo e indicar «Ir a paneles», evitando
   sugerir que el destino filtra paneles de ese nodo.

La navegación funciona con cero nodos. Se conservan el mensaje de parque vacío,
los estados de carga/error, la tabla y el menú lateral. No se modifican
`Nodos.razor`, `Paneles.razor`, sus formularios ni sus procedimientos de gestión.

### Presentación — etapa 2: paneles dentro del nodo (RF-04 a RF-08)

#### Carga y asociación

- Ampliar `CargarAsync` con una llamada a `IPanelesService.ObtenerTodosAsync()`
  por carga del mapa, después de las consultas existentes. Mantener las llamadas
  secuenciales y el mecanismo de carga actual, sin consultas por nodo.
- La consulta de paneles es **complementaria y no bloqueante** para el mapa:
  no debe impedir que se visualicen o utilicen parques y nodos si falla.
- Agrupar en memoria por `PanelDto.NodoId` y conservar un diccionario privado
  de cantidades por nodo. Utilizar nombres españoles y `_camelCase` para los
  campos nuevos, sin renombrar campos ajenos al cambio.
- Buscar la cantidad con el ID del nodo representado. La ausencia de una entrada
  tras una consulta exitosa equivale a cero; no utilizar índices de posición,
  nombres ni asociaciones supuestas. Los paneles de otros nodos no se dibujan
  en la tarjeta actual.
- Recalcular el diccionario en cada carga exitosa, reemplazando el anterior.
  Verificar que una entrada normal a `/admin/parques` después de registrar
  paneles utilice datos actualizados. Comprobar también el retorno mediante
  Atrás del navegador y documentar cualquier limitación: **no introducir lógica
  adicional de recarga, sincronización ni eventos de navegación** salvo que
  resulte estrictamente necesaria para cumplir el requisito y pueda hacerse
  sin poner en riesgo el mapa existente.
- Si falla la consulta de paneles, registrar el error y mostrar un aviso
  comprensible, utilizando el manejo de excepciones que corresponda al servicio
  existente (incluida `PanelesPersistenciaException` cuando aplique). Descartar
  indicadores anteriores y no presentar el fallo como cero paneles. Mantener
  **visibles y navegables** los parques y nodos ya cargados y conservar sus
  estados habituales; no reemplazar el estado general del mapa por un error
  exclusivo del resumen de paneles.

#### Renderizado y CSS

Dentro del enlace de cada nodo, mantener el nombre y la ubicación, y agregar
una fila compacta con elementos `span` rectangulares azules. Para una cantidad
`cantidad`, dibujar `Math.Min(cantidad, 4)` rectángulos y, si supera cuatro,
mostrar `+{cantidad - 4}`. No agregar indicadores cuando la cantidad sea cero.

Los rectángulos y el excedente son contenido del mismo enlace, sin botones,
`tabindex` ni eventos propios. Cualquier clic en ellos continúa a `/admin/paneles`.
Marcar los dibujos como decorativos con `aria-hidden` e incorporar la cantidad
real al nombre o descripción accesible del nodo cuando esté disponible.

Usar clases específicas para la fila e indicadores, evitando que las reglas
existentes de `span`, `small` o `:last-child` cambien su disposición. Conservar
el ancho de las tarjetas, su ubicación en el mapa y su presentación general;
ajustar solo el espacio interno estrictamente necesario para una fila pequeña,
sin ocultar nombre ni ubicación. Comprobar solapamientos, recorte del tablero
y disposición móvil con cantidades altas de paneles. El indicador `+N` expresa
paneles adicionales; no es un control de alta.

## Módulo Usuarios y comunicación con otros módulos

No hay cambios en ninguna capa de Usuarios ni en Keycloak. Se conserva la
protección de la página y las autorizaciones actuales de consulta y escritura.
La representación se ofrece a los mismos usuarios autenticados que consultan
el mapa, sin introducir controles de registro nuevos.

No hay cambios ni comunicación nueva con Tareas, Mediciones o el simulador.
Las navegaciones solo abren páginas existentes; sus operaciones conservan
sus efectos actuales. No se agregan dependencias, bibliotecas ni componentes.
Si se detecta una limitación que exija cambios significativos fuera de
Presentación, documentarla antes de ampliar el alcance.

## Verificación y trazabilidad

| Requisito | Comprobación al implementar |
| --- | --- |
| RF-01 | Dos parques con ID diferentes y uno sin nodos: cada tarjeta abre el mismo destino que su enlace de tabla. |
| RF-02 | Nodos de distintos parques: todos abren exactamente `/admin/paneles`, sin contexto transferido. |
| RF-03 | Hover y foco muestran nodos; mouse, Enter y toque activan un único destino con foco visible. |
| RF-04 y RF-05 | Cantidades 0, 1, 4, 5 y 10: respectivamente 0, 1, 4, 4 con +1 y 4 con +6 rectángulos. Nombre y ubicación siguen visibles. |
| RF-06 | Registrar un panel en un nodo, volver normalmente al mapa y comprobar la asociación y cantidad persistidas. Verificar Atrás y documentar una posible limitación de recarga, sin agregar mecanismos nuevos de forma preventiva. |
| RF-07 | Solo cambian los dos archivos de Presentación, sin dependencias ni modificaciones de formularios o backend. |
| RF-08 | Tabla, menú, formularios, permisos y estados de carga/error/vacío conservados; navegar no escribe datos. |

Probar varios nodos con distintas cantidades simultáneamente para detectar
asociaciones cruzadas. En etapa 2 comprobar también fallo de carga de paneles,
retorno tras editar su asociación y regresión de la navegación de etapa 1.
Revisar ambos roles, ADMIN y usuario autenticado sin ADMIN, y tamaños de
escritorio y móvil. Estas son verificaciones de implementación, no tareas
ejecutadas al generar documentos.

Desde la raíz, ejecutar al implementar cada etapa:

```powershell
dotnet build SolarUy.sln
dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj --no-build
```

Localizar y ejecutar también las pruebas estructurales configuradas al momento
de implementar. Si no existen, revisar dependencias manualmente y declarar la
ausencia de verificación automática; no instalar herramientas ni crear proyectos.
No generar pruebas unitarias nuevas de Presentación, Aplicación o Infraestructura.
Seguir `Docs/ReadmeSonarQube.md` para SonarQube cuando esté disponible y corregir
los hallazgos introducidos sin relajar reglas. Documentar resultados reales,
fallos preexistentes, bloqueos y comprobaciones pendientes.
