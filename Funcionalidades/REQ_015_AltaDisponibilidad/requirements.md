# REQ_015 — Alta disponibilidad activo-pasivo

## Objetivo

Se necesita una prueba de concepto local que permita seguir utilizando SolarUY
cuando se detenga su instancia principal, por ejemplo durante la instalación de
una nueva versión. La solución deberá ser sencilla de comprender y demostrar
para la entrega del 18 de noviembre de 2026.

## Despliegue

- Se utilizará un único proyecto Docker Compose, combinando `compose.yaml` y
  `compose.ha.yaml`, con MySQL y Keycloak existentes.
- Se ejecutarán dos instancias de SolarUY con la misma imagen Docker:
  `solaruy1` como principal y `solaruy2` como reserva.
- Nginx actuará como proxy inverso y utilizará `backup` para la reserva. Mientras
  la principal esté disponible, las nuevas peticiones deberán dirigirse a ella.
  Si deja de responder, la reserva deberá atender nuevas conexiones.
- La dirección pública será `http://localhost:7000`, exclusivamente para la
  prueba local, sin certificados ni configuración TLS.
- Se conservarán los puertos de MySQL, Keycloak, SonarQube y Rider. El despliegue
  de demostración no deberá alterar el desarrollo habitual desde Rider.
- Se reutilizarán los conceptos y archivos del ejemplo del profesor
  `01_1_webApiEjemplo` cuando corresponda, manteniendo los cambios al mínimo.

## Funcionamiento y autenticación

La aplicación deberá permitir navegar, autenticarse y utilizar la gestión tanto
en la principal como en la reserva. Blazor Interactive Server y SignalR deberán
funcionar en la instancia que esté atendiendo las conexiones.

El inicio y cierre de sesión seguirán utilizando Keycloak, con los roles y
controles de seguridad existentes. El flujo OIDC y sus cookies deberán funcionar
en Brave mediante HTTP local, sin desactivar validaciones ni protecciones del
navegador. Se conservará también el acceso actual desde Rider.

Las dos instancias deberán utilizar la misma base MySQL y sus datos actuales.
No se modificarán entidades, migraciones, esquema ni reglas de negocio. Tampoco
se ejecutarán migraciones concurrentes ni se duplicarán productores de mediciones
o procesos que modifiquen datos compartidos por iniciar dos instancias.

## Pérdida de sesión y reconexión

Se acepta que una caída interrumpa el circuito de Blazor, los formularios sin
guardar y una simulación activa. Para continuar, podrá ser necesario recargar la
aplicación e iniciar sesión nuevamente. Los datos ya confirmados en MySQL deberán
seguir disponibles; no se exige recuperar el estado anterior ni las cookies.

Se utilizará preferentemente el mecanismo de reconexión existente de Blazor,
sin cerrar sesiones automáticamente ante microcortes. Como mejora opcional,
se podrá mostrar un aviso de pérdida de conexión y posibles cambios sin guardar,
con un botón para recargar. Si esto complica la solución, se conservará la
interfaz estándar de Blazor.

## Restricciones y límites

- Las instancias deberán poder iniciarse, detenerse y reiniciarse de forma
  independiente. Se documentará cómo reemplazar una sin detener ambas a la vez.
- Se conservarán la base, usuarios, credenciales y volúmenes existentes. Los
  secretos no se incluirán en el repositorio ni en la imagen Docker.
- No se incorporarán Redis, Kubernetes, backplanes, afinidad de sesión,
  sincronización de memoria ni recuperación automática de formularios o simulaciones.
- No se requiere persistencia compartida de Data Protection. Si resultara
  imprescindible para el funcionamiento básico, deberá justificarse antes de
  ampliar el alcance.
- El objetivo es atender nuevas conexiones, no garantizar continuidad de
  solicitudes o sesiones activas ni ausencia de errores transitorios.
- Nginx, MySQL, Keycloak y el equipo local quedan fuera de la redundancia de esta
  prueba. La solución no cubre una caída completa del equipo o de Docker.
- El modo activo-activo queda fuera de esta etapa y requiere aprobación adicional.

## Criterios de aceptación

1. Levantar MySQL, Keycloak, Nginx y ambas instancias en el mismo proyecto Compose,
   reutilizando la base existente y sin modificar su esquema.
2. Acceder a `http://localhost:7000`, iniciar sesión y utilizar la gestión.
   Comprobar mediante logs que las nuevas peticiones llegan a `solaruy1`.
3. Detener únicamente `solaruy1`, recargar y comprobar que `solaruy2` permite
   autenticarse y utilizar la aplicación, con Blazor y SignalR funcionando.
4. Volver a iniciar `solaruy1` y comprobar su recuperación para nuevas conexiones,
   recargando y autenticándose nuevamente si hace falta.
5. Verificar el control independiente de ambas instancias, la conservación de
   datos y el funcionamiento habitual desde Rider, sin procesos duplicados.
6. Registrar el tiempo de conmutación y los errores transitorios observados.
   No considerar suficiente una respuesta de la página de inicio sin interacción
   autenticada ni exigir recuperación del circuito o simulación anteriores.
7. Compilar en Release y ejecutar las pruebas de arquitectura y funcionales
   existentes. Documentar las comprobaciones manuales y cualquier bloqueo,
   sin modificar ni debilitar las reglas de ArchUnitNET.
