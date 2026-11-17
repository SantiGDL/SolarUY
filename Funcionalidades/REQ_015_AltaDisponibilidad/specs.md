# REQ_015 — Especificaciones de alta disponibilidad activo-pasivo

## Alcance y estado

Diseño de la etapa 1 de requirements.md, incorporando la aclaración posterior del
usuario sobre aviso de desconexión y demostración. La implementación de la etapa obligatoria fue autorizada y está registrada en tasks.md. La etapa 2 activo-activo queda fuera del diseño implementable.

Seguir AGENTS.md y los seis Markdown de `.agents/Reglas/`, su ubicación real.
La solución no cambia entidades, esquema, migraciones, lógica de negocio ni reglas
de ArchUnitNET. No incluye recuperación de estado ni claves compartidas.

## Topología y configuración mínima

Navegador → Nginx → solaruy1 (principal) / solaruy2 (backup).
Ambas instancias utilizan el MySQL y Keycloak del Compose actual. Nginx es el
único punto de entrada de la aplicación HA; los backends no publican puertos al
host. Solaruy2 está iniciado, pero no recibe nuevas solicitudes mientras Nginx
considere disponible solaruy1. No usar ip_hash, sticky, Redis ni backplane.

Proponer `compose.ha.yaml` como archivo adicional explícito al compose.yaml actual.
Usar ambos desde la raíz y con el mismo proyecto Compose, sin `-p` nuevo, para
reutilizar red y volúmenes existentes. El Compose normal y los perfiles de Rider
no deben activar las aplicaciones HA ni Nginx. No agregar SonarQube a su arranque.
Los nombres de servicio serán exactamente `solaruy1`, `solaruy2` y `nginx`.
No fijar container_name ni dependencias entre solaruy1 y solaruy2.

### Publicación

Crear posteriormente un Dockerfile equivalente al concepto del profesor: publicar
SolarUy/SolarUy.csproj en Release y ejecutar con runtime ASP.NET Core 10. Puede
utilizarse build multietapa SDK/runtime para no depender de un publish manual.
Usar una misma imagen local etiquetada para ambas instancias, sin mounts de código
ni dotnet watch. Incluir static web assets de Sensor y fuentes DejaVu/Liberation
requeridas por el exportador PDF. No crear un servicio Sensor independiente.

Backends escuchan HTTP interno en 8080 y entorno Production. Esto evita el bloque
actual de migraciones de Development sin tocar migraciones ni agregar un ejecutor
DDL. Precondición: esquema existente ya preparado; si faltan tablas, detener la
prueba y explicar el bloqueo, no migrar automáticamente en ambas instancias.
Proporcionar ConnectionStrings__SolarUy con Server=mysql, base/usuario actuales y
contraseña local por entorno. No copiar User Secrets de Windows ni .env a la imagen.

Una comprobación HTTP de la landing puede confirmar que el proceso responde,
pero no acredita MySQL/Keycloak ni autorización. Mantener el healthcheck MySQL y
verificar discovery de Keycloak antes de iniciar la demostración. Si se usa
healthcheck HTTP en las aplicaciones, usar una herramienta explícita en la imagen
y la ruta existente, sin un nuevo endpoint de negocio. Nginx puede depender de
ambas aplicaciones sanas solo al arranque; detener una luego no detiene Nginx.
No interpretar depends_on/healthcheck como orquestación de failover.

### Origen público HTTP local

Decisión definitiva: `http://localhost:7000`. Nginx escucha HTTP interno en 80 y
publica `127.0.0.1:7000:80`. Keycloak mantiene 8080, SonarQube 9000 y Rider sus
puertos/perfiles actuales. No hay otro transporte, material criptográfico local
que preparar ni cambios de confianza de Windows en este despliegue.
Los backends conservan HTTP interno en 8080; no publican puertos al host.

En el modo HA explícito, omitir redirección de transporte y HSTS para servir el
origen HTTP elegido. Mantener el comportamiento actual fuera de HA, en particular
Rider. Esto no implica desactivar autenticación, antiforgery ni validación de
certificados remotos. No falsear X-Forwarded-Proto: el esquema público real es http.

