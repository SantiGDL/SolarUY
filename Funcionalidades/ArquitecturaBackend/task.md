# Tareas para la estructura de carpetas del backend

## Ampliación: conservar Modulos y Compartido en Git

- [x] Revisar la solicitud y las rutas actuales, sin el nivel intermedio Backend.
- [x] Crear 47 archivos `.gitkeep` vacíos en las subcarpetas vacías de
  `SolarUyBack/Modulos/` y `SolarUyBack/Compartido/`.
- [x] Verificar tamaño cero, cobertura de todos los directorios y visibilidad en Git.
- [x] Ejecutar `dotnet build`: 0 errores y 0 advertencias.
- [x] Actualizar Readme.md con las rutas y los marcadores verificados.

No existen proyectos de pruebas ni configuración de ArchUnitNET o SonarQube;
estos controles no se ejecutaron. Los marcadores quedan listos para incluir en
un commit; esta tarea no realiza commit ni push.

## Entrega inicial

Estado: implementación de la estructura de carpetas completada y verificada por
solicitud del equipo. No se generaron archivos de código ni proyectos de pruebas.

## Documentación

- [x] Inspeccionar AGENTS.md y los documentos actuales de la funcionalidad.
- [x] Ajustar requirement.md para excluir todos los archivos individuales.
- [x] Adaptar spec.md, conservando las carpetas modificadas por el equipo.
- [x] Adaptar task.md y retirar tareas de código, proyectos y dependencias.
- [x] Incorporar en requirement.md y spec.md las carpetas de comunicación mediante
  APIs locales y eventos, con sus responsabilidades y sin implementar contratos.
- [x] Unificar los cuatro módulos con DTOs, Interfaz/Apis/Remota,
  Interfaz/Apis/Local, Interfaz/Eventos/IN, Interfaz/Eventos/OUT,
  Aplicacion/Servicios, Aplicacion/Implementaciones, Dominio, Controladores,
  Infraestructura/Persistencia e Infraestructura/Identidad.
- [x] Alinear las tareas de creación y verificación con las carpetas de comunicación.
- [x] Revisar el diseño y las tareas ajustados antes de generar la estructura.
- [x] Recibir la solicitud del equipo para implementar el requerimiento.

## Creación de directorios

- [x] Registrar el estado de los directorios y archivos existentes para verificar
  que la operación no modifica ni elimina contenido.
- [x] Crear las carpetas de Usuarios según spec.md.
- [x] Crear las carpetas de Parques según spec.md.
- [x] Crear las carpetas de Tareas según spec.md.
- [x] Crear las carpetas de Mediciones según spec.md.
- [x] Incluir en los cuatro módulos DTOs, Interfaz/Apis/Remota, Interfaz/Apis/Local,
  Interfaz/Eventos/IN e Interfaz/Eventos/OUT con las mayúsculas indicadas.
- [x] Incluir Aplicacion/Servicios, Aplicacion/Implementaciones,
  Infraestructura/Persistencia e Infraestructura/Identidad en los cuatro módulos,
  junto con Controladores y Dominio, sin otras subcarpetas.
- [x] Crear Backend/Compartido/Infraestructura/Persistencia.
- [x] Crear Backend/Compartido/Aplicacion/Eventos y
  Backend/Compartido/Infraestructura/Eventos.
- [x] Crear SolarUyPruebas con Unitarias/Parques, Unitarias/Tareas,
  Unitarias/Mediciones, Integracion y Arquitectura, sin crear un proyecto.

## Verificación de la estructura

- [x] Comparar todos los directorios con el árbol de spec.md.
- [x] Verificar que los cuatro módulos tengan exactamente la misma estructura,
  con APIs Remota/Local y eventos IN/OUT, y que Compartido separe las carpetas de
  aplicación e infraestructura para eventos.
- [x] Confirmar que no se generaron archivos individuales, incluidos .gitkeep.
- [x] Confirmar que se conservaron los archivos preexistentes y su contenido.
- [x] Ejecutar dotnet build desde la raíz tras crear los directorios.
- [x] Informar que las carpetas vacías no se registran en Git y que las pruebas,
  ArchUnitNET y SonarQube siguen pendientes de configuración.
- [x] Una vez implementado por solicitud del equipo y verificado el cumplimiento
  del requerimiento, actualizar Readme.md y el estado de los documentos. No crear
  ni modificar Readme.md durante la preparación o revisión de la propuesta.

## Resultados

- 83 directorios creados, con el árbol exacto de spec.md y sin archivos internos.
- 75 archivos preexistentes verificados mediante hashes antes y después de crear
  las carpetas y compilar, sin cambios de contenido; bin/obj se excluyeron por ser
  artefactos de compilación. La actualización posterior solo afecta la
  documentación de esta funcionalidad y Readme.md.
- dotnet build ejecutado desde la raíz: 0 errores y 0 advertencias.
- dotnet test no corresponde todavía: SolarUyPruebas no contiene un proyecto.
- ArchUnitNET y SonarQube no están configurados y no se ejecutaron.
- Las carpetas vacías no se registran en Git; no se crearon archivos .gitkeep.
