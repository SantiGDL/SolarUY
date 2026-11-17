# REQ_015 — Tareas de alta disponibilidad activo-pasivo

## Estado, autorización y prioridades

Requirements.md y specs.md se revisaron contra la confirmación más reciente del
usuario: un único proyecto Docker Compose, compose.yaml + compose.ha.yaml,
MySQL/Keycloak existentes, misma imagen para solaruy1 y solaruy2, Nginx con
principal y backup, y HTTP local definitivo en http://localhost:7000.
No se detectaron contradicciones técnicas que requieran reescribir esos documentos.
La confirmación de generar este plan supera sus notas previas de esperar antes
de crear tasks.md; no autoriza implementar.

Implementación autorizada y ejecutada. Las marcas identifican tareas verificadas; el registro final distingue resultados automáticos, confirmaciones del navegador y pendientes manuales. Las tareas opcionales no se implementaron.

Prioridad: despliegue funcional → autenticación → conmutación demostrable →
regresiones/documentación → aviso opcional. La demostración no depende del aviso.
Activo-activo, afinidad, Redis, backplanes, memoria sincronizada, recuperación de
formularios/circuitos y claves Data Protection compartidas quedan fuera de alcance.

## A. Preparación y publicación mínima

- [x] **HA-01 — Confirmar aprobación y precondiciones (despliegue).** Releer AGENTS.md,
  `.agents/Reglas/` y los tres documentos aprobados; revisar checkout y cambios
  ajenos antes de editar. Identificar el proyecto Compose que ya posee los
  contenedores/volúmenes MySQL y Keycloak, red, puerto 7000, .env y esquema actual.
  No imprimir credenciales. **Finalización:** se conoce cómo reutilizar exactamente
  esos recursos desde la raíz, sin otro `-p`, otro proyecto ni una base vacía.
  Si faltan Docker, datos de conexión, esquema o permisos, registrar el bloqueo;
  no resolverlo borrando volúmenes ni aplicando migraciones desde las réplicas.

- [x] **HA-02 — Imagen común de SolarUY (infraestructura).** Crear `Dockerfile` en
  la raíz con publicación Release de SolarUy/SolarUy.csproj y runtime ASP.NET Core
  10, incluyendo assets de Sensor y fuentes PDF. Crear `.dockerignore` y ajustar
  `.gitignore` solo para excluir .env, secretos, bin/obj,
  material local de HA y el ejemplo ajeno al build. No copiar credenciales ni
  otros secretos en capas de imagen. Depende HA-01. **Finalización:** una imagen
  etiquetada reproducible sirve para ambos hosts; no hay servicio Sensor separado
  ni modificaciones de proyectos, entidades o lógica para publicar.

- [x] **HA-03 — Origen HTTP local (infraestructura/configuración).** Confirmar
  http://localhost:7000, publicación 127.0.0.1:7000:80 y ausencia de conflicto de
  puerto. Mantener Keycloak 8080, SonarQube 9000 y puertos/perfiles Rider intactos.
  No preparar material de confianza de Windows. Depende HA-01. **Finalización:**
  todas las URLs y callbacks del modo HA usan el origen elegido; el plan no
  cambia otros servicios ni propone otro transporte.
## B. Un solo Compose y autenticación funcional

- [x] **HA-04 — Composición explícita HA (infraestructura).** Crear `compose.ha.yaml`
  que se combine con el compose.yaml existente. Agregar solaruy1, solaruy2 y nginx
  en la red del mismo proyecto; ambas apps usan exactamente la misma imagen y
  configuración equivalente, entorno Production y conexión a Server=mysql con
  credenciales locales por entorno. Backends HTTP interno 8080 sin puertos al host;
  Nginx publica únicamente el origen local 7000. Usar el MySQL/Keycloak y volúmenes
  actuales; SonarQube no se levanta por los comandos de demo. No usar container_name
  ni dependencias entre las apps que impidan control independiente. Depende HA-02
  y HA-03. **Finalización:** Compose combinado es válido, compose.yaml normal no
  activa HA y no cambia las referencias a volúmenes. Revisar `config --quiet`
  o salida redactada para no mostrar secretos expandidos.

