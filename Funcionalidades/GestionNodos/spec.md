# Diseño: gestión inicial de nodos

## Modelo

El diagrama de clases del proyecto define `Nodo` con identificador, nombre,
ubicación dentro del parque e identificador del parque. La relación es de
muchos nodos a un parque. Los límites de nombre (100) y ubicación (250)
mantienen los mismos tamaños que los campos equivalentes ya usados por la
gestión de parques.

## Arquitectura

- **Presentación:** la lista de parques enlaza a `/admin/parques/{id}/nodos`.
  La nueva pantalla muestra el parque, sus nodos y el formulario de alta.
- **Aplicación:** `INodosService` autoriza la consulta y el alta y valida que
  exista el parque antes de operar.
- **Dominio:** `Nodo.Crear` valida y normaliza los datos.
- **Infraestructura:** `NodoRepositorio` consulta y persiste mediante EF Core.
- **Persistencia:** tabla `Nodos`, FK a `Parques` con borrado restringido e
  índice por `ParqueId`.

No se agrega una API HTTP: Blazor Interactive Server llama a los servicios C#
del monolito.
