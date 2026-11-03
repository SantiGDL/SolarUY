# Instalación y primer análisis con SonarQube

## Objetivo

Instalar SonarQube localmente mediante Docker y realizar el primer análisis
de SolarUy, con un procedimiento sencillo que se pueda repetir.

## Preparación

Agregar un servicio de SonarQube al `compose.yaml` del proyecto. Docker
descargará su imagen al iniciarlo. La consola web estará disponible en el
puerto local `9000` y los datos se conservarán entre reinicios del contenedor.

## Plan de cinco pasos

1. **Levantar SonarQube:** iniciar su contenedor mediante Docker Compose y
   esperar a que el servicio termine de arrancar.
2. **Abrir la consola:** acceder a `http://localhost:9000`, iniciar sesión y
   cambiar la contraseña inicial.
3. **Crear el proyecto:** registrar `SolarUY` en SonarQube y generar un token
   para enviar los análisis, sin guardarlo en Git.
4. **Instalar el scanner:** instalar SonarScanner para .NET en Windows y
   comprobar que puede comunicarse con el servidor local.
5. **Ejecutar y revisar el análisis:** analizar la solución `SolarUy.sln`,
   compilarla durante el proceso y consultar los resultados y el Quality Gate
   en la consola web.

## Resultado esperado

- SonarQube funciona localmente y muestra el primer análisis de SolarUy.
- La configuración funciona con Docker Desktop en Windows en modo contenedores
  Linux, ejecutando Compose desde PowerShell sin abrir una terminal WSL ni
  instalar manualmente Docker Engine en Linux. Docker Engine en WSL se admite
  como alternativa, sin ser un requisito del procedimiento.
- El scanner y .NET se ejecutan en Windows en ambos casos.
- Los pasos comprobados quedan documentados en la guía de SonarQube del proyecto.
- Se informa el resultado real del análisis y del Quality Gate, incluidos
  errores o verificaciones pendientes.

## Alcance

Este trabajo prepara la herramienta y realiza el primer análisis. La corrección
de los hallazgos se revisará después. No requiere modificar `Home.razor` ni las
funcionalidades de SolarUy, y no incluye integración con CI/CD ni despliegue
de SonarQube para producción.

## Estado

Requisitos adoptados como base del diseño y la planificación. Los cinco pasos
de implementación todavía no se han ejecutado.
