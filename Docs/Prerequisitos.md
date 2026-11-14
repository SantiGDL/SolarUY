# Prerrequisitos de SolarUY en Windows

[← Volver al README versión 2](../Readme.v2.md)

Esta guía prepara las herramientas. La creación de credenciales y el arranque
del proyecto continúan en el README versión 2.

## Docker Desktop y Docker Compose

Seguir la [instalación oficial de Docker Desktop para Windows](https://docs.docker.com/desktop/setup/install/windows-install/),
comprobar sus requisitos e instalar el programa. Si se utiliza el backend WSL 2,
seguir también la [guía de Docker para WSL 2](https://docs.docker.com/desktop/features/wsl/).
Reiniciar Windows si la instalación lo solicita.

Abrir Docker Desktop, esperar a que su motor esté disponible y utilizar
**contenedores Linux**. Docker Desktop incluye Docker Compose. Desde PowerShell:

```powershell
docker version
docker compose version
```

`docker version` debe mostrar tanto el cliente como el servidor. Si no puede
conectarse al motor, comprobar que Docker Desktop esté iniciado antes de continuar.

## SDK .NET 10

Seguir la [instalación oficial de .NET en Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows)
y elegir el **SDK de .NET 10** para la arquitectura del equipo. El SDK incluye
las herramientas para compilar y ejecutar; instalar solamente el runtime no basta.

También se puede instalar con WinGet:

```powershell
winget install Microsoft.DotNet.SDK.10
```

Abrir una terminal nueva después de instalar y comprobar:

```powershell
dotnet --list-sdks
```

Debe aparecer una versión `10.x`. SolarUY tiene como destino `net10.0`.

## Repositorio y herramientas opcionales

Descargar o clonar el repositorio y abrir una terminal en la carpeta que contiene
`SolarUy.sln` y `compose.yaml`. Git se necesita para clonar y gestionar cambios;
una descarga ZIP permite obtener los archivos sin instalarlo.

Rider es opcional. Los comandos del README permiten trabajar desde PowerShell.
MySQL y Keycloak se ejecutan en Docker; MySQL Workbench es una interfaz opcional
cuya conexión con este entorno queda pendiente de validación práctica.

Con estas herramientas disponibles, continuar con
[la preparación y ejecución del proyecto](../Readme.v2.md#2-preparar-y-levantar-el-proyecto).
