Crear la página `Mediciones.razor` dentro de `Pages` del proyecto `Sensor`, ejecutada con `InteractiveWebAssembly`. Se accederá desde la opción **Producción** del menú de SolarUy.

### Diseño

Basar el diseño en las imágenes de referencia guardadas en `Docs`:

- `SolarUyPantallaMedicionesTotal`: vista de todos los paneles.
- `SolarUyPantallaMedicionIndividual`: vista de un panel individual.

Reutilizar el menú lateral y la barra superior existentes de SolarUy, respetando los estilos y componentes actuales del proyecto.

**Respetar fielmente el diseño proporcionado**, incluyendo la distribución de los bloques, proporciones, tipografía, tamaños de texto y controles, espaciados, colores, bordes, tarjetas e iconografía. No limitarse a utilizar colores similares ni reemplazar la composición de las referencias por otra distribución.

Reproducir la cabecera con título, descripción e indicador de estado; la fila de selectores con el botón **Conectar** y la tarjeta verde de conexión; y la franja destacada de potencia y generación actual.

En la vista individual, respetar esta composición:

- Franja horizontal de indicadores: icono de rayo, etiqueta **Potencia** y `PotenciaActual` del panel a la izquierda; **Generando ahora** y su medición actual a la derecha, separados por una línea vertical.
- Fila central con la gráfica **Generación en tiempo real** a la izquierda, ocupando aproximadamente dos tercios del ancho, y la tarjeta **Información del panel** a la derecha, ocupando aproximadamente un tercio.
- La tarjeta de información debe mostrar **Panel**, **Parque**, **Nodo**, **Orientación** y **Estado**, con el botón **Deshabilitar panel** debajo.
- Tarjeta inferior **Panel seleccionado**, a todo el ancho, con una tabla de una fila y las columnas **Panel**, **Orientación**, **Potencia**, **Generando ahora** y **Estado**.

Para la vista de todos los paneles, respetar la distribución de su imagen de referencia.

**Instalar y configurar `Radzen.Blazor` en el proyecto `Sensor` para implementar la gráfica en WebAssembly**, utilizando `RadzenChart` con una serie de área. Configurar línea amarilla, relleno amarillo suave, cuadrícula, tiempo en el eje horizontal y valores en W en el vertical. Actualizarla con las muestras generadas cada 10 segundos. Conservar los componentes y estilos existentes de SolarUy para el resto de la página.

**Utilizar SVG para los iconos**, incluido el rayo de la franja de indicadores. Reutilizar los SVG existentes o agregar los necesarios para reproducir las referencias. No incorporar una biblioteca adicional de iconos ni sustituirlos por caracteres de texto.

Adaptar el diseño a pantallas pequeñas manteniendo su jerarquía visual. Comparar visualmente ambas vistas con las referencias antes de dar la implementación por terminada.

### Arquitectura y responsabilidades

Mantener la arquitectura del monolito modular y respetar la estructura existente:

- El proyecto `Sensor`, ejecutado en WebAssembly en el navegador, contiene la página y la lógica para generar los datos simulados.
- `Sensor` se comunica con el servidor mediante **SignalR** para solicitar parques, nodos y paneles y enviar las mediciones que deben guardarse.
- En el servidor, las consultas y el guardado de esta funcionalidad se coordinan desde el módulo `Mediciones`.
- El módulo `Mediciones` debe solicitar los datos de parques, nodos y paneles al módulo correspondiente de parques, mediante contratos locales internos del proyecto o eventos, según corresponda.
- Revisar y reutilizar los contratos y eventos existentes. Si es necesario crear otros, ubicarlos en las carpetas correspondientes dentro de los módulos.
- El Hub de SignalR debe delegar las consultas y operaciones a los servicios de aplicación correspondientes, sin acceder directamente al `DbContext` ni contener lógica de negocio.
- Reutilizar los repositorios existentes para las consultas y el guardado de mediciones. Si no existe el repositorio necesario, crearlo en el módulo y la capa correspondientes.
- Revisar y reutilizar las implementaciones existentes antes de crear otras nuevas.
- `Sensor` debe conocer los DTO necesarios para recibir y mostrar la información y enviar las mediciones. Resolver su ubicación siguiendo la estructura existente, sin replicar las entidades de los módulos ni establecer de antemano el enlace de archivos del backend desde `Sensor.csproj`.

**La generación de datos se realiza en Sensor. No implementar un generador ni un servicio hospedado de simulación en el servidor.**

