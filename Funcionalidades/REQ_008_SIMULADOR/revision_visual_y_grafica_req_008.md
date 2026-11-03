# REQ_008 — Revisión visual, Radzen y selección inmediata

Fecha: 3 de noviembre de 2026. Complementa [el informe anterior](implementacion_y_verificacion_req_008.md), sin sustituir la evidencia de aquella implementación.

## Adaptaciones realizadas

- Instalado Radzen.Blazor 12.1.10 en Sensor; registro de componentes y recursos del host, conservando controles y layout existentes.
- Componente GraficaMediciones con RadzenChart y RadzenAreaSeries: línea amarilla, área suave, cuadrícula, fechas reales en el eje horizontal y W en el vertical. Eje temporal ajustado al intervalo de la sesión, incluyendo el caso de una sola muestra.
- Historial en memoria de cada entrega de todos los paneles y su total. El servicio proyecta la serie del nodo o del panel visible; conserva las muestras al cambiar panel y Conectar sobre el mismo nodo, y las limpia al cambiar de parque/nodo o salir. Si un panel no existía en una entrega antigua, no se le inventan muestras con valor cero.
- Eliminado el panel pendiente: su selector usa exclusivamente el nodo activo y cambia indicadores, gráfica, ficha y tabla inmediatamente, sin Conectar, spinner ni envío adicional. Parque/nodo siguen pendientes hasta confirmar; un aviso explica esa situación.
- Reorganizada la pantalla según las referencias: cabecera y tarjeta verde, franja horizontal de Potencia/Generando ahora, gráfica y ficha en fila, tabla inferior completa. La ficha individual contiene Panel, Parque, Nodo, Orientación y Estado.
- Rayo e indicadores de esta pantalla en SVG. Deshabilitar panel permanece deshabilitado y marcado Pendiente; Ver detalle selecciona el panel localmente.
- CSS adaptable: selectores reorganizados, gráfica y ficha apiladas en móvil, tabla con desplazamiento horizontal contenido. Los estados se muestran con los nombres reales del catálogo; Operativos cuenta paneles en estado Bueno, sin alterar generación ni estado de negocio.
- Persistencia, Hub, DTO y permisos de la implementación previa reutilizados. No se agregaron migraciones ni se rediseñó autenticación/autorización.

## Verificaciones de esta revisión

- Compilación de la solución: correcta, sin errores ni advertencias.
- Proyecto xUnit existente: 45 pruebas aprobadas. No cambió el comportamiento de Dominio; no se añadieron pruebas unitarias de otras capas.
- Arranque local con MySQL y Keycloak: correcto; ambos contextos informaron que sus migraciones están actualizadas.
- Gráfica vacía antes de recibir la primera muestra y gráfica real con una, dos y varias muestras comprobadas en navegador.
- Cambio inmediato a Panel 01 y Panel 02, sin pulsar Conectar: ficha, indicadores y tabla cambiaron y la curva conservó todos los puntos de la sesión. Se observaron 12 puntos antes y después de uno de los cambios.
- Conectar sobre el mismo nodo: se observaron 15 puntos antes y después, sin reinicio de la serie ni cambio del último horario guardado por la pulsación.
- Vistas total e individual comparadas con las referencias a 1585 × 992: bloques, franja, gráfica lateral, ficha y tabla inferior presentes. Menú/barra existentes conservan sus propias dimensiones; los valores, estados y curva son los datos de la sesión, no las cifras ilustrativas.
- Móvil a 390 × 844: jerarquía, gráfica y ficha apiladas revisadas. La medición DOM informó ancho de documento y contenido de 375 px, sin desbordamiento horizontal de la página; la tabla desplaza dentro de su contenedor.
- Confirmaciones Entrega guardada observadas mientras se muestra un panel. Generación y envío siguen recorriendo todos los paneles del nodo.

## Problema detectado y corregido

La primera configuración de la serie pasó MarkerType como parámetro directo de RadzenAreaSeries. La compilación no lo rechazó, pero el navegador informó que esa propiedad no es un parámetro público del componente. Se corrigió usando RadzenMarkers como componente hijo, se recompiló y se volvió a ejecutar la prueba con muestras reales: el gráfico renderizó y el aviso de error quedó oculto.

También se corrigió el envolvimiento del texto de conexión en móvil y la escala temporal del caso de una sola muestra. Los comandos y resultados de la entrega anterior no se presentan como prueba automática de estos cambios.

Comandos desde la raíz:

```powershell
dotnet build SolarUy.sln --nologo -m:1 -p:UseSharedCompilation=false
dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj --no-build --nologo
```

La restauración de NuGet y la ejecución se realizaron fuera del sandbox con autorización, porque el entorno restringido bloqueaba red y hosts de compilación. La verificación del navegador integrado se recuperó reiniciando su sesión.

## Comprobaciones aún pendientes

- Cuenta autenticada sin ADMIN y matriz completa de invocación directa del Hub; la restricción existente no se modificó.
- Cambio confirmado a otro parque/nodo y aviso de selección pendiente con datos diferentes; la base usada en la prueba visual solo ofrecía un parque/nodo elegible.
- Catálogo vacío, potencia cero, eliminación de panel y modificación concurrente de catálogo en una sesión visual preparada para esos casos, sin alterar datos existentes del usuario.
- Fallos inducidos de conexión/base de datos y callbacks tardíos bajo latencia artificial.
- ArchUnitNET/CodeQuality.Tests y SonarQube no están configurados; se realizó revisión manual de dependencias y no se instalaron herramientas nuevas.

Estas comprobaciones permanecen pendientes en tasks.md; no se afirma que toda la matriz manual esté completada. Deshabilitar panel sigue siendo un requerimiento extra sin efectos en esta entrega.

## Evidencia visual de esta revisión

- [Vista total](revision_radzen_vista_total.jpg).
- [Vista individual](revision_radzen_vista_individual.jpg).
- [Móvil](revision_radzen_movil.jpg).

Referencias técnicas: [RadzenAreaSeries](https://blazor.radzen.com/docs/api/Radzen.Blazor.RadzenAreaSeries-1) y [configuración oficial](https://blazor.radzen.com/get-started). Los cambios y posibles errores se registran aquí; el README describe únicamente funcionamiento y comunicación.
