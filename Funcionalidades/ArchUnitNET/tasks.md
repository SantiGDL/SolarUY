# Tareas de implementación de ArchUnitNET

## Resultados

Ejecutar desde la raíz:

```powershell
dotnet test SolarUyPruebas/Arquitectura/SolarUyArquitecturaPruebas.csproj
```

La comprobación automatiza las dependencias entre módulos y los límites de
Dominio, Aplicación y Presentación. `Program` se identifica por el punto de entrada
del ensamblado y puede componer Infraestructura. La aprobación de contratos y
eventos, la lógica de negocio, el acceso dinámico por reflexión y los tiempos
de vida de DI siguen sujetos a revisión de código.

La solución compiló con 0 errores y 3 avisos NU1903 de la misma dependencia
preexistente. La ejecución final pasó las 112 pruebas unitarias y 21 de las 22
pruebas de arquitectura. ARQ-01 sigue fallando por las 22 dependencias de la
tabla. El descubrimiento se verificó con `dotnet test`. Queda por verificar
desde la interfaz de Rider. SonarQube quedó fuera de esta ejecución.

La verificación temporal agregó una referencia de Dominio al contexto compartido
y un `@inject` del mismo contexto en Razor. ARQ-02 y ARQ-04 detectaron los
dos accesos. ARQ-01 registró el enum `EstadoPanel` expuesto por el DTO temporal y
detectó la referencia del consumidor al enum. El consumidor también construyó
un DTO permitido y recibió otro del contrato local. Esas referencias quedaron
permitidas. Se retiraron los archivos temporales.

### Incumplimientos preexistentes de ARQ-01

Cada destino de la tabla corresponde a una dependencia prohibida. Las correcciones
requieren revisión antes de modificar la aplicación.

| Origen | Destinos | Corrección propuesta |
|---|---|---|
| `Consultas.ConsultaNaturalRequest` | `Mediciones.ModoHistorial`, `Mediciones.PeriodoHistorial` | Diseñar filtros del contrato local de historial y mapearlos en Mediciones. |
| `Consultas.ConsultaNaturalParser` | `Mediciones.OpcionHistorialDto`, `Mediciones.ModoHistorial`, `Mediciones.PeriodoHistorial` | Consumir las opciones y filtros definidos por ese contrato. |
| `Consultas.ConsultaNaturalService` | `Mediciones.FilaHistorialDto`, `Mediciones.OpcionHistorialDto`, `Mediciones.ResultadoHistorialDto`, `Mediciones.HistorialMedicionesService`, `Mediciones.ConsultaHistorial`, `Mediciones.ModoHistorial`, `Mediciones.PeriodoHistorial` | Exponer una API local de lectura de historial con DTO y filtros acordados. |
| `Consultas.ConsultaNaturalService` | `Tareas.ITareasService`, `Tareas.TareaDto`, `Tareas.EstadoTarea` | Exponer una API local de lectura de mantenimiento con su DTO de salida. |
| `Parques.PanelService` | `Tareas.ITareasService`, `Tareas.TareaDto` | Definir una operación local acotada para solicitar el mantenimiento automático. |
| `Tareas.ITareasService` | `Parques.EstadoPanel` | Definir la entrada de la operación automática en el contrato local. |
| `Tareas.TareasService` | `Parques.IUsuarioActual`, `Parques.AccesoNoAutorizadoException`, `Parques.EstadoPanel` | Definir la responsabilidad de usuario actual, expresar el error en Tareas y consumir la entrada contractual. |
| `Tareas.TareaRepositorio` | `Parques.Panel` | Resolver la existencia del panel mediante una operación de lectura del módulo Parques y mantener el identificador en Tareas. |

## Lista de implementación

- [x] **T01 — Todos los módulos y capas.** Inventariar tipos compilados, namespaces, componentes Razor y miembros de composición. **Finalización** — Quedan identificados los tipos representativos de cada capa, la infraestructura compartida y el registro en `Program.cs`.
- [x] **T02 — Comunicación entre módulos.** Inventariar las interfaces expuestas en `Interfaz.Apis.Local.Contratos`, los DTO que componen sus entradas y salidas, y los eventos expuestos. Depende de T01. **Finalización** — Quedan identificados los contratos y DTO de Parques, los tipos internos usados desde otros módulos y los DTO o eventos cuya exposición requiere confirmación. Cada entidad o enum interno expuesto por una firma o DTO queda fuera del permiso y se registra con el contrato, el miembro y el tipo.
- [ ] **T03 — Proyecto de pruebas.** Crear `SolarUyArquitecturaPruebas.csproj` en `SolarUyPruebas/Arquitectura`, incorporarlo a la solución y agregar la referencia a SolarUy y los paquetes compatibles con .NET 10 y xUnit 2. Depende de T01. **Finalización** — La solución restaura y compila, y Rider y `dotnet test` descubren las pruebas.
- [x] **T04 — Selectores de módulos, capas y frameworks.** Cargar el ensamblado de SolarUy y los tipos externos requeridos por los controles. Depende de T03. **Finalización** — Se verifican las selecciones esperadas, incluidos Razor y la infraestructura compartida. Una selección obligatoria vacía produce un diagnóstico y Mediciones puede tener una superficie de contratos locales vacía.
- [x] **T05 — Comunicación entre módulos (ARQ-01).** Comprobar las relaciones entre propietarios por identidad del tipo referenciado, independientemente de cómo se obtuvo la instancia. Depende de T02 y T04. **Finalización** — Mediciones puede usar los catálogos de Parques y referenciar sus DTO permitidos. La prueba identifica origen y destino cuando un consumidor referencia contratos de servicio, implementaciones, DTO internos o una entidad o enum interno que aparezca en una firma expuesta.
- [x] **T06 — Dominio y Aplicación (ARQ-02 y ARQ-03).** Implementar los límites de capa y framework sobre tipos compilados. Depende de T04 y T05. **Finalización** — Las dependencias prohibidas se informan y los catálogos locales confirmados siguen disponibles para los consumidores permitidos.
- [x] **T07 — Presentación y composición (ARQ-04 y ARQ-05).** Cubrir los componentes compilados y los adaptadores de interfaz, incluidas referencias introducidas por inyección. Depende de T04. **Finalización** — Se detecta el acceso directo a persistencia y la excepción de registro queda limitada al punto de composición identificado.
- [x] **T08 — Verificación.** Compilar la solución y ejecutar los proyectos de pruebas de arquitectura y de Parques. Depende de T05 a T07. **Finalización** — Se comprueban los casos controlados de acceso a Infraestructura, uso de un DTO permitido y referencia a un enum interno expuesto. Se retiran los cambios temporales, se repite la ejecución y se registran resultados, fallos preexistentes, correcciones propuestas y bloqueos. Cualquier cambio de comportamiento de Dominio que se autorice después llevará pruebas xUnit en su propia tarea.
- [x] **T09 — Entrega.** Documentar el comando exacto de arquitectura, las reglas ejecutadas y las comprobaciones de revisión manual. Depende de T08. **Finalización** — Los resultados y las decisiones pendientes quedan disponibles para revisión, con las refactorizaciones de aplicación fuera de esta entrega.
