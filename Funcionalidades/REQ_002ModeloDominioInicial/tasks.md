# Tareas: clases iniciales del dominio

Implementación iniciada. Mediciones y Tareas esperan la confirmación de los detalles de T01.

- [ ] T01 — Diseño: confirmar los detalles pendientes de [specs.md](specs.md): tipo del estado de medición, observaciones, fechas opcionales y relaciones de la tarea con usuarios. Finaliza cuando el diseño indique sus tipos y cardinalidades.
- [x] T02 — Usuarios / Dominio: crear Usuario, UsuarioTecnico, UsuarioAdmin y sus enumerados. Definir Imagen como `string` para guardar una ruta relativa a `wwwroot` y conservar TipoImagenAceptada para la futura validación de formatos, sin implementar carga ni validación de archivos. Finaliza cuando propiedades, herencia y nombres coincidan con el diseño.
- [x] T03 — Parques / Dominio: crear Parque, Nodo, Panel y EstadoPanel. Finaliza cuando sus propiedades y comentarios de relación estén completos.
- [ ] T04 — Mediciones / Dominio: crear Medicion con los tipos acordados y PanelId. Finaliza cuando incluya `//Relacion con Panel (Muchos a 1)` y no dependa de entidades de Parques.
- [ ] T05 — Tareas / Dominio: crear TareaMantenimiento y TipoMantenimiento, con propiedades y relaciones acordadas. Finaliza cuando todos sus comentarios respeten el formato solicitado.
- [ ] T06 — Todos los módulos / Dominio: comparar los archivos con el diagrama y revisar manualmente nombres, tipos, relaciones y dependencias. Finaliza sin clases duplicadas, lógica de negocio ni referencias a Entity Framework.
- [ ] T07 — Verificación: ejecutar `dotnet build SolarUy.sln` desde la raíz y corregir errores introducidos. Ejecutar las pruebas existentes aplicables, incluidas las estructurales si existen. No crear pruebas de propiedades automáticas; si se añade comportamiento de dominio, cubrirlo con xUnit según las reglas del proyecto.
- [ ] T08 — Calidad y cierre: ejecutar SonarQube si está configurado según el procedimiento del proyecto. Registrar resultados y verificaciones no disponibles; actualizar el estado de estas tareas únicamente después de completarlas.

## Verificación parcial

Se ejecutó dotnet build SolarUy.sln --no-restore con las clases de Usuarios y Parques y los cuatro enumerados: 0 errores y 0 advertencias. Se deberá repetir al completar las clases restantes. No existen proyectos de pruebas ni ArchUnitNET configurado. SonarQube queda pendiente por decisión del equipo.

