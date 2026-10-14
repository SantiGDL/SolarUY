# Tareas de limpieza de Git

- [x] Inspeccionar archivos versionados y configuración actual.
- [x] Documentar el alcance solicitado y crear `.gitignore`.
- [x] Retirar del índice únicamente archivos locales del IDE y de compilación.
- [x] Verificar exclusiones, copias locales y conservación de código y `.gitkeep`.
- [x] Ejecutar `dotnet build` y comprobar el estado posterior de Git.
- [x] Actualizar Readme.md y registrar los resultados.

## Resultados

- 1.210 archivos retirados del índice: 6 de `.idea/`, 44 de `bin/obj` del backend
  y 1.160 de `bin/obj` del frontend. Sus copias locales se conservan.
- `.gitignore` preparado en el índice junto con las bajas; commit y push pendientes.
- Los 47 archivos `.gitkeep` y los archivos de código, solución, proyectos,
  configuración y recursos existentes permanecen versionados e intactos.
- Compilación desde la raíz: 0 errores y 0 advertencias. Sus salidas quedan ignoradas.
- No existen proyectos de pruebas ni configuración de ArchUnitNET o SonarQube;
  esos controles no se ejecutaron.
- No se modifica el historial de commits existentes. La limpieza se aplica al
  siguiente commit que incluya estos cambios.
