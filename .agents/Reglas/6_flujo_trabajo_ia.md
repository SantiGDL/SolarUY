Instrucciones obligatorias de flujo de trabajo — SolarUY
El agente debe generar código que respete los requisitos, las reglas de diseño
y los controles de calidad del proyecto.

1. Lectura y aplicación de las reglas
- Antes de generar o modificar código, leer las reglas aplicables de
  Clean Architecture, nomenclatura, SOLID y modelado de dominio.
- Resolver las rutas según los archivos reales del repositorio.
  No asumir nombres o archivos que todavía no existen.
- Respetar los requisitos y las especificaciones de la funcionalidad.
- Si las instrucciones presentan una contradicción que afecta la implementación,
  explicarla y solicitar aclaración antes de realizar el cambio dependiente.
- No crear funcionalidades ni configurar herramientas por la mera existencia
  de estas reglas.

2. Verificación de los cambios
- Después de un conjunto coherente de cambios y antes de entregar el trabajo,
  compilar y ejecutar las verificaciones aplicables.
- Localizar la solución y los proyectos de pruebas en el repositorio.
- Utilizar dotnet build sobre la solución o el proyecto correspondiente.
- Ejecutar con dotnet test las pruebas de Dominio y las pruebas estructurales
  existentes que correspondan a los cambios.
- Si existe CodeQuality.Tests, localizar su archivo de proyecto y ejecutar
  dotnet test indicando esa ruta.
- Si el proyecto utiliza otro nombre o un procedimiento de verificación
  documentado, seguir la configuración real.
- No es necesario ejecutar todas las pruebas después de cada edición pequeña.
  Repetir las verificaciones afectadas después de corregir un fallo.
- No considerar que una compilación exitosa demuestra por sí sola
  el cumplimiento de las reglas arquitectónicas.

3. Pruebas estructurales con ArchUnitNET
- Ejecutar las pruebas de ArchUnitNET cuando estén configuradas.
- Si una prueba falla, leer el diagnóstico y corregir el código fuente
  que incumple la regla.
- Volver a ejecutar las pruebas afectadas hasta resolver el problema.
- No trasladar al usuario la corrección de un error introducido por el agente.
- Si las pruebas estructurales todavía no existen, revisar manualmente
  las dependencias y declarar que la verificación automática no está disponible.
- No crear ni instalar ArchUnitNET o un proyecto de calidad automáticamente
  sin que esa configuración forme parte de la tarea solicitada.

4. Pruebas unitarias de Dominio
- Cada vez que se agregue o modifique comportamiento de Dominio,
  agregar o actualizar las pruebas unitarias correspondientes con xUnit.
- Cubrir el comportamiento requerido, las validaciones y los casos límite
  relevantes para el cambio.
- No agregar pruebas triviales únicamente para comprobar propiedades
  automáticas o reproducir la implementación.
- Si cambia un requisito, actualizar la prueba para reflejar el nuevo
  comportamiento y explicar el motivo.
- No cambiar los resultados esperados de una prueba para ocultar
  un defecto del código.
- Si no existe un proyecto de pruebas de Dominio o xUnit no está configurado,
  informar la situación y solicitar la definición necesaria para cumplir
  esta obligación; no afirmar que el comportamiento fue probado.
- Para Aplicación, Infraestructura y Presentación, no generar pruebas unitarias
  ni proponer su creación por iniciativa propia. Esto no impide ejecutar
  las pruebas existentes ni cumplir una solicitud explícita del usuario.
- Estas reglas exigen pruebas de comportamiento de Dominio; no imponen TDD.
  Si se solicita TDD, escribir primero la prueba, comprobar que falla por
  el comportamiento faltante y luego implementar lo necesario para que pase.

5. Integridad de las pruebas estructurales
- No modificar, eliminar, omitir ni desactivar pruebas de CodeQuality.Tests
  o sus equivalentes para conseguir que el código pase.
- No relajar reglas, exclusiones o configuración del análisis para ocultar
  una dependencia o estructura incorrecta.
- Corregir el código fuente para que respete las reglas acordadas.
- Si una regla parece incorrecta, desactualizada o incompatible con una
  decisión de diseño aprobada, detener el cambio afectado, explicar
  el conflicto y solicitar autorización explícita antes de modificar
  las pruebas o su configuración.
- Esta protección se refiere a las pruebas estructurales y no impide
  agregar las pruebas unitarias de Dominio exigidas por este documento.

6. Análisis con SonarQube
- Si SonarQube está configurado, seguir el procedimiento de análisis
  documentado por el proyecto y utilizar sus comandos reales.
- No asumir que dotnet test ejecuta un análisis de SonarQube.
- Revisar los hallazgos aplicables al código modificado y corregir
  los problemas introducidos por el cambio.
- No desactivar reglas, agregar supresiones ni modificar el umbral de calidad
  para ocultar problemas sin una justificación y autorización explícita.
- No instalar ni configurar SonarQube si no forma parte de la tarea.
- Si el análisis depende de un pipeline o servicio que el agente no puede
  ejecutar o consultar, indicar que su resultado está pendiente.
- No afirmar que el análisis o el control de calidad de SonarQube pasó
  sin disponer de su resultado.

7. Fallos y bloqueos
- Distinguir errores introducidos por los cambios, fallos preexistentes
  y bloqueos del entorno.
- Corregir los errores introducidos por el agente y repetir la verificación.
- No realizar cambios ajenos al alcance para resolver fallos preexistentes
  sin evaluar y explicar su relación con la tarea.
- Si falta el SDK, no se pueden restaurar dependencias o una herramienta
  está inaccesible, informar el bloqueo y las verificaciones pendientes.
- No presentar verificaciones omitidas, bloqueadas o fallidas como exitosas.

8. Entrega del trabajo
- Resumir qué se cambió y qué verificaciones se ejecutaron.
- Informar el resultado de compilación, pruebas y análisis aplicables.
- Declarar expresamente los fallos o comprobaciones pendientes.
- No afirmar que todas las reglas se validan automáticamente:
  ArchUnitNET solo comprueba las reglas implementadas en sus pruebas,
  y los análisis automáticos no sustituyen la revisión del diseño.