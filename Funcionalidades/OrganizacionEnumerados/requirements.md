# Requisitos: organización de enumerados

Ubicar todos los enumerados en una carpeta `Dominio/Enums/` dentro de su módulo, separados de las entidades:

- **Usuarios:** TipoImagenAceptada y TipoUsuario.
- **Parques:** EstadoPanel.
- **Tareas:** TipoMantenimiento.

Las rutas estarán dentro de `SolarUy/Backend/Modulos/`. Mover los archivos existentes desde `Dominio/Entidades/`, actualizar sus namespaces y las referencias necesarias, sin cambiar sus nombres ni valores.

`UsuarioAdmin.Imagen` seguirá siendo de tipo `string`.

La tarea estará terminada cuando los enumerados estén en las carpetas indicadas, las clases utilicen sus nuevas ubicaciones y la solución compile.