### Compatibilidad OIDC y cookies en localhost

El flujo actual usa code con PKCE. Conservar formato de respuesta actual, state,
nonce, correlación, firma, issuer, audience, expiración y roles. Las cookies de
nonce/correlación de ASP.NET Core usan SameSite=None, HttpOnly y SecurePolicy=Always:
no quitar Secure ni relajar SameSite para forzar el login.
La documentación de cookies contempla la excepción Secure en localhost. Esta
es la base de viabilidad para Brave/Chromium, no una prueba ya ejecutada ni una
promesa de compatibilidad con cualquier navegador o con direcciones de red.
Usar literalmente localhost tanto en la aplicación como en Keycloak; no alternar
con 127.0.0.1, nombres internos de Docker o IPs LAN en la navegación pública.

Conservar la política actual de la cookie de autenticación (HttpOnly y su
SecurePolicy/SameSite por defecto), cambiando solo su nombre para aislar HA de
Rider. Conservar antiforgery. No aplicar CookiePolicy global que quite Secure a
nonce/correlación, no desactivar Shields ni usar flags de navegador inseguros.
Los puertos no aíslan cookies: SolarUy.HA.Session sigue siendo necesaria para no
colisionar con SolarUy.Session, aunque ambos orígenes estén en localhost.

Comprobación futura obligatoria en Brave: inspeccionar atributos y aceptación de
cookies sin copiar valores sensibles; completar challenge, callback, intercambio
de código/token, acceso autenticado y logout en ambos backends. Verificar ausencia
de bucles y errores de nonce/correlación. La recarga tras caída puede iniciar un
login nuevo; SSO de Keycloak es válido sin volver a pedir contraseña.
Si la versión/configuración real del navegador bloquea el flujo, detener y registrar
la causa; no introducir una relajación de controles ni cambiar de transporte sin
revisión del usuario. HTTP no cifra tráfico: este diseño está restringido al PC
local y no constituye un despliegue seguro para redes externas.

## Nginx: adaptación del ejemplo del profesor

Reutilizar la estructura de `01_1_webApiEjemplo/01_1_webApiEjemplo/nginx.conf` y
su Compose, sin modificar esos archivos de referencia. Crear una configuración
propia de SolarUY. Elegir al implementar una versión estable identificada de la
imagen Nginx estándar; no depender de módulos comerciales ni de nginx:latest.

Upstream conceptual:

```nginx
upstream solaruy {
    server solaruy1:8080 max_fails=1 fail_timeout=5s;
    server solaruy2:8080 backup;
}
```

Compartir el estado de fallos entre workers mediante una zona upstream pequeña,
o usar un único worker para esta PoC. Preferir un worker, como simplificación del
ejemplo local. No confundir fail_timeout con un monitor que consulta cada 5 s.
Tras un fallo se evita principal durante el intervalo; luego una petición puede
volver a probarla. Una caída intermitente no garantiza continuidad de circuito.

En location / preservar rutas y consultas, y usar explícitamente HTTP/1.1 hacia
backend; pasar Upgrade y Connection mediante map, apropiado también para HTTP
normal. Esto cubre `/_blazor`, negociación y `/hubs/mediciones` sin upstreams
separados ni reglas que puedan dirigir las dos conexiones a procesos distintos.
Establecer proxy_read_timeout/proxy_send_timeout de 3600 s para conexiones
persistentes; los keepalives de SignalR siguen funcionando normalmente.

Reutilizar como punto inicial proxy_connect_timeout 1s, proxy_next_upstream
error timeout http_502, máximo 2 intentos y un límite total de reintento de 10s.
Los tiempos no son una garantía de recuperación en 10 s para toda operación:
una respuesta ya iniciada o un WebSocket establecido no se pueden transferir.
No habilitar `non_idempotent` ni reintentar indiscriminadamente POST/escrituras.
Un intento incierto puede requerir recarga manual; no repetir entregas del simulador.

