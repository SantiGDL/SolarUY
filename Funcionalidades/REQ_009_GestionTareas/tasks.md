# Tareas: gestión de tareas de mantenimiento

- [x] Definir modelo, estado, permisos y creación manual/automática.
- [x] Implementar entidad, DTOs y casos de uso.
- [x] Implementar persistencia MySQL y FK a panel.
- [x] Generar tareas desde creación/cambio de estado de panel.
- [x] Implementar finalización de tareas por ADMIN.
- [x] Implementar pantalla de tareas, navegación y contador del dashboard.
- [x] Mejorar pantalla con selección por parque, vista esquemática de nodos y
  filtrado de tareas por parque.
- [x] Agregar pruebas unitarias de dominio, reglas, permisos y flujo automático.
- [ ] Aplicar migración y validar el flujo interactivo con un usuario `ADMIN`.

## Decisiones de esta versión

- Intermedio crea `Limpieza`; Rotura crea `Reparacion`.
- El sistema evita duplicar una tarea automática activa del mismo tipo y panel.
- Solo ADMIN consulta, crea y completa tareas; no hay asignación a técnicos.
- La aplicación no crea usuarios ni contraseñas y no cambia automáticamente el
  estado del panel cuando se completa una tarea.
