# Requisitos: gestión de paneles solares

Estado: propuesta pendiente de aprobación.

## Objetivo
Permitir registrar, consultar y modificar los paneles solares de un nodo
desde el panel administrativo de SolarUy.

## Alcance
- Consultar los paneles registrados y el detalle de uno.
- Registrar un panel asociado a un nodo existente.
- Modificar los datos de un panel existente (incluido el nodo).
- Persistir en MySQL con Entity Framework Core.
- Integrar en el monolito Blazor Web App (Interactive Server).

Fuera de alcance: eliminar paneles, gestión de nodos (alta, baja,
modificación), mediciones, tareas de mantenimiento y cambios automáticos de
estado.

## Datos del panel
Nodo (obligatorio, debe existir), estado (Bueno, Intermedio, Rotura),
potencia actual y orientación.

## Reglas y validaciones
- El nodo debe existir; si no, se rechaza la operación.
- Orientación obligatoria, sin solo espacios, máximo 50 caracteres,
  con recorte de espacios externos.
- Potencia actual: número finito mayor o igual a 0.
- El estado debe ser un valor válido del enumerado.
- MySQL genera el identificador.
- Las validaciones se ejecutan en el servidor; el formulario solo ayuda.

## Acceso
- Cualquier usuario autenticado puede consultar.
- Solo el rol `ADMIN` puede registrar y modificar (se verifica en la capa de
  aplicación, además de ocultar los controles).

## Criterios de aceptación
1. Un usuario autenticado ve la lista de paneles.
2. Un ADMIN registra un panel con datos válidos y un nodo existente.
3. Un ADMIN modifica orientación, potencia, estado y nodo de un panel.
4. Un usuario sin rol ADMIN no puede registrar ni modificar, ni siquiera
   invocando el caso de uso fuera de la interfaz.
5. Se rechazan nodo inexistente, orientación vacía o demasiado larga y
   potencia negativa o no numérica.
6. Modificar un panel inexistente informa un error comprensible.
7. Los datos persisten tras reiniciar la aplicación.
8. La solución compila y las pruebas de Dominio pasan.

## Decisiones pendientes
- Unidad de la potencia actual (el diagrama no la indica).
- A futuro, Estado y PotenciaActual los actualizarán Mediciones y Tareas;
  por ahora se editan a mano.
- Cómo se crearán los nodos (otra funcionalidad).