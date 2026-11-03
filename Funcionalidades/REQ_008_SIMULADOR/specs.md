# REQ_008 — Especificaciones técnicas revisadas

## Alcance y estado

Diseño para la versión actual de [req_008_requirements.md](req_008_requirements.md). Esta revisión planifica las adaptaciones sobre la implementación existente; no ejecuta tareas, modifica código ni instala dependencias. SolarUy mantiene Interactive Server y Sensor utiliza InteractiveWebAssembly.

El [informe de implementación anterior](implementacion_y_verificacion_req_008.md) conserva sus resultados y pendientes como evidencia de aquella versión. Sus capturas y pruebas no acreditan los nuevos criterios visuales, la gráfica ni el cambio inmediato de panel.

## Base existente y diferencias

| Área | Estado observado | Adaptación requerida |
| --- | --- | --- |
| Simulación | SimulacionMedicionesService genera todos los paneles cada 10 segundos en Sensor. | Conservar ciclo y agregar almacenamiento de muestras de la sesión. |
| Transporte y persistencia | MedicionesCliente, MedicionesHub, MedicionesService, catálogo local, repositorio, contexto y migración implementados. | Reutilizar; la gráfica no necesita consultas históricas ni cambios de esquema. |
| Selección | Panel pendiente se aplica con Conectar, junto con parque/nodo. | Solo parque/nodo quedan pendientes; panel cambia inmediatamente sobre el nodo activo. |
| Diseño | Tarjetas apiladas, sin gráfica y rayo como carácter. | Reproducir composición de las referencias, gráfica Radzen e iconos SVG. |
| Dependencias | Sensor tiene cliente SignalR; no Radzen.Blazor. | Agregar y configurar Radzen únicamente al implementar. |
| Autorización | Página y Hub exigen ADMIN; casos de uso reciben acceso verificado en el servidor. | Reutilizar y completar matriz de acceso pendiente. |

## Módulo Parques

### Domain

Conservar entidades e invariantes existentes. PotenciaActual es el máximo de la simulación y el dato mostrado bajo la etiqueta Potencia, según el requisito. No agregar capacidad nominal ni nuevas reglas por estado del panel.

### Application

Reutilizar CatalogoProduccionService e ICatalogoProduccion, sus DTO y los repositorios existentes. El catálogo devuelve únicamente parques con nodos que tengan paneles, ordenados por identificador ascendente. Mantener Id, nombre, ubicación, orientación, estado y PotenciaActual necesarios para la pantalla.

Mediciones solicita y revalida relaciones por el contrato local. No consultar tablas ni entidades internas de Parques desde otro módulo. La fachada existente evita depender de la identidad del circuito Blazor sin modificar la autenticación global.

### Infrastructure

Mantener repositorios y configuración de Parques. La revisión visual no modifica tablas, permisos de consultas generales ni reglas de eliminación.

### Presentation e interfaz local

Conservar la fachada local del módulo en sus carpetas de Interfaz. No agregar eventos para consultas ya resueltas por el contrato existente ni nuevas pantallas de Parques.

## Módulo Mediciones del servidor

### Domain

Reutilizar Medicion con Id, ParqueId, NodoId, PanelId, ValorGenerado y FechaHora, e IMedicionRepository. Conservar invariantes de identificadores positivos, generación finita no negativa y fecha informada, así como las pruebas xUnit existentes. No agregar entidades persistentes para puntos de la gráfica ni dependencias de Radzen.

### Application

Conservar MedicionesService para catálogo, validación de selección y registro atómico de entregas. Verificar acceso ADMIN, pertenencia de las asociaciones, un valor por cada panel sin duplicados, valores entre cero y PotenciaActual y fecha válida.

El cambio local de panel no llama a ValidarSeleccion ni RegistrarEntrega por sí mismo. Conectar valida parque/nodo antes de activarlos. Las entregas periódicas siguen incluyendo todos los paneles del nodo activo, independientemente del panel visible o del contenido de la gráfica.

### Infrastructure

