# Requisitos: gestión inicial de nodos

## Objetivo

Permitir consultar y registrar nodos asociados a un parque solar existente.

## Alcance

- Consultar los nodos de un parque desde la gestión de parques.
- Registrar un nodo asociado al parque seleccionado.
- Guardar nombre y ubicación dentro del parque.
- Persistir los nodos en MySQL mediante Entity Framework Core.
- Reutilizar la autenticación y autorización de SolarUy: usuarios autenticados
  consultan; solo usuarios con rol `ADMIN` registran.

La edición y eliminación de nodos, la gestión de paneles, las mediciones y la
asignación de usuarios quedan fuera de esta primera entrega.

## Reglas y validaciones

- El parque asociado debe existir.
- Nombre y ubicación son obligatorios; se recortan espacios externos.
- El nombre admite hasta 100 caracteres y la ubicación hasta 250.
- La validación se realiza en el servidor; la validación del formulario solo
  mejora la experiencia.

## Criterios de aceptación

1. Un usuario autenticado puede abrir un parque y consultar solo sus nodos.
2. Un usuario `ADMIN` puede registrar nodos para el parque seleccionado.
3. Un usuario sin `ADMIN` no puede registrar nodos aunque invoque el servicio.
4. No se puede registrar un nodo para un parque inexistente.
5. Los datos se guardan en MySQL y el FK impide nodos huérfanos.
6. La solución compila y pasan las pruebas del módulo.