- [x] **HA-05 — Proxy seguro y configuración aislada (composición de aplicación).**
  Modificar únicamente lo necesario en `SolarUy/Program.cs` para habilitar por
  configuración HA forwarded headers confiables antes de redirecciones/auth,
  con ForwardLimit=1 y red/dirección de Nginx explícita. Resolver una subred privada
  no conflictiva en la red del mismo proyecto, sin convertirlo en dos proyectos
  ni agregar una infraestructura de redes compleja. No aceptar cualquier proxy.
  Configurar cookie HA propia y MetadataAddress opcional OIDC/JWT conservando los
  valores normales de Rider, PKCE, roles, antiforgery e issuer. No crear contratos
  ni capas para esta configuración. Depende HA-04. **Finalización:** callbacks usan
  el origen HTTP público real, sin redirección de transporte ni HSTS solo en HA, cookie HA no colisiona con SolarUy.Session y no se
  agregan claves compartidas, logout automático ni validaciones deshabilitadas.

- [x] **HA-06 — Keycloak público/interno (configuración).** En el override proponer
  hostname público http://localhost:8080 y backchannel dinámico, usando discovery
  interno http://keycloak:8080/realms/solaruy/.well-known/openid-configuration y
  Authority/issuer público. Añadir callbacks/origen/logout del 7000 al cliente
  existente sin quitar los de Rider. Documentar el ajuste de realm ya inicializado
  en lugar de confiar solo en el JSON de importación; si se actualiza ese JSON,
  limitarlo a entradas HA necesarias. Conservar usuarios y volumen. Depende HA-05.
  **Finalización:** confirmar issuer y URLs de autorización/logout públicos,
  token/userinfo/JWKS y PAR cuando aplique alcanzables desde las apps; mantener
  RequireHttpsMetadata=false solo para el proveedor HTTP local ya existente.
  No reescribir tokens ni desactivar issuer/state/nonce, firma, audience o expiración.
  Mantener Secure/HttpOnly/SameSite=None en cookies nonce/correlación; verificar
  en Brave la excepción de localhost, sin desactivar Shields ni controles.
  Si este diseño no funciona, registrar evidencia y revisar el ajuste mínimo
  antes de añadir otra solución de autenticación.

- [x] **HA-07 — Nginx principal/reserva (infraestructura).** Crear
  `Infraestructura/AltaDisponibilidad/nginx.conf` reutilizando estructura del ejemplo,
  sin modificarlo. Usar un worker, solaruy1 principal con fallos pasivos y solaruy2
  marcado backup; no ip_hash/sticky. Servir HTTP interno 80 y publicar 127.0.0.1:7000:80, preservar Host/puerto y establecer
  cabeceras reenviadas controladas; HTTP/1.1, map Upgrade/Connection para /_blazor y
  /hubs/mediciones. Aplicar timeouts/intent límites de specs.md, sin non_idempotent.
  Logs con upstream/status y URI sin query ni secretos. Fijar imagen estable de
  Nginx identificada. Depende HA-04 y HA-05. **Finalización:** `nginx -t` correcto,
  ambas rutas SignalR pueden funcionar sin split normal entre instancias y
  no hay reintentos que reproduzcan escrituras inciertas.