Reutilizar MedicionRepository, MedicionesDbContext y migración existentes. Mantener MySQL, contexto por operación y fecha UTC. No crear migraciones para esta revisión de UI. La tabla conserva asociaciones históricas por identificadores sin restricciones nuevas de eliminación ni borrados en cascada.

### Presentation: SignalR

Reutilizar MedicionesHub en /hubs/mediciones, autorizado para ADMIN, y sus operaciones ObtenerCatalogo, ValidarSeleccion y RegistrarEntrega. El Hub convierte mensajes y delega a Aplicación; no genera valores ni accede a DbContext o repositorios.

Los DTO de transporte permanecen en Sensor/Contratos/Mediciones y solo el adaptador del servidor conoce esos tipos. Aplicación y Dominio conservan sus propios modelos. No enlazar fuentes del backend ni crear referencias inversas de Sensor hacia SolarUy.

## Sensor: servicios y estado de sesión

### Domain y reglas de generación

Sensor no replica entidades del dominio del servidor. Mantener generación y suma en SimulacionMedicionesService, fuera del componente: un valor aleatorio entre cero y PotenciaActual para cada panel del nodo, en W, cada 10 segundos. Potencia cero produce cero; no excluir paneles por Estado.

### Application y coordinación local

Separar tres estados:

- Selección pendiente: ParqueId y NodoId de los dos primeros selectores.
- Selección activa: parque/nodo confirmados y catálogo de sus paneles.
- Panel visible: identificador opcional perteneciente exclusivamente al nodo activo; nulo significa Todos los paneles.

El selector de panel se alimenta del nodo activo, incluso cuando parque/nodo tienen cambios pendientes. Mostrar un aviso de que esos cambios esperan Conectar y que el panel sigue correspondiendo al nodo activo. Preparar los paneles del nodo pendiente sin reemplazar los activos.

Flujo inicial: conectar, obtener catálogo, elegir primer parque y nodo elegibles y confirmar automáticamente con vista Todos los paneles. Catálogo vacío: mensaje explicativo y controles deshabilitados. Antes de la primera entrega: Esperando mediciones, sin inventar valores ni una curva.

Cambiar parque actualiza sus nodos y elige el primero en la selección pendiente. Cambiar nodo prepara sus datos pendientes. Ninguno cambia ciclo, gráfico ni datos visibles. Conectar captura parque/nodo pendientes y valida en el servidor. Si confirma otro parque/nodo, detener y esperar el ciclo anterior, activar el nuevo, restablecer Todos los paneles y limpiar las muestras anteriores. Si falla, conservar el estado activo y su historial de sesión.

Conectar sobre el mismo parque/nodo no reinicia el ciclo ni limpia muestras. El reinicio de vista a Todos los paneles corresponde a un cambio efectivo de parque/nodo, no a una pulsación repetida sin cambio.

Cambiar panel visible es una operación local inmediata: actualizar indicadores, serie, ficha y tabla, sin Conectar, spinner, consulta al servidor, nuevo envío ni reinicio del temporizador. No mantener un panel pendiente separado.

### Muestras y proyección para la gráfica

Ampliar el servicio de simulación con un historial en memoria asociado al parque/nodo activos y a una versión de sesión. Cada entrega agrega una fecha, los valores de todos los paneles y su total. Conservar muestras de todos los paneles aunque solo uno sea visible; de ese modo cambiar de panel muestra inmediatamente su serie de la misma sesión.

Exponer una proyección de puntos de gráfica con FechaHora y ValorGenerado, independiente de Radzen: total por entrega para Todos los paneles o valor del panel elegido. La UI enlaza esa proyección, sin recalcular reglas de producción. Conservar todas las muestras de la sesión activa; no introducir truncamiento o ventana temporal que el requisito no establece. Liberar la colección al cambiar parque/nodo o abandonar la página.

Registrar la muestra al generarse y distinguir estado de guardado. Un fallo de persistencia no transforma la curva en ficticia ni descarta una muestra generada localmente. No reconstruir la serie desde MySQL ni agregar consultas históricas, reportes o sincronización entre pestañas.

