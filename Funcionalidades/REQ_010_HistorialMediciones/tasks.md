# REQ_010 — Plan de implementación del histórico

Basado en [requirements.md](requirements.md) y [specs.md](specs.md).
Todas las tareas están pendientes. Este plan no inicia la implementación.
Las decisiones abiertas condicionan únicamente las tareas que dependen de ellas.

- [ ] **T01 — Requisitos / reglas de resumen.** Acordar promedio o muestra diaria,
  límites y origen de ventanas, zona horaria, promedio por panel o conjunto,
  selección Todos y tratamiento de simulaciones simultáneas. **Finalización:**
  actualizar requirements y specs con reglas explícitas y ejemplos numéricos;
  no sustituir estas decisiones por supuestos técnicos.
- [ ] **T02 — Usuarios y Parques / acceso y catálogo.** Definir permisos de
  consulta/exportación y catálogo de selección, incluidos deshabilitados y
  registros cuyos identificadores no tengan entidad actual. **Finalización:**
  documentar política y comportamiento sin ocultar historial ni introducir
  referencias entre entidades de módulos. **Depende de:** T01 para opciones Todos.
- [ ] **T03 — Mediciones / inventario técnico.** Revisar código vigente,
  repositorios, índices, pruebas, mecanismos de descarga y puntos compartidos
  con Tareas. **Finalización:** identificar reutilización y cambios concretos;
  confirmar rutas reales y coordinar menú y composición.
- [ ] **T04 — Mediciones / Dominio.** Definir criterios de lectura independientes
  de persistencia y reglas de períodos y resumen acordadas. Ampliar el contrato
  de repositorio sin cambiar escritura. **Finalización:** tipos sin EF, HTTP ni
  dependencias de Aplicación; W, vacíos y límites coherentes. **Depende de:** T01, T03.
- [ ] **T05 — Mediciones / pruebas de Dominio.** Agregar o actualizar xUnit para
  comportamiento de Dominio incorporado: límites temporales, ventanas sin
  solapamiento, promedios, entradas inválidas y ausencia de datos según reglas
  acordadas. **Finalización:** casos significativos pasan en el proyecto existente;
  no agregar pruebas triviales ni unitarias de otras capas. **Depende de:** T04.
- [ ] **T06 — Parques / contrato local de lectura.** Reutilizar o completar las
  consultas de catálogo necesarias sin acoplar histórico al control del motor.
  **Finalización:** asociaciones y opciones válidas, datos deshabilitados e
  identificadores históricos tratados según T02; ningún DbContext cruza módulos.
  **Depende de:** T02, T03.
- [ ] **T07 — Mediciones / Infraestructura, consulta cruda.** Implementar filtros
  por selección y rango en MySQL, lectura sin seguimiento, orden determinista,
  contexto por operación y cancelación. **Finalización:** comparar resultados
  con filas conocidas y límites; sin cargar toda la tabla ni truncar resultados.
  **Depende de:** T04.
- [ ] **T08 — Mediciones / Infraestructura, resúmenes.** Implementar promedio y
  diario de acuerdo con T01, delegando a BD las agregaciones traducibles.
  **Finalización:** validar SQL y resultados con MySQL, ventanas vacías y datos
  parciales; no confundir promedio de filas con potencia total ni calcular kWh.
  **Depende de:** T01, T04, T07.
- [ ] **T09 — Mediciones / Aplicación.** Implementar servicio de histórico, DTO,
  autorización y validación de selección. Capturar una referencia UTC por
  consulta y devolver instantánea completa con contexto. **Finalización:** crudos,
  promedio y diario utilizables sin Sensor; vacío y error distinguibles;
  entidades persistentes no expuestas a UI. **Depende de:** T02, T06–T08.
- [ ] **T10 — Mediciones / exportación, decisión técnica.** Seleccionar mecanismo
  PDF compatible, revisar licencia y forma de descarga, y documentar la decisión.
  Dimensionar instantánea para modo crudo sin límites silenciosos.
  **Finalización:** solución concreta que conserve exactamente los resultados
  consultados, sin introducir infraestructura compartida innecesaria.
  **Depende de:** T03, T09.
- [ ] **T11 — Mediciones / Aplicación e Infraestructura, PDF.** Implementar contrato
  de exportación y adaptador, revalidar acceso y generar documento desde la
  instantánea, sin repetir la consulta. **Finalización:** archivo válido con
  selección, período, modo, unidades, detalle completo, cabeceras y paginación;
  biblioteca PDF confinada a Infraestructura. **Depende de:** T02, T10.
- [ ] **T12 — Mediciones / Presentación, formulario.** Crear página Interactive
  Server con layout existente, selectores dependientes, períodos, modos y
  Consultar. **Finalización:** selección coherente, permisos acordados, estados
  de carga/vacío/error y cancelación; sin motor WASM ni acceso a repositorios.
  **Depende de:** T06, T09.
- [ ] **T13 — Mediciones / Presentación, resultados y descarga.** Mostrar tabla
  según modo, filtros ejecutados y aviso de datos disponibles; separar controles
  pendientes y resultado confirmado. Conectar descarga PDF. **Finalización:**
  cambiar controles o recibir nuevas mediciones no altera el resultado exportado;
  paginación visual no recorta exportación y respuestas antiguas no pisan la
  consulta vigente. **Depende de:** T11, T12.
- [ ] **T14 — Composición y navegación.** Registrar dependencias y enlace al
  histórico coordinando los archivos compartidos con Tareas. **Finalización:**
  acceso directo y desde menú; Producción y Tareas conservan su comportamiento.
  **Depende de:** T11–T13.
- [ ] **T15 — Verificación funcional y de persistencia.** Recorrer combinaciones
  Parque/Nodo/Panel, cuatro períodos y tres modos, límites exactos, vacíos,
  deshabilitados, identificadores antiguos, errores y cancelación. Verificar
  que consultar/exportar no inserta ni modifica mediciones ni inicia Sensor.
  **Finalización:** evidencia de resultados y permisos de T02, con datos parciales
  y nuevas inserciones posteriores a una consulta. **Depende de:** T14.
- [ ] **T16 — Verificación de PDF y pantalla.** Comparar cada modo con su PDF,
  incluyendo crudos de varias páginas, acentos, nombres largos y filtros cambiados
  después de consultar. Revisar escritorio/móvil y renderizar el PDF para detectar
  recortes. **Finalización:** mismas filas y valores que la instantánea, unidades
  legibles y todas las páginas completas. **Depende de:** T13, T15.
- [ ] **T17 — Calidad y regresión.** Ejecutar `dotnet build SolarUy.sln` y
  `dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj`.
  Revisar límites de capas y ejecutar pruebas estructurales si existen al
  implementar. Seguir `Docs/ReadmeSonarQube.md` para el análisis configurado.
  **Finalización:** corregir fallos introducidos y registrar resultados reales
  o bloqueos; no instalar herramientas ni relajar reglas para superar controles.
  Aplicar `.agents/Reglas/6_flujo_trabajo_ia.md`, archivo real vigente del flujo.
  **Depende de:** T05, T15, T16.
- [ ] **T18 — Documentación de cierre.** Registrar evidencias, decisiones y
  pendientes; actualizar README cuando la funcionalidad esté implementada y
  verificada. **Finalización:** documentación coherente con comportamiento real,
  sin presentar planificación como implementación y sin modificar reglas.
  **Depende de:** T17.