- [x] **HA-08 — Arranque y login antes de probar failover (integración).** Levantar
  infraestructura y aplicaciones con ambos archivos Compose y servicios explícitos.
  Usar el healthcheck MySQL actual, comprobar discovery y respuesta HTTP de las
  apps; si se agrega healthcheck HTTP, usar ruta existente y herramienta declarada
  en imagen. No interpretar Up como readiness integral ni healthcheck como retirada
  del upstream. Verificar login, logout, rol ADMIN/no ADMIN y navegación interactiva
  por http://localhost:7000. Depende HA-06 y HA-07. **Finalización:** ambos hosts
  inician sin migraciones/DDL, el acceso por Nginx funciona y nuevas peticiones usan
  principal; endpoints de autenticación no anuncian direcciones inaccesibles.
  Inspeccionar cookies aceptadas/rechazadas y callback completo en Brave, sin
  registrar sus valores. No avanzar a failover dejando errores OIDC sin resolver.
  Si HTTP local resulta incompatible en ese entorno, registrar bloqueo y pedir
  revisión; no quitar Secure ni relajar validaciones para completar la prueba.

## C. Demostración obligatoria y regresiones

- [x] **HA-09 — Demostrar caída y reserva (integración manual).** Confirmar solaruy1
  en logs; detener únicamente solaruy1. Mantener Nginx, solaruy2, MySQL y Keycloak.
  Recargar en Brave y usar el login normal si la cookie no sirve. Verificar solaruy2
  en upstream logs, página protegida con interacción real, lectura de MySQL y
  SignalR. Registrar tiempos y errores iniciales. Depende HA-08. **Finalización:**
  la reserva permite autenticarse y usar la gestión mientras principal sigue
  detenida. Pérdida del circuito/formulario/simulación anterior es aceptada; no
  exigir conservación de login ni considerar suficiente la landing.

- [x] **HA-10 — Retorno y control independiente (integración manual).** Iniciar
  solaruy1 sin recrear o reiniciar el resto, esperar readiness/reintento de Nginx,
  recargar y verificar principal en nuevas peticiones y login si fuera necesario.
  Probar stop/start/restart independiente de solaruy2 con principal sana. Depende
  HA-09. **Finalización:** demostración de seis pasos completa, sin mover WebSockets
  de reserva al principal ni agregar afinidad para conservar esos circuitos.

- [ ] **HA-11 — Conservación funcional, simulador y Rider (regresión manual).**
  Verificar consultas de gestión, Historial/Consultas, PDF con fuentes Linux y
  roles en los backends que atienden. Usar una sola simulación controlada y cerrar
  su pestaña/sensor antes de abrir otra tras failover. Revisar que el arranque de
  dos hosts solo replica limpieza local de sesiones, no productores o trabajos
  globales; no modificar datos arbitrariamente para probar. Verificar flujo Rider
  con sus puertos, cookie e issuer originales y documentar efectos del override
  de Keycloak al pasar entre modos. Depende HA-10. **Finalización:** no cambios de
  esquema, procesos duplicados ni regresiones observadas; pruebas no realizadas
  quedan expresamente pendientes, no se consideran aprobadas.

- [x] **HA-12 — Recreación individual y DNS (despliegue).** Documentar y verificar
  por separado una recreación individual con la otra instancia lista, usando
  --no-deps y la recarga controlada de Nginx prevista (`nginx -t`, luego reload)
  para resolver cambios de IP. No confundir restart con desplegar imagen nueva.
  No detener/recrear ambas ni usar down/-v. Depende HA-10. **Finalización:**
  procedimiento revisable de reemplazo de una instancia a la vez sin apuntar a
  IPs obsoletas; describir pérdida de circuitos aceptada. No agregar descubrimiento
  dinámico, drenaje o automatización sofisticada para este escenario académico.

- [x] **HA-13 — Compilación y verificaciones automáticas (calidad).** Compilar
  SolarUy.sln en Release y ejecutar proyectos existentes de arquitectura y pruebas
  funcionales con el código final. Revisar cobertura de los tipos de composición
  afectados sin modificar ni debilitar reglas ArchUnitNET. No agregar pruebas
  unitarias de Aplicación/Infraestructura/Presentación por iniciativa propia;
  no se prevén cambios de Dominio. Si surge uno, detener el desvío de alcance y
  cumplir las reglas xUnit de Dominio antes de continuar. Depende HA-11 y HA-12.
  **Finalización:** build correcto y pruebas existentes aprobadas, sin usar los
  resultados anteriores de REQ_014 como evidencia de esta implementación.

