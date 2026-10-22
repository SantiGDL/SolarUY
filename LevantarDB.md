# Entorno local de MySQL y Keycloak

Este entorno utiliza MySQL 8.4 para guardar parques y Keycloak para iniciar
sesión. Las credenciales son locales: no se agregan al repositorio.

## Requisitos

- Docker Desktop con Docker Engine iniciado.
- SDK de .NET 10.
- Git Bash, PowerShell o una terminal compatible con Docker Compose.

## Iniciar los servicios

Desde la raíz del repositorio, crear el archivo local `.env` a partir del
ejemplo:

```powershell
Copy-Item .env.example .env
```

Editar `.env` y reemplazar todos los valores `replace-with-*` por contraseñas
locales. No compartirlas ni subir `.env` a Git. Luego iniciar los servicios:

```powershell
docker compose up -d
docker compose ps
```

MySQL conserva sus datos en el volumen `solaruy_mysql_data`, y Keycloak conserva
su base local en `solaruy_keycloak_data`. Keycloak importa el realm `solaruy` al
iniciar. El panel de administración local de Keycloak está en
`http://localhost:8080`; sus credenciales iniciales son las definidas en `.env`.

## Crear una cuenta de prueba ADMIN

En Keycloak, seleccionar el realm `solaruy` y crear un usuario de desarrollo.
Establecer una contraseña local y desactivar la opción de cambio obligatorio si
se desea evitar el paso de cambio de contraseña al iniciar sesión. En la ficha
**Role mapping**, asignar el rol de realm `ADMIN` al usuario.

El cliente `solaruy` es público y utiliza Authorization Code con PKCE S256. No
requiere un secreto de cliente en la aplicación.

## Configurar la conexión de la aplicación

Configurar la cadena de conexión mediante User Secrets desde la carpeta del
proyecto `SolarUy`:

```powershell
dotnet user-secrets init --project SolarUy/SolarUy.csproj
dotnet user-secrets set "ConnectionStrings:SolarUy" "Server=localhost;Port=3306;Database=solaruy;User ID=solaruy_app;Password=TU_PASSWORD_LOCAL" --project SolarUy/SolarUy.csproj
```

Usar como `TU_PASSWORD_LOCAL` el valor de `MYSQL_PASSWORD` en `.env`. La
aplicación ejecuta las migraciones de Entity Framework Core al iniciar en
Development; no hace falta crear tablas manualmente.

## Ejecutar y comprobar

Con MySQL y Keycloak iniciados:

```powershell
dotnet run --project SolarUy/SolarUy.csproj --launch-profile http
```

Abrir `http://localhost:5058` y seleccionar **Ingresar al panel**. Keycloak
solicita iniciar sesión. Cualquier usuario autenticado puede consultar parques;
el formulario de registro solo aparece a usuarios con rol `ADMIN`. La API
administrativa se implementa como servicios C# internos en el monolito Blazor,
no como una API HTTP separada.

## Detener servicios

```powershell
docker compose down
```

El comando conserva el volumen. Para borrar también los datos de MySQL:

```powershell
docker compose down --volumes
```

Ese último comando elimina los datos locales de esta base de datos.
