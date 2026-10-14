# Reglas de documentación — SolarUY

## Alcance de los documentos

- AGENTS.md establece las instrucciones globales y el flujo de trabajo del agente.
- Reglas/ contiene las reglas de arquitectura, convenciones,
  verificación y documentación.
- Funcionalidades/ contiene los requisitos, diseños y tareas
  de cada funcionalidad.
- README.md presenta el proyecto y explica cómo ejecutarlo y utilizarlo
  en su estado actual.

Mantener cada contenido en su documento correspondiente.
Evitar repetir información que pueda quedar desactualizada
o resultar contradictoria. Usar enlaces cuando sea necesario.

## Propósito del README

Escribir README.md para una persona que todavía no conoce SolarUY.

Explicar únicamente con información confirmada:
- Qué es el sistema y para qué sirve.
- Qué funcionalidades tiene implementadas.
- Cómo está organizado el proyecto.
- Qué requisitos necesita para ejecutarse.
- Cómo ejecutarlo y utilizarlo.

No convertir el README en una explicación detallada del código.
Los detalles de diseño técnico pertenecen a las especificaciones
y a las reglas de arquitectura.

## Lenguaje y estilo

- Redactar en español, con lenguaje natural, claro y accesible.
- Priorizar la descripción del proyecto y de su funcionamiento.
- Usar términos técnicos cuando ayuden a comprender o ejecutar el sistema.
- Explicar brevemente los términos que no resulten evidentes.
- Escribir como documentación permanente, no como una respuesta
  del agente al equipo.
- No usar frases como “como solicitaste”, “te explico” o “ahora podés”.
- No describir acciones del agente como “implementé”, “agregué”
  o “verifiqué”.
- No redactar un historial de cambios como “se agregó” o “se modificó”.
- Describir directamente el estado actual:
  “El sistema permite...” o “Esta carpeta contiene...”.

El informe final del agente puede dirigirse al equipo y explicar
el trabajo realizado. No copiar ese informe como contenido del README.

## Árbol de carpetas

Incluir en README.md un árbol de carpetas en un bloque de texto
que muestre la estructura principal existente.

- Inspeccionar la estructura real antes de redactarlo o actualizarlo.
- Mostrar solo las carpetas y archivos relevantes para orientarse.
- Excluir dependencias, archivos temporales y carpetas generadas,
  como bin/, obj/ y .git/.
- Respetar los nombres y las mayúsculas existentes.
- Acompañar el árbol con descripciones breves y sencillas
  del propósito de las carpetas y proyectos principales.
- No presentar carpetas propuestas como si ya existieran.
- No describir una carpeta vacía como una funcionalidad implementada.
- Actualizar el árbol cuando cambie la organización del proyecto.

Si se muestran directorios vacíos que Git no conserva,
aclarar esa limitación cuando afecte las instrucciones de clonación
o ejecución. No crear archivos marcadores sin autorización.

## Veracidad del contenido

- Describir únicamente la organización y el comportamiento
  efectivamente existentes y comprobados.
- Diferenciar una estructura creada de una funcionalidad implementada.
- No presentar decisiones futuras como capacidades actuales.
- No afirmar que una herramienta está configurada o que un control pasó
  sin haberlo comprobado.
- No inventar comandos, rutas, configuraciones ni resultados.
- Si falta información, señalarla en el documento de trabajo correspondiente
  en lugar de completar el README con suposiciones.

## Condiciones para actualizar README.md

Actualizar README.md como parte de una funcionalidad únicamente cuando:
- El equipo haya solicitado implementar el requerimiento.
- La implementación esté terminada.
- Se haya verificado el cumplimiento de sus criterios de aceptación.

Preparar, revisar o aprobar requirement.md, spec.md o task.md
no autoriza por sí solo a crear o modificar README.md.

Durante esas etapas, mantener las propuestas y decisiones pendientes
en Funcionalidades/.

Si la implementación queda incompleta o no pudo verificarse,
registrar el avance y los pendientes en task.md.

Una solicitud explícita del equipo para corregir o actualizar README.md
permite editarlo, conservando las reglas de veracidad.

Para tareas técnicas acotadas, documentar el resultado en README.md
cuando la tarea esté autorizada, terminada y verificada,
y el cambio sea relevante para conocer o utilizar el proyecto.

## Requisitos y especificaciones

- requirement.md describe qué necesita el sistema:
  objetivo, alcance, comportamiento, criterios de aceptación
  y decisiones pendientes.
- spec.md describe cómo se propone implementarlo:
  componentes, contratos, validaciones y estrategia de verificación.
- task.md divide el trabajo en tareas verificables y registra su estado.
- Mantener los tres documentos coherentes cuando cambien los requisitos.
- Distinguir las propuestas de las decisiones aprobadas.
- Marcar únicamente tareas realmente completadas.
- Registrar limitaciones y verificaciones pendientes sin presentar
  el trabajo como terminado.

Para tareas técnicas acotadas no es obligatorio crear los tres documentos.
Registrar el alcance y la verificación en la tarea, y las decisiones
permanentes en el archivo correspondiente de Reglas/.

## Comandos, configuración y seguridad

- Documentar comandos comprobados y la carpeta desde la que se ejecutan.
- Indicar los requisitos previos necesarios.
- Distinguir comandos ejecutados en el equipo de comandos ejecutados
  dentro de contenedores cuando corresponda.
- No incluir secretos, tokens, contraseñas ni credenciales reales.
- Usar valores de ejemplo claramente identificados.
- No presentar una configuración de desarrollo como apta para producción
  sin una decisión y verificación específicas.

## Mantenimiento

- Actualizar las secciones existentes antes de crear otras.
- Incluir únicamente secciones relevantes.
- Evitar información duplicada o contradictoria.
- Comprobar que los enlaces internos apunten a archivos existentes.
- Mantener consistentes los nombres de archivos y rutas.
- Respetar el nombre real del README; no crear otro archivo
  que solo difiera en mayúsculas.
- No modificar AGENTS.md ni los archivos de Reglas/
  sin una instrucción explícita o aprobación del equipo.