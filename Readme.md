# SolarUY

Prueba de concepto académica para gestionar parques solares, organizada en una
solución .NET 10 con frontend Blazor WebAssembly y backend ASP.NET Core Web API.

## Organización actual

```text
SolarUy.sln
SolarUyFont/
SolarUyBack/
    Modulos/
        Usuarios/
        Parques/
        Tareas/
        Mediciones/
    Compartido/
        Aplicacion/
        Infraestructura/
SolarUyPruebas/
Funcionalidades/
```

| Ruta | Contenido |
| --- | --- |
| SolarUyFont/ | Proyecto Blazor WebAssembly con la interfaz de plantilla. |
| SolarUyBack/ | Proyecto ASP.NET Core con configuración de controladores y OpenAPI. |
| SolarUyBack/Modulos/ | Carpetas de Usuarios, Parques, Tareas y Mediciones, con archivos marcadores y sin código. |
| SolarUyBack/Compartido/ | Carpetas de aplicación para eventos e infraestructura para persistencia y eventos, con archivos marcadores y sin código. |
| SolarUyPruebas/ | Carpetas vacías para pruebas unitarias, de integración y de arquitectura; no es todavía un proyecto .NET. |
| Funcionalidades/ | Requisitos, diseños y tareas de las funcionalidades. |

Los cuatro módulos tienen la misma estructura de carpetas: Controladores, DTOs,
Interfaz, Aplicacion, Dominio e Infraestructura. Interfaz distingue Apis/Remota,
Apis/Local y Eventos/IN y OUT. Aplicacion contiene Servicios e Implementaciones;
Infraestructura contiene Persistencia e Identidad.

Esta estructura reserva la organización del backend. Los módulos no contienen
código ni implementan todavía APIs, eventos, persistencia o identidad. El
[diseño de la estructura](Funcionalidades/ArquitecturaBackend/spec.md) detalla
las responsabilidades previstas y la conservación de las carpetas en Git.

Las subcarpetas sin código de Modulos y Compartido contienen archivos `.gitkeep`
vacíos para conservar su estructura al incluirlos en un commit y clonar el
repositorio. SolarUyPruebas contiene carpetas vacías sin marcadores; Git no
conserva esa estructura por sí sola.

## Compilación y verificación

Con el SDK de .NET 10, desde la raíz de la solución:

```bash
dotnet build
```

La estructura se verificó contra el diseño y la solución compiló sin errores ni
advertencias. No existen proyectos de pruebas ni configuración de ArchUnitNET o
SonarQube; esos controles están pendientes.
