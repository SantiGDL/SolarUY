# Especificación técnica de ArchUnitNET

## Proyecto de pruebas

Crear `SolarUyPruebas/Arquitectura/SolarUyArquitecturaPruebas.csproj` con destino `net10.0`, referencia a `SolarUy/SolarUy.csproj` e integración `TngTech.ArchUnitNET.xUnit` compatible con xUnit 2, siguiendo la configuración del proyecto de pruebas de Parques. Fijar las versiones verificadas al restaurar. Incorporar el proyecto a `SolarUy.sln` dentro de `SolarUyPruebas`.

Importar una vez el ensamblado de SolarUy mediante un tipo existente, como `Parque`. Para las reglas de frameworks, comprobar que el análisis puede resolver referencias a EF Core, Blazor y ASP.NET Core. Ejecutar las pruebas sobre el código compilado sin iniciar el host, Docker, MySQL, Keycloak ni SonarQube.

## Módulos y capas

Los namespaces de backend se agrupan bajo `SolarUy.Backend.Modulos.<Modulo>.<Capa>`. Los selectores incluyen el namespace exacto y sus descendientes. El primer segmento luego de `Modulos` identifica al propietario de cada tipo.

| Módulo | Dominio | Aplicación | Infraestructura | Presentación y contratos locales |
|---|---|---|---|---|
| Parques | Entidades, enums e interfaces de repositorios | Servicios de parques, nodos, paneles y catálogos | Repositorios | API local de catálogos bajo `Interfaz.Apis.Local.Contratos` |
| Tareas | Entidades y enums de mantenimiento | Servicios, contratos y DTO de tareas | Repositorios | Páginas Blazor de mantenimiento dentro de `SolarUy.Components` |
| Mediciones | Entidades, historial e interfaces de repositorios | Producción, historial y contratos de integración | Persistencia, exportación, sesiones y SignalR | `Interfaz.SignalR.MedicionesHub` y páginas Blazor de producción e historial |
| Usuarios | Entidades y enums | Confirmar el inventario compilado | Confirmar el inventario compilado | Confirmar el inventario compilado |
| Consultas | Confirmar el inventario compilado | Consulta natural, contratos y DTO | Integración de IA | Componentes Blazor que consumen el servicio de consulta |

Incluir `SolarUy.Backend.Infraestructura` en Infraestructura. Incluir `SolarUy.Components` y sus descendientes en Presentación y comprobar los tipos Razor compilados, con ejemplos de páginas y layouts. Incluir los adaptadores de los namespaces `Interfaz`, como el hub. Clasificar los contratos locales identificados por tipo como superficie de comunicación, aunque estén bajo `Interfaz`. Si comparten namespace con una implementación, esta conserva su clasificación y queda fuera de la superficie expuesta.

Verificar el inventario compilado por módulo y capa frente a tipos representativos. Toda selección que deba contener tipos y quede vacía debe fallar e indicar el módulo, la capa o el framework afectado. Una superficie de contratos locales puede estar vacía. En Mediciones, `Interfaz/Apis/Local/Contratos` contiene un `.gitkeep` y su consumo de catálogos de Parques se verifica con la regla general.

## Comunicación entre módulos

Comprobar cada dependencia entre tipos con propietarios distintos. La comunicación por contratos locales se hace mediante llamadas C# en el mismo proceso. Seleccionar las interfaces expuestas en `Interfaz.Apis.Local.Contratos` del módulo destino. Recorrer sus entradas y salidas, incluidos los tipos genéricos y los tipos que componen cada DTO, para identificar los DTO propios del módulo que se comparten. Registrar el permiso por identidad completa de cada DTO, incluso si reside en `Aplicacion.DTOs`. Admitir los eventos cuya exposición entre módulos esté documentada. Los catálogos de Parques están respaldados por REQ_008 y REQ_010 y por sus firmas:

- `ICatalogoProduccion` y los DTO `ParqueProduccion`, `NodoProduccion` y `PanelProduccion`.
- `ICatalogoHistorial` y los DTO `CatalogoHistorial`, `ParqueHistorial`, `NodoHistorial` y `PanelHistorial`.

Evaluar la referencia estática del consumidor al tipo permitido. El permiso de un DTO es el mismo si se recibe del contrato, se crea en el consumidor o se obtiene de otro método. La prueba no necesita seguir el origen de cada instancia.

Al revisar las firmas, inspeccionar también los miembros públicos de los DTO compartidos y desenvolver los tipos usados en colecciones, genéricos y otros contenedores. Si una firma o un DTO expone una entidad o un enum interno de Dominio, registrar para revisión el contrato, el miembro que lo expone y el tipo interno. Mantener ese tipo fuera de la lista de permisos. Si un consumidor lo referencia directamente, informar la infracción de ARQ-01 con el origen y el destino. La presencia de ese tipo interno en la firma no amplía el permiso del DTO a sus dependencias.

Son internos los contratos de servicio en `Aplicacion.Contratos`, las implementaciones de servicios y de `Interfaz.Apis.Local.Implementaciones`, las entidades, los repositorios, los contextos, las excepciones y los DTO ajenos a las firmas expuestas. Esto incluye a `CatalogoProduccionService` y `CatalogoHistorialService`. El inventario de eventos del proyecto muestra carpetas con `.gitkeep`. Verificar en los tipos compilados si existe algún evento expuesto.