### Autenticación y autorización existentes

El simulador está destinado a administradores autenticados. Reutilizar la autenticación y autorización existentes para el acceso a la página y al Hub de SignalR, incluyendo las consultas y el envío de mediciones para su guardado.

El rediseño o la corrección de esa funcionalidad pertenece a su propio requerimiento y queda fuera del alcance de Mediciones.

Si durante la integración se detectan problemas:

- Si el código no cumple el requisito existente, registrar un bug vinculado al requerimiento de autenticación y autorización.
- Si ese requisito contiene errores conceptuales o está incompleto, documentar la corrección necesaria en el requerimiento correspondiente.
- Si el problema bloquea Mediciones, informar el bloqueo y la corrección necesaria antes de modificar la funcionalidad de autenticación y autorización.

Reflejar este alcance en el spec y en las tareas de implementación, especialmente en T02 si se conserva esa identificación.

### Conexión y carga inicial

Al acceder a la página, establecer una conexión con el Hub de SignalR de SolarUy y solicitar los parques disponibles.

El servidor debe devolver únicamente parques que tengan al menos un nodo con paneles. Para cada parque, ofrecer únicamente los nodos que tengan paneles.

Ordenar los parques y nodos por su identificador de forma ascendente para determinar cuál se selecciona primero.

La información de los paneles recibida por `Sensor` debe incluir los datos necesarios para mostrarlos y su `PotenciaActual`, utilizada como máximo para la generación simulada.

Si no existe ningún parque que cumpla estas condiciones, mostrar un mensaje explicativo y deshabilitar los selectores y el botón **Conectar**.

En la carga inicial:

1. Seleccionar automáticamente el primer parque disponible.
2. Seleccionar su primer nodo disponible.
3. Seleccionar la opción **Todos los paneles**.
4. Aplicar automáticamente esa selección e iniciar la simulación en `Sensor`, sin necesidad de pulsar **Conectar**.

### Cambios de selección

Al cambiar de parque:

- Actualizar sus nodos.
- Seleccionar el primero disponible.
- Preparar los paneles de ese nodo para cuando se confirme el cambio.

Al cambiar de nodo:

- Preparar sus paneles para cuando se confirme el cambio.

Después de la carga inicial, **solo los cambios de parque o nodo requieren pulsar Conectar**.

Mantener separadas la **selección pendiente de parque y nodo** y la **selección activa** que corresponde a las mediciones visibles. Mientras no se pulse **Conectar**, la simulación y los datos visibles deben seguir correspondiendo al parque y nodo activos.

Al confirmar correctamente un cambio de parque o nodo con **Conectar**, activar la nueva selección y restablecer la vista a **Todos los paneles**. Si falla, conservar la selección y la simulación activas.

**Cambiar entre Todos los paneles y un panel individual actualiza inmediatamente los indicadores, la gráfica, la información y la tabla, sin pulsar Conectar.** Este cambio no debe reiniciar ni duplicar el ciclo de generación, ni modificar las mediciones que se generan, envían y guardan.

El selector de panel debe corresponder al nodo activo. Mientras exista un cambio pendiente de parque o nodo, conservar los paneles del nodo activo hasta confirmar con **Conectar**, indicando claramente esta situación.

Indicar claramente a qué parque, nodo y panel corresponden los datos visibles.

### Indicadores de carga

Mostrar un **spinner de carga** mientras se procesan las operaciones que requieren espera en el cliente, incluyendo:

- La conexión y carga inicial al ingresar a la página.
- La actualización de nodos y paneles cuando requiera esperar una respuesta.
- La aplicación del cambio de parque o nodo al pulsar **Conectar**.

Durante cada operación, deshabilitar los controles involucrados para evitar acciones repetidas. Al finalizar, tanto si la operación tiene éxito como si falla, ocultar el spinner y actualizar el estado de los controles. Si ocurre un error, mostrar un mensaje explicativo.

El cambio local del panel visible, las actualizaciones periódicas y el envío de mediciones deben realizarse sin activar el spinner ni interrumpir la visualización.

### Generación de mediciones en Sensor

La simulación debe ejecutarse en `Sensor`, dentro del navegador, únicamente mientras se está utilizando la página.

Cada **10 segundos**, generar un valor aleatorio para **cada panel del nodo activo**, entre **0 y la `PotenciaActual` de ese panel**, expresado en **W**.

Generar datos para todos los paneles del nodo activo aunque el usuario esté visualizando un solo panel.

Mantener un único ciclo de generación dentro de cada instancia de la página:

