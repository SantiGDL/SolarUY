# Requisitos: gestión de tareas de mantenimiento

## Objetivo

Registrar y completar tareas de mantenimiento vinculadas a los paneles solares.

## Alcance

- Solo los administradores pueden consultar, crear y completar tareas.
- El administrador puede crear tareas manualmente para un panel existente.
- Para gestionar tareas, el administrador selecciona primero un parque; los
  nodos, paneles y tareas mostrados corresponden únicamente a ese parque.
- La pantalla ofrece un mapa esquemático de los nodos y el estado de sus
  paneles, sin presentarlo como un mapa geográfico.
- El sistema genera una tarea cuando se registra un panel en estado `Intermedio`
  o `Rotura`, o cuando su estado cambia a uno de esos valores.
- No se asignan tareas a usuarios ni se requiere un rol de técnico.
- El estado de tarea sigue `Pendiente` → `Completada`.
- `Intermedio` genera `Limpieza`; `Rotura` genera `Reparacion`.
- Las tareas se guardan en MySQL y mantienen una FK al panel.

La aplicación no administra cuentas de Keycloak. La baja/eliminación y
re-apertura de tareas quedan fuera del alcance.

## Criterios de aceptación

1. Un `ADMIN` crea y consulta tareas de paneles existentes.
2. Una transición del estado de un panel a `Intermedio` o `Rotura` genera una
   tarea del tipo correspondiente, sin duplicar la misma tarea activa.
3. Un `ADMIN` completa una tarea pendiente y se guardan las fechas de trabajo y
   cierre.
4. La autorización de consulta, creación y finalización se aplica en el
   servidor, no solo en UI.
5. Al cambiar de parque, no se muestran paneles ni tareas pertenecientes a otros
   parques; el nodo y el panel se eligen en ese orden al crear una tarea.
6. El esquema y los datos persisten con EF Core/MySQL.
