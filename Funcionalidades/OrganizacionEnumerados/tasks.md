# Tareas: organización de enumerados

Implementación completada.

- [x] T01 — Usuarios / Dominio: mover TipoImagenAceptada y TipoUsuario a `Dominio/Enums/` y actualizar sus namespaces y referencias. Finaliza sin copias en Entidades y con Imagen de tipo string.
- [x] T02 — Parques / Dominio: mover EstadoPanel a `Dominio/Enums/`, actualizar su namespace y la referencia de Panel. Finaliza con el tipo resuelto desde la nueva ubicación.
- [x] T03 — Tareas / Dominio: mover TipoMantenimiento a `Dominio/Enums/` y actualizar su namespace y referencias existentes. Finaliza sin copias en Entidades.
- [x] T04 — Documentación: ajustar las rutas de enumerados en las especificaciones de ModeloDominioInicial para mantener ambos diseños coherentes.
- [x] T05 — Verificación: revisar nombres, valores, referencias y dependencias; ejecutar `dotnet build SolarUy.sln` y las pruebas existentes aplicables. Finaliza sin errores introducidos por el cambio. No agregar pruebas triviales ni instalar herramientas de calidad.
- [x] T06 — Cierre: registrar resultados de las verificaciones y actualizar esta lista. Indicar que SonarQube queda pendiente y si no hay pruebas de arquitectura disponibles.

## Resultado

Los cuatro enumerados se movieron a Dominio/Enums en sus módulos. Rider actualizó los namespaces y la referencia de Panel a EstadoPanel. Los otros tres enumerados todavía no tienen consumidores. Se actualizaron las especificaciones del modelo inicial.

Compilación: dotnet build SolarUy.sln --no-restore, 0 errores y 0 advertencias. La revisión de dependencias es manual; no existen proyectos de pruebas ni ArchUnitNET configurado. No se agregaron pruebas triviales porque no cambió el comportamiento. SonarQube queda pendiente según lo acordado.

