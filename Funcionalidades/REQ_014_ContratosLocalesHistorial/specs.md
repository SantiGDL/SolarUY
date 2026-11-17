# REQ_014 — Especificaciones técnicas

## Criterio general

Aplicar los límites que implementa ARQ-01: cada módulo consume únicamente
interfaces de otro bajo Interfaz.Apis.Local.Contratos y DTOs alcanzables desde
sus firmas. La prueba no habilita enums internos aunque se incluyan en un DTO.
Los adaptadores Local.Implementaciones son Infraestructura según ARQ-03/04.
No mover interfaces internas para exponer servicios completos ni exponer TareaDto,
porque sus propiedades TipoTarea y Estado referencian enums de dominio.

Los contratos propuestos ya están implementados tras la aprobación del usuario.
No hay un contrato de Tareas existente que pueda reutilizarse para esos cruces:
ITareasService es interno. Los catálogos locales de Parques no ofrecen una
consulta puntual de existencia y requieren necesidades/permisos diferentes.
Reutilizar su convención de organización y las implementaciones actuales.

## Etapa 1 — Solución existente

### Mediciones

- Dominio: ConsultaHistorial y sus enums conservan las validaciones y ventanas.
- Aplicación: HistorialMedicionesService mantiene catálogo, autorización,
  consulta y exportación. OpcionHistorialDto y FilaHistorialDto se reutilizan.
- Contrato local: IHistorialMediciones expone opciones, consulta con nombres de
  período/modo y exportación; ResultadoHistorialLocalDto/ConsultaHistorialLocalDto
  transportan filas, selección y metadatos sin objetos de dominio.
- Infraestructura: HistorialMedicionesLocalService delega al servicio. Convierte
  nombres a enums internamente; para exportar reconstruye ConsultaHistorial con
  Hasta original y las filas originales, sin nueva consulta ni hora de descarga.
- Presentación: página de historial y PDF existentes sin reorganización.

### Consultas y composición

ConsultaNaturalService consume IHistorialMediciones; parser y solicitud usan
los enums propios de Consultas en FiltrosHistorial.cs. Program registra el
adaptador scoped y usa esos filtros en la herramienta de consultas existente.
Se adaptaron tipos en ConsultaNaturalServiceTests y se incorporó HistorialLocalTests.
La verificación anterior pasó 50 pruebas relacionadas y eliminó las 12 infracciones.

## Etapa 2 — Propuesta mínima

### Tareas: aplicación y contratos locales

Crear `Interfaz/Apis/Local/Contratos/ITareasLocales.cs` con dos operaciones:

- ObtenerTodasAsync(CancellationToken): devuelve IReadOnlyList<TareaConsultaDto>.
- CrearAutomaticaAsync(int panelId, string estadoPanel, CancellationToken): devuelve
  Task, sin DTO de resultado porque PanelService lo descarta actualmente.

TareaConsultaDto, en el mismo namespace contractual, conserva los ocho datos de
TareaDto: Id, PanelId, TipoTarea, FechaCreada, FechaFinalizado, Estado,
Observaciones y FechaRealizada. TipoTarea/Estado son nombres canónicos string
(Limpieza/Reparacion y Pendiente/Completada), no etiquetas traducidas ni enums.
Los tipos restantes conservan sus tipos CLR y nulabilidad actuales.

Crear `Interfaz/Apis/Local/Implementaciones/TareasLocalService.cs`, que implementa
ambas operaciones delegando a ITareasService. Lectura convierte TareaDto al DTO
contractual y mantiene su orden. La operación automática espera y descarta el
resultado interno; no captura ni oculta las excepciones actuales.
No exponer el CRUD manual de tareas a otros módulos.

Cambiar únicamente el parámetro estadoPanel de CrearAutomaticaAsync en
ITareasService y TareasService a string. PanelService envía estado.ToString().
Mantener en Tareas el switch Intermedio → Limpieza, Rotura → Reparacion y el caso
sin tarea para otros estados; conservar las observaciones y el orden de comprobación
actual (ADMIN, tipo, existencia, duplicado, alta). No transferir esa lógica a Parques.
TareaMantenimiento y enums de Tareas no requieren cambios de dominio.

