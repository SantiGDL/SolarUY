# REQ_008 — Implementación, verificaciones y pendientes

Fecha de verificación: 3 de noviembre de 2026.

## Cambios implementados

- Página `/sensor/mediciones` en Sensor con InteractiveWebAssembly, selección inicial automática y estados separados de selección pendiente y activa.
- Simulación en un servicio cliente, cada 10 segundos y por instancia de página, para todos los paneles del nodo. Valores aleatorios entre cero y PotenciaActual en W; la vista individual no reduce la entrega.
- Cliente SignalR y Hub `/hubs/mediciones`, protegido con sesión y rol ADMIN existentes. Consultas, validación y registro delegados a Aplicación.
- Fachada local de Parques y DTO de transporte en Sensor, sin entidades compartidas ni enlace de fuentes del backend.
- Entidad Medicion, validaciones de Dominio, registro atómico por entrega, MedicionesDbContext, repositorio y migración MySQL con historial propio. Se conservan parque, nodo, panel, valor y fecha UTC.
- Menú Producción habilitado, layout existente reutilizado y botón Deshabilitar panel marcado Pendiente, sin efectos.
- README dedicado a funcionalidades, arquitectura, comunicación y uso. Este documento contiene el detalle de cambios y comprobaciones.

## Decisiones de integración

La fachada `CatalogoProduccionService` utiliza los repositorios ya existentes dentro de Aplicación de Parques, en lugar de invocar servicios que dependen de la identidad del circuito Blazor. No se modifica `UsuarioActual`, la configuración de Keycloak, los roles ni los permisos de las páginas anteriores. El Hub obtiene el acceso ADMIN del principal autenticado del servidor y lo comunica explícitamente a los casos de uso y a la fachada local; ese indicador no forma parte de los mensajes aceptados del navegador. La fachada y todos los casos de uso de Mediciones exigen ese acceso. La integración autenticada funcionó sin corregir la autenticación existente.

Cada instancia genera independientemente; no hay coordinación entre pestañas ni generador en el servidor. La simulación actualiza la vista antes de confirmar persistencia y muestra el estado del guardado por separado. Un envío tiene un límite cliente de ocho segundos; un resultado incierto no se reintenta automáticamente para evitar duplicados. No se incluye recuperación de entregas offline. Cambiar solo vista o repetir Conectar mantiene el ciclo; al cambiar nodo se cancela y espera el ciclo anterior.

La migración `20261103033637_CrearMediciones` crea tabla e índices sin claves foráneas a Parques, para conservar identificadores históricos sin alterar eliminaciones. En desarrollo se aplica al arrancar SolarUy. Fuera de desarrollo debe aplicarse mediante el procedimiento de despliegue del equipo, seleccionando explícitamente MedicionesDbContext.

## Verificación ejecutada

| Control | Resultado de esta ejecución |
| --- | --- |
| Compilación de solución | Correcta, sin advertencias ni errores. |
| xUnit del proyecto existente | 45 pruebas aprobadas; incluye 10 casos nuevos de Dominio de Mediciones. |
| Migración MySQL | Aplicada en la base local; tabla e índices creados. |
| Correspondencia modelo/migración | EF informó que no hay cambios de modelo pendientes. |
| Acceso anónimo a la página | Redirige al inicio de sesión de Keycloak. |
| Acceso ADMIN | Página, catálogo, generación y confirmaciones de guardado comprobados con sesión real local. |
| Vista total e individual | Datos visibles y etiquetas de selección activa comprobados; botón pendiente sin efectos. |
| Selección pendiente | Elegir un panel no cambia la vista hasta Conectar. |
| Persistencia por panel | Consulta MySQL confirmó dos filas por entrega del nodo de dos paneles, incluso en vista individual. |
| Intervalo | Entregas locales separadas aproximadamente por 10 segundos. |
| Límites de valores persistidos | Consulta sobre mediciones y paneles encontró cero lecturas fuera de rango. |
| Dos pestañas | Entregas independientes con distinta fase temporal, ambas guardadas con dos paneles por entrega. |
| Conectar repetido | No reinició ni duplicó el ciclo observado de la segunda instancia. |
| Salida de las páginas | Navegación a Home y cierre de la segunda pestaña; dejaron de producir entregas de esas instancias. |
| Revisión de arquitectura | Manual: Hub sin EF/repositorios; UI sin persistencia; Aplicación sin SignalR ni DTO de Sensor; entidades sin referencias a otros módulos. |

Comandos ejecutados desde la raíz del repositorio:

```powershell
dotnet build SolarUy.sln --nologo -m:1 -p:UseSharedCompilation=false
dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj --nologo -m:1 -p:UseSharedCompilation=false
dotnet ef migrations has-pending-model-changes --project SolarUy/SolarUy.csproj --context MedicionesDbContext --no-build
```

El entorno restringido bloqueó comunicación del compilador/host WebAssembly, host de pruebas y sockets MySQL. Las verificaciones se repitieron fuera del sandbox con autorización y funcionaron. Estos errores del entorno no se atribuyen a la funcionalidad.

## Comprobaciones que permanecen pendientes

- Escenario con usuario autenticado sin ADMIN, incluyendo invocación directa de métodos del Hub. Los controles por rol están implementados, pero no se probó una segunda cuenta con ese perfil.
- Catálogo vacío, otro parque/nodo y potencia cero en una sesión visual; no se alteraron los datos existentes para fabricar esos escenarios.
- Fallo controlado de base de datos, desconexión/reconexión y catálogo modificado durante un envío.
- Eliminación de un panel con historial y comprobación posterior de conservación de filas; no se eliminaron datos existentes del usuario.
- Prueba visual en pantallas móviles y validación de callbacks tardíos bajo latencia artificial.
- No se dispone de pruebas estructurales ArchUnitNET ni análisis SonarQube configurado; no se instalaron herramientas ni se afirma que esos controles pasaron.

No se detectó un bug confirmado de autenticación/autorización durante las pruebas realizadas. Si aparece uno, vincularlo al requerimiento responsable según REQ_008 e informar cualquier bloqueo antes de modificar esa funcionalidad.

## Requerimiento extra

Deshabilitar panel continúa pendiente: no cambia estado, generación ni guardado en esta entrega.

## Evidencia visual

- [Vista total](verificacion_vista_total.jpg).
- [Vista individual](verificacion_vista_individual.jpg).

