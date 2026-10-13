# Instrucciones para el agente — SolarUY

## Objetivo y prioridades
SolarUY es una prueba de concepto académica para gestionar parques solares.
Priorizar una solución funcional, sencilla y comprensible para el equipo.
Trabajar de forma incremental. Evitar abstracciones, dependencias y funcionalidades
que no estén justificadas por los requisitos. Explicar las decisiones relevantes en español.

## Decisiones actuales
- Arquitectura del backend: monolito modular, con responsabilidades y límites explícitos.
- SolarUyFront: Blazor WebAssembly Standalone; componentes y lógica de interfaz en C#.
- SolarUyBack: ASP.NET Core Web API con controladores.
- Comunicación ordinaria: HTTP y JSON. Monitoreo en tiempo real: SignalR.
- Mantener los proyectos en la misma solución y repositorio.
- Conservar la versión de .NET declarada en los proyectos; no cambiarla sin acordarlo.
- Autenticación/autorización externa compatible con OpenID Connect, según la consigna.
- Captura de mediciones mediante un simulador del inversor, no hardware real.

## Cambios en las instrucciones del agente
- No modificar AGENTS.md ni los archivos de Reglas/ sin una
  instrucción explícita o aprobación del equipo.
- Si una regla es ambigua, está desactualizada o impide resolver
  una tarea, explicar el problema y proponer el cambio.
- No eliminar ni debilitar reglas para facilitar una implementación
  o hacer pasar los controles.
## Antes de realizar cambios
1. Inspeccionar la estructura y el código relevante existentes.
2. Leer Reglas/arquitectura.md y Reglas/convenciones.md, si existen.
3. Leer requirement.md, spec.md y task.md de la funcionalidad en Funcionalidades/.
4. Si falta un documento necesario, indicarlo y proponer una primera versión.
5. No inventar requisitos, campos o decisiones de negocio. Señalar las dudas que
   afecten el comportamiento y pedir su resolución antes de implementarlo.
6. Cargar únicamente la documentación relevante para la tarea.

## Separación de responsabilidades
- Los componentes de presentación manejan formularios, estado visual y mensajes.
- Los servicios cliente encapsulan las peticiones a la API y la conexión de monitoreo.
- El frontend no accede a la base de datos ni referencia la implementación del backend.
- Los controladores reciben solicitudes, delegan en servicios de aplicación y traducen
  los resultados a respuestas HTTP. No acceden directamente a la persistencia.
- Los servicios de aplicación coordinan los casos de uso.
- El dominio contiene entidades y reglas de negocio. No depende de infraestructura,
  Entity Framework, controladores ni componentes de interfaz.
- Infraestructura implementa persistencia e integraciones externas.
- Cada módulo expone contratos explícitos. No acceder a detalles internos de otro módulo.
- La definición concreta de módulos, carpetas y dependencias se documentará en
  Reglas/arquitectura.md antes de realizar una reorganización amplia.

## Datos y seguridad
- Usar DTO para las solicitudes y respuestas de la API; no exponer directamente entidades.
- Validar en el servidor y verificar permisos antes de ejecutar operaciones protegidas.
- La validación del navegador mejora la experiencia; no sustituye la validación del backend.
- No guardar secretos, tokens o contraseñas en el código, documentación o repositorio.
- No implementar autenticación propia como sustituto del proveedor externo requerido.
- Mostrar errores comprensibles sin revelar secretos ni detalles internos.
- En monitoreo, distinguir datos actuales de datos desactualizados por desconexión.

## Flujo por funcionalidad
1. requirement.md: objetivo, alcance, criterios de aceptación y decisiones pendientes.
2. spec.md: diseño técnico, contratos, validaciones y estrategia de verificación.
3. Leer requirement.md de la funcionalidad en Funcionalidades/.
   Este documento contiene los requisitos definidos por el equipo.
   Si falta o tiene ambigüedades importantes, solicitar aclaración.

4. Si existen spec.md y task.md, leerlos y revisar su coherencia
   con requirement.md y las reglas del proyecto.
   Si no existen, generarlos como propuesta:
    - spec.md: diseño técnico de la solución.
    - task.md: tareas pequeñas y verificables para implementarla.
      Solicitar revisión del equipo antes de implementar.
    - Cuando cambie requirement.md, actualizar spec.md y task.md para mantenerlos coherentes, sin implementar los cambios salvo que esté autorizado.
5. Implementar las tareas acordadas sin cambios ajenos a la funcionalidad.
6. Actualizar la documentación en el Readme.md de la raíz y marcar solo tareas realmente completadas.

## Reglas de documentación del proyecto

README.md describe la organización y el funcionamiento efectivamente
implementados y verificados del proyecto.

Actualizar README.md únicamente cuando:
- El equipo haya solicitado implementar el requerimiento.
- La implementación esté terminada.
- Se haya verificado el cumplimiento de sus criterios de aceptación.

Preparar, revisar o aprobar requirement.md, spec.md o task.md no autoriza
por sí solo a crear o modificar README.md. Durante esas etapas, mantener
las propuestas y decisiones pendientes en Funcionalidades/.

Una decisión aprobada pero todavía no implementada no debe describirse
en README.md como parte del funcionamiento actual.

Si la implementación queda incompleta o su cumplimiento no pudo
verificarse, registrar el avance y los pendientes en task.md.

Una solicitud explícita del equipo para corregir o actualizar README.md
permite editarlo, conservando estas reglas de veracidad.

Actualizar las secciones existentes antes de crear otras, evitando
información duplicada o contradictoria. Describir el estado actual,
sin redactar un historial de cambios como “se agregó...” o “se modificó...”.

Incluir únicamente secciones relevantes con información confirmada.
## Verificación y revisión
- Ejecutar dotnet build desde la carpeta de la solución.
- Ejecutar dotnet test cuando existan proyectos de pruebas.
- Incorporar pruebas de comportamiento relevantes para los criterios de aceptación.
- Verificar reglas estructurales mediante ArchUnitNET cuando esté configurado.
- Ejecutar el análisis de SonarQube mediante el procedimiento documentado cuando esté
  configurado; revisar los resultados y corregir los problemas pertinentes.
- Si una herramienta todavía no está configurada, informar el pendiente. No afirmar
  que se ejecutó ni que el código pasó controles que no se realizaron.
- No debilitar pruebas ni desactivar reglas solo para hacer pasar los controles.
- Al finalizar, explicar cambios, verificaciones realizadas y pendientes.
- El equipo revisa el código y debe poder explicar su funcionamiento.

## Convenciones iniciales
- Usar nombres descriptivos en español para el código propio, conservando los nombres
  exigidos por frameworks y bibliotecas. Respetar las convenciones existentes.
- Preferir operaciones asíncronas para acceso a datos y comunicaciones.
- Mantener código legible y funciones con responsabilidades claras.
- Comentar decisiones poco evidentes; evitar comentarios que repitan el código.
- No cambiar dependencias, arquitectura o configuración de despliegue sin justificarlo
  y obtener acuerdo cuando implique una decisión nueva del equipo.

## Decisiones pendientes
- Módulos y contratos entre ellos.
- Base de datos y herramienta de persistencia.
- Proveedor de identidad y roles concretos.
- Biblioteca de componentes visuales, si se utiliza.
- Entorno de despliegue y estrategia de alta disponibilidad durante actualizaciones.
- Configuración y comandos concretos de SonarQube y ArchUnitNET.

Este archivo contiene pautas globales. Los detalles de arquitectura y convenciones
pertenecen a Reglas/; los detalles de cada funcionalidad, a Funcionalidades/.