Crear `Aplicacion/Contratos/IAccesoTareas.cs` con Task<bool> EsAdministradorAsync().
El UsuarioActual existente implementará también esta interfaz mediante el mismo
método que ya consulta AuthenticationStateProvider. Registrar ambas interfaces
scoped con la misma instancia scoped de UsuarioActual, sin referencia desde Tareas
al proveedor concreto, a componentes ni al contrato de Parques.

TareasService sustituye AccesoNoAutorizadoException de Parques por la excepción
estándar UnauthorizedAccessException, conservando el mensaje. No agregar una nueva
jerarquía de excepciones. Las capturas afectadas de Tareas.razor deben reconocer
ambos errores: todavía llama a servicios de Parques que lanzan la excepción antigua.
Revisar mensajes de Paneles y Consultas ante rechazo automático sin alterar sus
permisos; no capturar ni referenciar una excepción interna ajena desde el backend.

### Consultas

ConsultaNaturalService sustituye ITareasService por ITareasLocales y las firmas
ResponderTareas/FormatearListadoTareas/FormatearDetalleTarea usan TareaConsultaDto.
Retirar imports de contratos internos, DTOs internos y enums de Tareas. Conservar
comparaciones mediante nombres canónicos, textos actuales y filtro por PanelId.
El orden actual de EstadoTarea es Pendiente=0, Completada=1: conservar explícitamente
ese orden, no ordenar los nombres alfabéticamente. Reparacion sigue mostrándose
como Reparación; preservar fechas, conteos, vacíos y observaciones.
No cambiar parser de historial, proveedor IA ni flujos de generación/PDF.

### Parques: contrato puntual de existencia e integración automática

Crear `Interfaz/Apis/Local/Contratos/IConsultaPaneles.cs` con
Task<bool> ExisteAsync(int panelId, CancellationToken).
Crear `Interfaz/Apis/Local/Implementaciones/ConsultaPanelesService.cs` usando el
IDbContextFactory<ParquesDbContext> existente: crear/liberar contexto por operación
y ejecutar exactamente la comprobación AsNoTracking().AnyAsync(p => p.Id == panelId).
Esta implementación está clasificada como Infraestructura; Aplicación de Tareas
no depende de ella. No llamar a PanelService ni exigir un permiso distinto al
ADMIN que ya valida TareasService; este contrato solo devuelve existencia.
No usar SQL manual, GetType, reflexión, dynamic ni duplicar entidades de EF.

PanelService consume ITareasLocales para la creación automática y conserva sus
condiciones, orden y tratamiento de fallo parcial. No cambiar Panel, repositorios
de Parques, configuración EF o contratos de catálogos.

### Tareas: infraestructura y persistencia

TareaRepositorio recibe IConsultaPaneles además de sus dependencias actuales.
Su ExistePanelAsync conserva la firma interna de ITareaRepositorio y delega a
ExisteAsync dentro del try/catch actual. Retirar cualquier referencia a Panel,
context.Paneles o sus genéricos desde Tareas. No mover esta comprobación a otro
momento del caso de uso ni eliminarla para confiar solo en una clave foránea.

La implementación puntual de Parques deja propagar MySqlException, igual que la
consulta EF actual, para que TareaRepositorio conserve el log y la conversión a
TareasPersistenciaException sin referenciar excepciones internas de Parques.
La cancelación se propaga sin convertirla en error de persistencia.

ListarAsync, ObtenerPorIdAsync, ExisteActivaAsync, AgregarAsync y ActualizarAsync
conservan código y comportamiento. Se mantienen context.Tareas, FK restrictiva
TareasMantenimiento.PanelId y manejo del error 1452 al insertar. La consulta de
existencia usa un contexto distinto al de escritura, como ya ocurría: no se
introduce una transacción nueva ni se elimina la protección de la FK ante carreras.
ParquesDbContext contiene el mapeo compartido y queda intacto; es composición de
persistencia fuera de los módulos, no una nueva referencia directa desde Tareas.

### Presentación, DI y simulador

Program registra ITareasLocales/TareasLocalService e
IConsultaPaneles/ConsultaPanelesService como scoped, y enlaza IAccesoTareas con el
UsuarioActual ya registrado. No cambiar lifetimes de repositorios o contextos.

Grafo esperado: PanelService → ITareasLocales → TareasService → TareaRepositorio
→ IConsultaPaneles → factory de contexto. Consultas → ITareasLocales comparte ese
camino; IAccesoTareas termina en UsuarioActual. No hay retorno a PanelService y,
por tanto, no se introduce ciclo de construcción. Probar la resolución real de DI.
Las dependencias entre módulos son solo contractuales; no agregar eventos.