### Infrastructure: transporte y dependencias

Reutilizar MedicionesCliente y su conexión por página. Mantener generaciones y envíos sin spinner; no reintentar automáticamente escrituras de resultado incierto ni añadir una cola offline. La reconexión revalida el nodo activo, conserva parque/nodo pendientes y panel visible si sigue siendo válido, y no crea un segundo ciclo. Evitar callbacks de la sesión anterior al cambiar nodo.

Al implementar, agregar una versión estable compatible de Radzen.Blazor a Sensor y fijarla en el proyecto tras comprobar compatibilidad con .NET 10. Incorporar imports, recursos JavaScript y estilos necesarios según la guía oficial para la versión elegida. SolarUy hospeda los recursos de Sensor: configurar sus referencias en el documento host cuando sea necesario, sin duplicar la carga ni aplicar un tema que cambie globalmente menú, barra o controles actuales.

Utilizar Radzen solo para la gráfica requerida; no reemplazar los demás componentes por controles de esa biblioteca ni agregar una biblioteca de iconos.

## Sensor: Presentation

### Composición y referencias

Conservar Sensor/Pages/Mediciones.razor, su ruta /sensor/mediciones, InteractiveWebAssembly sin prerender y el MainLayout del servidor. Separar componentes visuales si facilita aislar gráfica y ficha, sin mover generación o persistencia al marcado/code-behind.

Referencias obligatorias: [todos los paneles](../../Docs/SolarUyPantallaMedicionesTotal.png) y [panel individual](../../Docs/SolarUyPantallaMedicionIndividual.png). Respetar su distribución, proporciones, tipografía, tamaños, espacios, bordes y colores. No considerar suficiente una paleta parecida. Mantener menú y barra superior actuales.

| Bloque | Composición requerida |
| --- | --- |
| Cabecera | Título, descripción e indicador de estado como la referencia. Identificar que los datos son simulados sin sustituir el estado de conexión. |
| Selectores | Parque, nodo, panel, Conectar y tarjeta verde de conexión en una fila de escritorio con tamaños y separación de referencia. |
| Franja destacada | Rayo SVG a la izquierda; Potencia y PotenciaActual; separador vertical; Generando ahora y última medición a la derecha. En total, sumar valores correspondientes del nodo. |
| Fila central | Gráfica a la izquierda, aproximadamente dos tercios del ancho, e información a la derecha, aproximadamente un tercio. |
| Ficha individual | Panel, Parque, Nodo, Orientación y Estado, y botón Deshabilitar panel debajo. |
| Ficha total | Información del nodo: Parque, Nodo, Ubicación, cantidad de Paneles y Operativos según el estado del catálogo; no redefinir el estado de negocio. |
| Tabla inferior | Tarjeta a todo el ancho. Individual: Panel seleccionado con una fila; columnas Panel, Orientación, Potencia, Generando ahora y Estado. Total: Paneles del nodo, con filas de todos los paneles y composición de referencia. |

Si se conserva Ver detalle en la tabla total, su acción es seleccionar inmediatamente ese panel en la misma página; no agrega rutas ni casos de uso. El conteo visual de Operativos debe usar el estado existente que corresponda; revisar el enum y su significado al implementar, sin inventar umbrales o afectar generación.

Usar SVG para rayo y demás iconos añadidos o corregidos de esta pantalla. Reutilizar recursos existentes o agregar SVG locales adecuados; no utilizar caracteres como ϟ, ● u ○ como iconos. Iconos decorativos con aria-hidden; estados y controles con texto accesible. No introducir otra biblioteca de iconos ni cambiar globalmente iconos de otras páginas.

### Gráfica Radzen

Implementar RadzenChart con RadzenAreaSeries, enlazada a puntos reales de la sesión. Línea amarilla, relleno amarillo suave, cuadrícula, eje horizontal temporal y eje vertical con valores en W. Ajustar tipografía, altura, ejes, leyenda y márgenes al bloque de referencia. Usar fecha real de la muestra; formatear horario para presentación sin cambiar la fecha persistida.

