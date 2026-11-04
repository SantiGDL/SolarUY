# Diseño: organización de enumerados

Basado en [requirements.md](requirements.md).

Crear `Dominio/Enums/` dentro de cada módulo involucrado y mover allí los
enumerados existentes. Mantener un archivo por enumerado, sus nombres y valores.

| Módulo | Archivos que se moverán desde Dominio/Entidades/ |
| --- | --- |
| Usuarios | TipoImagenAceptada.cs, TipoUsuario.cs |
| Parques | EstadoPanel.cs |
| Tareas | TipoMantenimiento.cs |

Las rutas parten de `SolarUy/Backend/Modulos/`. Los namespaces pasarán a ser
`SolarUy.Backend.Modulos.{Modulo}.Dominio.Enums`.

## Responsabilidades por módulo

- **Usuarios / Dominio:** mover sus dos enumerados. UsuarioAdmin.Imagen seguirá
  siendo `string`.
- **Parques / Dominio:** mover EstadoPanel y actualizar la referencia de Panel
  mediante el namespace nuevo.
- **Tareas / Dominio:** mover TipoMantenimiento y actualizar cualquier referencia
  existente.
- **Aplicación, Infraestructura y Presentación de estos módulos:** sin nuevas
  funcionalidades; actualizar referencias únicamente si las hubiera.
- **Mediciones:** sin cambios previstos.

El cambio no agrega comunicación entre módulos ni modifica el simulador.
Las entidades permanecen en `Dominio/Entidades/`.

## Verificación

Comprobar que no queden copias de los enumerados en Entidades y que sus usos
resuelvan los namespaces nuevos. Revisar que Dominio no incorpore dependencias
de otras capas y ejecutar `dotnet build SolarUy.sln` desde la raíz.

Ejecutar las pruebas existentes aplicables, incluidas las estructurales si están
configuradas. No agregar pruebas de propiedades o valores de enumerados:
este cambio no incorpora comportamiento. SonarQube queda pendiente según lo
acordado con el equipo.
