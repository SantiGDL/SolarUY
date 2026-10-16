# Reglas de documentación — SolarUY

## 1. Alcance de los documentos

- `AGENTS.md` establece las instrucciones generales del agente.
- `.agents/rules/` contiene las reglas del proyecto.
- `.agents/skills/` contiene los flujos de trabajo invocables.
- `requirements.md`, `specs.md` y `tasks.md` describen los requisitos,
  el diseño y las tareas de cada funcionalidad en su carpeta correspondiente.
- `README.md` presenta el proyecto y explica cómo ejecutarlo y utilizarlo
  en su estado actual.

Mantener cada contenido en su documento correspondiente.
Evitar repetir información que pueda quedar desactualizada o resultar
contradictoria. Usar enlaces a otros documentos cuando sea necesario.

## 2. Propósito y contenido del README

Escribir el README para una persona que todavía no conoce SolarUY.

Explicar con información confirmada:

- Qué es el sistema y para qué sirve.
- Qué funcionalidades están implementadas.
- Qué tecnologías utiliza y qué función cumple cada una.
- Qué arquitectura utiliza y cómo organiza las responsabilidades.
- Cómo están organizadas las carpetas y los proyectos principales.
- Qué requisitos y configuraciones necesita para ejecutarse.
- Cómo ejecutarlo y realizar las operaciones principales.
- Qué herramientas de pruebas y calidad están configuradas
  y cómo utilizar sus controles.

Incluir únicamente las secciones que correspondan al estado actual.

No convertir el README en una explicación detallada del código.
Los contratos, métodos, decisiones internas y diseños de funcionalidades
pertenecen a las especificaciones o a otros documentos técnicos.

## 3. Lenguaje y estilo

- Redactar en español, con lenguaje natural, claro y accesible.
- Priorizar qué hace el proyecto, cómo se utiliza y cómo orientarse
  dentro del repositorio.
- Utilizar términos técnicos cuando sean necesarios y explicar
  brevemente su propósito.
- No eliminar nombres de tecnologías o comandos para evitar tecnicismos.
- Evitar explicaciones extensas de patrones, bibliotecas o clases.
- Escribir como documentación permanente, no como una respuesta al equipo.
- No usar expresiones como “como solicitaste”, “te explico”
  o “ahora podés”.
- No describir acciones del agente como “implementé”, “agregué”
  o “verifiqué”.
- No convertir el README en un historial de cambios.
- Describir directamente el estado actual:
  “El sistema permite...” o “Esta carpeta contiene...”.

El informe final del agente puede explicar al equipo el trabajo realizado.
No copiar ese informe como contenido del README.

## 4. Tecnologías y arquitectura

- Identificar las tecnologías a partir de la configuración y el código reales.
- Explicar brevemente para qué se utiliza cada tecnología relevante.
- Documentar versiones únicamente cuando estén confirmadas
  y sean necesarias para ejecutar o comprender el proyecto.
- Describir el modo de Blazor realmente configurado.
- Explicar la arquitectura adoptada y sus límites principales
  sin reproducir todas las reglas de arquitectura.
- Distinguir la organización en módulos de la separación
  de responsabilidades entre capas.
- No presentar carpetas como prueba suficiente de que se respetan
  los límites arquitectónicos.
- Enlazar las reglas o documentos de diseño existentes
  cuando se necesite mayor detalle.

## 5. Estructura de carpetas

Incluir una representación breve y legible de la estructura principal
existente, mediante una lista con niveles o una tabla de rutas y propósitos.

- Inspeccionar la estructura real antes de documentarla.
- Mostrar únicamente carpetas y archivos relevantes para orientarse.
- Excluir dependencias, archivos temporales y carpetas generadas,
  como `bin/`, `obj/` y `.git/`.
- Respetar los nombres y las mayúsculas existentes.
- Explicar brevemente el propósito de los directorios
  y proyectos principales.
- No presentar carpetas propuestas como si ya existieran.
- No describir una carpeta vacía como una funcionalidad implementada.
- Actualizar esta sección cuando cambie la organización del proyecto.
- Si una carpeta vacía no se conserva al clonar y eso afecta la ejecución,
  documentar cómo se crea o prepara.
- No crear archivos marcadores únicamente para completar
  la estructura documentada.

## 6. Pruebas y herramientas de calidad

- Documentar xUnit, ArchUnitNET y SonarQube únicamente
  cuando estén configurados en el proyecto.
