# REQ_014 — Tareas y estado

## Etapa 1 — Implementada y verificada previamente

Las marcas completas reflejan trabajo anterior, no ejecución de esta planificación.

- [x] E1-01 — Mediciones, contrato/infraestructura: IHistorialMediciones y DTOs locales, adaptador al servicio actual. Finalización: no exponer dominio del historial.
- [x] E1-02 — Consultas, aplicación: solicitud, filtros y consumidores del contrato; Program, DI y herramienta existente. Finalización: conservar comportamiento.
- [x] E1-03 — Verificación anterior: solución compila, 50 pruebas de Historial/Consultas pasan y arquitectura 21/22; ARQ-01 baja de 22 a 10 dependencias. No se ejecutó SonarQube por falta de token.

## Planificación actual

- [x] P-01 — Revisar código y reglas; actualizar requirements.md en dos etapas, redactar specs.md y tasks.md. Solo documentación, sin implementar etapa 2.

## Etapa 2 — Implementación y verificación automática realizadas

El usuario aprobó explícitamente los documentos. Las tareas manuales y SonarQube siguen pendientes.

- [x] E2-00 — Confirmar aprobación y estado del checkout. Releer reglas y documentos aprobados; preservar los cambios existentes de etapa 1. Finalización: alcance confirmado sin iniciar correcciones ajenas.
- [x] E2-01 — Tareas, contrato/aplicación. Crear ITareasLocales y TareaConsultaDto con strings canónicos, e IAccesoTareas; cambiar solo el parámetro automático de ITareasService/TareasService a string. Depende E2-00. Finalización: no referencias de Tareas a EstadoPanel, mismas decisiones, observaciones, duplicados y autorización.
- [x] E2-02 — Tareas, infraestructura local. Crear TareasLocalService que delega lectura/alta automática al servicio existente y convierte el DTO, sin exponer TareaDto ni enums. Depende E2-01. Finalización: salida equivalente, operación automática Task sin resultado, mismos errores.
- [x] E2-03 — Parques, contrato/infraestructura. Crear IConsultaPaneles y ConsultaPanelesService con factory existente y consulta AnyAsync equivalente. Depende E2-00. Finalización: bool, sin PanelService, sin escritura, sin permiso nuevo y con cancelación/errores MySQL conservados.
- [x] E2-04 — Tareas, repositorio. Delegar ExistePanelAsync en IConsultaPaneles conservando ITareaRepositorio y try/catch actual. Depende E2-03. Finalización: ninguna referencia a entidad Panel desde Tareas; escrituras, FK y manejo de 1452 intactos; sin migraciones.
- [x] E2-05 — Autorización, aplicación/presentación/DI. UsuarioActual implementa IAccesoTareas; TareasService usa excepción estándar con mismo mensaje; Tareas.razor reconoce los errores nuevos y los de Parques. Depende E2-01. Finalización: ADMIN/no ADMIN se conserva, sin dependencia de Tareas hacia componentes o contratos internos ajenos.
- [x] E2-06 — Consumidores, aplicación. PanelService usa alta automática local con estado.ToString(); ConsultaNaturalService usa lectura y DTO local, manteniendo filtros, textos y orden Pendiente/Completada. Depende E2-02. Finalización: eliminadas las cinco referencias consumidoras prohibidas sin tocar catálogo, historial ni simulador.
- [x] E2-07 — Composición. Registrar contratos locales scoped y reutilizar la instancia scoped de UsuarioActual para ambos contratos. Depende E2-02 a E2-06. Finalización: resolver consumidores sin ciclo de DI ni cambio de lifetimes.
- [x] E2-08 — Pruebas existentes. Adaptar dobles/tipos de TareasServiceTests, PanelServiceTests y ConsultaNaturalServiceTests al contrato, estado string y rechazo estándar; conservar casos y expectativas funcionales. Depende E2-07. Finalización: pruebas compilan sin retirar ni omitir escenarios. No crear por iniciativa propia nuevas suites unitarias de Aplicación, Infraestructura o Presentación. Si excepcionalmente se modifica comportamiento de Dominio, detener el desvío de alcance y cumplir las pruebas xUnit de Dominio exigidas por las reglas.
- [x] E2-09 — Compilar y ejecutar verificaciones. Depende E2-08. Ejecutar los comandos de specs.md sobre solución, arquitectura y proyecto funcional completo. Finalización: compilación correcta, las 22 pruebas de arquitectura pasan, ARQ-01 sin infracciones y pruebas funcionales aprobadas; conservar también las 50 de etapa 1. No cambiar reglas/selectores de arquitectura para obtener ese resultado.
- [ ] E2-10 — Comprobar integración manual. Depende E2-09. Verificar tareas manuales y automáticas (Intermedio/Rotura/Bueno), duplicados, cierre, panel inexistente, permisos, fallo parcial, consultas de tareas y PDF/historial con resultado original. Revisar persistencia y resolución DI. Finalización: registrar resultados o bloqueos sin afirmar comprobaciones no realizadas; no probar borrados concurrentes destruyendo datos de trabajo.
- [ ] E2-11 — SonarQube y entrega. Depende E2-09 y E2-10. Seguir la guía existente si hay servicio/token; registrar bloqueos y avisos preexistentes. Revisar diff para excluir esquema, reglas y áreas ajenas; listar archivos y resultados, actualizar el estado de estos documentos. Finalización: evidencia revisable de aceptación o pendientes explícitos; no marcar etapa completa si hay verificaciones requeridas pendientes.

