# Requisitos: gestión inicial de parques solares

Estado: requisitos aprobados para la implementación inicial.

## Objetivo

Permitir consultar y registrar parques solares desde el panel administrativo de
SolarUy.

## Alcance

- Consultar los parques registrados.
- Registrar un parque con nombre y dirección.
- Visualizar en la parte superior de la gestión un tablero interactivo de los
  parques y sus nodos asociados.
- Persistir los datos en MySQL local usando Entity Framework Core.
- Integrar la gestión en el monolito Blazor Web App con Interactive Server.
- Exigir autenticación con Keycloak en el panel y autorizar el registro solo a
  usuarios con el rol `ADMIN`.

Quedan fuera de esta etapa la modificación y eliminación de parques, la gestión
de paneles, las mediciones, el mantenimiento y el proveedor de identidad
institucional/de producción. Los nodos se gestionan desde el flujo asociado a
cada parque.

## Reglas y validaciones

- Nombre y dirección son obligatorios y no pueden consistir solo en espacios.
- El nombre admite hasta 100 caracteres y la dirección hasta 250.
- Se recortan los espacios externos antes de guardar.
- El nombre debe ser único después de recortar espacios y comparar sin
  distinguir mayúsculas y minúsculas.
- MySQL genera el identificador entero.
- Las validaciones se ejecutan en la lógica del servidor; la validación del
  formulario solo mejora la experiencia.

## Acceso

- Keycloak es el proveedor OpenID Connect local.
- La aplicación utiliza Authorization Code con PKCE y cookies de sesión; no se
  guarda un secreto de cliente en el código.
- Cualquier usuario autenticado puede consultar la lista.
- Solo un usuario autenticado con el rol `ADMIN` puede registrar.
- La autorización se verifica en la capa de aplicación, además de ocultar el
  formulario a usuarios que no sean administradores.

## Criterios de aceptación

1. Un usuario autenticado puede consultar y visualizar la lista de parques.
2. Un usuario con rol `ADMIN` puede registrar un parque.
3. Un usuario sin rol `ADMIN` no puede registrar, incluso si invoca el caso de
   uso fuera de la interfaz.
4. Se rechazan datos vacíos, espacios en blanco y longitudes fuera de límites.
5. Los nombres duplicados se rechazan sin distinguir mayúsculas ni espacios
   externos.
6. Los parques se conservan en MySQL tras reiniciar la aplicación y el
   contenedor mientras se conserve el volumen.
7. La interfaz presenta confirmaciones y errores comprensibles sin filtrar
   detalles internos.
8. La solución compila y las pruebas de comportamiento pasan.

## Decisiones pendientes

- El proveedor institucional de identidad y los roles de producción permanecen
  fuera de alcance.
