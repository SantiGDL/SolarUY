# Diseño: documentación de incorporación al proyecto

## Documentos

- `Readme.v2.md`: presentación, capturas, prerrequisitos, arranque, enlaces de
  Keycloak/SonarQube y modalidad de programación con IA, en ese orden.
- `Docs/Prerequisitos.md`: instalación en Windows con fuentes oficiales y comandos de comprobación.
- `Docs/ReadmeKeycloak.md`: recorrido ilustrado a partir de `Docs/Tutorial/`,
  sin copiar credenciales reales. Usar un usuario de ejemplo propio y aclarar que
  los nombres de las capturas no son credenciales preinstaladas.
- `Docs/ReadmeSonarQube.md` y `Docs/Arquitectura.md`: referencias existentes;
  no modificarlas como parte de esta propuesta.

## Dependencias y precisión

La guía se contrasta con `compose.yaml`, el script local, `Program.cs`, el realm
y los perfiles de ejecución. El asistente crea `.env` y escribe únicamente
`ConnectionStrings:SolarUy` en User Secrets: no guarda allí la contraseña de
administración de Keycloak ni crea usuarios de SolarUY.

En una instalación nueva, el script precede al arranque de los contenedores.
En instalaciones existentes se conservan las credenciales y volúmenes; el
asistente no sirve para restablecer contraseñas ya persistidas.

## Capas y módulos

El cambio es exclusivamente documental. No altera Dominio, Aplicación,
Infraestructura ni Presentación de ningún módulo, ni sus dependencias.

## Verificación

Comparar hash de `Readme.md` antes y después. Revisar rutas relativas, imágenes,
comandos contra su configuración y formato con `git diff --check`.
No ejecutar instalación, migraciones ni análisis SonarQube por una edición de Markdown.
Registrar Workbench como pendiente de prueba práctica, sin darlo por validado.
