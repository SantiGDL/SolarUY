# REQ_010 — Especificaciones de consulta histórica

## Estado y alcance

Diseño para revisión basado en [requirements.md](requirements.md).
Los nombres de componentes y contratos nuevos son propuestas de implementación.
Las decisiones pendientes del requerimiento continúan abiertas: las partes
afectadas del diseño son condicionales y deben cerrarse antes de implementarlas.
Este documento no acredita implementación ni autoriza cambios de alcance.

La funcionalidad consulta mediciones persistidas, presenta resultados y genera
un PDF de la misma consulta. No inicia Sensor, no utiliza el Hub para consultar
y no modifica producción en tiempo real ni Tareas.

## Módulo Mediciones — Dominio

- Reutilizar `Medicion`, que conserva ParqueId, NodoId, PanelId,
  ValorGenerado en W y FechaHora normalizada a UTC.
- Ampliar `IMedicionRepository` con operaciones de lectura necesarias para el
  histórico, manteniendo la escritura existente. No introducir un repositorio
  genérico ni reorganizar las capas actuales.
- Definir los criterios y proyecciones que el contrato de lectura necesite
  dentro de Dominio, sin referencias a DTO de Aplicación, EF, SQL o IQueryable.
- Concentrar en Dominio las reglas de períodos y resumen que se incorporen:
  24 horas, 30 días y 365 días como duraciones móviles; datos crudos sin
  modificación; promedios calculados únicamente sobre valores existentes.
- No cambiar las entidades persistidas para almacenar resultados derivados.
  No calcular kWh ni interpretar promedios de filas como potencia total.

### Decisiones que condicionan el resumen

Antes de definir la agrupación diaria se debe acordar promedio o lectura
representativa, origen de las ventanas y zona horaria. La regla acordada tendrá
una sola definición de negocio, aunque su ejecución se traduzca a una consulta
agregada en la base de datos. Los intervalos adyacentes no deben duplicar filas
de sus límites y un intervalo sin registros no debe crear un valor cero.

Antes del promedio conjunto se debe definir su significado para varios paneles.
No implementar una suma o media entre paneles como supuesto implícito.

## Módulo Mediciones — Aplicación

Proponer `HistorialMedicionesService`, separado del servicio que coordina las
sesiones y órdenes del simulador. Sus dependencias se reciben por constructor.

### Entradas y resultados

- `ConsultaHistorialDto`: selección de parque, nodo y panel, período y modo.
  La nulabilidad de nodo/panel depende de aprobar la selección conjunta.
- `ResultadoHistorialDto`: filtros ejecutados, instante de referencia UTC,
  inicio opcional, fin, zona horaria de presentación, modo, unidad W y filas.
- Cada fila cruda conserva identificador, parque, nodo, panel, fecha y valor.
  Una fila resumida identifica intervalo, ámbito del resumen, valor y número
  de mediciones utilizadas; no inventa una medición ni su identificador.
- Mantener estos resultados independientes de entidades rastreadas por EF,
  tipos HTTP, tipos de bibliotecas PDF y componentes Blazor.

### Caso de uso de consulta

1. Verificar identidad y permisos con una abstracción de acceso adecuada al
   rol que se acuerde. No recibir una autorización confiable desde el navegador.
2. Validar selección, período y modo, y comprobar las asociaciones necesarias.
3. Capturar una única referencia UTC en el servidor. Calcular inicio y fin una
   sola vez. Propuesta para revisión: límites globales inclusivos; las ventanas
   internas se particionan sin solapamientos.
4. Consultar el repositorio con filtros explícitos. Todos los datos omite el
   límite inferior, manteniendo el límite superior.
5. Construir un resultado estable, ordenado por FechaHora e Id para crudos,
   o por intervalo e identificadores para resúmenes.
6. Devolver el resultado con su contexto. Ausencia de filas es un resultado
   vacío, distinto de un error de persistencia o una selección inválida.

No inferir cobertura completa por disponer de algunas filas. Mostrar un aviso
de que el resultado comprende únicamente mediciones registradas.

### Caso de uso de exportación

Definir `IExportadorHistorialPdf` en Aplicación, implementado en Infraestructura.
Recibe una instantánea del resultado y devuelve contenido de archivo mediante
tipos .NET, sin exponer la biblioteca PDF.

Revalidar el acceso al exportar. Utilizar la instantánea del resultado obtenido,
con sus filas y metadatos, sin repetir una consulta móvil. Mantenerla aislada
por componente/circuito y no en un singleton compartido. No confiar en filas
enviadas por el cliente como fuente del documento.

Si hay paginación visual, conservar la totalidad del resultado exportable.
No reconstruirlo consultando de nuevo solo por las mismas fechas: podrían
haber ingresado mediciones tardías. Para volúmenes que no permitan conservarlo
en memoria, revisar una estrategia de instantánea antes de introducir límites;
no truncar silenciosamente ni agregar almacenamiento compartido por rutina.

## Módulo Mediciones — Infraestructura

### Consulta y persistencia

- Reutilizar `MedicionesDbContext` y `MedicionRepository`, creando y liberando
  un contexto por operación con la fábrica existente.
- Aplicar filtros en MySQL mediante EF Core, sin cargar toda la tabla.
  Utilizar lectura sin seguimiento y consultas parametrizadas.
- Ordenar de manera determinista. Agregar en la BD cuando la regla acordada
  y el proveedor permitan traducirla; verificar SQL y resultados antes de
  aceptar la traducción. No trasladar fórmulas de negocio a componentes.
