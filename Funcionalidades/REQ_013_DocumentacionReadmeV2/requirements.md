# Requisitos: README versión 2

## Objetivo

Preparar `Readme.v2.md` como propuesta de documentación para revisar con el equipo,
sin modificar el `Readme.md` actual ni el código de la aplicación.

## Alcance y criterios de aceptación

- Presentar SolarUY con capturas de la landing y del panel administrativo de `Docs/Tutorial/`.
- Enumerar prerrequisitos y enlazar una guía separada para instalar Docker Desktop y el SDK .NET 10.
- Explicar la ejecución desde la raíz, la configuración asistida y su alternativa manual.
- Distinguir `.env` para Compose de User Secrets para la conexión MySQL de .NET.
- Documentar el orden ejecutable: preparar credenciales antes de `docker compose up -d`,
  porque Compose exige las variables de `.env` al evaluar la configuración.
- Enlazar un tutorial de Keycloak ilustrado con las capturas entregadas: administrador
  de `master`, usuario de `solaruy`, contraseña, rol `ADMIN` y renovación de sesión.
- Enlazar la guía existente de SonarQube y explicar su uso durante el desarrollo con IA.
- Describir en un párrafo el arnés de IA, reglas, requirements, specs, tasks y revisión humana.
- Mencionar MySQL Workbench como opcional y pendiente de validación; no instalarlo ni probarlo en esta tarea.
- Verificar enlaces locales y preservar el README actual byte por byte durante esta tarea.

## Fuera de alcance

Cambios de aplicación, credenciales, contenedores, reglas o skills; actualización de
la arquitectura histórica; publicación o reemplazo del README actual.
