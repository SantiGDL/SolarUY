# Diseño: gestión de tareas de mantenimiento

## Modelo y fechas

- `TareaMantenimiento` pertenece a un panel mediante `PanelId`.
- `TipoMantenimiento` usa `Limpieza` y `Reparacion`.
- `EstadoTarea` usa `Pendiente` y `Completada`.
- `FechaCreada` se asigna al crear la tarea.
- `FechaRealizada` y `FechaFinalizado` permanecen nulas mientras no esté
  completada; al completar, ambas se registran en UTC.
- Las observaciones son opcionales, con un máximo de 1000 caracteres.

## Flujos

1. El administrador crea una tarea manual o el guardado de un panel genera una
   tarea automática: `Intermedio` → `Limpieza`, `Rotura` → `Reparacion`.
2. La generación automática solo actúa al registrar un panel en estado de
   mantenimiento o cuando su estado cambia a uno no saludable; una tarea activa
   del mismo panel y tipo evita duplicaciones.
3. Un administrador completa cualquier tarea pendiente ingresando observaciones.
   El servidor exige el rol `ADMIN`.

## Persistencia e interfaces

- La tabla `TareasMantenimiento` se integra en `ParquesDbContext` para mantener
  la FK real a `Paneles`; el borrado del panel queda restringido.
- `TareasService` aplica autorización y reglas de flujo.
- `TareaRepositorio` encapsula EF Core/MySQL.
- `/admin/tareas` está restringida a administradores. El dashboard muestra el
  total de tareas no completadas.
- La pantalla de mantenimiento requiere seleccionar primero un parque y
  limita nodos, paneles, métricas y tareas a ese parque. Incluye una vista
  esquemática de nodos y paneles por estado; no representa coordenadas
  geográficas reales.
- No se agrega ni se requiere un rol `TECNICO` en Keycloak.
