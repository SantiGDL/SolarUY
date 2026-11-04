# Requisitos: clases iniciales del dominio

Crear las clases `.cs` y los enumerados del [Diagrama de Clases v2](../../Diagramas/Diagrama%20de%20Clases%20v2.png), con los nombres y atributos que aparecen en la imagen, respetando las convenciones de C# del proyecto.

Ubicar cada clase en el dominio del módulo correspondiente:

- **Usuarios:** Usuario, UsuarioTecnico y UsuarioAdmin.
- **Parques:** Parque, Nodo y Panel.
- **Mediciones:** Medicion.
- **Tareas:** TareaMantenimiento.

Ubicar los enumerados en el módulo que los utiliza. Las relaciones entre módulos se representarán mediante identificadores, sin duplicar las clases.

Agregar sobre cada propiedad que represente una relación un comentario con el formato exacto `//Relacion con NombreClase (Tipo de relacion)`, indicando la clase relacionada y la cardinalidad. Por ejemplo, sobre `PanelId` en `Medicion`: `//Relacion con Panel (Muchos a 1)`. Mantener este formato, sin reemplazarlo por frases descriptivas.

Esta tarea incluye únicamente las clases y sus propiedades. La lógica de negocio y la implementación de Entity Framework quedan pendientes.

Como ajuste al diagrama, `UsuarioAdmin.Imagen` será de tipo `string` y guardará la ruta relativa a `wwwroot` de la imagen, por ejemplo `imagenes/usuarios/avatar.png`. Se conservará el enumerado `TipoImagenAceptada` para una futura validación de formatos al cargar imágenes. La carga de archivos y su validación quedan pendientes.

La tarea estará terminada cuando las clases estén en sus módulos, representen los atributos del diagrama y la solución compile.
