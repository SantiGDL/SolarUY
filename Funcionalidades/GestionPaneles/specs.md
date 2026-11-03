# Diseño: gestión de paneles solares

Basado en [requirements.md](requirements.md).

## Organización del diseño

La funcionalidad se integra con el módulo de Parques, con validación de acceso en Usuarios y con la capa de presentación de Blazor. El diseño mantiene la separación de responsabilidades de Clean Architecture y evita referencias a infraestructura desde la UI o al dominio desde la presentación.

## Módulo Parques

### Dominio

**Entidades y reglas principales**

- `Panel`
  - `int Id`
  - `EstadoPanel Estado`
  - `double PotenciaActual`
  - `string Orientacion`
  - `int NodoId`
- `EstadoPanel` con valores: `Bueno`, `Intermedio`, `Rotura`.

**Regla de dominio**

- El panel representa la entidad física asociada a un nodo de un parque.
- La orientación debe conservarse como texto normalizado: sin espacios externos, con longitud máxima de 50 caracteres, conforme a la validación de la capa de aplicación.
- La potencia actual es un valor numérico no negativo y se expresa en la unidad que defina el negocio en la siguiente etapa de análisis.
- El estado y el nodo forman parte del contrato del panel y deben validarse antes de persistir la entidad.

**Relaciones**

- `NodoId` identifica el nodo propietario del panel.
- La relación con `Nodo` se expresa por identificador, sin introducir dependencias cruzadas entre módulos en el dominio.
- La entidad no debe conocer detalles de Entity Framework ni de la base de datos.

### Aplicación

**Casos de uso y contratos**

- Consultar paneles registrados.
- Consultar el detalle de un panel.
- Registrar un panel asociado a un nodo existente.
- Modificar un panel existente y cambiar su nodo si corresponde.

**Servicios**

- `PanelService` coordina las operaciones de lectura, validación y persistencia.
- Utiliza `IPanelRepository` para consultar y guardar paneles.
- Utiliza `INodoRepository` o el contrato equivalente de validación de nodos para confirmar que el nodo existe antes de registrar o actualizar.

**Validaciones del caso de uso**

- Rechazar operaciones si el usuario no tiene rol `ADMIN`.
- Rechazar si el nodo no existe.
- Rechazar si la orientación es nula, vacía o compuesta solo por espacios.
- Rechazar si la orientación supera 50 caracteres después del recorte.
- Rechazar si la potencia es negativa o no es un número finito.
- Rechazar si el estado no es un valor válido del enumerado.
- Informar un error comprensible cuando el panel no existe en la operación de modificación.

**Resultado esperado**

- Las reglas de negocio se resuelven en la capa de aplicación y no en la interfaz.
- Los datos de entrada de la operación son modelos de aplicación o DTOs con los campos necesarios para la operación, sin exponer entidades persistentes a la UI.

### Infraestructura

**Persistencia**

- Implementar `PanelRepository` en Infraestructura.
- Definir el mapeo de `Panel` en `ParquesDbContext` o el contexto del módulo correspondiente.
- Guardar `NodoId` como clave de relación con `Nodo` y mantener el campo de estado como valor enumerado mapeado por EF Core.

**Consideraciones**

- El acceso a datos queda encapsulado en Infraestructura.
- El repositorio expone operaciones de consulta y escritura, sin mezclar lógica de dominio ni UI.
- La persistencia no debe incluir reglas de negocio complejas; solo la traducción entre entidad de dominio y tabla de datos.

### Presentación

**Responsabilidad de la interfaz**

- Mostrar listado de paneles para cualquier usuario autenticado.
- Habilitar registro y edición solo cuando el usuario actual tiene rol `ADMIN`.
- Validar campos de ayuda en la vista, pero nunca sustituir la validación de servidor.
- Presentar mensajes claros cuando la operación falle por nodo inexistente, orientación inválida o potencia negativa.

**Comportamiento de la UI**

- La página o componente administrativo mostrará la lista y el formulario de edición.
- Los botones de creación y edición estarán ocultos o deshabilitados según la autorización del usuario.
- Los eventos de formulario invocan al caso de uso de aplicación correspondiente.

## Módulo Usuarios

### Dominio y aplicación

- La autenticación de usuarios y la verificación del rol `ADMIN` se consideran parte del módulo de Usuarios.
- El caso de uso de paneles debe consultar la identidad actual y autorizar la operación según el rol.
- La autorización se aplica en la capa de aplicación, además de la ocultación visual de controles.

**Regla de seguridad**

- El comportamiento protegido no debe depender solo de la UI; si se invoca el caso de uso desde fuera de la interfaz, debe rechazarse igual.

## Comunicación entre módulos

- Presentación -> Aplicación: los componentes Blazor invocan servicios de aplicación, nunca repositorios ni DbContext.
- Aplicación -> Dominio: los casos de uso validan, coordinan y delegan reglas de negocio a entidades y contratos del dominio.
- Aplicación -> Infraestructura: las operaciones de persistencia se implementan a través de interfaces definidas en dominio y concretadas en infraestructura.
- Usuarios -> Parques: el módulo de usuarios proporciona la identidad y el rol para autorizar la gestión.
- No existe integración con un simulador ni con sensores en esta funcionalidad; la gestión se limita a la persistencia y la interfaz administrativa.

## Datos persistidos

- `Panel` se guarda en MySQL por medio de EF Core.
- La clave primaria la genera la base de datos.
- El identificador de nodo es obligatorio y debe existir en el momento de persistir.
- La persistencia debe sobrevivir reinicios de la aplicación sin cambios manuales adicionales.

## Verificación propuesta

- Compilar la solución con `dotnet build SolarUy.sln`.
- Ejecutar las pruebas de Dominio disponibles y las pruebas estructurales si existieran.
- Revisar que la capa de presentación no esté accediendo a infraestructura directamente.
- Verificar que la autorización por rol está aplicada en la lógica de aplicación y no solo en la vista.

## Limitaciones y decisiones pendientes

- La unidad de la potencia actual queda por confirmar en la siguiente etapa de diseño.
- La evolución futura de `Estado` y `PotenciaActual` por mediciones y tareas queda fuera del alcance de esta funcionalidad.
- La creación y mantenimiento de nodos no se resuelve aquí; la validación de existencia exige ese proceso ya definido por otra funcionalidad.
