# REQ_010 — Consulta histórica de mediciones y exportación a PDF

## Objetivo

Consultar las mediciones persistidas del parque, nodo o panel seleccionado,
mostrar los resultados en una página y permitir su exportación a PDF.
El histórico utiliza los datos disponibles y su fecha y hora de registro;
no requiere que el simulador permanezca activo ni que exista información
para todo el período solicitado.

## Formulario de consulta

Utilizar un formulario similar al de Generación en tiempo real, con:

- Selectores dependientes de Parque, Nodo y Panel.
- Selección del período de consulta.
- Selección del modo de presentación de los datos.
- Una acción para ejecutar la consulta.

Los nodos disponibles deben corresponder al parque seleccionado y los paneles
al nodo seleccionado. Un cambio de selección debe impedir consultar
combinaciones incompatibles.

## Períodos disponibles

Tomar una única fecha y hora de referencia al ejecutar cada consulta.

| Opción | Datos incluidos |
| --- | --- |
| Todos los datos | Todas las mediciones disponibles de la selección hasta el instante de consulta. |
| Últimas 24 horas | Mediciones desde 24 horas antes del instante de consulta hasta ese instante. |
| Últimos 30 días | Mediciones desde 30 días antes del instante de consulta hasta ese instante. |
| Últimos 365 días | Mediciones desde 365 días antes del instante de consulta hasta ese instante. |

Los períodos son móviles, no días, meses o años calendario. El filtrado debe
realizarse en la base de datos por selección y fecha y hora, para admitir
volúmenes mayores aunque la prueba de concepto tenga pocos registros.

## Modos de presentación

Priorizar las siguientes tres opciones:

1. **Datos diarios:** reducir la cantidad de resultados a una presentación
   diaria, con intervalos de 24 horas, en lugar de mostrar cada lectura
   generada aproximadamente cada cinco segundos.
2. **Promedio del período:** mostrar el promedio de los valores registrados
   que correspondan a la selección y al período consultados.
3. **Datos crudos:** mostrar todas las mediciones registradas que correspondan
   a la selección y al período consultados, conservando sus valores y fechas.

El promedio se obtiene únicamente de mediciones existentes. Los intervalos
sin registros no deben introducir ceros ni mediciones ficticias.

La reducción por intervalos de una hora queda fuera de la primera versión.
La definición exacta del resumen diario se registra como decisión pendiente.

## Resultados en pantalla

- Mostrar los datos en la página, identificando la selección, el período,
  sus fechas de inicio y fin cuando correspondan y el modo consultado.
- Identificar la fecha y hora de las lecturas o el intervalo resumido y
  la unidad de los valores mostrados.
- Los valores actuales de las mediciones representan potencia en W.
  Un promedio de estas lecturas también se expresa en W; no presentarlo
  como energía acumulada en kWh.
- Mostrar los registros disponibles aunque solo cubran parte del período.
  No presentar la ausencia de registros como generación cero ni como
  cobertura completa del período.
- Informar cuando la consulta no tenga resultados y cuando ocurra un error.
- Mantener identificados los filtros del resultado obtenido aunque el
  usuario prepare otra selección antes de volver a consultar.
- La consulta no debe generar nuevas mediciones ni modificar el historial.

## Exportación a PDF

Permitir descargar un PDF del resultado consultado. Debe incluir la selección,
el período, las fechas de referencia, el modo, las unidades y los resultados.
La exportación debe corresponder a los datos de la consulta mostrada, sin
recalcular el período móvil al descargar ni incluir nuevas mediciones.
En modo crudo, exportar todos los registros del resultado, sin recortes
silenciosos si la pantalla utiliza paginación.

## Alcance y límites

- Reutilizar las mediciones ya persistidas y mantener el simulador actual.
- Respetar la autenticación y autorización del proyecto; el permiso concreto
  para consultar y exportar debe definirse antes de implementar el acceso.
- Conservar las responsabilidades del monolito modular y las reglas de
  `.agents/Reglas/`. Los detalles de implementación corresponden a `specs.md`.
- No modificar el módulo Tareas, desarrollado en paralelo.
- Alta disponibilidad, consultas en lenguaje natural y cálculo de energía
  acumulada en kWh quedan fuera de esta primera versión.

## Decisiones pendientes

- **Resumen diario:** confirmar si cada intervalo muestra un promedio de sus
  mediciones o una lectura representativa. Definir si las ventanas de 24 horas
  se cuentan desde el inicio del período o se agrupan por día calendario.
  No asumir que estas alternativas producen el mismo resultado.
- **Promedio con varios paneles:** definir si se muestra un promedio por panel
  o un resumen del conjunto. Promediar todas las filas no equivale a calcular
  la potencia total promedio de un parque o nodo.
- **Selección conjunta:** confirmar las opciones Todos los nodos y Todos los
  paneles, propuestas para consultar un parque o nodo completo.
- **Acceso y fechas:** definir los roles autorizados y la zona horaria de
  presentación y agrupación, manteniendo coherencia con las fechas guardadas.
- **Simulaciones simultáneas:** definir cómo interpretar registros de varias
  sesiones sobre los mismos paneles. No descartar ni sumar registros como
  producción única sin una regla acordada.

## Criterios de aceptación

- Las opciones del formulario mantienen la relación Parque → Nodo → Panel.
- Cada período devuelve exclusivamente las mediciones de la selección y
  el rango solicitado, usando el mismo instante de referencia.
- Los datos crudos conservan valores y fechas; el promedio usa solo las
  mediciones existentes; el modo diario sigue la regla que se acuerde.
- Una consulta con información parcial muestra los datos disponibles y una
  consulta vacía informa la ausencia de registros.
- Los resultados indican filtros, fechas, modo y unidades sin confundir
  potencia con energía acumulada.
- El PDF contiene los mismos resultados consultados y su contexto, incluso
  cuando la descarga se solicita después o se modifican los controles.
- Consultar y exportar no altera mediciones ni activa el simulador.

Estado: requisitos para revisión; las decisiones pendientes no representan
comportamientos aprobados ni implementación terminada.
