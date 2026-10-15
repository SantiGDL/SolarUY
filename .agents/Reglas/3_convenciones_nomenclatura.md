# Convenciones de Nomenclatura en .NET

Al generar código, refactorizar o proponer soluciones, el agente debe respetar las siguientes convenciones para mantener un código legible, uniforme y predecible.

Estas reglas se aplican dentro de cada módulo del monolito modular. Las convenciones verificables mediante ArchUnitNET se comprobarán cuando existan pruebas de arquitectura implementadas para ellas.

## 1. Reglas Generales de C#

- **Interfaces:** Deben comenzar con la letra `I` mayúscula.
  - Ejemplos: `IParqueRepository`, `ILecturaService`.

- **Excepciones:** Las clases personalizadas que hereden de `System.Exception` deben terminar con el sufijo `Exception`.
  - Ejemplo: `LecturaInvalidaException`.

- **PascalCase:** Se utiliza para clases, records, interfaces, métodos, propiedades y constantes.
  - Ejemplos: `Parque`, `RegistrarLectura()`, `Nombre`, `IntervaloPredeterminado`.

- **camelCase:** Se utiliza para variables locales y parámetros.
  - Ejemplos: `parqueId`, `lecturaRecibida`.

- **_camelCase:** Se utiliza para campos de instancia privados, incluidas las dependencias recibidas por constructor.
  - Ejemplo: `private readonly IParqueRepository _parqueRepository;`.

## 2. Convenciones por Responsabilidad

### Repositorios

- Las implementaciones del patrón Repository deben terminar con el sufijo `Repository`.
- Su interfaz correspondiente debe conservar el mismo nombre, prefijado con `I`.
  - Ejemplo: `IParqueRepository` → `ParqueRepository`.
- No utilizar los términos `Dao` o `GestorDatos` para nombrar repositorios.
- Esta regla no se aplica a todas las clases de acceso a datos. Los contextos de EF Core, configuraciones y migraciones deben conservar nombres acordes a su función.
  - Ejemplos: `ParquesDbContext`, `ParqueConfiguration`.

### Servicios de Aplicación

- Las clases que coordinan casos de uso deben terminar con el sufijo `Service`.
  - Ejemplos: `ParqueService`, `LecturaService`.
- Esta es una convención del proyecto, no una exigencia de Clean Architecture.
- No introducir MediatR ni convenciones de `CommandHandler` o `QueryHandler` como alternativa a esta regla.

### Presentación

- Si existen controladores de una API REST, deben terminar con el sufijo `Controller`.
  - Ejemplo: `ParquesController`.
- Los componentes Blazor deben tener nombres descriptivos en PascalCase.
  - Ejemplos: `ListadoParques.razor`, `DetalleNodo.razor`.
- Las páginas completas pueden utilizar el sufijo `Page`, manteniendo una convención uniforme en el proyecto.
- Los Hubs de SignalR deben terminar con el sufijo `Hub`.
  - Ejemplo: `SensoresHub`.

## 3. Identificadores y Acrónimos

- Utilizar siempre `Id` para identificadores, evitando variantes como `ID` o `id` en nombres públicos.
  - Ejemplos: `Id`, `ParqueId`, `IdUsuario`.
  - En variables locales y parámetros: `id`, `parqueId`, `idUsuario`.
- Tratar los demás acrónimos como palabras normales al aplicar PascalCase o camelCase.
  - Ejemplos: `XmlDocument`, `UrlServidor`, `DireccionIp`.
- Respetar los nombres establecidos por .NET y las bibliotecas externas; no renombrar sus APIs para adaptarlas a estas reglas.

## 4. Idioma

### Nombres de Negocio en Español

- Los nombres que representan conceptos y operaciones del negocio deben escribirse en español.
  - Entidades: `Parque`, `Nodo`, `Panel`, `Lectura`.
  - Métodos: `CrearParque()`, `AsignarNodo()`, `RegistrarLectura()`.
  - Variables y campos: `panelActivo`, `_ultimaLectura`.
- Evitar tildes y la letra `ñ` en identificadores para mantener una convención uniforme.
  - Ejemplos: `Configuracion`, `Anio`.

### Términos Técnicos en Inglés

- Mantener en inglés los prefijos y sufijos técnicos definidos en estas reglas.
  - Ejemplos: `ParqueRepository`, `LecturaService`, `LecturaInvalidaException`, `SensoresHub`.
- Los nombres técnicos del framework y de las bibliotecas deben conservar su denominación original.