Enviar Host con el puerto público y X-Forwarded-Proto/For/X-Forwarded-Host
controlados por Nginx, no aceptar valores suministrados por el cliente como
identidad del proxy. En Program habilitar forwarded headers solo para HA, antes
de redirecciones/autenticación/antiforgery, limitados a la red interna de proxy
configurada, con ForwardLimit=1. No vaciar las listas de confianza para aceptar
cualquier origen. Reservar/configurar una subred privada no conflictiva o dirección
de proxy conocida al implementar; backends sin puertos publicados al host.

Registrar access logs con nombre/dirección upstream y estado para mostrar qué
instancia atiende. Evitar parámetros de callback, tokens, cookies o secretos en
los logs de demostración: preferir $uri sin query string. No agregar página ni
endpoint diagnóstico de negocio. La correspondencia IP↔servicio se obtiene con
Compose/Docker inspect.

## Autenticación y configuración de arranque

### Cambios limitados al punto de composición

En Program.cs, bajo configuración explícita de HA y sin alterar el flujo Rider:

- Activar el procesamiento confiable de forwarded headers antes de los middlewares
  que lo necesitan; Kestrel conserva el esquema externo real http y el puerto público 7000.
- Dar al modo HA una cookie propia, por ejemplo SolarUy.HA.Session, distinta de
  SolarUy.Session. Los navegadores no aíslan cookies de localhost por puerto.
  Conservar opciones HttpOnly y política de seguridad existente.
- Permitir configurar MetadataAddress de OIDC/JWT cuando sea necesario para el
  acceso interno a Keycloak, manteniendo Authority/issuer público, client solaruy,
  PKCE, roles y comprobaciones criptográficas. La configuración local existente
  de Rider conserva sus valores por defecto.

Data Protection permanece por instancia, sin volumen de claves compartido ni
identificador común para compartir cookies. La nueva réplica puede considerar
inválida una cookie anterior: un GET nuevo a la página protegida usa el flujo de
challenge/login actual; no se implementa un logout artificial. Una sesión SSO de
Keycloak puede evitar pedir de nuevo la contraseña, lo cual es válido. El criterio
es poder autenticarse, no obligar a escribir credenciales de nuevo.

Una autenticación OIDC iniciada antes de la caída puede perder su estado protegido;
el usuario reinicia el acceso desde la aplicación. No ignorar errores de state,
nonce o correlación ni reusar códigos. Si hay bucles persistentes en lugar de un
login nuevo, corregir su causa antes de aceptar la PoC, sin introducir claves
compartidas preventivamente. Si fueran estrictamente imprescindibles, informar
el diagnóstico y pedir revisión del alcance.

### Keycloak: una identidad pública, backchannel interno

Mantener el origen público existente de Keycloak `http://localhost:8080` y el
issuer del realm solaruy. Los navegadores siguen usando ese origen; los backends
no deben conectarse a su propio localhost ni anunciar http://keycloak al navegador.

Proponer, solo en el modo HA, hostname público fijo de Keycloak
http://localhost:8080 y hostname-backchannel-dynamic=true, conservando el volumen
actual. Las apps consultan discovery en
http://keycloak:8080/realms/solaruy/.well-known/openid-configuration mediante
MetadataAddress configurable, con Authority público. Comprobar antes de aceptar
esta configuración: issuer público y authorization/logout URL alcanzables desde
Windows, y token/userinfo/JWKS (incluido PAR si lo anuncia Keycloak y lo usa .NET)
alcanzables desde las apps. Esta separación usa la capacidad oficial de Keycloak;
no reemplazar issuer ni desactivar su validación para compensar errores.
RequireHttpsMetadata=false solo para el proveedor local HTTP ya usado por el
proyecto, suministrado explícitamente al modo HA; no cambiar validación global.

Agregar a la configuración del cliente existente, sin eliminar entradas Rider:

- Callback http://localhost:7000/signin-oidc.
- Origen http://localhost:7000.
- URLs de logout/post-logout necesarias para el flujo actual de ASP.NET Core,
  incluyendo callback de salida y retorno al origen público según se verifique.