Tareas.razor solo ajusta capturas de autorización. PanelAdmin.razor y su consumo
interno de ITareasService no requieren cambios. Simulador, SignalR y Mediciones
no cambian en etapa 2.

## Matriz de trazabilidad

| Infracciones | Cambio previsto |
| --- | --- |
| Consultas → ITareasService/TareaDto/EstadoTarea (3) | ITareasLocales y TareaConsultaDto sin enums |
| PanelService → ITareasService/TareaDto (2) | Operación local automática sin resultado |
| ITareasService → EstadoPanel (1) | Estado contractual string |
| TareasService → IUsuarioActual/AccesoNoAutorizadoException/EstadoPanel (3) | IAccesoTareas, excepción estándar y estado string |
| TareaRepositorio → Panel (1) | IConsultaPaneles, misma consulta dentro de Parques |

## Archivos previstos para etapa 2

Nuevos, relativos a SolarUy/Backend/Modulos:

- Tareas/Interfaz/Apis/Local/Contratos/ITareasLocales.cs (incluye DTO).
- Tareas/Interfaz/Apis/Local/Implementaciones/TareasLocalService.cs.
- Tareas/Aplicacion/Contratos/IAccesoTareas.cs.
- Parques/Interfaz/Apis/Local/Contratos/IConsultaPaneles.cs.
- Parques/Interfaz/Apis/Local/Implementaciones/ConsultaPanelesService.cs.

Modificar:

- Tareas/Aplicacion/Contratos/ITareasService.cs.
- Tareas/Aplicacion/Implementaciones/TareasService.cs.
- Tareas/Infraestructura/Repositorios/TareaRepositorio.cs.
- Parques/Aplicacion/Implementaciones/PanelService.cs.
- Consultas/Aplicacion/Implementaciones/ConsultaNaturalService.cs.
- SolarUy/Components/Servicios/UsuarioActual.cs.
- SolarUy/Components/Pages/Tareas.razor y Paneles.razor (solo errores de permisos).
- SolarUy/Program.cs.
- SolarUyPruebas/Unitarias/Parques/TareasServiceTests.cs,
  PanelServiceTests.cs y Mediciones/ConsultaNaturalServiceTests.cs: adaptar tipos,
  dobles y excepción esperada sin retirar escenarios ni cambiar resultados.

No cambiar ITareaRepositorio, entidades, DbContext, migraciones, contratos de
arquitectura ni archivos de la etapa 1 salvo el consumidor Consultas y Program.

## Riesgos y verificación futura

- DTOs con enums internos volverían a fallar: revisar firmas y tipos anidados.
- Strings contractuales deben conservar nombres exactos y orden semántico.
- Capturas de autorización deben cubrir ambos módulos y mantener mensajes.
- No duplicar altas automáticas ni cambiar sus reglas por simplificar el contrato.
- La carrera entre comprobar panel y guardar tarea sigue protegida por la FK.
- La comprobación nueva no debe pasar por un servicio que dependa de Tareas.
- El paso de tests no sustituye prueba manual de persistencia, DI y permisos.

Comandos previstos, no ejecutados en esta planificación:

```powershell
dotnet build SolarUy.sln -c Release
dotnet test SolarUyPruebas/Arquitectura/SolarUyArquitecturaPruebas.csproj -c Release --no-build
dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj -c Release --no-build
```

Usar Release si la aplicación en Rider bloquea Debug. Verificar también registro
manual de tarea, panel inexistente, duplicados por tipo, cambio de estado, cierre,
ADMIN/no ADMIN, respuestas de Consultas y PDF del historial. Ejecutar SonarQube
según Docs/ReadmeSonarQube.md si están disponibles servicio y token; registrar el
bloqueo si falta alguno. El aviso NU1903 previo no se corrige dentro de este alcance.

## Estado

Etapas 1 y 2 implementadas. Paneles.razor también reconoce el rechazo estándar de Tareas para conservar el mensaje en creación automática; Consultas conserva su captura genérica existente. Compilación Release, 22 pruebas de arquitectura y 113 funcionales aprobadas. Las verificaciones manuales y SonarQube permanecen pendientes; ver tasks.md.