## Objetivo

Agregar pruebas automáticas de arquitectura a SolarUy con ArchUnitNET y xUnit. Ubicar las pruebas en `SolarUyPruebas/Arquitectura` y permitir su ejecución desde Rider y PowerShell con `dotnet test`, sin levantar SolarUy, Docker, MySQL, Keycloak ni SonarQube.

El alcance se limita a la configuración de ArchUnitNET y las pruebas de arquitectura. La edición de `flujo de trabajo_ia` queda a cargo del usuario.

## Preparación

Revisá la solución, los proyectos de pruebas y la estructura real de SolarUy antes de definir los filtros. Identificá los namespaces y tipos compilados de cada módulo y capa, incluidos los componentes Razor, la infraestructura compartida y los tipos responsables del registro de dependencias.

Agregá ArchUnitNET y su integración con xUnit mediante NuGet al proyecto de pruebas que aloje `SolarUyPruebas/Arquitectura`, con versiones compatibles con .NET 10. Si hace falta un proyecto de pruebas nuevo para esa carpeta, incorporalo a la solución existente.

## Reglas que deben automatizarse

1. **Dependencias entre módulos.** Un módulo puede referenciar los contratos locales que otro expone bajo `Interfaz.Apis.Local.Contratos`, los DTO compartidos por esos contratos y los eventos expuestos para comunicación entre módulos. La comunicación mediante contratos locales ocurre dentro del mismo proceso. Usá los namespaces compilados que correspondan a esa estructura.
   - Los DTO compartidos pueden estar en `Aplicacion.DTOs`. Identificalos a partir de las firmas de los contratos locales y de los DTO que componen sus entradas y salidas. Aplicá el permiso a cada tipo de DTO compartido. La prueba verifica qué tipos referencia el módulo consumidor, independientemente de cómo obtuvo cada instancia.
   - Si una firma del contrato o uno de sus DTO expone una entidad o un enum interno de Dominio, mantené ese tipo fuera de la lista permitida y registrá la exposición para revisión. Si la exposición de un DTO o evento es ambigua, indicá qué tipo requiere confirmación.
   - Las interfaces de servicios de `Aplicacion.Contratos` son internas al módulo. También quedan dentro del módulo las implementaciones de servicios, las implementaciones de `Interfaz.Apis.Local.Implementaciones`, las entidades, los repositorios, los `DbContext`, las excepciones y los DTO internos. Si un módulo referencia alguno de esos tipos internos de otro módulo, registrá la infracción.
   - Un módulo puede consumir contratos de otro sin exponer contratos locales propios. Mediciones consume contratos locales de Parques y su carpeta `Interfaz/Apis/Local/Contratos` solo contiene un `.gitkeep`. Esa selección vacía es válida. El consumo de los contratos de Parques queda cubierto por la regla general.

2. **Dominio.** Los tipos de Dominio de cada módulo no pueden depender de Aplicación, Infraestructura ni Presentación, ni de Entity Framework, Blazor o ASP.NET Core. También se les aplica la regla de dependencias entre módulos.

3. **Aplicación.** Los tipos de Aplicación pueden usar el Dominio de su propio módulo y, para comunicarse con otros módulos, los contratos locales, DTO compartidos y eventos permitidos por la regla anterior. Clasificá los tipos de `Interfaz.Apis.Local.Contratos` como contratos locales para esta comprobación. Los tipos de Aplicación no pueden depender de Presentación ni de implementaciones de Infraestructura, incluidos `DbContext`, repositorios concretos y componentes Blazor.

4. **Presentación.** Los tipos de Presentación, incluidos los componentes Razor, acceden a las operaciones del módulo mediante servicios y contratos de Aplicación. La prueba debe detectar dependencias directas hacia Infraestructura, repositorios de persistencia o `DbContext`, incluso mediante inyección de dependencias.

5. **Composición.** `Program.cs` y los métodos de registro de dependencias pueden referenciar implementaciones de Infraestructura para configurarlas. Limitá la excepción a los puntos concretos de composición del proyecto. Los componentes Razor y el resto de Presentación siguen sujetos a la regla anterior.

Implementá estas reglas en la primera integración. Presentá cualquier comprobación adicional como recomendación para revisión.

## Verificación y entrega

- Aplicá las reglas a las dependencias entre tipos compilados que detecta ArchUnitNET, incluso cuando varias capas están en el mismo proyecto. Comprobá que los filtros cubren los módulos, capas y componentes previstos. Una selección que deba contener tipos y quede vacía debe fallar con un diagnóstico claro.
- Compilá y ejecutá las pruebas. Introducí temporalmente una dependencia prohibida para confirmar que la prueba falla e identifica los tipos involucrados. Retirá el cambio y volvé a ejecutar.
- Si aparecen incumplimientos preexistentes, registrá la regla, el tipo de origen, el tipo referenciado y la corrección propuesta. Mantené visible el fallo y dejá cualquier refactorización de la aplicación para revisión.
- Entregá el comando exacto para ejecutar las pruebas, las reglas automatizadas, las comprobaciones que todavía requieren revisión manual y los resultados o bloqueos de la ejecución.