Documentar el ajuste en el realm existente: cambiar el JSON importado no actualiza
por sí solo un realm ya inicializado. No borrar usuarios ni volúmenes. El override
puede reiniciar Keycloak una vez al preparar la PoC, no durante cada stop/start de
SolarUY. Verificar también que Rider conserva su login y su issuer al finalizar.
Si discovery/backchannel no se comportan como lo propuesto, registrar el bloqueo
y ajustar solo esa configuración en revisión, no agregar traducción arbitraria
de tokens o reemplazo manual de endpoints disperso por el código.

## Presentación: aviso sencillo de desconexión

### Implementación existente revisada

App.razor ya incluye ReconnectModal. Existen tres archivos colocalizados en
Components/Layout: ReconnectModal.razor, .razor.js y .razor.css.
El JavaScript escucha components-reconnect-state-changed:

- show abre el diálogo; hide lo cierra.
- failed habilita reintento al volver visible el documento.
- rejected recarga automáticamente hoy.
- Retry llama Blazor.reconnect(); ante rechazo intenta la API existente de
  resumeCircuit y finalmente recarga. No hay que construir otro reconector.

### Personalización propuesta, secundaria

Reutilizar el mismo diálogo, IDs, clases y evento del framework. No agregar otro
Blazor.start, un temporizador propio, detección de backend ni un handler global
que sustituya la política de reintentos de Blazor.

| Estado | Presentación y acción |
| --- | --- |
| show/retrying | Mensaje breve de reconexión en curso, conservando reintentos actuales. Sin logout, borrado de cookie ni recarga forzada por microcorte. |
| hide | Cerrar diálogo y continuar el mismo circuito si el framework lo recuperó. |
| failed | Mostrar aviso final y botón Recargar aplicación; puede conservarse Reintentar como opción secundaria con el mecanismo existente. |
| rejected | El proceso respondió pero el circuito ya no está disponible. Preferir mostrar el mismo aviso y botón en lugar de la recarga automática existente. No intentar recuperar estado entre réplicas. |

Texto final, conservando el propuesto por el usuario:

> Se perdió la conexión con el servidor. Es posible que los cambios que no hayas
> guardado se hayan perdido. Lamentamos las molestias. Recargá la aplicación para continuar.

El botón será HTML type=button y un listener JavaScript llamará location.reload();
no usar @onclick porque el circuito perdido no puede ejecutar esa acción.
Asegurar que el diálogo esté abierto en estados finales y que los selectores CSS
muestren el texto/botón para failed y rejected. Evitar controles duplicados o
pantalla vacía al cancelar el estado rejected. Mantener accesibilidad del diálogo.
Si el reintento manual devuelve false, mostrar el mismo estado final, sin habilitar
persistencia de circuitos ni construir una recuperación alternativa.

No cambiar las opciones ni servicios de pausa/persistencia de circuitos; conservar
el código estándar no relacionado salvo el mínimo necesario para este aviso.
No hacer SignOut, invocar /logout ni redirigir a Keycloak desde el evento de
reconexión. Solo la navegación HTTP tras recargar determina si hay cookie válida
y aplica el login normal. El usuario puede perder formularios y simulación.

**Criterio de recorte:** si el cambio exige sustituir el reconector del framework,
introducir recuperación de estado o depurar una solución extensa, conservar los
tres archivos estándar y documentar que se usó su experiencia de retry/reload.
El proxy y la conmutación tienen prioridad. Este recorte es autorización del
usuario y no exige implementar la personalización a cualquier costo.

El diálogo cubre el circuito Interactive Server. La desconexión del hub del motor
WebAssembly conserva sus eventos actuales: no crear un segundo sistema de avisos
ni prometer que ReconnectModal supervise automáticamente ese hub independiente.

## Responsabilidades por módulo/capa

| Área | Dominio | Aplicación | Infraestructura | Presentación |
| --- | --- | --- | --- | --- |
| Usuarios/autenticación | Sin cambios | Sin nuevos contratos ni permisos | Configuración OIDC/JWT/cookie y proxy en composición | Login/logout actuales; sin logout por microcorte |
| Parques y Tareas | Sin cambios | Sin cambios | Repositorios y esquema intactos, mismo MySQL | Formularios existentes, pérdida sin guardar aceptada |
| Mediciones/Historial | Sin cambios | Conservar sesiones y reglas actuales | Una memoria por instancia; limpieza solo local | Reabrir página crea sesión nueva, sin recuperación automática |
| Consultas | Sin cambios | Sin cambios | Configuración actual del proveedor; no hacer Ollama obligatorio | Mantener consultas e historial/PDF existentes |
| Sensor | Contratos intactos | Motor actual | Publicación de assets con SolarUY | Mismo hub público y mismo backend estable; detener/reabrir manualmente tras caída |
| Composición/despliegue | No aplica | No nueva capa | Dockerfile, override Compose, Nginx/HTTP y entorno | ReconnectModal opcionalmente personalizado |

