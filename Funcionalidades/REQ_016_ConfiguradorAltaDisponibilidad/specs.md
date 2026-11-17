# REQ_016 — Especificaciones del configurador HA

## Alcance

Crear únicamente los dos scripts independientes y actualizar la guía de alta
disponibilidad. Implementación realizada según el diseño aprobado; los resultados
y límites se registran en tasks.md. REQ_015 y los scripts de configuración local
no se modifican.

El configurador pertenece a las herramientas de preparación del entorno. No
incorpora componentes en Domain, Application ni Presentation de ningún módulo,
ni cambia contratos entre módulos, el simulador o la persistencia.

## Lanzador y validaciones

`Configurar-AltaDisponibilidad.cmd` invocará Windows PowerShell con `-NoProfile`,
`-ExecutionPolicy Bypass` y la ruta del nuevo `.ps1`, conservando su código de
salida. Seguirá el estilo del lanzador local.

El PowerShell resolverá la raíz desde `$PSScriptRoot`, sin depender del directorio
desde el que se ejecuta. Usará sintaxis compatible con Windows PowerShell 5.1,
mensajes en español y un resultado 0 para éxito y distinto de 0 para errores.
Permitirá conservar visible el resultado al ejecutarlo mediante doble clic,
siguiendo el mecanismo de espera del configurador local.

Validará `.env`, Docker CLI, Docker Compose, acceso al motor y los contenedores
existentes. Los comandos nativos deberán comprobar su código de salida. No
ejecutará `up`, `start`, `stop`, `restart`, `down` ni operaciones sobre volúmenes.

## Detección del proyecto y la red

1. Consultar los identificadores de los servicios existentes mediante
   `docker compose -f compose.yaml ps -a -q`, desde la raíz. No utilizar el
   override HA para descubrir la red: exige la variable que se quiere preparar.
2. Inspeccionar los contenedores de MySQL y Keycloak y sus etiquetas Compose.
   Exigir un único proyecto coherente y un único contenedor por servicio.
3. Localizar la red común identificada como `default` por las etiquetas Compose
   de ese proyecto. Es la red utilizada por la configuración actual de HA.
   No deducir su nombre concatenando una cadena ni elegir la primera red.
4. Inspeccionar IPAM y exigir una única subred IPv4 válida. Una topología ambigua
   o incompatible se informa como error; no se crean redes ni se fija una subred.

Si no se encuentran los servicios, indicar que se debe preparar e iniciar el
entorno habitual con el mismo proyecto Compose que contiene los datos. No buscar
otro proyecto arbitrariamente ni alterar su nombre para conseguir que funcione.

## Administración de Keycloak sin archivos de secretos

Se utilizará la API administrativa HTTP local de Keycloak mediante los comandos
HTTP disponibles en PowerShell 5.1. No se necesitan bibliotecas adicionales ni
archivos de configuración de `kcadm` con tokens.

Las credenciales se obtendrán en memoria de las variables
`KC_BOOTSTRAP_ADMIN_USERNAME` y `KC_BOOTSTRAP_ADMIN_PASSWORD` del contenedor
Keycloak identificado. Son los valores locales que Compose configuró a partir
del entorno habitual; esto evita implementar un lector propio de contraseñas
dotenv con comillas y escapes. No imprimir el resultado de la inspección.

Comprobar disponibilidad en `http://127.0.0.1:8080`, con tiempos de espera
acotados. Solicitar un token administrativo al realm `master` con `admin-cli`
y enviar las credenciales en el cuerpo de la petición, nunca en la URL o en
argumentos de un proceso externo. Mantener credenciales y token solo en memoria.
No mostrar respuestas de error completas que puedan contener información sensible.
Si las credenciales iniciales no son válidas, finalizar indicando que hace falta
revisar el acceso administrativo local, sin restablecer credenciales ni usuarios.

Localizar el realm `solaruy` y buscar el cliente cuyo `clientId` sea exactamente
`solaruy`. Exigir un único resultado y obtener su representación completa para
preservar atributos, incluidos PKCE y cierre de sesión.

Comparar de forma exacta las entradas requeridas con `redirectUris`, `webOrigins`
y `attributes['post.logout.redirect.uris']`. Este último campo contiene entradas
separadas por `##`; conservar las existentes y añadir la faltante. No eliminar
entradas ni normalizar otras configuraciones.

Si falta alguna entrada, enviar un único PUT con los campos `redirectUris`,
`webOrigins` y el mapa completo de atributos conservado. No enviar usuarios,
roles ni una importación del realm. Volver a leer el cliente para comprobar
las entradas y la conservación de la configuración afectada. Si ya están las
tres entradas, no emitir PUT. No reintentar automáticamente una escritura fallida.

## Escritura mínima de .env

Realizar las comprobaciones de red y autenticación antes de escribir. Tras
verificar Keycloak, guardar la subred detectada en `HA_PROXY_NETWORK`.

Leer el archivo sin ejecutar su contenido. Sustituir únicamente el valor de la
asignación de `HA_PROXY_NETWORK`, conservando los demás bytes de texto, comentarios,
codificación y saltos de línea. Si no existe, añadir la asignación respetando el
formato. Reconocer el valor existente entre comillas para evitar una reescritura
innecesaria. Si hay asignaciones duplicadas o un formato ambiguo de esa variable,
informar el error antes de modificar Keycloak o el archivo.

No generar copias nuevas de `.env` con credenciales. Comprobar que su contenido no
cambió desde la lectura antes de escribir. Si una etapa falla después de actualizar
Keycloak, informar que sus entradas ya se verificaron y que queda pendiente `.env`;
la siguiente ejecución deberá poder completar el trabajo sin duplicados.

## Documentación y comprobaciones

Actualizar únicamente `Docs/AltaDisponibilidad.md` fuera de REQ_016: preparar el
entorno habitual, ejecutar el nuevo `.cmd` y después levantar HA con los comandos
Compose existentes. Explicar prerrequisitos, errores y el carácter repetible del
configurador. No cambiar infraestructura ni el funcionamiento activo-pasivo.

Verificar con Windows PowerShell 5.1 real, ejecutar dos veces en el entorno
preparado y comparar `.env`, los campos del cliente, los scripts locales y los
documentos de REQ_015. Las comparaciones de secretos serán internas, sin volcarlos
a consola ni a nuevos archivos. Comprobar estado de contenedores y HTTP en 7000.

El entorno actual ya tiene las entradas HA: permite comprobar la ruta sin cambios.
Para verificar adiciones y errores, utilizar datos ficticios y respuestas
controladas en una comprobación aislada, sin eliminar entradas reales ni detener
servicios. Distinguir esas comprobaciones de una integración real pendiente en
una instalación que todavía no tenga las entradas.

Compilar y ejecutar las pruebas de arquitectura existentes conforme a las reglas
del proyecto. No agregar pruebas de dominio porque no cambia su comportamiento.
Registrar los resultados y las comprobaciones no realizadas, incluido SonarQube
si no se dispone de acceso para ejecutar su procedimiento.
