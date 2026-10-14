# Excluir archivos locales y generados de Git

Solicitud autorizada por el equipo: evitar que la configuración local del IDE
y las salidas de compilación generen cambios y conflictos entre colaboradores.

## Alcance y aceptación

- Ignorar bin/obj, la configuración local de Rider/Visual Studio y resultados de pruebas.
- Retirar del seguimiento los archivos ya versionados de esas carpetas,
  conservando las copias locales.
- Conservar código, proyectos, solución, recursos web, documentación y `.gitkeep`.
- Verificar las exclusiones y ejecutar `dotnet build`.
- No realizar commit, push ni reescribir el historial.

No hay decisiones de negocio involucradas ni pendientes para esta limpieza.
