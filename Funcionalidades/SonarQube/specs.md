# Diseño técnico: SonarQube local

## Responsabilidades

SonarQube es una herramienta de desarrollo externa a SolarUy. Se ejecutará en
Docker y recibirá los análisis enviados desde Windows. Esta integración no
modifica las capas de los módulos, Home.razor ni el simulador.

## Servidor y persistencia

Agregar el servicio sonarqube al compose.yaml existente, con imagen de Community
Build fijada a una versión verificada, puerto local 9000 y volúmenes propios.
La instancia será para evaluación local. No reutilizar MySQL de SolarUy ni
borrar sus volúmenes. Verificar requisitos del motor Docker y recursos de la versión
seleccionada; un contenedor iniciado no implica que el servidor esté operativo.

El entorno principal compatible será Docker Desktop en Windows con contenedores
Linux y Compose desde PowerShell. No exigir comandos exclusivos de Linux, rutas
de WSL ni una instalación manual de Docker Engine en una distribución Linux.
Docker Desktop puede administrar su propio backend WSL 2 según su configuración;
esto no requiere que el usuario trabaje en una terminal Linux. Admitir también
Docker Engine en WSL como alternativa usando el mismo compose.yaml.

## Scanner y autenticación

Registrar dotnet-sonarscanner con versión fija en el manifiesto dotnet-tools.json
existente. Ejecutarlo en Windows, donde está el SDK .NET 10 y se compila SolarUy.
El servidor tendrá un proyecto con clave SolarUY y un token con permiso de análisis.
El token se proporcionará durante la sesión usando el mecanismo admitido por
esa versión del scanner, sin escribirlo en archivos versionados ni documentación.
Actualizar .gitignore para los archivos generados por el análisis y comprobar
que no se envíen secretos locales al servidor.

## Ejecución y resultados

Desde la raíz del repositorio, begin configura el análisis, la compilación de
SolarUy.sln sin compilación incremental permite analizar el código y end envía
el reporte. Comprobar códigos de salida y detener la secuencia ante errores.
Verificar después que el servidor termine de procesarlo y consultar su Quality
Gate. Envío, procesamiento y gate aprobado son resultados distintos.

Ejecutar las pruebas existentes aplicables y registrar su resultado por separado.
No se introduce comportamiento de Dominio que requiera nuevas pruebas unitarias.
La cobertura no se importa en esta primera integración. Tampoco se incluyen
scripts nuevos, CI/CD ni correcciones de hallazgos. No relajar reglas o umbrales
para conseguir un gate aprobado.

## Verificación y documentación

Validar Compose en Docker Desktop desde PowerShell, disponibilidad desde Windows,
persistencia y restauración del
scanner. Comprobar el alcance real de archivos analizados, incluido el tratamiento
de Razor, sin asumir que todos los tipos reciben las mismas comprobaciones.
Documentar comandos y versiones probados en Docs/ReadmeSonarQube.md y las
secciones afectadas del README al terminar la integración.

El agente realizará la implementación y las comprobaciones mediante las
herramientas disponibles. Si no tiene acceso al motor Docker o a la consola,
registrará el bloqueo y solicitará solo la intervención concreta necesaria.
No presentará verificaciones pendientes como exitosas.
