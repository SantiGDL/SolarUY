# Keycloak: preparar un usuario para SolarUY

[← Volver al README versión 2](../Readme.v2.md)

Keycloak gestiona las identidades y el inicio de sesión de SolarUY. Un **realm**
agrupa usuarios, clientes y roles. Este proyecto utiliza el realm `solaruy`;
la administración inicial de Keycloak utiliza una cuenta del realm `master`.

Antes de comenzar, preparar `.env`, iniciar los contenedores y esperar a que
Keycloak esté disponible, siguiendo el [README versión 2](../Readme.v2.md).
Para la prueba final también debe estar ejecutándose SolarUY.
El realm y el cliente se importan desde
[solaruy-realm.json](../SolarUyKeycloak/solaruy-realm.json); ese archivo no crea usuarios.

## 1. Ingresar a la consola de administración

Abrir [la consola de Keycloak](http://localhost:8080/admin/).

![Ingreso a la consola de administración de Keycloak](Tutorial/Tutorial_3.png)

Utilizar `KEYCLOAK_BOOTSTRAP_ADMIN` y `KEYCLOAK_BOOTSTRAP_PASSWORD` de `.env`,
con los valores utilizados al inicializar este Keycloak. Si la cuenta ya existía
y su contraseña se cambió desde la consola, usar la contraseña vigente.

El asistente de `Scripts` configura esas credenciales iniciales; **no restablece
contraseñas existentes ni crea una cuenta de SolarUY**. Volver a ejecutarlo o
editar `.env` no recupera una contraseña ya cambiada en Keycloak.

## 2. Seleccionar el realm solaruy

Abrir **Manage realms** en el menú lateral y seleccionar **solaruy**.

![Acceso a Manage realms desde la consola](Tutorial/Tutorial_4.png)

![Selección del realm solaruy](Tutorial/Tutorial_5.png)

Comprobar que el realm actual sea `solaruy` y abrir **Users**.

![Menú Users dentro del realm solaruy](Tutorial/Tutorial_6.png)

El nombre `admin` de la esquina superior derecha identifica la sesión de la
consola. No significa que el usuario de SolarUY tenga el rol `ADMIN`.

## 3. Crear el usuario de la aplicación

Pulsar **Create new user**, o **Add user** si ya hay otros usuarios.

![Listado vacío con Create new user](Tutorial/Tutorial_7.png)

Completar el nombre de usuario, correo y los campos de perfil que exija el
formulario. Elegir datos propios; `santi` es el usuario mostrado en las capturas,
no una cuenta incluida con el proyecto. Para la prueba local, dejar
**Required user actions** vacío salvo que se quiera exigir una acción adicional
en el primer acceso. Pulsar **Create**.

![Formulario para crear el usuario](Tutorial/Tutorial_8.png)

Mantener el usuario **Enabled** y guardar los cambios de perfil con **Save**.

![Detalles del usuario creado](Tutorial/Tutorial_9.png)

## 4. Asignar una contraseña

En **Credentials**, pulsar **Set password**. Para una cuenta que ya tenga
contraseña, utilizar la acción de restablecimiento disponible en esa pestaña.

![Pestaña Credentials sin contraseña asignada](Tutorial/Tutorial_10.png)

Escribir y confirmar una contraseña propia. Para ingresar directamente durante
la prueba local, desactivar **Temporary** antes de guardar con **Save** y
confirmar si se solicita. Si se deja activado, Keycloak exigirá cambiar la
contraseña en el siguiente acceso; la captura muestra precisamente esa opción
activada para ilustrar su significado.

![Asignación de contraseña y opción Temporary](Tutorial/Tutorial_11.png)

Esta contraseña pertenece al usuario de `solaruy`. Es independiente de las
contraseñas de MySQL y del administrador de Keycloak definidas en `.env`.

## 5. Asignar el rol ADMIN antes de iniciar sesión en SolarUY

Abrir **Role mapping**. El rol `default-roles-solaruy` por sí solo no concede
los permisos de administración de la aplicación.

![Roles iniciales del usuario](Tutorial/Tutorial_12.png)

Pulsar **Assign role** y elegir **Realm roles**.

![Selección de roles de realm](Tutorial/Tutorial_13.png)

Marcar **ADMIN**, en mayúsculas, y confirmar con **Assign**. Comprobar que
aparezca en la lista de roles asignados al usuario.

![Asignación del rol ADMIN](Tutorial/Tutorial_14.png)

El cliente `solaruy` está configurado en el archivo de importación para enviar
los roles en el campo `roles`; SolarUY comprueba `ADMIN` para las operaciones
administrativas. No basta con llamar `admin` a un usuario.

## 6. Ingresar a SolarUY

Abrir [SolarUY](http://localhost:5058) y pulsar **Ingresar al panel**.

![Acceso al panel desde la landing](Tutorial/Tutorial_16.png)

En la pantalla de **SOLARUY**, ingresar con el usuario recién creado y su contraseña.

![Inicio de sesión del usuario del realm solaruy](Tutorial/Tutorial_17.png)

Si Keycloak solicita completar nombre, apellido u otros datos del perfil,
completarlos y pulsar **Submit**. Esta pantalla corresponde a la información
requerida del usuario, no a un fallo de conexión.

![Actualización de los datos obligatorios del perfil](Tutorial/Tutorial_18.png)

Al finalizar, debe abrirse el panel con la sesión autenticada.

![Panel administrativo después de iniciar sesión](Tutorial/Tutorial_19.png)

## 7. Si el rol está asignado pero la aplicación rechaza el acceso

Si se inició sesión antes de asignar `ADMIN`, SolarUY puede conservar una cookie
con los permisos anteriores. En esta situación se observó el error
**Solo un usuario ADMIN puede administrar tareas** al cargar el panel.

![Error de acceso por rol no reconocido en la sesión](Tutorial/Posible%20error%20por%20ingresar%20con%20el%20usuario%20antes%20de%20registrarlo%20como%20admin%20en%20el%20realm_Solucion_BorrarCookies.png)

Primero comprobar el rol en **solaruy → Users → usuario → Role mapping**.
Después cerrar la sesión de SolarUY y volver a entrar. Una ventana privada
permite comprobar si el problema está en la sesión anterior.

Si la página de error impide cerrar sesión, en Brave o un navegador Chromium:

1. Abrir `http://localhost:5058` y presionar **F12**.
2. Abrir **Application**; si no aparece, buscarla en el menú **»**.
3. En **Cookies**, seleccionar `http://localhost:5058`.
4. Eliminar `SolarUy.Session` y sus fragmentos `SolarUy.SessionC1`, `C2`, etc.,
   si existen. Recargar la página e iniciar sesión nuevamente.

Como alternativa, en **Application → Storage**, desmarcar todo salvo **Cookies**
y pulsar **Clear selected**, como en la captura. Esa alternativa elimina las
cookies de `localhost`, por lo que también puede cerrar la sesión de la consola
de Keycloak. No borra cookies de otros dominios ni usuarios de la base de datos.

![Limpieza de cookies del sitio con las otras opciones desmarcadas](Tutorial/SolucionError.png)

Si falla también con una sesión nueva, revisar los roles y la configuración
del cliente; no asumir que todos los rechazos se resuelven borrando cookies.
