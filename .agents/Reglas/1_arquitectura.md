Reglas de Clean Architecture — SolarUY
Estas reglas se aplican al generar, modificar, refactorizar o revisar código.
El proyecto es una Blazor Web App con interactividad Interactive Server.
La presentación y los servicios del backend se ejecutan en el servidor y pueden
comunicarse mediante llamadas C# dentro del mismo proceso.
Este documento establece responsabilidades y dependencias entre capas lógicas.
No prescribe una estructura de carpetas ni exige proyectos .NET independientes.
Respetar la organización y los nombres existentes del proyecto.

1. Dependencias entre capas
- Dominio es el núcleo: no depende de Aplicación, Infraestructura ni Presentación.
- Aplicación depende de Dominio y utiliza las interfaces de repositorios
  definidas allí. Define los contratos de integraciones que necesitan sus
  casos de uso. No depende de implementaciones de Infraestructura.
- Infraestructura puede depender de Aplicación y Dominio para implementar
  esos contratos.
- Presentación utiliza los casos de uso y contratos de Aplicación.
  No accede directamente a Infraestructura.
- El punto de composición de la aplicación, como Program.cs y los métodos
  de registro de dependencias, puede referenciar Infraestructura para configurar
  sus implementaciones. Esa excepción no se extiende al código de los componentes.
- No introducir dependencias circulares.
- Mantener estas reglas aunque todas las capas estén en un único proyecto.
  La separación en carpetas no impide por sí sola las dependencias incorrectas.

2. Dominio
- Colocar aquí las entidades, objetos de valor y reglas propias del negocio.
- Definir aquí las interfaces de repositorios. Sus implementaciones pertenecen
  a Infraestructura.
- Mantener las interfaces de repositorios independientes de Entity Framework,
  DbContext, SQL y tipos específicos de proveedores de persistencia.
- Mantener el dominio independiente de Blazor, ASP.NET Core, Entity Framework
  Core y proveedores externos.
- No utilizar DbContext, SQL, peticiones HTTP, componentes de interfaz
  ni servicios de aplicación.
- Configurar los detalles de persistencia fuera del dominio.
- Se permiten entidades simples y propiedades con setters públicos
  durante las primeras etapas.
- Cuando un cambio de estado deba preservar una regla de negocio,
  concentrar esa regla en un método de dominio.
- No duplicar las mismas reglas de negocio en componentes o repositorios.
- No introducir agregados, eventos de dominio u otros patrones
  sin una necesidad concreta.

3. Aplicación
- Implementar y coordinar los casos de uso utilizando el dominio.
- Coordinar persistencia e integraciones mediante contratos,
  sin conocer sus implementaciones concretas.
- Utilizar las interfaces de repositorios definidas en Dominio.
  No duplicarlas en Aplicación.
- Definir aquí los contratos de integraciones externas que necesitan
  los casos de uso, sin depender de proveedores o implementaciones concretas.
- No utilizar DbContext, SQL, repositorios concretos, HttpContext,
  componentes Blazor ni tipos de respuesta HTTP.
- Definir entradas y resultados independientes de la interfaz y la persistencia.
- Utilizar DTO u otros modelos de aplicación cuando sean necesarios para
  delimitar los datos de un caso de uso. No crear conversiones sin utilidad.
- Mantener explícitos los errores esperados, como datos inválidos,
  recursos inexistentes o falta de permisos.
- Validar las condiciones del caso de uso y hacer cumplir la autorización
  de las operaciones protegidas, mediante las abstracciones necesarias.
- Delegar las reglas propias del negocio en el dominio.
- No crear una interfaz para cada servicio de aplicación por rutina.
4. Infraestructura y acceso a datos
- Implementar aquí los contratos de persistencia e integraciones externas.
- Encapsular DbContext, consultas SQL, clientes HTTP y detalles del proveedor.
- No colocar aquí reglas de negocio ni la coordinación completa de casos de uso.
- No exponer DbContext, IQueryable ligado a persistencia, tipos del proveedor
  ni detalles del seguimiento de entidades fuera de Infraestructura.
- Implementar aquí las interfaces de repositorios definidas en Dominio
  y los contratos de integraciones definidos en Aplicación.
- Diseñar los contratos según las operaciones necesarias; no crear
  repositorios genéricos o Unit of Work adicionales sin justificación.
- No elegir ni configurar una tecnología de persistencia o integración
  solo por la existencia de estas reglas.

5. Presentación con Blazor Interactive Server
- Los componentes llaman a los servicios de Aplicación mediante inyección
  de dependencias y llamadas C# dentro del mismo proceso.
- No crear una API REST, controladores ni clientes HTTP para comunicar
  los componentes con los casos de uso internos.
- Las integraciones con sistemas externos siguen encapsuladas en Infraestructura.
- Los componentes gestionan interacción, formularios, estado visual
  y presentación de resultados.
- Se permiten filtros, condiciones y cálculos exclusivamente visuales.
- No implementar reglas de negocio ni coordinar persistencia
  dentro de componentes o sus archivos de código asociado.
- No inyectar repositorios, DbContext ni implementaciones de Infraestructura
  en componentes.
- Las validaciones visuales y la visibilidad de botones no sustituyen
  la validación ni la autorización del caso de uso.
- No entregar entidades persistentes a la interfaz para que modifique
  directamente el estado del negocio; utilizar entradas y resultados
  de Aplicación apropiados para cada operación.

6. Inyección de dependencias y tiempos de vida
- Usar inyección por constructor en servicios de aplicación,
  repositorios e integraciones.
- En componentes, utilizar los mecanismos de inyección de Blazor.
- No resolver servicios mediante un localizador de servicios ni instanciar
  manualmente dependencias que debe gestionar el contenedor.
- Se permite usar new para entidades, DTO, objetos de valor y otros
  objetos que no sean servicios administrados.
- Configurar las dependencias en el punto de composición, fuera
  de las reglas de negocio.
- Elegir tiempos de vida compatibles. No capturar servicios scoped
  en servicios singleton ni compartir estado de usuario en un singleton.
- En Interactive Server, no asumir que scoped equivale a una petición HTTP:
  puede durar toda la sesión interactiva del usuario (circuito).
- Si se utiliza Entity Framework Core, gestionar la creación y liberación
  del DbContext en Infraestructura según la operación. No mantener
  por defecto un contexto compartido durante todo el circuito.
- No utilizar una misma instancia de DbContext en operaciones concurrentes.
  Si se necesita una fábrica de contextos, mantenerla dentro de Infraestructura.

7. Simplicidad y verificación
- Mantener responsabilidades claras y evitar clases que mezclen
  presentación, casos de uso y persistencia.
- No introducir proyectos, capas, abstracciones, bibliotecas ni patrones
  únicamente para aparentar Clean Architecture.
- Las convenciones de nombres, como el prefijo I para interfaces o
  el sufijo Repository, no sustituyen el respeto de las dependencias.
- Revisar dependencias también en parámetros, resultados, atributos
  y tipos genéricos; no limitar la revisión a las directivas using.
- Al implementar o modificar código, comprobar que se conservan
  los límites definidos en este documento.
- Cuando existan pruebas estructurales con ArchUnitNET, ejecutarlas
  según las instrucciones de verificación del proyecto.
- No afirmar que estos límites se verifican automáticamente
  mientras no existan pruebas configuradas y ejecutadas.