- [ ] **HA-14 — Guía breve y entrega del núcleo (documentación).** Crear
  `Docs/AltaDisponibilidad.md` con preparación HTTP local/Keycloak, arranque explícito,
  lectura segura de logs y comandos de demo, parada independiente, recreación y
  vuelta al uso Rider. Documentar mismo proyecto/volúmenes, límites y pasos de
  reautenticación, sin secretos. Revisar diff y registrar resultados en REQ_015.
  Seguir la guía SonarQube existente si hay servicio/token; registrar bloqueo si
  falta alguno, sin afirmar Quality Gate. Depende HA-13. **Finalización:** solución
  obligatoria sencilla, demostración reproducible y verificaciones trazables;
  ninguna funcionalidad/infraestructura ajena o etapa activo-activo incluida.

## D. Mejora opcional, solo después del núcleo funcional

- [ ] **HA-O1 — Evaluar/personalizar aviso existente (presentación, opcional).**
  Solo después de HA-14 y si no pone en riesgo la entrega, adaptar los tres
  archivos existentes `SolarUy/Components/Layout/ReconnectModal.razor`, `.razor.js`
  y `.razor.css`, conservando eventos/IDs/reintentos de Blazor. En failed/rejected
  mostrar el mensaje aprobado y botón HTML Recargar aplicación mediante JavaScript
  location.reload(), sin depender de @onclick del circuito perdido. No borrar
  cookies ni hacer logout ante show/retrying o microcortes. **Finalización:**
  aviso funciona con circuito perdido y no interfiere con reconexión normal.
  Si exige sustitución del reconector o complejidad significativa, omitir esta
  mejora y registrar que se conserva el mecanismo estándar; no bloquea aceptación.

- [ ] **HA-O2 — Verificar mejora o registrar omisión (opcional).** Depende HA-O1.
  Si se personalizó, probar microcorte con mismo circuito, fallo prolongado,
  rejected y recarga/login por el backend disponible; repetir build y las
  verificaciones afectadas tras el cambio. No crear otro sistema de reconexión
  para el hub WebAssembly. Si se omitió, conservar archivos estándar intactos y
  documentar retry/reload existentes. **Finalización:** resultado registrado sin
  prometer recuperación de formularios, sesiones ni simulación.

## Comandos previstos para la futura ejecución

No se ejecutaron al generar este documento. Mantener explícitos ambos archivos
en todos los comandos de HA desde la raíz del repositorio:

```powershell
docker compose -f compose.yaml -f compose.ha.yaml config --quiet
docker compose -f compose.yaml -f compose.ha.yaml up -d --build mysql keycloak solaruy1 solaruy2 nginx
docker compose -f compose.yaml -f compose.ha.yaml ps -a
docker compose -f compose.yaml -f compose.ha.yaml logs --tail 30 nginx solaruy1 solaruy2
docker compose -f compose.yaml -f compose.ha.yaml stop solaruy1
# Recargar, autenticar si hace falta y verificar solaruy2.
docker compose -f compose.yaml -f compose.ha.yaml start solaruy1
# Esperar, recargar y verificar nuevas peticiones a solaruy1.
```

```powershell
dotnet build SolarUy.sln -c Release
dotnet test SolarUyPruebas/Arquitectura/SolarUyArquitecturaPruebas.csproj -c Release --no-build
dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj -c Release --no-build
```

## Evidencias y criterio de cierre

Registrar: versión/tag de imagen compartida, mismo proyecto Compose y base,
URL HTTP local, resultado del flujo OIDC desde Windows/Docker, upstream antes/
durante/después de caída, interacción/roles/SignalR, tiempos, pruebas funcionales,
resultado de Rider, recreación DNS y bloqueos manuales/SonarQube. No guardar tokens,
contraseñas, cookies, consultas de callback ni claves privadas como evidencia.

