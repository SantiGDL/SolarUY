# Tareas: gestión de paneles solares

Estado: pendiente de aprobación.

- [x] T01 — Parques / Dominio: definir la entidad `Panel` y el enumerado `EstadoPanel` con los campos `Id`, `Estado`, `PotenciaActual`, `Orientacion` y `NodoId`, y documentar la relación con `Nodo`. Finaliza cuando la entidad y el enumerado queden en dominio, con propiedades relevantes y sin dependencias externas.
- [x] T02 — Parques / Aplicación: crear los contratos de acceso a datos necesarios para paneles y la validación del caso de uso `PanelService`. Finaliza cuando se verifiquen las condiciones de validación: nodo existente, orientación no vacía ni demasiado larga, potencia finita no negativa, estado válido y autorización `ADMIN`.
- [x] T03 — Parques / Infrastructure: implementar `PanelRepository` y el mapeo de `Panel` en EF Core para persistir en MySQL. Finaliza cuando los datos se guarden con el identificador de nodo y la entidad quede mapeada sin mezclar lógica de negocio en infraestructura.
- [x] T04 — Presentación / Blazor: construir el listado y el detalle de paneles para usuarios autenticados y habilitar formulario de registro y edición solo para `ADMIN`. Finaliza cuando la vista muestre datos reales, oculte controles según el rol y comunique errores comprensibles del servidor.
- [x] T05 — Parques / Validación: implementar o ajustar las comprobaciones del caso de uso para rechazar panel inexistente, nodo inexistente, orientación inválida y potencia negativa. Finaliza cuando las operaciones fallidas no persistan cambios y el mensaje devuelto sea claro.
- [x] T06 — Pruebas de Dominio: crear pruebas xUnit para las reglas de validación y los casos límite relevantes de `Panel`, o documentar que la funcionalidad no agrega comportamiento de dominio y que no hay proyecto de pruebas configurado para este cambio. Finaliza cuando el comportamiento esperado quede cubierto o claramente pendiente y registrado.
- [x] T07 — Verificación: ejecutar `dotnet build SolarUy.sln` y las pruebas aplicables para confirmar compilación y validación del comportamiento. Finaliza cuando la solución compile y las comprobaciones del alcance se hayan ejecutado con resultados registrados.
- [x] T08 — Calidad y cierre: revisar dependencias entre capas y confirmar que la UI no accede a infraestructura ni repositorios directamente, que la autorización se valida en capa de aplicación y que el diseño queda coherente con [specs.md](specs.md). Finaliza sin referencias incorrectas ni dependencias circulares.

## Verificación parcial

La funcionalidad todavía no está implementada; por tanto, todas las tareas quedan pendientes. La validación final se realizará cuando se complete la implementación y los resultados de compilación y pruebas queden registrados.
