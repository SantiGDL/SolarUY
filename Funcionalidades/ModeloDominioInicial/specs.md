# Diseño: clases iniciales del dominio

Basado en [requirements.md](requirements.md) y el [diagrama v2](../../Diagramas/Diagrama%20de%20Clases%20v2.png).

## Organización

Usar las carpetas existentes `SolarUy/Backend/Modulos/{Modulo}/Dominio/Entidades/`.
Crear un archivo por clase en Entidades y un archivo por enumerado en Dominio/Enums, con namespace acorde a su ruta.
Las propiedades serán públicas con `get; set;`, sin métodos de negocio,
atributos de Entity Framework ni dependencias externas.

## Usuarios

**Dominio:** representar las clases y la herencia del diagrama.

| Clase | Propiedades |
| --- | --- |
| Usuario | `int Id`, `string Nombre` |
| UsuarioTecnico : Usuario | Sin propiedades adicionales |
| UsuarioAdmin : Usuario | `string Correo`, `string Pass`, `string Imagen` |

Enumerados: `TipoImagenAceptada` (Jpg, Jpeg, Png) y `TipoUsuario`
(Admin, Plebe), siguiendo los nombres del dibujo y las convenciones C#.
No agregar una propiedad TipoUsuario que no aparece en las clases del diagrama.
`Pass` se conserva como propiedad del modelo solicitado; no se implementa
almacenamiento de credenciales ni autenticación.

`Imagen` guardará la ruta relativa a `wwwroot`, por ejemplo
`imagenes/usuarios/avatar.png`, en lugar del formato de imagen. No guardará
una ruta absoluta del equipo ni incluirá `wwwroot` en su valor.
Se conserva `TipoImagenAceptada` para una futura validación de formatos al
cargar imágenes; no será el tipo de la propiedad Imagen. La carga, el guardado
del archivo y su validación no se implementan en esta tarea.

**Aplicación, Infraestructura y Presentación:** sin cambios en esta tarea.

## Parques

**Dominio:** crear las siguientes clases.

| Clase | Propiedades |
| --- | --- |
| Parque | `int Id`, `string Nombre`, `string Direccion` |
| Nodo | `int Id`, `int ParqueId`, `string UbicacionDentroParque`, `string Nombre` |
| Panel | `int Id`, `EstadoPanel Estado`, `double PotenciaActual`, `string Orientacion`, `int NodoId` |

Crear `EstadoPanel` con Bueno, Intermedio y Rotura.
Representar las relaciones mediante ParqueId y NodoId, sin colecciones inversas
por ahora. Sobre cada propiedad incluir, respectivamente:

```csharp
//Relacion con Parque (Muchos a 1)
//Relacion con Nodo (Muchos a 1)
```

**Aplicación, Infraestructura y Presentación:** sin cambios en esta tarea.

## Mediciones

**Dominio:** crear `Medicion` con `int Id`, `DateTime FechaHora`,
`double GeneracionKw`, `Estado` y `int PanelId`.
El tipo de Estado queda pendiente porque el diagrama no lo indica.
No usar directamente EstadoPanel del dominio de Parques.

Sobre PanelId incluir exactamente:

```csharp
//Relacion con Panel (Muchos a 1)
```

**Aplicación, Infraestructura y Presentación:** sin cambios en esta tarea.

## Tareas

**Dominio:** crear `TareaMantenimiento` con `int Id`,
`TipoMantenimiento TipoTarea`, `DateTime FechaCreada`,
`DateTime FechaFinalizado`, `string Estado`, `Observaciones`,
`DateTime FechaRealizada` y `int PanelId`.
Crear `TipoMantenimiento` con Limpieza y Reparacion. Se cambia el nombre del
enumerado del dibujo para que no coincida con el de la clase.

Sobre PanelId incluir `//Relacion con Panel (Muchos a 1)`.
La relación con el técnico que resolvió la tarea y la asignación realizada
por el administrador requieren precisar la cardinalidad antes de agregar
sus propiedades y comentarios.

**Aplicación, Infraestructura y Presentación:** sin cambios en esta tarea.

## Relaciones y detalles pendientes

Las relaciones entre módulos usan identificadores. No se implementa comunicación
entre módulos ni con el simulador en esta versión.

Antes de implementar los elementos afectados, confirmar:

- El tipo de `Medicion.Estado` y de `TareaMantenimiento.Observaciones`, ausentes
  en el dibujo.
- Si FechaFinalizado y FechaRealizada deben permitir valores nulos.
- Los identificadores y cardinalidades para administrador y técnico en una tarea.

## Verificación

Comparar clases, propiedades, enumerados y comentarios con el diagrama y este
diseño. Compilar desde la raíz con `dotnet build SolarUy.sln` y revisar que
el dominio no dependa de otras capas ni de entidades de otros módulos.
Ejecutar las pruebas existentes aplicables. Al tratarse de propiedades simples,
no agregar pruebas triviales; si se incorpora comportamiento, aplicar las pruebas
xUnit exigidas por las reglas. Ejecutar ArchUnitNET y SonarQube solo si están
configurados y registrar cualquier comprobación pendiente.

