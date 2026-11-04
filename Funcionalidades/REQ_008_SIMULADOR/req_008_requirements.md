Crear la página `Mediciones.razor` en el proyecto **SolarUy**, utilizando **InteractiveServer**, accesible desde **Producción** del menú.

Integrar **Sensor/WebAssembly en la misma página como simulador sin interfaz propia**. Al entrar a Producción, debe inicializarse automáticamente, conectarse mediante **SignalR**, recibir la configuración de paneles y devolver las mediciones generadas.

**No utilizar iframe, abrir otra pestaña ni agregar una pantalla, ruta propia de Sensor, enlace o paso manual para iniciarlo.** La pantalla pertenece a SolarUy; la generación aleatoria se ejecuta en WebAssembly dentro del navegador.

### Arquitectura

Respetar el monolito modular y reutilizar las implementaciones existentes:

- La página utiliza servicios de aplicación del módulo **Mediciones**.
- Mediciones obtiene parques, nodos y paneles mediante **contratos locales internos del módulo de parques**. Estas consultas no utilizan SignalR.
- SignalR comunica el servidor con Sensor para enviar configuración, controlar la simulación, cambiar la habilitación de paneles y recibir mediciones.
- El Hub delega en servicios de aplicación, sin lógica de negocio ni acceso directo a repositorios o `DbContext`.
- Utilizar los repositorios correspondientes para consultar y guardar. Crear únicamente los contratos, DTO, servicios y repositorios que falten, dentro de sus módulos y capas.
- No replicar entidades entre módulos.
- La generación aleatoria se realiza exclusivamente en Sensor/WebAssembly.

Para arrancar Sensor, utilizar un componente mínimo sin contenido visual que inicialice y libere sus servicios. Alojar la pantalla InteractiveServer y el componente InteractiveWebAssembly como **raíces hermanas dentro de un contenedor con renderizado estático**, sin anidar WebAssembly dentro del árbol InteractiveServer.

Revisar la configuración de renderizado y enrutamiento existente para permitir esta composición sin alterar el comportamiento de las demás páginas. Los parámetros compartidos entre ambas raíces deben ser serializables; su comunicación operativa se realiza mediante SignalR.

Vincular cada instancia de Producción con su componente Sensor y dirigir las órdenes únicamente a su conexión. Inicializarlo una sola vez, evitar duplicaciones por renderizado y liberar conexión y ciclo al abandonar la página. No agregar coordinación de generadores entre usuarios o navegadores.

### Diseño

Reproducir fielmente las imágenes de `Docs`:

- `SolarUyPantallaMedicionesTotal`.
- `SolarUyPantallaMedicionIndividual`.

Respetar distribución, proporciones, tipografía, tamaños, espaciados, colores, bordes e iconos. Reutilizar el menú lateral y la barra superior existentes.

Mantener cabecera, selectores, botón **Conectar**, tarjeta verde de conexión y franja de potencia y generación actual.

En la vista individual:

- Indicadores superiores de **Potencia** (`PotenciaActual`) y **Generando ahora**, con rayo SVG y separador vertical.
- Gráfica a la izquierda, aproximadamente dos tercios del ancho.
- Información del panel a la derecha: Panel, Parque, Nodo, Orientación y Estado, con el botón de habilitación debajo.
- Tabla inferior **Panel seleccionado**, de una fila: Panel, Orientación, Potencia, Generando ahora y Estado.

Para todos los paneles, seguir su referencia y mostrar generación total, información del nodo y tabla individual de sus paneles.

Utilizar **Radzen.Blazor en SolarUy** para la gráfica y **SVG para los iconos**, sin otra biblioteca de iconos. Adaptar a móvil y verificar visualmente ambas vistas contra las referencias.

### Carga inicial y selección

Al entrar a Producción:

1. Cargar el catálogo mediante servicios y contratos locales del servidor.
2. Seleccionar automáticamente el primer parque, su primer nodo y **Todos los paneles**.
3. Inicializar automáticamente Sensor/WebAssembly en la misma página.
4. Cuando su conexión SignalR esté disponible, enviar los identificadores, `PotenciaActual` y el flag de deshabilitación de los paneles.
5. Comenzar la simulación sin pulsar **Conectar**.

Consultar únicamente parques con nodos que tengan paneles y únicamente esos nodos. Incluir los paneles deshabilitados para mostrar su estado. Ordenar parques y nodos por identificador ascendente.

Sin catálogo disponible, mostrar un mensaje y deshabilitar selectores y Conectar. Si falla la conexión automática de Sensor, indicar el error sin generar valores en el servidor.

**Los selectores dependientes deben actualizarse al cambiar su selección, sin esperar a pulsar Conectar:**

- Al cambiar de parque, actualizar inmediatamente sus nodos, seleccionar el primero disponible, cargar los paneles de ese nodo y restablecer el selector de panel a **Todos los paneles**.
- Al cambiar de nodo, actualizar inmediatamente sus paneles y restablecer el selector de panel a **Todos los paneles**.
- El selector de panel corresponde al nodo seleccionado en los controles, aunque ese nodo todavía esté pendiente de aplicar.

