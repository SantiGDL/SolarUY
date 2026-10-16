# Se crearon las funcionalidades tomando en cuenta los siguientes requisitos de la letra:

4\. Principales Requisitos Funcionales

La siguiente es una lista de requerimientos a ser tomados en cuenta. Es un punto de partida para orientar la solución final.

El sistema deberá contemplar, como punto de partida, las siguientes funcionalidades:

- Gestión multiparque:

    - Capacidad para crear y administrar diferentes parques solares con configuraciones independientes.

    - Configurar nodos: Cada parque solar cuenta con 1 o más nodos. Un nodo es un conjunto de paneles fotovoltaicos. Los nodos tienen características que lo diferencian, por ejemplo su ubicación dentro del parque, la cantidad de paneles, etc. Cada nodo cuenta con un Inversor que convierte corriente continua (CC) en corriente alterna (CA).

    - Administrar paneles: interesa conocer los paneles con los que se cuenta, las características del mismo, en qué nodo está instalado.

    - Solo usuarios con rol de administrador podrán realizar estas operaciones.

- Gestionar las tareas de mantenimiento de los paneles. Éstos requieren un mantenimiento contínuo, por ejemplo limpieza, que tiene que ser realizado por personal especializado. Gestionar estas tareas es importante para el ininterrumpido funcionamiento del parque.

- Monitoreo en tiempo real de generación de corriente y estado de los paneles. Se quiere saber en todo momento cuánta energía está generando el parque, cada nodo y cada panel conectado. Además interesa chequear el estado de salud de cada panel para reaccionar rápidamente ante la existencia de un fallo.

- Obtener información histórica detallada del funcionamiento del parque: por ejemplo, generación de energía en un día, mes, año determinado. La información a registrar debe contemplar futuros requerimientos o necesidades de información.

Casos de Uso

- Gestión multiparque:

    - Capacidad para crear y administrar diferentes parques solares con configuraciones independientes.

    - Configurar nodos: Cada parque solar cuenta con 1 o más nodos. Un nodo es un conjunto de paneles fotovoltaicos. Los nodos tienen características que lo diferencian, por ejemplo su ubicación dentro del parque, la cantidad de paneles, etc. Cada nodo cuenta con un Inversor que convierte corriente continua (CC) en corriente alterna (CA).

    - Administrar paneles: interesa conocer los paneles con los que se cuenta, las características del mismo, en qué nodo está instalado.

    - Solo usuarios con rol de administrador podrán realizar estas operaciones.

### 6. Requerimientos no funcionales

**Alta disponibilidad.** Se desea tener alta disponibilidad de la aplicación de gestión. No es tolerable que el sistema no esté disponible por ejemplo al momento de instalar una nueva versión de la aplicación.

Sistema de **autenticación/autorización** externo que cumpla con el estándar **OpenID**.

Posibilidad de interactuar con el sistema con **lenguaje natural** solo para las funcionalidades de <u>consulta de información</u>. De esta manera si un usuario quiere conocer el estado del sistema lo puede hacer con una consulta del estilo: “¿cómo está funcionando todo hoy?”

# Primer boceto de funcionalidades de SolarUY

Propuesta inicial para discutir qué se quiere que haga cada caso de uso. Las operaciones mencionadas son ejemplos y pueden ajustarse durante el diseño.

## 1. Parque solar

Se pueden crear y administrar varios parques con configuraciones independientes. Cada parque cuenta con uno o más nodos. La administración de parques, nodos y paneles corresponde al administrador.

### Crear parque

- **Actor:** administrador.

- **Objetivo:** registrar un parque que luego se pueda configurar y consultar.

- **Operaciones propuestas:** CrearParque(datosParque) y CrearParqueConNodos(datosParque, datosNodos).

- **Resultado esperado:** el parque queda identificado y su configuración puede administrarse independientemente de los demás.

### Consultar parque

- **Objetivo:** conocer los parques registrados y consultar su configuración y sus nodos.

- **Operaciones propuestas:** ListarParques() y ConsultarParque(parqueId).

- **Resultado esperado:** ListarParques muestra todos los parques; ConsultarParque muestra el detalle de uno solo.

### Modificar parque

- **Actor:** administrador.

- **Objetivo:** actualizar la configuración de un parque.

- **Operación propuesta:** ModificarParque(parqueId, datosParque).

