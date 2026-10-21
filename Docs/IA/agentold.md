# Instrucciones para el agente — SolarUY

## Objetivo y prioridades

SolarUY es una prueba de concepto académica para gestionar parques solares.

Priorizar una solución funcional, sencilla y comprensible para el equipo.
Trabajar de forma incremental. Evitar abstracciones, dependencias y funcionalidades
que no estén justificadas por los requisitos.

Explicar las decisiones relevantes en español.
El equipo debe poder comprender y explicar la solución implementada.

## Decisiones actuales

- Arquitectura del backend: monolito modular, con responsabilidades y límites explícitos.
- SolarUyFront: Blazor WebAssembly Standalone; componentes y lógica de interfaz en C#.
- SolarUyBack: ASP.NET Core Web API con controladores.
- Comunicación ordinaria: HTTP y JSON.
- Monitoreo en tiempo real: SignalR.
- Mantener los proyectos en la misma solución y repositorio.
- Conservar la versión de .NET declarada en los proyectos; no cambiarla sin acordarlo.
- Autenticación/autorización externa compatible con OpenID Connect, según la consigna.
- Captura de mediciones mediante un simulador del inversor, no hardware real.

## Cambios en las instrucciones del agente

- No modificar AGENTS.md ni los archivos de Reglas/ sin una instrucción explícita
  o aprobación del equipo.
- Si una regla es ambigua, está desactualizada o impide resolver una tarea,
  explicar el problema y proponer el cambio.
- No eliminar ni debilitar reglas para facilitar una implementación
  o hacer pasar los controles.

## Antes de realizar cambios

1. Inspeccionar la estructura y el código relevante existentes.
2. Leer Reglas/arquitectura.md y Reglas/convenciones.md, si existen.
3. Leer requirement.md, spec.md y task.md de la funcionalidad en Funcionalidades/,
   según la etapa de trabajo y cuando existan.
4. Si falta un documento necesario, indicarlo y seguir el procedimiento
   definido en la sección Flujo por funcionalidad.
5. No inventar requisitos, campos ni decisiones de negocio. Señalar las dudas
   que afecten el comportamiento y pedir su resolución antes de implementar
   la parte afectada.
6. Cargar únicamente la documentación relevante para la tarea.

## Separación de responsabilidades

- Los componentes de presentación manejan formularios, estado visual y mensajes.
- Los servicios cliente encapsulan las peticiones a la API y la conexión de monitoreo.
- El frontend no accede a la base de datos ni referencia la implementación del backend.
- Los controladores reciben solicitudes, delegan en servicios de aplicación
  y traducen los resultados a respuestas HTTP.
  No acceden directamente a la persistencia.
- Los servicios de aplicación coordinan los casos de uso.
- El dominio contiene entidades y reglas de negocio.
  No depende de infraestructura, Entity Framework, controladores
  ni componentes de interfaz.
- Infraestructura implementa persistencia e integraciones externas.
- Cada módulo expone contratos explícitos.
  No acceder a detalles internos de otro módulo.
- La definición concreta de módulos, carpetas y dependencias se documentará
  en Reglas/arquitectura.md antes de realizar una reorganización amplia.

## Datos y seguridad

- Usar DTO para las solicitudes y respuestas de la API;
  no exponer directamente entidades.
- Validar en el servidor y verificar permisos antes de ejecutar
  operaciones protegidas.
- La validación del navegador mejora la experiencia;
  no sustituye la validación del backend.
- No guardar secretos, tokens ni contraseñas en el código,
  documentación o repositorio.
- No implementar autenticación propia como sustituto
  del proveedor externo requerido.
- Mostrar errores comprensibles sin revelar secretos ni detalles internos.
- En monitoreo, distinguir datos actuales de datos desactualizados
  por desconexión.

## Flujo por funcionalidad

### 1. Definir y revisar los requisitos

Leer requirement.md de la funcionalidad en Funcionalidades/.

Este documento contiene los requisitos definidos y validados por el equipo:
- Objetivo.
- Alcance.
- Comportamiento esperado y reglas de negocio.
- Criterios de aceptación.
- Decisiones pendientes.

La IA puede ayudar a redactar y organizar este documento a partir
de la información proporcionada por el equipo.
No debe completar información faltante con requisitos inventados.

Si requirement.md no existe, proponer un borrador con la información disponible
y señalar las dudas pendientes. Solicitar su revisión antes de diseñar
o implementar las partes cuyo comportamiento todavía no esté definido.

Si contiene ambigüedades importantes, pedir aclaración antes
de implementar las partes afectadas.

### 2. Preparar el diseño y las tareas

Si existen spec.md y task.md, leerlos y revisar su coherencia
con requirement.md y las reglas del proyecto.

Si no existen, generarlos como propuesta:

- spec.md: diseño técnico de la solución, componentes involucrados,
  contratos, validaciones y estrategia de verificación.
- task.md: tareas pequeñas y verificables para implementar la funcionalidad.

Solicitar revisión del equipo antes de implementar las propuestas.
Si la solicitud actual ya aprueba expresamente el diseño y autoriza
su implementación, continuar dentro de ese alcance.

Cuando cambie requirement.md, actualizar spec.md y task.md
para mantenerlos coherentes. La actualización de estos documentos
no autoriza por sí sola a implementar los cambios.

### 3. Implementar

Implementar las tareas acordadas de forma incremental.

- Mantener los cambios dentro del alcance de la funcionalidad.
- Respetar las reglas de arquitectura y convenciones.
- Evitar cambios ajenos a la tarea.
- Incorporar las verificaciones correspondientes a los criterios de aceptación.

### 4. Verificar y registrar el estado

Ejecutar las verificaciones definidas para la funcionalidad
y los controles disponibles del proyecto.

