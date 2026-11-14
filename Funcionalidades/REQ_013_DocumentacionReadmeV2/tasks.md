# Tareas: README versión 2

- [x] T01 — Documentación: leer AGENTS.md, reglas reales y guía de documentación;
  inspeccionar README, script, configuración y capturas.
- [x] T02 — Documentación: registrar requisitos y diseño, distinguiendo alcance y pendientes.
- [x] T03 — Crear Readme.v2.md en el orden solicitado, sin editar Readme.md.
- [x] T04 — Crear guía de prerrequisitos con fuentes oficiales y comprobaciones.
- [x] T05 — Crear tutorial ilustrado de Keycloak, contraseñas, ADMIN y cookies antiguas.
- [x] T06 — Verificar enlaces, formato y conservación del README actual; registrar resultados.
- [ ] T07 — Validar MySQL Workbench junto con el usuario en una tarea posterior.

## Verificaciones

Se comprobaron 33 enlaces locales e imágenes de los seis documentos nuevos;
todos apuntan a rutas existentes. Los bloques de código tienen cierre y
`git diff --check` no informa errores. Se inspeccionaron las capturas utilizadas.

El hash SHA-256 de Readme.md se mantuvo antes y después:
`C7675D75785BF36E9708749C3C20D28C8BEBBD217B7CBD4D954952A4812F05F8`.
Sus modificaciones previas a esta tarea se conservaron.

Los comandos de configuración se contrastaron con el script, el archivo Compose
y los perfiles reales; no se volvió a ejecutar el asistente ni se alteraron
credenciales para documentarlos. Las guías de instalación se apoyan en fuentes
oficiales de Docker y Microsoft; no se reinstalaron herramientas.

No se ejecutaron compilación, pruebas ni un nuevo análisis SonarQube: el cambio
es exclusivamente documental. Workbench sigue pendiente de prueba práctica.