- **Resultado esperado:** se actualizan los datos del parque seleccionado.

### Agregar o retirar nodos

- **Actor:** administrador.

- **Objetivo:** administrar los nodos que componen un parque.

- **Operaciones propuestas:** AgregarNodoAParque(parqueId, nodoId) y RetirarNodoDeParque(parqueId, nodoId).

- **Resultado esperado:** se actualiza la composición del parque.

## 2. Nodo

Un nodo agrupa paneles dentro de un parque, tiene una ubicación y cuenta con un inversor propio.

### Crear nodo

- **Actor:** administrador.

- **Objetivo:** registrar un nodo dentro de un parque.

- **Operación propuesta:** CrearNodoEnParque(parqueId, datosNodo).

- **Resultado esperado:** el nodo queda registrado y asociado al parque.

Observacion: Al crear un Nodo se crea el Inversor también.

Algo así: CrearNodo(Inversor inversorAsociado, datosNodo)

### Consultar nodo

- **Objetivo:** conocer los nodos de un parque y consultar sus características.

- **Operaciones propuestas:** ListarNodosDeParque(parqueId) y ConsultarNodo(nodoId).

- **Resultado esperado:** se muestra la lista de nodos o el detalle de uno, incluyendo ubicación, paneles e inversor.

### Modificar nodo

- **Actor:** administrador.

- **Objetivo:** actualizar la configuración de un nodo.

- **Operación propuesta:** ModificarNodo(nodoId, datosNodo).

- **Resultado esperado:** se actualizan los datos del nodo seleccionado.

### Agregar o retirar paneles

- **Actor:** administrador.

- **Objetivo:** administrar los paneles que componen un nodo.

- **Operaciones propuestas:** AgregarPanelANodo(nodoId, panelId) y RetirarPanelDeNodo(nodoId, panelId).

- **Resultado esperado:** se actualiza la composición del nodo sin borrar los paneles registrados.

## 3. Panel Solar

Se registran las características de cada panel y el nodo donde está instalado. El sistema permite consultar su generación y su estado de salud.

### Crear panel

- **Actor:** administrador.

- **Objetivo:** registrar un panel para poder consultarlo y asociarlo a un nodo.

- **Operación propuesta:** RegistrarPanel(datosPanel).

- **Resultado esperado:** el panel queda identificado dentro del sistema.

### Consultar panel

- **Objetivo:** conocer los paneles registrados, sus características y su ubicación.

- **Operaciones propuestas:** ListarPaneles(filtros) y ConsultarPanel(panelId).

- **Resultado esperado:** se muestra una lista de paneles o el detalle de uno solo.

### Modificar panel

- **Actor:** administrador.

- **Objetivo:** actualizar las características registradas de un panel.

- **Operación propuesta:** ModificarPanel(panelId, datosPanel).

- **Resultado esperado:** se actualizan los datos del panel seleccionado.

## 4. Inversor

Cada nodo tiene un inversor que concentra la información de sus paneles.

El inversor se crea y asocia automáticamente al crear un nuevo Nodo.

### Vincular Paneles

- **Actor:** administrador.

- **Objetivo:** vincular el inverosr con los paneles del Nodo al que pertenece.

- **Operación propuesta:** VincularPanel(Panel) y VincularPaneles(List&lt;Panel&gt;) para vincular varios.

- **Resultado esperado:** el sistema identifica qué paneles pertenecen al qué inversor.

### Capturar mediciones

- **Actor:** sistema y simulador.

- **Objetivo:** obtener y registrar información de generación y salud de los paneles.

- **Operaciones propuestas:** ObtenerLecturasDeNodo(nodoId) y RegistrarMediciones(lecturas).

- **Resultado esperado:** las mediciones quedan disponibles para el monitoreo y las consultas históricas.

### Capturar medición 1 Panel

- **Actor:** sistema y simulador.

- **Objetivo:** obtener y registrar información de generación y salud de 1 panel específico.

- **Operaciones propuestas:** ObtenerLecturasDeNodo(nodoId) y RegistrarMediciones(lecturas).

- **Resultado esperado:** las mediciones quedan disponibles para el monitoreo y las consultas históricas.

### Enviar órdenes al inversor

- **Actor:** usuario autorizado, por definir.

- **Objetivo:** solicitar la activación o desactivación de paneles y habilitar o deshabilitar el volcado a la red.

