# SolarUY

Prueba de concepto académica para gestionar parques solares, organizada en una
solución .NET 10 con frontend Blazor WebAssembly y backend ASP.NET Core Web API.

## Organización actual

| Ruta | Contenido |
| --- | --- |
| SolarUyFont/ | Proyecto Blazor WebAssembly con la interfaz de plantilla. |
| SolarUyBack/ | Proyecto ASP.NET Core con configuración de controladores y OpenAPI. |
| SolarUyBack/Backend/Modulos/ | Carpetas vacías de Usuarios, Parques, Tareas y Mediciones. |
| SolarUyBack/Backend/Compartido/ | Carpetas vacías de aplicación para eventos e infraestructura para persistencia y eventos. |
| SolarUyPruebas/ | Carpetas vacías para pruebas unitarias, de integración y de arquitectura; no es todavía un proyecto .NET. |
| Funcionalidades/ | Requisitos, diseños y tareas de las funcionalidades. |

Los cuatro módulos tienen la misma estructura de carpetas: Controladores, DTOs,
Interfaz, Aplicacion, Dominio e Infraestructura. Interfaz distingue Apis/Remota,
Apis/Local y Eventos/IN y OUT. Aplicacion contiene Servicios e Implementaciones;
Infraestructura contiene Persistencia e Identidad.

Esta estructura reserva la organización del backend. Los módulos no contienen
código ni implementan todavía APIs, eventos, persistencia o identidad. El
[diseño de la estructura](Funcionalidades/ArquitecturaBackend/spec.md) detalla el
árbol completo y las responsabilidades previstas.

Las carpetas vacías existen localmente, pero Git no las registra. No contienen
archivos marcadores, por lo que no se conservan por sí solas en un commit o un clon.

## Compilación y verificación

Con el SDK de .NET 10, desde la raíz de la solución:

```bash
dotnet build
```

La estructura se verificó contra el diseño y la solución compiló sin errores ni
advertencias. No existen proyectos de pruebas ni configuración de ArchUnitNET o
SonarQube; esos controles están pendientes.