Las pruebas automáticas no certifican proxy, HTTP local ni autenticación en navegador.
Nginx/MySQL/Keycloak/PC siguen siendo puntos únicos de fallo. La aceptación es
para nuevas conexiones tras caída, con recarga/login permitidos, no continuidad
instantánea de solicitudes ni trabajo previo. No marcar el núcleo completo si
faltan sus comprobaciones requeridas; lo opcional omitido no lo bloquea.

## Archivos previstos y límite de esta ejecución

Nuevos en implementación: Dockerfile, .dockerignore si no existe, compose.ha.yaml,
Infraestructura/AltaDisponibilidad/nginx.conf y Docs/AltaDisponibilidad.md.
Cambios mínimos previstos: SolarUy/Program.cs, .gitignore y, si hace falta,
SolarUyKeycloak/solaruy-realm.json solo para entradas HA; configuración efectiva del
realm existente documentada. El compose.yaml base se conserva salvo necesidad
mínima justificada para red/compatibilidad; preferir todo lo HA en el override.
ReconnectModal es opcional; modelos, migraciones, repositorios, lógica de negocio,
reglas ArchUnitNET y ejemplo del profesor permanecen intactos.

El plan aprobado ya se ejecutó; los resultados reales y sus límites están registrados abajo.
Implementación realizada; permanecen pendientes las verificaciones manuales complementarias y SonarQube indicadas abajo.


El puerto definitivo es 7000 con HTTP local. Ver resultados de ejecución a continuación.

## Resultados de implementación — 17 de noviembre de 2026

### Implementado y comprobado

- Dockerfile .NET 10 Release con publicación conjunta de Sensor, fuentes PDF y
  proceso sin privilegios. Ambas instancias utilizan la misma imagen solaruy:ha-local.
- compose.ha.yaml combinado con compose.yaml dentro del proyecto solaruy, red
  solaruy_default. Se conservaron solaruy_solaruy_mysql_data y
  solaruy_solaruy_keycloak_data. No se borraron ni reinicializaron volúmenes.
- Se añadió HA_PROXY_NETWORK=172.18.0.0/16 al .env local, sin cambiar credenciales;
  en otros equipos se obtiene su red real. .local-ha está excluido de Git/build.
- Cambios de aplicación solo en Program: cookie HA independiente, metadata opcional,
  forwarded headers con red conocida y omisión de redirección/HSTS únicamente HA.
  No se alteraron lógica, entidades, repositorios, migraciones ni reglas ArchUnitNET.
- Keycloak conserva issuer público y anuncia token/userinfo/JWKS/PAR internos. Se
  añadieron callback/origen/logout del 7000 al cliente existente manteniendo Rider;
  copia local anterior en .local-ha/cliente-antes.json. Realm/usuarios conservados.
- Nginx 1.30.6-alpine, principal + backup, WebSockets y logs sin query/cookies.
  Configuración válida con nginx -t. Se corrigió el 400 de cabeceras/cookies con
  buffers acotados de 32 KB para entrada y 16 KB para respuesta, sin relajar seguridad.
- La primera publicación omitía blazor.web por restaurar sin .razor; se corrigió el
  Dockerfile, sin alterar componentes. La imagen final sirve assets Blazor con HTTP 200.
- Compose valida. Ambas aplicaciones iniciaron healthy en Production, sin ejecutar
  migraciones. MySQL conserva el esquema de 7 tablas; comprobación solo de lectura.
- Se verificó misma imagen en ambas réplicas e IPs de upstream identificables.
  Nuevas solicitudes van a principal; al detenerla se sirven desde reserva.
- Con imagen final, petición HTTP de reserva respondió 200 en aproximadamente
  1,9 segundos desde el inicio de esa petición (no es medición completa de una
  sesión de usuario ni garantía de RTO). Logs registran intento fallido principal
  y respuesta de reserva. La redirección alcanza el formulario Keycloak en ambas.