- Conservar los índices actuales y evaluar su uso; agregar otro solo si se
  justifica por las consultas finales. No se prevé una migración por defecto.
- Propagar cancelación y traducir fallos de persistencia a errores de
  aplicación, sin revelar credenciales ni consultas internas al usuario.

### Generación de PDF

No hay una dependencia PDF en `SolarUy.csproj` al preparar este diseño.
Seleccionar en la tarea técnica una biblioteca compatible con el proyecto,
revisando licencia y ejecución en servidor; no instalarla durante esta fase.

El adaptador debe generar un PDF descargable, con encabezado de selección,
período y fechas, modo, unidades, detalle y paginación. Repetir cabeceras de
tablas al cambiar de página, admitir caracteres del español y evitar recortes.
Exportar valores consistentes con la pantalla y todos los registros del
resultado; indicar que corresponden a mediciones disponibles del simulador.

## Presentación de Mediciones — Blazor Interactive Server

Proponer `HistorialMediciones.razor`, con ruta `/admin/historial` y el layout
existente. El rol de acceso se define antes de configurar su autorización.

- Formulario similar a Producción con selectores dependientes, período, modo
  y botón Consultar. No incluir el componente MotorSensor ni una nueva raíz WASM.
- Invocar Aplicación mediante inyección y llamadas C#; no inyectar DbContext
  o repositorios ni crear una API REST para las consultas internas.
- Separar controles pendientes del resultado confirmado. Evitar que respuestas
  antiguas sobrescriban consultas posteriores mediante cancelación o secuencia.
- Mostrar carga, errores, selección inválida y resultado vacío. Restaurar los
  controles después de fallos y cancelar operaciones al salir de la página.
- Mostrar tabla de datos con columnas apropiadas al modo y contexto del
  resultado. La gráfica puede reutilizar Radzen si aporta valor; no es un
  criterio obligatorio del requerimiento ni debe sustituir las filas crudas.
- Paginar visualmente si es necesario, sin cambiar la semántica del modo
  crudo ni el conjunto exportable.
- Descargar el archivo generado mediante el mecanismo de descarga de Blazor
  y un recurso JS local mínimo si resulta necesario. La UI no genera el PDF.
- Al cambiar controles sin consultar, conservar el resultado y exportar sus
  filtros originales. Deshabilitar exportación mientras no exista resultado.
- Reutilizar navegación y estilos, verificando escritorio y móvil. Coordinar
  cambios de menú y `Program.cs` con el trabajo paralelo de Tareas.

## Módulo Parques y comunicación local

- **Dominio:** conservar sus entidades y reglas sin referencias a Mediciones.
- **Aplicación:** ofrecer únicamente las consultas necesarias del catálogo
  mediante un contrato local de lectura, sin exponer entidades internas.
- **Infraestructura:** resolver esas consultas usando los repositorios del
  módulo, sin acceso desde Mediciones al contexto de Parques.
- **Presentación:** los selectores del histórico consumen el caso de uso de
  Mediciones; no coordinan persistencia de ambos módulos.

El contrato actual `ICatalogoProduccion` está orientado a ADMIN y al catálogo
actual de nodos con paneles, e incluye una operación de escritura. No reutilizar
sus restricciones automáticamente para histórico. Definir una proyección local
de lectura solo si el catálogo necesario no queda cubierto por contratos
existentes. Un panel deshabilitado puede conservar mediciones consultables.

Las filas históricas conservan identificadores aunque cambie el catálogo o se
elimine un panel. No borrarlas ni ocultarlas silenciosamente al resolver nombres.
Definir en el cierre del catálogo cómo seleccionar esos identificadores y
presentar los que ya no tengan nombre vigente.

## Usuarios, Tareas y Sensor

Usuarios aporta la identidad mediante los mecanismos existentes de Keycloak.
La política de consulta y exportación debe ser coherente en UI y Aplicación;
no se agregan roles o credenciales por iniciativa de esta funcionalidad.
No se requieren cambios de Dominio, Infraestructura ni páginas de Usuarios.
Tareas y Sensor permanecen fuera de la implementación del histórico.

Las simulaciones simultáneas conservan la ambigüedad señalada en requirements:
el modelo actual no registra una sesión de origen en `Medicion`. No prometer
una separación por sesión ni deduplicación que sus datos no permiten.

## Verificación

Comprobar filtros y límites temporales con registros anteriores, interiores y
posteriores a cada rango; períodos vacíos y parciales; combinaciones inválidas;
los tres modos conforme a las reglas cerradas; exportación después de cambiar
controles o recibir datos nuevos; errores, cancelación y acceso no autorizado.
Verificar con MySQL la traducción y los resultados de las consultas agregadas.
Comparar filas y valores del PDF con la instantánea y revisar visualmente un
documento de varias páginas con texto largo y acentos.

Agregar pruebas xUnit únicamente para comportamiento de Dominio incorporado
o modificado. Ejecutar las pruebas existentes y revisar dependencias manualmente.
No crear pruebas unitarias de otras capas ni configurar ArchUnitNET por esta
planificación. Ejecutar controles estructurales si existen al implementar y
SonarQube según su guía actual, declarando bloqueos o resultados pendientes.

Comandos previstos al implementar:

```powershell
dotnet build SolarUy.sln
dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj
```

Las tareas y dependencias están en [tasks.md](tasks.md). No actualizar el README
como si esta funcionalidad estuviera implementada durante esta planificación.