- Explicar de forma breve qué comprueba cada herramienta.
- Indicar los comandos o procedimientos reales para ejecutar
  pruebas y análisis.
- Aclarar si un control se ejecuta localmente, en un contenedor
  o mediante un pipeline.
- No afirmar que `dotnet test` ejecuta SonarQube,
  salvo que exista una integración comprobada que lo haga.
- No afirmar que todas las reglas arquitectónicas están automatizadas:
  ArchUnitNET comprueba las reglas implementadas en sus pruebas.
- No registrar resultados pasajeros como garantías permanentes,
  por ejemplo “todas las pruebas siempre pasan”.
- Si se incluyen insignias de estado, utilizar enlaces reales
  correspondientes al proyecto.
- Registrar los resultados de una ejecución concreta en el informe
  de la tarea o en el sistema de integración correspondiente.

## 7. Veracidad del contenido

- Describir únicamente la organización y el comportamiento
  existentes y comprobados.
- Diferenciar una estructura creada de una funcionalidad implementada.
- No presentar decisiones futuras como capacidades actuales.
- Identificar claramente el uso de mocks o simuladores.
  No describirlos como una integración con hardware real.
- No afirmar que una herramienta está configurada
  sin haberlo comprobado.
- No inventar comandos, rutas, configuraciones ni resultados.
- Cuando falte información, registrarla en el documento
  de trabajo correspondiente.
- Documentar las limitaciones actuales que afecten
  la ejecución o el uso del sistema.

## 8. Cuándo actualizar el README

- Al terminar una funcionalidad o tarea técnica autorizada,
  revisar si cambia la información necesaria para conocer,
  ejecutar o utilizar el proyecto.
- Actualizar las secciones afectadas cuando el cambio esté terminado
  y se hayan verificado sus criterios aplicables.
- No modificar el README si el cambio no afecta su contenido.
- Preparar o revisar `requirements.md`, `specs.md` o `tasks.md`
  no autoriza por sí solo a actualizar el README.
- Durante el diseño y la planificación, mantener las propuestas
  en los documentos de la funcionalidad.
- Si una implementación queda incompleta, registrar el avance
  y los pendientes en `tasks.md`, sin presentarla como terminada.
- Una funcionalidad incompleta no impide documentar otro cambio
  independiente que sí esté terminado y verificado.
- Una solicitud explícita para crear, corregir o actualizar el README
  permite editarlo respetando las reglas de veracidad.
- No sobrescribir contenido válido ni cambios del equipo
  ajenos a la tarea.

## 9. Requisitos, especificaciones y tareas

- `requirements.md` describe qué necesita el sistema:
  objetivo, alcance, comportamiento, criterios de aceptación
  y decisiones pendientes.
- `specs.md` describe cómo se propone implementarlo:
  responsabilidades, componentes, contratos, validaciones
  y estrategia de verificación.
- `tasks.md` divide el trabajo en tareas verificables
  y registra su estado.
- Mantener estos documentos coherentes cuando cambien los requisitos.
- Distinguir propuestas de decisiones aprobadas.
- Marcar únicamente tareas realmente completadas.
- Registrar limitaciones y verificaciones pendientes
  sin presentar el trabajo como terminado.
- Para tareas técnicas acotadas, no exigir estos tres documentos
  salvo que el flujo de trabajo aplicable lo indique.

## 10. Comandos, configuración y seguridad

- Documentar comandos comprobados y la carpeta
  desde la que se ejecutan.
- Indicar los requisitos previos y los pasos de configuración necesarios.
- Distinguir comandos del equipo local de los ejecutados
  dentro de contenedores.
- No incluir secretos, tokens, contraseñas ni credenciales reales.
- Usar valores de ejemplo claramente identificados.
- Explicar cómo proporcionar la configuración sensible
  mediante el mecanismo utilizado por el proyecto.
- No presentar una configuración de desarrollo como apta
  para producción sin una decisión y verificación específicas.

## 11. Mantenimiento

- Actualizar las secciones existentes antes de crear otras.
- Evitar contenido duplicado, innecesario o contradictorio.
- Comprobar que los enlaces internos apunten a archivos existentes.
- Mantener consistentes los nombres de archivos y rutas.
- Respetar el nombre real del README; no crear otro archivo
  que solo difiera en mayúsculas.
- No modificar `AGENTS.md`, reglas ni skills como parte
  de una actualización del README, salvo instrucción explícita del equipo.