- **Operaciones propuestas:** SolicitarActivacionPanel(panelId), SolicitarDesactivacionPanel(panelId) y SolicitarVolcadoARed(nodoId, habilitado).

- **Resultado esperado:** el simulador recibe la orden e informa su resultado.

- **Alcance:** inclusión por confirmar con el equipo.

## 5. Tareas de mantenimiento

El sistema permite organizar el mantenimiento de los paneles y registrar el trabajo realizado por personal especializado.

### Crear tarea de mantenimiento

- **Actor:** usuario autorizado para planificar mantenimiento.

- **Objetivo:** registrar un trabajo que debe realizarse sobre uno o varios paneles.

- **Operación propuesta:** CrearTareaMantenimiento(datosTarea).

- **Resultado esperado:** la tarea queda registrada para su seguimiento.

### Asignar responsable

- **Actor:** usuario autorizado para asignar mantenimiento.

- **Objetivo:** indicar quién realizará la tarea.

- **Operación propuesta:** AsignarResponsable(tareaId, responsableId).

- **Resultado esperado:** la tarea queda vinculada al personal responsable.

### Consultar tareas

- **Objetivo:** conocer los mantenimientos registrados y su situación.

- **Operaciones propuestas:** ListarTareas(filtros) y ConsultarTarea(tareaId).

- **Resultado esperado:** se muestra una lista de tareas o el detalle de una sola.

### Registrar ejecución de mantenimiento

- **Actor:** personal especializado autorizado.

- **Objetivo:** registrar el inicio y la finalización del trabajo.

- **Operaciones propuestas:** IniciarTarea(tareaId) y CompletarTarea(tareaId, resultado).

- **Resultado esperado:** se actualiza el estado de la tarea y queda registrado el trabajo realizado.

## 6. Monitoreo en tiempo real

El sistema muestra la generación de parques, nodos y paneles, junto con la salud de los paneles y posibles pérdidas de comunicación.

### Consultar monitoreo

- **Actor:** usuario con permiso de consulta.

- **Objetivo:** conocer el funcionamiento actual de un parque y sus componentes.

- **Operaciones propuestas:** ConsultarMonitoreoParque(parqueId), ConsultarMonitoreoNodo(nodoId) y ConsultarMonitoreoPanel(panelId).

- **Resultado esperado:** se muestran las últimas mediciones y se actualizan al recibir nuevos datos.

### Mostrar fallos y desconexiones

- **Actor:** sistema.

- **Objetivo:** señalar paneles con fallos o componentes sin información reciente.

- **Resultado esperado:** el usuario puede distinguir el funcionamiento normal, los fallos y la ausencia de datos.

## 7. Información histórica

El sistema conserva mediciones para consultar la generación registrada de un parque por día, mes o año.

### Consultar generación histórica

- **Actor:** usuario con permiso de consulta.

- **Objetivo:** conocer la generación de un parque durante un período.

- **Operación propuesta:** ConsultarGeneracionHistorica(parqueId, periodo, agrupacion).

- **Resultado esperado:** se muestra la generación correspondiente al período seleccionado, indicando si faltan datos.

## 8. Acceso y permisos

El acceso se realiza mediante un proveedor externo compatible con OpenID Connect. Cada usuario puede realizar las operaciones permitidas por su rol.

### Iniciar y cerrar sesión

- **Actor:** usuario.

- **Objetivo:** acceder al sistema mediante el proveedor externo y finalizar la sesión.

- **Resultado esperado:** el usuario queda identificado y puede acceder a las funcionalidades autorizadas.

### Controlar permisos

- **Actor:** sistema.

- **Objetivo:** comprobar que el usuario tiene permiso para realizar cada operación.

- **Resultado esperado:** la administración de parques, nodos y paneles queda restringida a administradores; los demás permisos se definirán con el equipo.

## 9. Consultas en lenguaje natural

El usuario puede realizar preguntas sobre el funcionamiento y la generación del sistema, como «¿cómo está funcionando todo hoy?».

### Consultar mediante una pregunta

- **Actor:** usuario con permiso de consulta.

- **Objetivo:** obtener información de monitoreo e histórico mediante lenguaje natural.

- **Operación propuesta:** ConsultarEnLenguajeNatural(pregunta).

- **Resultado esperado:** se responde con información disponible y autorizada, sin modificar datos ni ejecutar órdenes.
