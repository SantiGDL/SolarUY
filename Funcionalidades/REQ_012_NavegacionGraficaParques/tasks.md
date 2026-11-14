# REQ_012 — Tareas de navegación y representación gráfica

Plan basado en `requirements.md` y `specs.md` actualizados. El estado de cada tarea se indica a continuación. La etapa 1 se entrega independientemente de la etapa 2.
Los identificadores de este plan corresponden al alcance actual.

**Principio de implementación:** cambios mínimos, sin refactorizaciones ni
limpiezas preventivas. Preservar el diseño del mapa, la posición de los nodos
y los comportamientos que no interfieran con el nuevo requisito.

## Etapa 1 — Navegación de parques y nodos

- [x] **NG-01 — Parques / Presentación (RF-01, RF-08).** Buscar referencias a
  `AlternarParque`, `parqueSeleccionadoId` e `is-selected` antes de modificar
  la selección. Sustituir el botón principal de `Parques.razor` por un enlace
  relativo a `/admin/parques/{parque.Id}/nodos`. Conservar estados y métodos
  que no interfieran; retirar únicamente código cuya eliminación sea
  indispensable y cuyas dependencias se hayan comprobado. No hacer limpieza
  ni refactorización adicional.
  **Finalización:** cada parque, incluso sin nodos, navega usando su ID real
  al mismo destino que la tabla, sin crear datos. **Dependencias:** ninguna.
- [x] **NG-02 — Parques / Presentación (RF-02).** Cambiar el destino de todos
  los nodos gráficos a `/admin/paneles`, manteniendo sus enlaces separados del
  enlace del parque y sin navegación en contenedores compartidos.
  **Finalización:** cada nodo abre ese destino exacto, sin parámetros,
  preselección ni modificación de `Nodos.razor` o `Paneles.razor`, y no dispara
  la navegación del parque. **Dependencias:** NG-01.
- [ ] **NG-03 — Parques / Presentación (RF-03, RF-07).** Actualizar ayuda y
  nombres accesibles para ambos destinos; adaptar solo el CSS indispensable,
  conservando diseño, posición de nodos, hover, foco, disposición móvil y
  movimiento reducido.
  **Finalización:** los nodos se muestran con hover/foco, el foco es visible
  y mouse, Enter y toque navegan sin interferencias ni zonas bloqueadas.
  **Dependencias:** NG-02.
- [ ] **NG-04 — Parques y Usuarios / Verificación funcional (RF-01 a RF-03,
  RF-08).** Probar dos parques con distintos ID, con y sin nodos, y nodos de
  distintos parques, con ADMIN y usuario autenticado sin ADMIN. Revisar
  escritorio/móvil, tabla, menú, formularios y estados de carga/error/vacío.
  **Finalización:** se cumplen los criterios de aceptación de etapa 1,
  sin escrituras por navegar ni cambios de permisos. Documentar resultados
  y comprobaciones pendientes. **Dependencias:** NG-03.
- [ ] **NG-05 — Calidad / Verificación técnica de etapa 1.** Ejecutar
  `dotnet build SolarUy.sln` y
  `dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj --no-build`.
  Localizar y ejecutar pruebas estructurales si existen; de lo contrario,
  revisar dependencias manualmente y declarar la limitación. Ejecutar
  SonarQube según `Docs/ReadmeSonarQube.md` si está disponible.
  **Finalización:** corregir fallos introducidos y documentar resultados,
  bloqueos y verificaciones pendientes. No cambiar controles de calidad ni
  agregar pruebas unitarias de otras capas; no hay cambios de Dominio que
  requieran nuevas pruebas de comportamiento. **Dependencias:** NG-04.

## Etapa 2 — Representación de paneles

- [x] **NG-06 — Parques / Presentación (RF-05 a RF-07).** Inyectar
  `IPanelesService` en `Parques.razor` y ampliar `CargarAsync` con una consulta
  secuencial a `ObtenerTodosAsync()`. Agrupar por `PanelDto.NodoId` en un
  diccionario de cantidades que se reemplace en cada carga exitosa. Tratar la
  consulta como complementaria: su resultado no debe condicionar la carga ni
  la navegación de parques y nodos.
  **Finalización:** cada nodo obtiene su cantidad real, sin consultas por
  tarjeta, datos simulados, cambios de contratos ni acceso a persistencia.
  **Dependencias:** etapa 1 terminada.
- [ ] **NG-07 — Parques / Presentación (RF-06, RF-08).** Tratar fallos de carga
  de paneles con registro del error y aviso al usuario, descartando indicadores
  anteriores, pero **sin bloquear ni sustituir el mapa existente**. Verificar
  que una entrada normal al mapa después de registrar un panel muestre los
  datos persistidos. Comprobar también Atrás del navegador y documentar si
  conserva datos antiguos; no agregar mecanismos nuevos de recarga o
  sincronización de manera preventiva. Solo considerar un ajuste mínimo al
  ciclo de carga si resulta estrictamente necesario y seguro.
  **Finalización:** el mapa sigue usable si falla la consulta; el resumen se
  indica como no disponible y no como cero paneles. El retorno normal muestra
  datos actualizados y cualquier limitación de Atrás queda documentada.
  No modificar formularios. **Dependencias:** NG-06.