La única tarea de fondo registrada que se replica es limpieza local de sesiones;
no modifica un conjunto global de trabajos persistidos. No agregar productores,
reintentos de entregas inciertas, locks distribuidos ni simulaciones por host.

## Demostración prevista con Docker Compose

Los comandos siguientes son el procedimiento de la futura implementación, no
comandos ejecutados en esta etapa. Todos se usan desde la raíz con el mismo
proyecto y los archivos base+override. La demo principal usa stop/start, que no
reconstruye ni elimina contenedores; permite control independiente.

### Preparación

Confirmar Docker disponible, .env existente, esquema preparado, callbacks de Keycloak y puerto 7000 libre. No publicar contraseñas.

```powershell
docker compose -f compose.yaml -f compose.ha.yaml up -d --build mysql keycloak solaruy1 solaruy2 nginx
docker compose -f compose.yaml -f compose.ha.yaml ps -a
docker compose -f compose.yaml -f compose.ha.yaml logs --tail 30 nginx solaruy1 solaruy2
```

Esperar disponibilidad real. Abrir http://localhost:7000, ingresar al panel y
verificar interacción, roles y lectura de datos. Confirmar mediante upstream logs
que nuevas solicitudes usan solaruy1. No basta ver la landing o contenedores Up.

### Caída y conmutación

```powershell
docker compose -f compose.yaml -f compose.ha.yaml stop solaruy1
```

Mantener solaruy1 detenido; observar el aviso/reconexión estándar, recargar y
entrar nuevamente al panel. Si la cookie no sirve, seguir /login y Keycloak.
Comprobar nuevas solicitudes a solaruy2 mediante logs, navegación interactiva,
lectura de base y funcionamiento de SignalR. Medir tiempo y errores transitorios.
El fallo del circuito anterior es esperado y no invalida el failover de nuevas
conexiones. Usar una sola simulación controlada; cerrar la pestaña/sensor anterior
antes de iniciar otra y no convertir esta prueba en una carga que duplique datos.

### Retorno

```powershell
docker compose -f compose.yaml -f compose.ha.yaml start solaruy1
```

Esperar readiness y el período de nueva prueba de principal. Recargar nuevamente,
reautenticarse si es necesario y confirmar solaruy1 en nuevas solicitudes. No
exigir que un WebSocket aún abierto en solaruy2 se migre al principal.

Los servicios también admitirán stop/start/restart solaruy2 de forma independiente.
Detener reserva con principal sana no debe interrumpir nuevas conexiones normales.
No reiniciar Nginx ni Keycloak para la demo básica de stop/start solaruy1.

### Recreación y actualización individual

Para recrear con imagen nueva, construir primero y reemplazar solo una instancia
con up --no-deps -d --force-recreate del servicio correspondiente. No usar down,
--volumes ni una recreación conjunta. Esta variante necesita más que restart.

Nginx con resolución estática puede conservar la IP anterior. Para mantener la
configuración simple, usar recarga controlada de Nginx tras confirmar el nuevo
backend: `nginx -t` y luego `nginx -s reload` mediante compose exec, sin detener el
proxy. Documentar el cambio de rol y no retirar el backend que queda hasta que
la otra instancia esté lista. No prometer descubrimiento dinámico inexistente.
Verificar manualmente stop/start y recreación como escenarios distintos.

## Archivos previstos para implementación posterior

- Nuevos: Dockerfile de SolarUY, compose.ha.yaml, configuración Nginx propia y guía
  breve de ejecución/demostración. Rutas concretas se enumerarán en tasks.md.
