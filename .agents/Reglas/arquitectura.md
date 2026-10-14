# Reglas de arquitectura — SolarUY

## Organización y dependencias

- Mantener el backend como un monolito modular.
- Respetar los módulos Usuarios, Parques, Tareas y Mediciones,
  ubicados en SolarUyBack/Backend/Modulos/.
- Separar las responsabilidades mediante las carpetas acordadas:
  Controladores, DTOs, Interfaz, Aplicacion, Dominio e Infraestructura.
- Mantener los proyectos en la misma solución y repositorio.
- No interpretar la separación en carpetas como una obligación de crear
  proyectos .NET independientes por capa o módulo.
- No crear proyectos, módulos, subcarpetas ni abstracciones adicionales
  sin una necesidad concreta y una decisión justificada.
- Conservar la versión de .NET declarada en los proyectos.
- Estas reglas orientan las implementaciones futuras; no autorizan
  por sí solas a crear código ni configurar integraciones.
- Documentar los cambios de límites y dependencias antes de realizar
  una reorganización amplia.

## Dominio

- Colocar entidades y reglas de negocio en Dominio.
- No introducir dependencias hacia Entity Framework, ASP.NET Core,
  Blazor, infraestructura, servicios de aplicación ni contratos de transporte.
- No realizar directamente consultas a la base de datos, peticiones HTTP
  ni operaciones de interfaz.
- Se permiten entidades simples y propiedades con setters públicos
  en las primeras etapas.
- Cuando una modificación de estado deba preservar una regla de negocio,
  proponer un método de dominio que concentre esa validación.
- No introducir patrones complejos sin una necesidad concreta.

## Aplicación

- Coordinar los casos de uso utilizando el dominio.
- Colocar los contratos de servicios de aplicación que sean necesarios
  en Aplicacion/Servicios y sus implementaciones
  en Aplicacion/Implementaciones.
- No crear una interfaz para cada clase sin justificar su utilidad.
- Depender de abstracciones para persistencia e integraciones externas.
- Definir la ubicación de esas abstracciones en el diseño de la funcionalidad,
  sin introducir dependencias hacia infraestructura.
- No utilizar implementaciones concretas de infraestructura, DbContext,
  SQL, controladores ni componentes Blazor.
- Se permite coordinar varias operaciones cuando formen parte del mismo
  caso de uso y estén previstas en los requisitos.
- Mantener explícitos los resultados y errores esperados del caso de uso.
- No duplicar reglas de negocio que correspondan al dominio.

## Infraestructura

- Colocar las implementaciones de acceso a datos
  en Infraestructura/Persistencia.
- Colocar las integraciones de identidad específicas del módulo
  en Infraestructura/Identidad cuando sean necesarias.
- Implementar los contratos definidos en la capa acordada para cada módulo.
- No colocar reglas de negocio que correspondan al dominio
  o a la aplicación.
- No exponer detalles de persistencia a controladores,
  contratos públicos ni componentes del frontend.
- No asumir una base de datos ni una herramienta de persistencia
  mientras esa decisión esté pendiente.
- No implementar autenticación propia como sustituto
  del proveedor externo compatible con OpenID Connect.
- No duplicar la configuración común de identidad en cada módulo.

## API y composición del backend

- Colocar los controladores en Controladores.
- Recibir solicitudes, delegar en servicios de aplicación
  y traducir sus resultados a respuestas HTTP.
- No acceder directamente a repositorios ni a contextos de persistencia.
- Usar DTO para solicitudes y respuestas; no exponer directamente entidades.
- Validar las solicitudes y verificar permisos en el servidor
  antes de ejecutar operaciones protegidas.
- El punto de arranque del backend puede referenciar infraestructura
  para registrar sus implementaciones en el contenedor de dependencias.
- Esa excepción no permite utilizar infraestructura directamente
  desde controladores o servicios de aplicación.

## Contratos de acceso remoto y local

- Utilizar Interfaz/Apis/Remota para los contratos de acceso remoto
  del módulo, según el diseño aprobado.
- Exponer las operaciones HTTP mediante los controladores.
- No crear interfaces C# por controlador ni duplicar contratos
  únicamente para llenar las carpetas.
- Utilizar Interfaz/Apis/Local para los contratos que el módulo
  expone a otros módulos del monolito.
