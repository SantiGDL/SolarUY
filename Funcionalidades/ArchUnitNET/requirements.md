# Pruebas de arquitectura con ArchUnitNET

## Objetivo

Incorporar pruebas automáticas que comprueben las dependencias entre capas de
SolarUy según las reglas del proyecto. Ejecutarlas desde Rider o PowerShell
con `dotnet test`, sin iniciar SolarUy, Docker, MySQL, Keycloak ni SonarQube.

## Plan de cinco pasos

1. **Revisar las dependencias actuales.** Identificar las capas y sus namespaces
   reales, incluidos los componentes Razor y la infraestructura compartida.
   Registrar incumplimientos encontrados y delimitar las comprobaciones iniciales
   antes de modificar código de la aplicación.
2. **Preparar las pruebas.** Crear un proyecto de pruebas de arquitectura dentro
   de `SolarUyPruebas`, incorporarlo a `SolarUy.sln` e instalar mediante NuGet
   ArchUnitNET con integración para xUnit y versiones compatibles con .NET 10.
   Las dependencias deben quedar registradas para que el equipo pueda restaurarlas.
3. **Escribir las reglas iniciales.** Comprobar independencia de Dominio,
   dependencias de Aplicación y acceso de Presentación a Infraestructura según
   el alcance detallado abajo. Cada fallo debe identificar los tipos involucrados.
4. **Ejecutar y verificar.** Compilar la solución y ejecutar las pruebas de
   arquitectura y las pruebas existentes aplicables. Verificar con una infracción
   controlada y reversible que una regla detecta una dependencia prohibida;
   retirar esa infracción antes de entregar. Resolver los problemas introducidos
   por la integración y registrar por separado los incumplimientos preexistentes.
5. **Documentar el uso.** Agregar al README el comando real para ejecutar las
   pruebas y una explicación breve de cómo leer un fallo. Indicar qué reglas
   están automatizadas y cuáles siguen dependiendo de revisión manual.

## Reglas de la primera integración

- **Dominio:** no depender de Aplicación, Infraestructura ni Presentación, ni
  de Entity Framework, Blazor o ASP.NET Core.
- **Aplicación:** no depender de implementaciones de Infraestructura ni de
  Presentación; no usar DbContext, repositorios concretos ni componentes Blazor.
- **Presentación:** no depender directamente de Infraestructura ni utilizar
  repositorios o DbContext para acceder a datos; utilizar servicios y contratos
  de Aplicación.
- **Composición:** respetar la excepción de Program.cs y los métodos de registro
  de dependencias, que sí pueden configurar implementaciones de Infraestructura.
  Esa excepción no debe extenderse a los componentes.

Las reglas se aplican a tipos compilados seleccionados por sus namespaces y
responsabilidades, aunque varias capas pertenezcan al mismo proyecto. Deben
considerar las referencias que la herramienta pueda detectar, no limitarse a
buscar directivas `using` en los archivos.

## Criterios de aceptación

- Un compañero puede clonar, restaurar y ejecutar las pruebas en Windows con
  el SDK .NET 10 y acceso a NuGet, sin instalaciones globales adicionales.
- Las pruebas aparecen en Rider y se ejecutan con `dotnet test`.
- Las selecciones de tipos cubren las capas existentes; una selección vacía
  accidental no se presenta como comprobación exitosa de una capa.
- Una dependencia prohibida produce un fallo comprensible y las dependencias
  permitidas por las reglas no provocan falsos positivos.
- Las pruebas estructurales no se omiten, desactivan ni debilitan para ocultar
  incumplimientos. Cualquier excepción adicional requiere justificación y revisión.
- Se registra qué se ejecutó, qué pasó, qué falló y cualquier bloqueo del entorno.

## Alcance y límites

ArchUnitNET comprueba las reglas que escribamos; no certifica automáticamente
toda Clean Architecture, SOLID ni el comportamiento funcional. Es independiente
del análisis de SonarQube y no automatiza sus comandos.

Esta primera integración se centra en dependencias entre capas. No incluye
automatización de CI/CD, convenciones completas de nombres ni nuevas reglas
entre módulos sin contratos previamente definidos. Si aparecen incumplimientos
preexistentes que requieren cambios en la aplicación, presentar la corrección
concreta para revisar su alcance antes de realizar una refactorización amplia.

## Estado

Requisitos propuestos para revisión. Solo se preparó este documento; todavía no
se creó el proyecto de arquitectura, instaló ArchUnitNET ni ejecutaron sus reglas.
Después de revisar los requisitos se prepararán `specs.md` y `tasks.md`.

Referencia: [documentación oficial de ArchUnitNET](https://github.com/TNG/ArchUnitNET/blob/main/documentation/docs/guide.md).
