# Diseño de la limpieza de Git

Crear `.gitignore` en la raíz con patrones específicos para carpetas generadas
y preferencias locales. No excluir globalmente archivos JSON, DLL, recursos
web ni configuraciones compartidas como `.csproj`, `.sln` o `launchSettings.json`.

Inventariar los archivos versionados de `.idea/` y `bin/obj` de ambos proyectos.
Usar `git rm --cached` limitado a esos archivos para preparar su eliminación del
repositorio sin borrarlos del disco. Agregar `.gitignore` al índice junto con la
limpieza para que las exclusiones acompañen las bajas en el siguiente commit.

Verificar que no queden archivos generados versionados, que Git ignore las rutas
retiradas y que el código y los archivos `.gitkeep` sigan presentes y versionados.
Ejecutar `dotnet build` y comprobar que sus salidas no vuelvan a aparecer como
archivos nuevos. Registrar el resultado y actualizar Readme.md al completar.
