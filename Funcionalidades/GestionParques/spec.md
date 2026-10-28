# Diseño: gestión inicial de parques solares

Estado: diseño adaptado a la arquitectura vigente de `origin/main` y aprobado
para implementación.

## Arquitectura

La funcionalidad se integra en el monolito modular `SolarUy/`, una Blazor Web
App con interactividad Interactive Server. Los componentes llaman a los
servicios de aplicación mediante inyección de dependencias y C# dentro del
mismo proceso; no se agrega una API HTTP separada.

- **Presentación:** `Components/Pages/Parques.razor` muestra la lista y el
  formulario; `PanelAdmin.razor` muestra el total y enlaza a la gestión. La
  gestión comienza con un tablero visual interactivo: al enfocar, pasar el
  cursor o seleccionar un parque se muestran sus nodos.
- **Aplicación:** `IParquesService` coordina consulta y registro; devuelve DTOs
  y comprueba autenticación/rol mediante el contrato `IUsuarioActual`.
- **Dominio:** `Parque` aplica límites, recorte y normalización del nombre.
  `IParqueRepositorio` define las operaciones necesarias.
- **Infraestructura:** `ParquesDbContext` configura el modelo EF Core y
  `ParqueRepositorio` implementa el contrato con MySQL.
- **Composición:** `Program.cs` registra servicios, configura OIDC y aplica la
  migración inicial únicamente en Development.

## Persistencia

- MySQL 8.4 se ejecuta localmente mediante Docker Compose.
- Entity Framework Core 10 y `MySql.EntityFrameworkCore` 10.0.12.
- La tabla `Parques` contiene identificador autogenerado, nombre, dirección y
  nombre normalizado.
- El tablero carga todos los nodos en una consulta y los agrupa por parque para
  evitar consultas adicionales por cada tarjeta.
- Un índice único en `NombreNormalizado` protege contra registros duplicados
  concurrentes.
- La cadena de conexión se configura con User Secrets o variables de entorno;
  nunca se guarda en el repositorio.
- El volumen `solaruy_mysql_data` conserva los datos del contenedor.

## Autenticación y autorización

- Keycloak 26.8.1 inicia en local con el realm `solaruy`.
- El cliente público utiliza Authorization Code con PKCE S256 y no contiene
  secreto de cliente.
- ASP.NET Core mantiene la sesión con cookie y valida los tokens OIDC.
- Un mapper del realm expone los roles como claim `roles`; la aplicación usa
  `ADMIN` como rol para registrar parques.
- Las rutas administrativas requieren autenticación. La capa de aplicación
  permite consultar a cualquier usuario autenticado y restringe el alta a
  `ADMIN`.

## Verificación

- Pruebas unitarias de normalización, validaciones, permisos, duplicados y
  registro.
- `dotnet build SolarUy.sln` y `dotnet test SolarUy.sln`.
- `docker compose config` y validación sintáctica del JSON del realm.
- Con Docker activo: aplicar migración, crear un usuario `ADMIN` de prueba y
  verificar login, consulta, permisos, alta, duplicados y persistencia.