## Estado actual

La etapa 2 está implementada y las verificaciones automáticas pasaron. E2-10 sigue pendiente (pruebas manuales con sesión y base real). E2-11 es parcial: diff y documentación revisados; SonarQube bloqueado por falta de token. No se acredita aceptación manual completa ni Quality Gate.
## Resultados de etapa 2 — 17 de noviembre de 2026

- `dotnet build SolarUy.sln -c Release --no-restore`: correcto, cero errores.
- `dotnet test SolarUyPruebas/Arquitectura/SolarUyArquitecturaPruebas.csproj -c Release --no-build`: 22/22 aprobadas, cero omitidas; ARQ-01 sin dependencias prohibidas.
- `dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj -c Release --no-build`: 113/113 aprobadas, cero omitidas. Incluye Historial y Consultas.
- Verificación temporal fuera del repositorio usando los registros del Program actual y una conexión ficticia: consumidores resueltos, ValidateScopes/ValidateOnBuild activos, sin ciclos y misma instancia scoped para IUsuarioActual/IAccesoTareas. No se inició el host, no se consultó DB ni se ejecutaron migraciones.
- Revisado el diff: enums y sus valores intactos (Bueno=0, Intermedio=1, Rotura=2; Pendiente=0, Completada=1; Limpieza=0, Reparacion=1), condiciones automáticas y prevención de duplicados conservadas. Las escrituras de TareaRepositorio y el manejo MySQL 1452 no cambiaron. No se modificaron DbContext, entidades, migraciones ni pruebas/reglas de arquitectura.
- Cambios de etapa 2: los cinco archivos nuevos previstos en specs.md; TareasService, ITareasService, TareaRepositorio, PanelService, ConsultaNaturalService, UsuarioActual y Program; capturas de autorización en Tareas.razor y Paneles.razor; adaptación de tres archivos de pruebas existentes y actualización de estos documentos.
- Pruebas manuales no realizadas: navegación con ADMIN/no ADMIN, altas/cierre y duplicados en base real, fallo parcial, error 1452 concurrente y descarga PDF desde navegador. No hay una sesión de navegador administrable disponible; no se inició la aplicación ni se modificó la base para simular estos casos. No se acredita persistencia real ni revisión visual por las pruebas unitarias.
- SonarQube no ejecutado: faltan credenciales/token en esta sesión. Persiste NU1903 preexistente para Microsoft.AspNetCore.Components.WebAssembly.Server 10.0.13. VSTest también avisa sobre VSTEST_ARTIFACTS_DIRECTORY; las pruebas finalizaron con código 0. La comprobación temporal DI mostró NU1900 al consultar el índice de vulnerabilidades, sin impedir su ejecución.
- Sin commit ni push. Pendiente revisión del usuario.