| Referencia observada | Aplicación de la regla y corrección a revisar |
|---|---|
| `PanelService` y `ConsultaNaturalService` consumen `ITareasService`. | `ITareasService` es interno por estar en `Tareas.Aplicacion.Contratos`. Revisar un contrato local de lectura para Consultas y otro acotado para la creación automática desde Parques. |
| Consultas usa `TareaDto` y `EstadoTarea`. Tareas usa el enum de Dominio `EstadoPanel` de Parques. | Registrar las referencias compiladas fuera de la superficie permitida. Confirmar si un futuro contrato local de Tareas expondría `TareaDto` y qué representación contractual usaría para los estados. |
| `ConsultaNaturalService` consume `HistorialMedicionesService`. Consultas usa `OpcionHistorialDto`, `ResultadoHistorialDto` y tipos de `Mediciones.Dominio.Historial` como `PeriodoHistorial` y `ModoHistorial`. | La clase concreta es interna. Los DTO quedan fuera de la superficie actual porque Mediciones carece de un contrato local publicado. Confirmar cuáles DTO y filtros se expondrían en un futuro contrato de lectura, conforme a REQ_010 y REQ_011. |
| `TareasService` usa `IUsuarioActual` y `AccesoNoAutorizadoException` de Parques. | Ambos tipos son internos de Parques. Revisar qué módulo debe proveer el acceso al usuario actual y cómo expresar el error sin depender de una excepción interna de Parques. |

El inventario de código fuente orienta la revisión. Determinar los incumplimientos con dependencias compiladas, incluidas las firmas y sus tipos genéricos. Los tipos con exposición ambigua quedan fuera de la lista hasta su confirmación. Registrar los fallos con el tipo de origen, el destino y la regla.

La integración actual de Mediciones con Sensor usa SignalR. Los tipos de Sensor quedan fuera de la comprobación entre módulos del ensamblado SolarUy. Los tipos del hub y del adaptador SignalR de SolarUy sí se comprueban según su capa. El diseño conserva la comunicación existente con el simulador.

## Reglas estructurales

| ID | Comprobación |
|---|---|
| ARQ-01 | Los tipos de un módulo solo pueden referenciar los tipos expuestos de otro. Comprobar la identidad del tipo referenciado con independencia del origen de la instancia. Las entidades y los enums internos de Dominio quedan fuera del permiso aunque aparezcan en una firma expuesta. Evaluar esta condición además de los límites de capa. |
| ARQ-02 | Dominio es independiente de Aplicación, Infraestructura, Presentación, EF Core y ASP.NET Core, incluido Blazor. |
| ARQ-03 | Aplicación usa su Dominio y puede consumir contratos locales, DTO compartidos y eventos permitidos por ARQ-01. Detectar accesos a Presentación, componentes Blazor y tipos de Infraestructura como repositorios concretos o `DbContext`, incluidos los contextos de infraestructura compartida. Clasificar `Interfaz.Apis.Local.Contratos` como contratos locales para esta regla. |
| ARQ-04 | Presentación accede a los casos de uso por Aplicación. Detectar referencias a Infraestructura, interfaces de repositorio y `DbContext` o fábricas de contexto, incluso cuando aparezcan en parámetros de constructor o propiedades Razor con `@inject`. Identificar interfaces de persistencia por su responsabilidad y ubicación real. |
| ARQ-05 | El punto de composición puede configurar implementaciones de Infraestructura. El registro de servicios se encuentra en `SolarUy/Program.cs`. Verificar su tipo compilado y los miembros de registro antes de delimitar la excepción. Aplicarla solo a la referencia permitida por composición y a los miembros o tipos auxiliares que realmente participen del registro. |

Usar restricciones de dependencias de ArchUnitNET para límites de capa y evaluar las relaciones por propietario e identidad de destino para ARQ-01. Las pruebas deben informar la regla y los tipos que generan cada infracción. El acceso dinámico por reflexión, las responsabilidades de negocio, los tiempos de vida de DI y la intención de una API nueva requieren revisión de código.

## Verificación prevista

```powershell
dotnet build SolarUy.sln
dotnet test SolarUyPruebas/Arquitectura/SolarUyArquitecturaPruebas.csproj
dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj
```

Tomar una primera ejecución como referencia. Introducir una dependencia temporal y reversible desde un tipo conocido de Dominio hacia `ParquesDbContext`. Verificar que aparezca la nueva infracción con ambos tipos, retirar el cambio y comprobar que desaparece. Validar además que Mediciones pueda consumir los catálogos de Parques aun con su propia selección de contratos vacía, que las implementaciones de esos catálogos queden excluidas del permiso y que la cobertura incluya referencias por inyección en Razor.

Para ARQ-01, comprobar que un consumidor pueda referenciar el mismo DTO permitido al recibirlo de un catálogo o al construirlo localmente. En un caso controlado, exponer temporalmente un enum interno de Dominio a través de un DTO compartido, verificar que el inventario registre la exposición y que una referencia al enum desde el consumidor falle. Retirar los cambios de comprobación.

Registrar los incumplimientos preexistentes con regla, tipos y corrección propuesta. Cualquier refactorización de la aplicación queda para revisión. Como la funcionalidad agrega pruebas estructurales sin cambios de comportamiento de Dominio, corresponde ejecutar las pruebas de Dominio existentes.