- El usuario confirmó en Brave login y navegación de gestión en principal y luego
  en reserva mientras solaruy1 estaba detenida. Logs muestran /admin 200 y WebSocket
  /_blazor 101 en reserva. No se utilizaron las credenciales compartidas en el chat
  para scripts ni se guardaron en código/guías.
- Negociación Blazor HTTP 200 y handshake WebSocket blazorpack aceptado por Nginx
  tanto en principal como reserva. Esto comprueba transporte; no sustituye una
  prueba manual del hub autenticado de simulación.
- Principal se recuperó con start; se comprobaron también recreaciones individuales,
  recarga controlada de Nginx y restart de reserva sin detener principal. Se deja
  el despliegue funcionando con ambas instancias; verificación final de estado/logs.
- `dotnet build SolarUy.sln -c Release --no-restore`: correcto, cero errores y
  únicamente los 3 avisos NU1903 preexistentes del mismo paquete vulnerable.
- `dotnet test SolarUyPruebas/Arquitectura/SolarUyArquitecturaPruebas.csproj -c Release --no-build`:
  22 aprobadas, 0 fallidas, 0 omitidas. ARQ-01 sin dependencias prohibidas.
- `dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj -c Release --no-build`:
  113 aprobadas, 0 fallidas, 0 omitidas. VSTest mantiene su aviso informativo de
  VSTEST_ARTIFACTS_DIRECTORY, con código de salida 0.
- Docs/AltaDisponibilidad.md contiene preparación, comandos exactos, configuración
  manual equivalente de Keycloak, parada/retorno/recreación y regreso a Rider.

### Pendientes y límites

- HA-11 parcial: falta recorrido manual completo de roles no ADMIN, logout,
  simulador autenticado /hubs/mediciones, PDF e Historial/Consultas en ambos hosts.
  Se verificaron fuente instalada, esquema conservado, código de procesos locales
  y login/navegación confirmados; no se generaron datos para simular carga.
- Falta prueba manual completa desde Rider después del override (callbacks y
  cookie originales conservados; antes de los cambios su script respondía 200).
- HA-14 parcial: guía y revisión de diff realizadas; SonarQube/Quality Gate no
  ejecutados porque no hay token disponible. No se acredita aceptación de esas
  comprobaciones pendientes solo por pasar pruebas automáticas.
- HA-O1/HA-O2 omitidas por instrucción explícita: no modificar ReconnectModal.
- Sin claves compartidas, afinidad, Redis ni recuperación de formularios/circuitos.
  Cookies ajenas pueden exigir nuevo login; Nginx/MySQL/Keycloak/PC siguen siendo
  puntos únicos de fallo. No se garantiza continuidad de operaciones en vuelo.
- No se realizó commit ni push.

### Estado final

Ambas instancias quedaron running/healthy con la misma imagen final. Tras recuperar
solaruy1, nueva petición a / y acceso a /login respondieron correctamente y los
logs confirmaron upstream de principal. No se interrumpió ni se eliminó MySQL,
Keycloak o SonarQube al realizar stop/start/recreación de las aplicaciones.

### Verificación del cambio de puerto a 7000 — 17/11/2026

- Actualizados Compose, importación de Keycloak, cliente existente y documentación.
- Conservados callbacks/orígenes de Rider y PKCE S256; logout admite HA y Rider.
- Nginx validado y recreado en 127.0.0.1:7000; ambas aplicaciones healthy.
- Inicio y recurso Blazor: HTTP 200. Keycloak muestra formulario en ambas instancias.
- Con solaruy1 detenida, Nginx atendió desde solaruy2; al recuperarla volvió a solaruy1.
- Inicio de sesión completo y navegación en Brave confirmados manualmente en 7000.
- Compilación Release: cero errores, tres advertencias NU1903 preexistentes.
- Arquitectura: 22/22; funcionales existentes: 113/113. Sin cambios de código C#.
- Los recorridos manuales adicionales y SonarQube indicados arriba siguen pendientes.