- Pulsar repetidamente **Conectar** con el mismo parque y nodo no debe reiniciar el ciclo ni crear temporizadores duplicados.
- Al confirmar con **Conectar** un cambio de parque o nodo, detener el ciclo anterior e iniciar el correspondiente a la nueva selección activa.
- Al cambiar entre todos los paneles y un panel individual, mantener el ciclo existente y su cadencia.
- Al abandonar la página, detener el ciclo y liberar los recursos utilizados.

Esta entrega no incluye coordinación de generadores entre distintos usuarios, pestañas o navegadores.

### Envío y guardado de mediciones

En cada entrega, `Sensor` debe enviar al servidor mediante **SignalR** las mediciones generadas para todos los paneles del nodo activo, con sus identificadores de parque, nodo y panel, valor generado y fecha y hora.

Crear una tabla `Mediciones` que registre cada medición individual y la asocie con su **parque, nodo y panel**, incluyendo:

- Identificador de la medición.
- Identificador del parque.
- Identificador del nodo.
- Identificador del panel.
- Valor generado en W.
- Fecha y hora de la medición.

Por cada entrega recibida, guardar una nueva medición por panel, conservando el historial.

Por ejemplo, si el nodo tiene los paneles 1, 2 y 3, `Sensor` genera y envía tres mediciones cada 10 segundos, y el servidor guarda una fila por cada una.

El Hub debe delegar la recepción y el guardado a los servicios de aplicación del módulo `Mediciones`, que utilizarán los repositorios correspondientes.

La selección de **Todos los paneles** o de un panel individual determina qué datos se muestran, pero no modifica qué mediciones se generan, envían y guardan.

Determinar la configuración del contexto y el mapeo de la tabla respetando la arquitectura existente y la responsabilidad del módulo `Mediciones`. No asumir de antemano que debe utilizarse `ParquesDbContext`.

No agregar restricciones para impedir la eliminación de paneles con historial ni modificar las reglas de eliminación existentes como parte de esta funcionalidad.

### Visualización de mediciones

Al seleccionar **Todos los paneles**, mostrar:

- La suma de la generación actual de los paneles del nodo activo, en W.
- La gráfica de generación total del nodo durante la sesión.
- Una tabla con los valores individuales de esos paneles, en W.
- Los demás bloques de información e indicadores presentes en su referencia de diseño.

Al seleccionar un panel individual, mostrar:

- La franja superior con **Potencia**, correspondiente a su `PotenciaActual`, y **Generando ahora**, correspondiente a su última medición.
- La gráfica de generación del panel durante la sesión.
- La tarjeta **Información del panel**, con Panel, Parque, Nodo, Orientación y Estado.
- La tabla **Panel seleccionado**, con una sola fila y las columnas Panel, Orientación, Potencia, Generando ahora y Estado.
- El botón **Deshabilitar panel**, como requerimiento extra pendiente y sin efectos.

La pantalla debe actualizarse con cada entrega generada por `Sensor` y mostrar los datos correspondientes a la selección activa. SignalR se utiliza para consultar la información al servidor y enviar las mediciones para su guardado.

### Gráfica en tiempo real

Implementar la gráfica con **Radzen.Blazor**, mediante `RadzenChart` y una serie de área, utilizando las muestras generadas durante la sesión actual:

- En **Todos los paneles**, representar la suma de generación de los paneles del nodo por cada entrega.
- En un panel individual, representar sus mediciones.

Mostrar el tiempo en el eje horizontal y la generación en W en el vertical. Respetar la apariencia de la referencia: línea amarilla, relleno amarillo suave y cuadrícula.

Los valores, horarios y la curva de la imagen son ilustrativos. No utilizar una curva ficticia fija ni datos de ejemplo como si fueran mediciones reales.

Conservar en memoria las muestras de todos los paneles del nodo activo para que el cambio de panel actualice la gráfica inmediatamente. Al confirmar otro parque o nodo, comenzar una nueva serie sin mezclar los datos anteriores.

La gráfica de la sesión no implica implementar consultas históricas a la base de datos ni reportes.

### Requerimiento extra: deshabilitar panel

Conservar el botón **Deshabilitar panel** mostrado en las referencias de diseño.

Registrar la funcionalidad de deshabilitar un panel como un **requerimiento extra pendiente**, sin implementar en esta entrega sus efectos sobre el estado del panel, la simulación o el guardado de mediciones. El botón debe quedar identificado como pendiente y sin ejecutar cambios.