- Resolver las llamadas locales dentro del mismo proceso.
  No introducir HTTP entre módulos sin una decisión explícita.
- No confundir los contratos remotos del módulo con las integraciones
  hacia proveedores externos.
- Mantener los contratos independientes de implementaciones
  y detalles de persistencia.

## Límites entre módulos

- Acceder a otro módulo únicamente mediante sus contratos locales
  o los eventos acordados.
- No acceder directamente a sus repositorios, implementaciones,
  entidades internas ni almacenamiento.
- No modificar los datos de otro módulo evitando sus contratos.
- No introducir dependencias circulares entre módulos.
- No utilizar Compartido para evitar estos límites.
- Revisar también las dependencias indirectas introducidas
  por DTO y otros tipos utilizados en los contratos.
- Si se acuerda un proyecto de contratos compartidos, mantenerlo
  independiente de infraestructura y de la implementación del backend.
- La existencia de estas reglas no autoriza a crear ese proyecto.

## Eventos entre módulos

- Utilizar Interfaz/Eventos/IN para los puntos de recepción
  de eventos del módulo.
- Delegar desde los receptores en servicios de aplicación;
  no concentrar allí reglas de negocio.
- Colocar los contratos de eventos publicados
  en Interfaz/Eventos/OUT.
- No incluir entidades internas ni detalles de persistencia
  en los eventos públicos.
- No duplicar contratos de eventos con definiciones incompatibles.
- Definir emisores, consumidores y comportamiento ante errores
  en la especificación de la funcionalidad que utilice eventos.
- No asumir un broker, biblioteca, ejecución asíncrona,
  orden de procesamiento ni garantías de entrega.
- No introducir eventos para todas las operaciones
  sin una necesidad concreta.

## Elementos compartidos

- Colocar las abstracciones comunes de publicación de eventos
  en Backend/Compartido/Aplicacion/Eventos.
- Colocar su implementación técnica
  en Backend/Compartido/Infraestructura/Eventos.
- Mantener las abstracciones de aplicación independientes
  del transporte o biblioteca utilizados.
- Utilizar Backend/Compartido/Infraestructura/Persistencia
  únicamente para elementos técnicos comunes acordados.
- No interpretar esa carpeta como autorización para crear
  un contexto único o acceder libremente a los datos de todos los módulos.
- No mover reglas de negocio específicas de un módulo a Compartido.
- Compartir código solo cuando exista una necesidad común real.

## Frontend Blazor WebAssembly

- Comunicar SolarUyFront con el backend mediante servicios cliente:
  HTTP y JSON para operaciones ordinarias y SignalR para monitoreo.
- No referenciar la implementación del backend ni acceder a la base de datos.
- No invocar directamente los servicios de aplicación del backend.
- Los componentes manejan interacción, formularios, estado visual
  y presentación de resultados.
- Se permiten condiciones, filtros y cálculos destinados exclusivamente
  a la presentación.
- No implementar reglas de negocio, permisos ni cálculos de negocio
  únicamente en el navegador.
- Las validaciones de interfaz no sustituyen las del servidor.
- La visibilidad de botones o pantallas no sustituye la autorización
  de operaciones en el backend.
- Distinguir datos actuales de datos desactualizados por desconexión.
- No agregar una referencia al backend para reutilizar DTO;
  acordar primero cómo compartir los contratos necesarios.

## Inyección de dependencias

- Usar inyección por constructor en servicios de aplicación,
  repositorios e integraciones.
- En componentes Blazor, utilizar el mecanismo de inyección
  admitido por el framework.
- No crear manualmente servicios o repositorios que deban gestionarse
  mediante el contenedor.
- Se permite usar new para entidades, DTO, objetos de valor
  y otros objetos que no sean servicios administrados.
- Elegir tiempos de vida compatibles entre dependencias.
  No capturar servicios scoped dentro de servicios singleton.
- Mantener el registro de dependencias separado de las reglas de negocio.

## Verificación de los límites

- No asumir que la separación en carpetas impide dependencias incorrectas.
- Revisar las referencias y los tipos utilizados al implementar
  o modificar código.
- Cuando ArchUnitNET esté configurado, verificar los límites
  mediante las pruebas estructurales acordadas.
- No afirmar que una regla está validada automáticamente
  si su prueba todavía no existe o no se ejecutó.
- Seguir Reglas/verificacion.md para ejecutar los controles
  y resolver sus resultados.