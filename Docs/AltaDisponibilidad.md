# Alta disponibilidad local de SolarUY

Esta demostración usa el mismo proyecto Docker Compose de MySQL y Keycloak,
con `solaruy1` principal, `solaruy2` encendido como reserva y Nginx en
[http://localhost:7000](http://localhost:7000).

Las conexiones nuevas usan principal; ante su caída pueden usar reserva.
Se acepta recargar y volver a iniciar sesión. No se recuperan formularios,
circuitos Blazor ni simulaciones en memoria.

## Preparación

Abrir Docker Desktop con contenedores Linux y PowerShell en la raíz de SolarUY.
La configuración local `.env`, el usuario ADMIN de SolarUY y el esquema MySQL deben
existir previamente. No se crean tablas ni usuarios desde las réplicas.
No ejecutar `down -v`, borrar volúmenes o cambiar el nombre del proyecto Compose.

### Configurador independiente

1. Configurar primero el entorno habitual con
   `Scripts/Configurar-EntornoLocal.cmd`, iniciar MySQL y Keycloak y completar
   la preparación de SolarUY indicada en el README. Si ya está configurado,
   conservar `.env` y los datos existentes: no repetir la inicialización.
2. Ejecutar `Scripts/Configurar-AltaDisponibilidad.cmd` mediante doble clic.
   También se puede ejecutar desde PowerShell:

   ```powershell
   .\Scripts\Configurar-AltaDisponibilidad.cmd
   ```

3. Cuando termine correctamente, levantar HA con los comandos de **Arranque**.

El nuevo configurador funciona con Windows PowerShell 5.1. Detecta la red del
proyecto Compose existente, guarda su subred en `HA_PROXY_NETWORK` dentro de
`.env` y agrega únicamente las tres entradas de localhost:7000 que falten en el
cliente de Keycloak. Conserva las demás variables, credenciales, callbacks y
atributos. Si todo está preparado, no reescribe `.env` ni actualiza el cliente.
Puede ejecutarse nuevamente después de corregir un error o si cambió la red.

No inicia, detiene ni recrea contenedores y no modifica volúmenes. El configurador
local anterior permanece independiente. No hace falta obtener la subred ni
editar manualmente Keycloak en el recorrido normal.

Si falta `.env`, preparar primero el entorno local. Si Docker o Keycloak no
responden, iniciar Docker Desktop y los servicios habituales y esperar su
disponibilidad. Si no puede identificar una red única, revisar que MySQL y
Keycloak pertenezcan al mismo proyecto Compose; no crear otro proyecto para
sortear el error. Si falla el acceso administrativo, revisar las credenciales
locales existentes: las credenciales bootstrap pueden haber dejado de ser válidas.
El configurador no las restablece ni muestra credenciales o tokens. Si una etapa
queda pendiente, el mensaje indica el problema; corregirlo antes de reejecutar.

No ejecutar `docker compose config` mostrando credenciales; usar `config --quiet`.

## Keycloak

El override usa hostname público `http://localhost:8080` y backchannel dinámico.
Las apps consultan discovery en `http://keycloak:8080`, manteniendo issuer público.
Se requiere `Keycloak:RequireHttpsMetadata=false` exclusivamente para el proveedor
HTTP local. No se desactiva validación de issuer, firma, audience, expiración,
state, nonce ni PKCE. No quitar Secure a cookies de nonce/correlación.

El primer arranque HA puede recrear solo el contenedor Keycloak para aplicar esas
variables. Su volumen, realm y usuarios se conservan. El archivo JSON de importación
incluye los nuevos callbacks para instalaciones nuevas, pero un realm ya existente
no se actualiza automáticamente con ese archivo.

El configurador agrega al cliente existente las siguientes entradas sin quitar
las anteriores. La tabla sirve para comprobar el resultado en la consola de
administración, no es un paso manual obligatorio:

| Campo | Entrada HA |
| --- | --- |
| Valid redirect URIs | `http://localhost:7000/signin-oidc` |
| Web origins | `http://localhost:7000` |
| Valid post logout redirect URIs | `http://localhost:7000/*` |

Se conservan el tipo público del cliente, PKCE S256, roles y usuarios.
La cookie de la app HA se llama `SolarUy.HA.Session`, distinta de la de Rider.
Cada réplica tiene sus propias claves Data Protection: una cookie de otra réplica
puede requerir un login nuevo. La sesión SSO de Keycloak puede evitar pedir de
nuevo la contraseña; eso no significa que se recuperó el circuito perdido.

## Arranque

```powershell
docker compose -f compose.yaml -f compose.ha.yaml config --quiet
docker compose -f compose.yaml -f compose.ha.yaml build solaruy1
docker compose -f compose.yaml -f compose.ha.yaml up -d --no-build mysql keycloak solaruy1 solaruy2 nginx
docker compose -f compose.yaml -f compose.ha.yaml ps -a
```

La imagen `solaruy:ha-local` es la misma para ambas instancias. La primera
construcción descarga SDK/runtime, fuentes y paquetes; puede tardar varios minutos.
El Dockerfile restaura después de copiar los `.razor`, para que .NET 10 incluya
los recursos de Blazor. No agregar un servicio Sensor: sus recursos están en la
publicación y el motor se ejecuta en el navegador al usar la aplicación.

Esperar `healthy` en MySQL y las aplicaciones. Keycloak también debe haber
terminado de iniciar; `Up` no garantiza que discovery esté listo. El auxiliar de
permisos Keycloak termina normalmente con `Exited (0)`. SonarQube no se agrega a
este arranque, aunque puede seguir funcionando si ya estaba encendido.

Abrir en Brave **http://localhost:7000**, usar Ingresar al panel y el usuario del
realm solaruy, no el administrador del realm master. Navegar por una página de
gestión y verificar interacción. No desactivar Shields ni flags de seguridad.
La excepción de cookies Secure de localhost debe comprobarse en ese navegador;
si falla, registrar el error en lugar de relajar sus controles.

## Demostración principal → reserva → principal

Ver IPs y logs sin exponer cookies ni query de callbacks:

```powershell
docker inspect solaruy-solaruy1-1 solaruy-solaruy2-1 --format '{{.Name}} {{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}'
docker compose -f compose.yaml -f compose.ha.yaml logs --tail 15 nginx
```

Los nombres de contenedor anteriores corresponden al proyecto `solaruy`; adaptarlos
solo si el proyecto existente se llama distinto. Las IPs pueden cambiar.
Con ambas sanas, nuevas peticiones deben indicar la IP de `solaruy1`.

1. Iniciar sesión y comprobar navegación/interacción por Nginx.
2. Detener solo la principal:

   ```powershell
   docker compose -f compose.yaml -f compose.ha.yaml stop solaruy1
   ```

3. Recargar. Si se perdió el circuito o la cookie no sirve, volver a Ingresar al
   panel y seguir Keycloak. Mientras principal esté detenida, verificar navegación
   en reserva y su IP en los logs. Un fallo inicial antes del reintento es posible.
4. Recuperar principal:

   ```powershell
   docker compose -f compose.yaml -f compose.ha.yaml start solaruy1
   ```

5. Esperar que esté lista y que Nginx vuelva a probarla después de su intervalo
   de fallo (5 s). Recargar/reautenticarse si hace falta. Las nuevas peticiones
   vuelven a principal; una conexión WebSocket anterior no se transfiere.

El aviso de Blazor conserva el comportamiento estándar; no se personalizó.
No se hace logout ante un microcorte ni se restauran simulaciones. Cerrar la
pestaña/sensor anterior antes de iniciar una nueva simulación controlada.
No utilizar simulaciones múltiples para demostrar el balanceo.

La reserva puede detenerse, iniciarse o reiniciarse por separado:

```powershell
docker compose -f compose.yaml -f compose.ha.yaml stop solaruy2
docker compose -f compose.yaml -f compose.ha.yaml start solaruy2
docker compose -f compose.yaml -f compose.ha.yaml restart solaruy2
```

## Recrear con una imagen nueva

`restart` no instala una imagen nueva. Construir primero y reemplazar una sola
instancia, conservando otra disponible. Por ejemplo, con principal atendiendo:

```powershell
docker compose -f compose.yaml -f compose.ha.yaml build solaruy1
docker compose -f compose.yaml -f compose.ha.yaml up -d --no-deps --no-build --force-recreate solaruy2
```

Esperar su disponibilidad. Como Nginx resuelve al cargar la configuración,
comprobar y recargar si cambió la IP:

```powershell
docker compose -f compose.yaml -f compose.ha.yaml exec -T nginx nginx -t
docker compose -f compose.yaml -f compose.ha.yaml exec -T nginx nginx -s reload
```

Después se puede detener principal, verificar nuevas conexiones en reserva,
recrear solo principal con `up -d --no-deps --no-build --force-recreate solaruy1`,
esperar y recargar Nginx de la misma forma. No recrear ambas simultáneamente.
Se acepta pérdida de sesiones durante el procedimiento. Usar versiones compatibles
con el esquema existente; aquí no se aplican migraciones de base de datos.

## Volver al desarrollo habitual

```powershell
docker compose -f compose.yaml -f compose.ha.yaml stop nginx solaruy1 solaruy2
```

MySQL y Keycloak siguen disponibles. Ejecutar el perfil http de SolarUy desde
Rider como siempre. Su cookie, puertos y User Secrets no cambian. La configuración
pública de Keycloak sigue siendo localhost:8080 y conserva callbacks de Rider.
Usar solo los comandos de base para desarrollo normal; no es necesario levantar HA.

## Verificaciones y límites

```powershell
dotnet build SolarUy.sln -c Release --no-restore
dotnet test SolarUyPruebas/Arquitectura/SolarUyArquitecturaPruebas.csproj -c Release --no-build
dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj -c Release --no-build
```

Los resultados de implementación y pruebas manuales pendientes se registran en
`Funcionalidades/REQ_015_AltaDisponibilidad/tasks.md`.
La landing o el formulario de Keycloak no prueban un login completo ni una sesión
interactiva autenticada; verificarlo en Brave junto con el simulador/PDF.
Nginx detecta fallos pasivamente; no migra WebSockets ni garantiza cero errores.
El modo solo se expone por loopback del equipo, sin TLS. No es una configuración
para Internet o una LAN. MySQL, Keycloak, Nginx, Docker y el equipo siguen siendo
puntos únicos de fallo. No se agregaron afinidad, Redis ni recuperación de estado.

### Cabeceras y cookies OIDC

Nginx está configurado con buffers de cabecera de hasta 32 KB y buffers de
respuesta de 16 KB. ASP.NET Core guarda tokens en la cookie y puede fragmentarla;
además, otros servicios de localhost envían cookies aunque usen otro puerto.
Esto evita el error `400 Request Header Or Cookie Too Large` observado en el
callback, sin quitar protecciones de cookies ni validaciones OIDC.

Si una autenticación se interrumpió, volver a la raíz e iniciar un login nuevo;
no recargar ni reenviar una URL antigua /signin-oidc. No copiar esa URL, cookies
ni tokens a capturas o reportes. El aviso estándar de Blazor continúa intacto.
