# Tareas de implementación: SonarQube

Estado: integración operativa y primer análisis verificado; cambio de contraseña pendiente.
Responsable de las tareas: el agente. Si una operación requiere acceso o una
credencial que no está disponible, se informa el bloqueo y se solicita únicamente
la intervención necesaria; no se traslada toda la tarea al usuario.

- [x] 1 Revisar el entorno y seleccionar versiones compatibles de SonarQube
  Community Build y SonarScanner para .NET 10. Comprobar acceso a Docker Desktop
  en Windows con contenedores Linux y Compose desde PowerShell,
  recursos y puerto 9000. Finaliza con versiones y requisitos identificados,
  y cualquier impedimento registrado antes de iniciar servicios.
- [x] 2 Modificar compose.yaml para incorporar sonarqube con puerto 9000 y
  volúmenes propios. Validar la configuración, iniciar el servicio y comprobar
  disponibilidad y persistencia tras reiniciarlo, sin borrar datos de SolarUy.
  Comprobar compatibilidad con Docker Desktop sin exigir terminal WSL; registrar
  por separado cualquier verificación pendiente por falta de acceso al motor.
- [ ] 3 Configurar la instancia local: cambiar la contraseña inicial, crear el
  proyecto SolarUY y generar su token de análisis mediante las herramientas
  disponibles. Finaliza con el proyecto preparado y credenciales fuera de Git;
  si falta acceso, registrar el paso concreto que requiere intervención.
- [x] 4 Añadir dotnet-sonarscanner con versión fija a dotnet-tools.json,
  restaurarlo en Windows y actualizar .gitignore para las salidas generadas.
  Finaliza con el scanner disponible y los archivos sensibles y temporales
  fuera del conjunto versionado.
- [x] 5 Ejecutar begin, compilar SolarUy.sln y ejecutar end, comprobando cada
  resultado antes de avanzar. Ejecutar las pruebas existentes aplicables y
  comprobar en el servidor el procesamiento, los archivos analizados y el
  Quality Gate. Registrar los resultados reales y los bloqueos.
- [x] 6 Actualizar Docs/ReadmeSonarQube.md y las secciones afectadas del README
  con versiones y comandos comprobados, preparación inicial y uso posterior.
  Finaliza con instrucciones para Docker Desktop desde PowerShell y una
  alternativa para Docker Engine en WSL. El scanner se ejecuta en Windows.

## Resultados de verificación (2026-11-03)

- Compose validó en Docker Desktop desde Windows. Se descargó e inició
  sonarqube:26.10.0.133078-community; la API confirmó estado UP.
- El scanner local 12.0.0 se restauró y ejecutó con .NET 10 correctamente.
- Se creó el proyecto privado SolarUY y se generó un token temporal de análisis,
  revocado al terminar. No se guardaron credenciales en el repositorio.
- Debug no pudo compilar por archivos de SolarUy en uso. Release compiló y las
  35 pruebas pasaron. Con los analizadores activos apareció una advertencia
  S6966 existente en Program.cs; no se modificó la aplicación.
- El reporte fue procesado y el Quality Gate devolvió OK. Al ser el primer
  análisis base, la API devolvió una lista de condiciones vacía: no demuestra
  que futuros cambios o toda la deuda existente estén aprobados.
- El servidor conserva el mismo identificador, proyecto y resultado después
  de reiniciar el contenedor. La API registró 75 archivos, incluidos 20 Razor.
  Incluir un archivo no implica que todas sus partes reciban las mismas reglas.
- Métricas iniciales: 0 bugs, 0 vulnerabilidades y 5 code smells. La cobertura
  figura en 0 % porque no se importó un reporte; no representa pruebas fallidas.
- La tarea 3 queda parcialmente pendiente: el usuario debe elegir su contraseña
  administrativa en http://localhost:9000. El proyecto y el primer token ya
  fueron preparados por el agente. Para análisis posteriores se requiere un
  token nuevo, porque el token de verificación fue revocado.

No se incluyen cobertura importada, scripts nuevos, CI/CD ni correcciones de hallazgos.