Marcar en task.md únicamente las tareas realmente completadas.
Registrar avances, limitaciones y pendientes cuando la implementación
o su verificación estén incompletas.

Actualizar README.md cuando se cumplan las condiciones de la sección
Reglas de documentación del proyecto.

## Reglas de documentación del proyecto

### Propósito y estilo del README

README.md presenta SolarUY a una persona que todavía no conoce el proyecto.

Debe explicar:
- Qué es el sistema y para qué sirve.
- Qué funcionalidades tiene implementadas.
- Cómo está organizado el proyecto.
- Qué se necesita para ejecutarlo.
- Cómo ejecutarlo y utilizarlo.

Redactar en español, con lenguaje natural, claro y accesible.
Priorizar la descripción del proyecto y de su funcionamiento
por encima de la explicación técnica de su implementación.

Usar términos técnicos cuando sean necesarios para comprender
o ejecutar el proyecto. Acompañarlos de una explicación breve
cuando no resulten evidentes para el lector.

Escribir como documentación permanente del proyecto,
no como una respuesta del agente al equipo:

- No usar frases como “como solicitaste”, “te explico” o “ahora podés”.
- No describir acciones del agente como “implementé”, “agregué”
  o “verifiqué”.
- No redactar un historial de cambios como “se agregó”
  o “se modificó”.
- Describir directamente el estado actual:
  “El sistema permite...” o “Esta carpeta contiene...”.

Los detalles técnicos de arquitectura y convenciones pertenecen a Reglas/.
Los requisitos, diseños y tareas de cada funcionalidad pertenecen
a Funcionalidades/.

El README puede resumir esa información y enlazar los documentos
correspondientes, sin duplicar sus explicaciones detalladas.

### Organización del proyecto

Incluir un árbol de carpetas en un bloque de texto que muestre
la estructura principal existente del repositorio.

- Mostrar solo las carpetas y archivos relevantes para orientarse.
- Excluir carpetas generadas, dependencias y archivos temporales,
  como bin/, obj/ y .git/.
- Acompañar el árbol con una descripción breve y sencilla
  del propósito de cada carpeta o proyecto principal.
- Reflejar únicamente la estructura existente.
  No presentar carpetas propuestas como si ya estuvieran creadas.
- Actualizar el árbol cuando cambie la organización del proyecto.

### Veracidad y condiciones de actualización

README.md describe la organización y el funcionamiento efectivamente
implementados y verificados del proyecto.

Actualizar README.md como parte de una funcionalidad únicamente cuando:

- El equipo haya solicitado implementar el requerimiento.
- La implementación esté terminada.
- Se haya verificado el cumplimiento de sus criterios de aceptación.

Preparar, revisar o aprobar requirement.md, spec.md o task.md
no autoriza por sí solo a crear o modificar README.md.
Durante esas etapas, mantener las propuestas y decisiones pendientes
en Funcionalidades/.

Una decisión aprobada pero todavía no implementada no debe describirse
en README.md como parte del funcionamiento actual.

Si la implementación queda incompleta o su cumplimiento no pudo
verificarse, registrar el avance y los pendientes en task.md.

Una solicitud explícita del equipo para corregir o actualizar README.md
permite editarlo, conservando estas reglas de veracidad.

Actualizar las secciones existentes antes de crear otras.
Evitar información duplicada o contradictoria.
Incluir únicamente secciones relevantes con información confirmada.

Documentar comandos de ejecución comprobados y los requisitos necesarios
para utilizarlos, sin incluir secretos ni credenciales.

## Verificación y revisión

- Ejecutar dotnet build desde la carpeta de la solución.
- Ejecutar dotnet test cuando existan proyectos de pruebas.
- Incorporar pruebas de comportamiento relevantes
  para los criterios de aceptación.
- Verificar reglas estructurales mediante ArchUnitNET
  cuando esté configurado.
- Ejecutar el análisis de SonarQube mediante el procedimiento documentado
  cuando esté configurado.
  Revisar los resultados y corregir los problemas pertinentes.
- Si una herramienta todavía no está configurada, informar el pendiente.
  No afirmar que se ejecutó ni que el código pasó controles
  que no se realizaron.
- No debilitar pruebas ni desactivar reglas solo para hacer pasar los controles.
- Al finalizar, explicar al equipo los cambios, las verificaciones realizadas
  y los pendientes.
- El equipo revisa el código y debe poder explicar su funcionamiento.

El informe final del agente puede dirigirse al equipo y describir
el trabajo realizado. Ese informe es independiente de la documentación
permanente del repositorio.

## Convenciones iniciales

- Usar nombres descriptivos en español para el código propio,
  conservando los nombres exigidos por frameworks y bibliotecas.
  Respetar las convenciones existentes.
- Preferir operaciones asíncronas para acceso a datos y comunicaciones.
- Mantener código legible y funciones con responsabilidades claras.
- Comentar decisiones poco evidentes;
  evitar comentarios que repitan el código.
- No cambiar dependencias, arquitectura ni configuración de despliegue
  sin justificarlo y obtener acuerdo cuando implique
  una decisión nueva del equipo.

## Decisiones pendientes

- Módulos y contratos entre ellos.
- Base de datos y herramienta de persistencia.
- Proveedor de identidad y roles concretos.
- Biblioteca de componentes visuales, si se utiliza.
- Entorno de despliegue y estrategia de alta disponibilidad
  durante actualizaciones.
- Configuración y comandos concretos de SonarQube y ArchUnitNET.

## Alcance de los documentos

Este archivo establece las pautas globales de trabajo del agente.

- Reglas/ contiene los detalles de arquitectura y convenciones.
- Funcionalidades/ contiene los requisitos, diseños y tareas
  de cada funcionalidad.
- README.md contiene la presentación y guía de uso del proyecto
  en su estado actual.