# REQ_014 — Corrección de contratos entre módulos

## Objetivo y restricciones

Resolver los incumplimientos de ARQ-01 en dos etapas, conservando Historial,
Consultas, Tareas y Parques para la entrega del 18 de noviembre de 2026.
Seguir requirements → specs → tasks → implementación. AGENTS refiere a
`.agents/rules/`; las reglas reales consultadas están en `.agents/Reglas/`.

No agregar funcionalidades, eventos, API HTTP, capas, paquetes ni migraciones.
No cambiar el esquema, las claves foráneas, los datos ni reglas de ArchUnitNET.
Reutilizar servicios y patrones locales existentes; no refactorizar áreas ajenas.

## Etapa 1 — Historial, implementada

Eliminar las 12 dependencias Consultas → Mediciones mediante IHistorialMediciones,
sus DTOs y un adaptador al servicio actual. Mantener autorización, errores,
filtros, filas y exportación con la fecha original sin volver a consultar.

Evidencia de la ejecución anterior: solución compilada en Release, 50 pruebas
relacionadas aprobadas, arquitectura 21/22 aprobadas y ARQ-01 reducido de 22 a
10 dependencias. Estos son resultados previos, no nuevas verificaciones de esta
planificación. La página Blazor y la persistencia del historial se conservaron.

## Etapa 2 — Tareas y Parques, implementada; revisión manual pendiente

### Dependencias a eliminar

| Origen | Destinos prohibidos | Cantidad |
| --- | --- | --- |
| Consultas.ConsultaNaturalService | Tareas.ITareasService, TareaDto, EstadoTarea | 3 |
| Parques.PanelService | Tareas.ITareasService, TareaDto | 2 |
| Tareas.ITareasService | Parques.EstadoPanel | 1 |
| Tareas.TareasService | Parques.IUsuarioActual, AccesoNoAutorizadoException, EstadoPanel | 3 |
| Tareas.TareaRepositorio | Parques.Panel | 1 |

### Requisitos

- R2.1: Consultas consume una operación local de lectura de tareas con DTOs sin
  enums ni entidades de dominio. Mantener respuestas, filtros, orden, fechas,
  tipos de mantenimiento y observaciones actuales.
- R2.2: Parques solicita mantenimiento automático mediante una operación local
  acotada, sin obtener ni referenciar TareaDto o ITareasService. Conservar la
  decisión actual de cuándo solicitarlo, después de guardar el panel.
- R2.3: Tareas interpreta el estado recibido sin depender de EstadoPanel de
  Parques: Intermedio genera limpieza, Rotura reparación, Bueno no genera tarea.
  Mantener no duplicación de tarea activa del mismo tipo, permisos, observaciones
  y mensajes de fallo parcial. No ampliar ni endurecer reglas de negocio.
- R2.4: Tareas exige ADMIN usando un contrato propio implementado por el proveedor
  de usuario ya existente. No trasladar servicios de otros módulos ni duplicar
  la lectura de autenticación. El rechazo conserva mensaje y tratamiento visual.
- R2.5: TareaRepositorio deja de consultar la entidad Panel ajena. Delegar la
  comprobación de existencia a un contrato local de Parques que solo devuelve
  bool, conservando el contrato interno ExistePanelAsync, sus llamadas y errores.
  No cambiar escrituras, consultas de tareas, relaciones, contexto ni migraciones.
- R2.6: Evitar ciclos de DI. La comprobación de existencia no debe usar PanelService
  ni disparar mantenimiento; debe ser una consulta independiente de solo lectura.
- R2.7: No exponer tipos internos de forma indirecta en propiedades de DTOs,
  interfaces heredadas, parámetros o resultados. Mantener etapa 1 sin regresiones.

## Criterios de aceptación finales

- ARQ-01 con cero dependencias prohibidas y las 22 pruebas actuales de arquitectura
  aprobadas, sin cambios en sus reglas, selectores o exclusiones.
- Solución compilada y pruebas funcionales existentes aprobadas, adaptando tipos
  y dobles afectados sin eliminar casos ni alterar expectativas funcionales.
- Verificación manual de tareas manuales/automáticas, duplicados, permisos,
  panel inexistente, fallo parcial, consultas de tareas e historial y PDF.
- Persistencia: misma base y claves foráneas, mismo manejo del error MySQL 1452;
  conservar protección ante una eliminación concurrente de un panel.
- Registrar resultados, avisos preexistentes y bloqueos, incluido SonarQube cuando
  no pueda ejecutarse. No afirmar ausencia de regresiones solo por compilar.

## Estado y autorización

Etapas 1 y 2 implementadas tras aprobación explícita. Verificaciones automáticas de etapa 2 aprobadas; revisión manual y SonarQube pendientes. No se realizaron commit ni push. Resultados y limitaciones en tasks.md.