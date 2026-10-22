# Tareas: gestión inicial de parques solares

Estado: implementación adaptada a la arquitectura vigente y verificada
parcialmente con MySQL y Keycloak locales. La consulta y el registro se probaron
desde el navegador con un usuario ADMIN; quedan verificaciones de seguridad y
persistencia tras reinicio.

## Backend modular

- [x] Definir entidad Parque, validaciones, recorte y normalización del nombre.
- [x] Definir DTOs de entrada/salida y contratos de aplicación/persistencia.
- [x] Implementar el servicio de aplicación para consulta y registro.
- [x] Validar autenticación para consultar y rol ADMIN para registrar en la
  capa de aplicación.
- [x] Implementar DbContext, repositorio MySQL, índice de unicidad y migración.
- [x] Aplicar migraciones al iniciar en Development.

## Interfaz y seguridad

- [x] Configurar Keycloak OIDC Authorization Code con PKCE y sesión mediante
  cookie.
- [x] Añadir el realm local, cliente público, claim de roles y rol ADMIN.
- [x] Proteger el panel, consulta y registro con la política de acceso acordada.
- [x] Añadir inicio/cierre de sesión y formulario/listado de parques.
- [x] Mostrar la cantidad de parques en el panel administrativo.

## Entorno y pruebas

- [x] Agregar Docker Compose para MySQL y Keycloak, volumen persistente y
  `.env.example`; excluir `.env` de Git.
- [x] Documentar el arranque en `LevantarDB.md`.
- [x] Agregar pruebas unitarias para dominio, validación, autenticación, rol y
  duplicados (11 aprobadas).
- [x] Validar `dotnet build SolarUy.sln` (0 errores, 0 advertencias).
- [x] Validar `dotnet test SolarUy.sln` (11 aprobadas, 0 fallidas).
- [x] Validar sintaxis de Compose y del realm Keycloak.
- [x] Generar el script SQL de la migración y comprobar la tabla e índice único;
  no se aplicó contra una base real.
- [x] Comprobar que la Home responde HTTP 200 en un arranque de humo sin
  conexión real a MySQL.
- [x] Iniciar MySQL y Keycloak; MySQL aparece `healthy` y el discovery OIDC del
  realm `solaruy` responde HTTP 200.
- [x] Aplicar la migración en MySQL real al iniciar SolarUy; EF creó `Parques`,
  su índice único y `__EFMigrationsHistory`.
- [x] Verificar que `/` responde HTTP 200 y que `/admin` redirige al formulario
  de inicio de sesión de Keycloak con el cliente `solaruy`.
- [x] Iniciar sesión en el navegador con `juanpi`, usuario que muestra el
  formulario de alta ADMIN; cargar el panel y consultar los tres parques
  existentes.
- [x] Registrar un parque temporal desde el formulario, comprobar la
  confirmación y su lectura desde MySQL; eliminar únicamente ese registro de
  prueba y comprobar que permanecen los tres parques previos.
- [x] Intentar registrar desde la interfaz un nombre ya existente y comprobar
  que se informa el duplicado sin crear otro registro.
- [ ] Verificar logout y que un usuario sin rol ADMIN no pueda registrar.
- [ ] Verificar la persistencia de los parques después de reiniciar el
  contenedor.
- [ ] Ejecutar revisión visual del flujo autenticado y corregir los problemas
  encontrados.

El README se actualiza cuando se completen los criterios de aceptación y la
verificación de extremo a extremo. En una prueba anterior se mostró
`Correlation failed` durante el retorno de OIDC; en la verificación actual el
navegador muestra una sesión autenticada y permite consultar y registrar
parques. La causa de ese error aislado no se determinó, por lo que conviene
repetir el flujo de inicio de sesión en otra sesión limpia antes de dar por
cerrada la verificación.