Actualizar Data/proyección y refrescar el gráfico con el mecanismo soportado por la versión seleccionada, una vez por entrega o cambio de panel. Con cero muestras mostrar estado vacío; con una o dos, representar las muestras disponibles sin completar una curva artificial. No reproducir cifras, horarios ni la forma ilustrativa de las imágenes como datos reales.

Documentación primaria de referencia: [RadzenChart](https://blazor.radzen.com/docs/api/Radzen.Blazor.RadzenChart), [RadzenAreaSeries](https://blazor.radzen.com/docs/api/Radzen.Blazor.RadzenAreaSeries-1) y [ejemplo de área](https://blazor.radzen.com/area-chart). Confirmar recursos y APIs de actualización para la versión instalada durante la implementación.

### Espera, errores y pantallas pequeñas

Spinner solo en conexión inicial, esperas reales de catálogo y confirmación de parque/nodo. Deshabilitar controles involucrados y restaurarlos en finally tanto en éxito como en error. Cambiar panel y recibir/enviar entregas no activa spinner. Etiquetar siempre el contexto activo de los datos.

En móviles, permitir envolver selectores y apilar gráfica/ficha manteniendo su orden y jerarquía. Evitar desbordes de texto y controles, permitir desplazamiento de tabla y asegurar que Radzen recalcule su tamaño. Comparar ambas vistas en escritorio y móvil contra sus referencias antes del cierre.

Conservar Deshabilitar panel identificado como Pendiente y sin efectos. No modifica estado, simulación ni guardado.

## Autenticación y autorización: límite de alcance

Reutilizar sesión y rol ADMIN existentes para página, Hub, consultas y registro, sin rediseñar autenticación, claims, roles o permisos de otros módulos. La visibilidad del menú no sustituye controles del servidor y de Aplicación.

Ante un incumplimiento de código, registrar bug vinculado al requerimiento responsable. Si el requisito es conceptualmente incorrecto o incompleto, documentar allí la corrección necesaria. Si bloquea esta funcionalidad, informar bloqueo y corrección antes de modificar autenticación/autorización y continuar solo trabajo independiente. T02 conserva expresamente este alcance.

## Verificación y documentación

Reutilizar las pruebas de Dominio y ejecutarlas junto con compilación. Si se modifica comportamiento de Dominio, actualizar/agregar sus pruebas xUnit; esta revisión no propone cambios de Dominio ni pruebas unitarias nuevas de otras capas. No instalar herramientas de calidad no acordadas.

Desde la raíz, al implementar: dotnet build SolarUy.sln y dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj. Ejecutar controles estructurales/SonarQube únicamente si están configurados y registrar sus resultados reales. No confundir pruebas de la versión anterior con validación de la revisión actual.

Validación manual: las dos vistas y sus capturas comparadas con las referencias; gráfico vacío y con varias muestras; suma por entrega; cambio inmediato de panel recuperando sus muestras; paneles activos mientras otro nodo queda pendiente; Conectar cambia nodo y limpia serie; Conectar repetido conserva cadencia; entregas completas sin duplicados; spinners; reconexión, disposición y acceso ADMIN/no ADMIN/anónimo.

Los cambios, resultados, diferencias visuales y posibles errores van en implementacion_y_verificacion_req_008.md o en un informe descriptivo de esta revisión, fuera del README. El README solo se actualiza tras implementar para describir comportamiento y comunicación vigentes, incluida la selección inmediata de panel y la gráfica. Esta ejecución de la skill solo modifica specs.md y tasks.md.

## Registro de implementación de la revisión

Los cambios de código, comparaciones y resultados se registran en [revision_visual_y_grafica_req_008.md](revision_visual_y_grafica_req_008.md). La integración utiliza Radzen.Blazor 12.1.10 y conserva los componentes actuales fuera de la gráfica. Consultar tasks.md para distinguir implementación realizada de comprobaciones manuales aún pendientes.