Mantener separadas la **selección pendiente de los controles** y la **selección activa de la simulación y los datos visibles**:

- Los cambios de parque o nodo quedan pendientes hasta pulsar **Conectar**. Actualizar los selectores no debe detener ni cambiar la simulación activa.
- Mientras haya otro parque o nodo pendiente, conservar los indicadores, gráfica, información y tabla de la selección activa, identificándola claramente.
- Elegir un panel del nodo pendiente no debe filtrar ni reemplazar los datos del nodo activo.
- Al confirmar otro parque o nodo, sustituir la configuración de Sensor, detener el ciclo anterior, activar la nueva selección, restablecer el panel a **Todos los paneles** y comenzar una nueva serie gráfica.
- Cuando el parque y nodo seleccionados coincidan con los activos, cambiar entre **Todos los paneles** y un panel individual actualiza inmediatamente la vista, **sin Conectar** ni reiniciar la simulación.
- Repetir Conectar con la misma configuración no reinicia ni duplica el ciclo.
- Si una operación falla, no presentarla como aplicada.

### Generación y guardado

Cada **5 segundos**, Sensor genera para todos los paneles habilitados del nodo activo un valor aleatorio entre **0 y su `PotenciaActual`**, en **W**, aunque se visualice uno solo.

Mantener un único ciclo por instancia del simulador y detenerlo al abandonar Producción. Si todos los paneles están deshabilitados, no generar lecturas e indicar esa situación.

Sensor envía las entregas por SignalR. El servidor valida asociaciones, valores y flags persistidos, guarda las mediciones y actualiza la página de SolarUy.

Crear o reutilizar la tabla `Mediciones`, con identificador, ParqueId, NodoId, PanelId, valor en W y fecha y hora. Guardar una nueva fila por panel habilitado de cada entrega aceptada, conservando el historial.

No guardar ni sumar lecturas tardías de paneles deshabilitados. Cambiar el panel visible no modifica el conjunto generado y guardado.

Mantener la persistencia bajo responsabilidad de Mediciones. No agregar restricciones de eliminación de paneles ni borrar su historial.

### Habilitar y deshabilitar paneles

Mostrar un único botón según el estado:

- Habilitado: **Deshabilitar panel**.
- Deshabilitado: **Habilitar panel**.

Persistir el flag mediante el módulo de parques y su contrato local. Después, comunicar por SignalR el identificador y el nuevo estado a Sensor y actualizar la pantalla.

Reutilizar el flag existente si corresponde o agregarlo, manteniéndolo independiente del estado técnico del panel. El estado persistido debe conservarse al cambiar de selección, recargar y reconectar.

Al deshabilitar:

- Dejar de generar para ese panel.
- Mostrarlo como Deshabilitado y con generación actual de **0 W**.
- Excluirlo de la suma actual, conservándolo visible en catálogo, ficha y tabla.

Al habilitar:

- Incluirlo desde el siguiente ciclo, sin reiniciar el temporizador ni pulsar Conectar.
- Volver a guardar y sumar sus nuevas mediciones.
- No mostrar una lectura antigua como generación actual mientras se espera la nueva.
- Reanudar la generación si todos estaban deshabilitados.

Conservar las mediciones y muestras históricas sin modificarlas retroactivamente.

Si falla el guardado del flag, conservar el estado anterior. Si se guarda pero falla la comunicación con Sensor, informar sincronización pendiente y enviar el estado vigente al reconectar. Evitar que órdenes antiguas reviertan cambios posteriores.

### Gráfica y carga

La gráfica muestra las muestras recibidas durante la sesión: suma por entrega en Todos los paneles o valores del panel seleccionado. Usar línea amarilla, relleno suave, cuadrícula, tiempo horizontal y W vertical.

Conservar las muestras de todos los paneles para cambiar de vista inmediatamente. Limpiar la serie al confirmar otro parque o nodo. No utilizar curvas ficticias ni implementar consultas históricas o reportes.

Mostrar spinner durante carga inicial, consultas con espera, Conectar y habilitación/deshabilitación. Si actualizar los selectores dependientes requiere una consulta, mostrar el indicador durante esa espera y actualizar sus opciones al finalizar, sin exigir Conectar.

Deshabilitar temporalmente los controles involucrados y restaurarlos ante éxito o error.

Cambiar de panel y recibir mediciones no activa spinner. Distinguir conexión de Sensor y resultado del guardado; no anunciar datos como guardados antes de confirmarlo.

### Acceso y límites

Reutilizar la autenticación y autorización existentes para administradores y los mecanismos de acceso de Sensor, sin crear roles, credenciales ni rediseñar esa funcionalidad.

Documentar sus posibles errores en el requerimiento responsable. Si bloquean Mediciones, informar antes de modificar autenticación o autorización.

Verificar el flujo completo, la inicialización automática de Sensor en la misma página **sin iframe ni pasos manuales**, la fidelidad visual, la actualización inmediata de selectores dependientes, la separación entre selección pendiente y activa, los ciclos, la persistencia, la habilitación/deshabilitación, la reconexión y la conservación del historial.