- [x] **NG-08 — Parques / Presentación (RF-04, RF-05).** Dibujar dentro del
  enlace de cada nodo una fila de hasta cuatro rectángulos azules y el excedente
  `+N`, con elementos no interactivos y clases CSS específicas. No rediseñar el
  mapa ni cambiar la posición de los nodos; ajustar solo el espacio interno
  mínimo imprescindible.
  **Finalización:** cero paneles no muestra indicadores; cinco muestra cuatro
  rectángulos y +1; diez muestra cuatro y +6. Se mantienen nombre, ubicación,
  ancho y disposición general, con una descripción accesible de la cantidad
  y sin nuevos controles de foco. **Dependencias:** NG-06.
- [ ] **NG-09 — Parques / Verificación funcional y visual (RF-04 a RF-08).**
  Comprobar nodos de distintos parques con 0, 1, 4, 5 y 10 paneles. Verificar
  asociación real, clic sobre rectángulos y excedente, alta y retorno normal
  al mapa, comportamiento de Atrás, fallo de consulta y ausencia de regresiones
  de etapa 1. Revisar ambos roles, escritorio y móvil, sin solapamientos,
  recortes ni desplazamientos nuevos de los nodos.
  **Finalización:** se cumplen los criterios de etapa 2 y la matriz de
  `specs.md`; todos los puntos del nodo abren `/admin/paneles` sin parámetros.
  Documentar cualquier limitación de recarga no resuelta.
  **Dependencias:** NG-07 y NG-08.
- [ ] **NG-10 — Calidad / Verificación técnica y entrega de etapa 2.** Repetir
  build, pruebas existentes aplicables, revisión de dependencias y SonarQube
  según NG-05 sobre el conjunto de cambios. Revisar que solo se modificaron
  `Parques.razor` y `Parques.razor.css` y que no cambiaron procedimientos de
  registro, edición o eliminación. Si surge una limitación que exige cambios
  significativos fuera de Presentación, documentarla antes de ampliar el alcance.
  **Finalización:** fallos introducidos corregidos y resultados reales,
  bloqueos y verificaciones pendientes informados. **Dependencias:** NG-09.


## Registro de implementación y verificación — 2026-11-14

- Implementadas ambas etapas en `Parques.razor` y `Parques.razor.css`.
  Los estados y métodos de selección existentes se conservaron; el clic del
  enlace principal ya no los invoca. El mensaje de nodo vacío no intercepta clics.
- NG-03: ayuda, nombres accesibles y CSS implementados. Falta comprobar la
  interacción real con mouse, teclado y toque; la tarea queda pendiente.
- NG-07: carga complementaria, agrupación por NodoId y tratamiento de errores
  implementados. El mapa se renderiza antes de esperar el resumen. Un fallo
  informa la indisponibilidad sin reemplazar el mapa. Falta comprobar retorno
  tras alta y Atrás del navegador; no se agregó recarga preventiva.
- Etapa 1 y conjunto final: compilación y 112 pruebas existentes correctas
  en cada ejecución (0 fallos, 0 omitidas). Se usó salida temporal mediante
  `--artifacts-path "$env:TEMP/SolarUy-REQ012"`, `-m:1` y
  `-p:UseSharedCompilation=false`, porque el ejecutable habitual estaba
  bloqueado por la instancia abierta y el sandbox impedía iniciar el host
  de tareas WebAssembly. No se detuvo la instancia del usuario.
- Advertencia preexistente NU1903 en el paquete WebAssembly.Server 10.0.13.
  No se modificaron paquetes ni se suprimieron advertencias.
- Sin pruebas estructurales/ArchUnitNET configuradas en el proyecto de pruebas
  inspeccionado. Revisión manual: la página solo consume contratos de Aplicación;
  no se cambiaron entidades, contratos, persistencia, permisos ni formularios.
- Se inició y detuvo una instancia temporal en localhost:5059 para intentar
  la comprobación visual. No se aplicaron migraciones: ambas bases de historial
  estaban actualizadas. La herramienta de control de aplicaciones detuvo
  la revisión porque no pudo determinar con confianza la URL del navegador.
- NG-04 y NG-09 pendientes: matriz visual/funcional de parques, cantidades
  0/1/4/5/10, permisos por sesión, navegación, retorno, toque y fallo de carga.
  La revisión de código no sustituye estas comprobaciones en navegador.
- NG-05 y NG-10 parcialmente verificadas: build y pruebas correctas;
  SonarQube iniciado, pero análisis pendiente por falta de token configurado.
  No se generaron credenciales ni se alteraron controles de calidad.