- Ajustes mínimos: Program.cs para configuración HA, .gitignore/.dockerignore
  para secretos/publicación y configuración documentada de Keycloak.
- Opcionales: ReconnectModal.razor, .razor.js, .razor.css para aviso final/recarga.
- Intactos: ejemplo del profesor, modelos, migraciones, DbContext, casos de uso,
  contratos internos/locales, reglas de arquitectura, circuitos y motor Sensor.
- No crear archivos Docker/Nginx ni cambiar ninguno de esos archivos en esta fase.

## Validación y límites de aceptación

En implementación: compilar Release y ejecutar pruebas de arquitectura y el
proyecto funcional completo según las reglas. Verificar además en navegador:
login/logout/roles en principal y reserva, lectura de gestión, historial/PDF,
WebSockets, caída/retorno, cookies ajenas, microcorte sin logout y aviso final si se
personalizó. Comprobar mismo MySQL y ausencia de DDL o productores duplicados.

Las 22 pruebas de arquitectura y las pruebas funcionales no acreditan por sí solas
Nginx, HTTP local, Keycloak, reconexión ni persistencia real. Registrar resultados y
bloqueos manuales y de SonarQube. No hacer pruebas destructivas sobre datos del
usuario. No acreditar las verificaciones previas del REQ_014 como pruebas de HA.

No se garantiza continuidad de formularios/circuitos/simulaciones ni autenticación
en curso; se acepta recarga y login. Principal intermitente o retorno puede
romper otro circuito. Nginx, MySQL, Keycloak y el PC siguen siendo puntos únicos
de fallo. No se agrega activo-activo ni infraestructura para resolver estos límites.

## Referencias y decisiones para revisión

- Ejemplo local del profesor: `01_1_webApiEjemplo/01_1_webApiEjemplo/`.
- [Nginx upstream y backup](https://nginx.org/en/docs/http/ngx_http_upstream_module.html).
- [Proxy WebSocket](https://nginx.org/en/docs/http/websocket.html).
- [Reconexión Blazor](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-10.0).
- [Keycloak hostname/backchannel](https://www.keycloak.org/server/hostname).

Verificar especialmente las cookies en http://localhost:7000 y el backchannel
Keycloak. Son configuración de despliegue local, no nuevas funciones. Si surgen
incompatibilidades en su validación futura, explicar el ajuste antes de ampliar
alcance. La implementación obligatoria y las verificaciones realizadas se registran en tasks.md.

### Fuentes de compatibilidad HTTP local

- [Cookies Secure y excepción de localhost](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Set-Cookie).
- [Políticas SameSite en ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-10.0).
- [Valores de CorrelationCookie](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.remoteauthenticationoptions.correlationcookie?view=aspnetcore-10.0).
- [Valores de NonceCookie](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.openidconnect.openidconnectoptions.noncecookie?view=aspnetcore-10.0).

La compatibilidad documental se contrastó después con login y navegación en Brave en principal y reserva; ver resultados de implementación en tasks.md.
## Ajustes verificados durante implementación

- Se implementó el modo HTTP local en 7000, sin material TLS ni cambios de puertos
  en otros servicios. La red actual se obtiene de solaruy_default y se configura
  como HA_PROXY_NETWORK local; no se recrea ni renombra el proyecto.
- El Dockerfile copia los archivos .razor antes de restaurar paquetes. En .NET 10
  la restauración sin esos archivos omitía los assets del framework; la imagen
  corregida publica y sirve el script blazor.web con fingerprint.
- La prueba manual detectó que las cookies superaban el buffer de Nginx. Se añadieron
  large_client_header_buffers 4 32k, proxy_buffer_size 16k, proxy_buffers 4 16k y
  proxy_busy_buffers_size 32k. Esto ajusta capacidad, no elimina protección de cookies.
- Se configuró el cliente de Keycloak existente conservando sus entradas anteriores;
  el respaldo de configuración quedó local y excluido de Git. No se cambiaron usuarios.
- No se implementó el aviso personalizado. Los resultados y pendientes efectivos
  se registran en tasks.md; las indicaciones de planificación anteriores no son
  evidencia de ejecución por sí mismas.