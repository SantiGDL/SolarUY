# REQ_008 — Producción Interactive Server con Sensor integrado

## Alcance y estado

Revisión del 4 de noviembre de 2026 basada en [req_008_requirements.md](req_008_requirements.md). Producción pertenece a SolarUy con InteractiveServer; Sensor/WebAssembly es un motor sin interfaz propia, integrado e iniciado automáticamente en la misma página. El requerimiento incluye habilitación y deshabilitación persistentes, selectores dependientes que siguen la selección pendiente y generación cada **5 segundos**. Esta revisión solo genera diseño y planificación.

Las reglas vigentes están en .agents/Reglas, incluido 6_flujo_trabajo_ia.md. Se resuelven las referencias por sus rutas reales; la mención residual a 7_flujo_trabajo_ia.md en la skill remite a ese archivo renombrado. No se modifican reglas ni la skill.

Las capturas previas disponibles no prueban cumplimiento de esta arquitectura. Los informes anteriores eliminados por el equipo no se recrean. Esta aplicación de la skill solo actualiza specs.md y tasks.md, sin modificar código, configuración, datos ni README.

## Base revisada

El código actual ya contiene ProduccionHost estático, Mediciones InteractiveServer en SolarUy y MotorSensor WebAssembly no visual como raíces hermanas. La gráfica, SVG y referencia Radzen.Blazor 12.1.10 están en SolarUy; Sensor conserva generador, cliente SignalR y DTO de transporte. Existen MedicionesService, sesiones/canales, transporte dirigido, ICatalogoProduccion, repositorio y MedicionesDbContext. Panel ya tiene Deshabilitado y operaciones Habilitar/Deshabilitar, con migración y pruebas de Dominio.

Las diferencias concretas con el requerimiento actualizado son: SimulacionMedicionesService conserva PeriodicTimer de 10 segundos; Mediciones.razor obtiene las opciones del selector de panel desde NodoActivo y utiliza un único _panelId para la vista. CambiarParque/CambiarNodo no resuelve por separado los paneles de los controles pendientes. Adaptar estos puntos a cinco segundos y a dos selecciones independientes; no repetir la migración, reinstalar Radzen o reconstruir componentes que ya existen.

Se reutilizan esos elementos adaptando responsabilidades. No se presupone que las carpetas o resultados anteriores implementen las nuevas órdenes, sesiones y deshabilitación.

## Módulo Parques

### Domain

Reutilizar Panel.Deshabilitado, booleano inicialmente false, y los métodos Deshabilitar() y Habilitar(), ambos idempotentes. Mantener EstadoPanel independiente: Bueno, Intermedio y Rotura no equivalen automáticamente a deshabilitación. Actualizar otros datos del panel no cambia el flag: solo las operaciones explícitas de habilitación lo modifican.

Conservar las pruebas xUnit existentes y completar únicamente cobertura faltante de estado inicial, deshabilitación, habilitación posterior, repetición de ambas operaciones, secuencias alternadas y conservación del flag tras Actualizar en ambos estados. Conservar identidad, potencia, relaciones y estado técnico.

### Application y contratos locales

Reutilizar ICatalogoProduccion y sus DTO que ya incluyen Deshabilitado. Mantener paneles deshabilitados en catálogo y nodos/parques con paneles, ordenados por Id ascendente. Retornar identificadores, nombres, ubicación, orientación, Estado y PotenciaActual.

Exponer habilitación y deshabilitación mediante un contrato local en Parques/Interfaz/Apis/Local, delegado al servicio de Aplicación. Propuesta: CambiarHabilitacionAsync recibe el estado deseado explícito, no una orden de alternar; valida ADMIN, existencia y pertenencia, invoca Habilitar() o Deshabilitar() y guarda por el repositorio existente. Retornar el flag vigente solo tras confirmar persistencia. Repetir el mismo estado es idempotente. El servicio no envía órdenes a Sensor como efecto oculto de una actualización genérica.

### Infrastructure

Conservar el mapeo del flag y la migración existente HabilitacionPanelProduccion, con false para filas anteriores. Verificar consultas y actualizaciones del repositorio para conservarlo; estos cambios de selectores/cadencia no requieren otra migración. No agregar restricciones de eliminación ni borrar mediciones históricas.

### Presentation

No se requiere otra pantalla de Parques. Mediciones utiliza sus contratos locales; Producción muestra Deshabilitado como condición operativa, diferenciada del estado técnico.

## Módulo Mediciones

### Domain

Conservar Medicion e IMedicionRepository: Id, ParqueId, NodoId, PanelId, ValorGenerado en W y FechaHora UTC. Mantener sus invariantes y pruebas. No introducir entidades de Parques, SignalR o Radzen ni modificar registros históricos por deshabilitar un panel.

### Application: consultas y control

Producción obtiene catálogo mediante llamadas C# a MedicionesService, que consulta el contrato local de Parques. La página no utiliza el Hub para consultar catálogo ni contiene generación.

Coordinar sesión, configuración, recepción y publicación en servicios de Aplicación con responsabilidades explícitas. Definir contratos de transporte y de registro de sesiones, implementados fuera de Aplicación. Los casos de uso protegidos comprueban acceso ADMIN; el propietario se obtiene del principal autenticado mediante el adaptador de Presentation, no de un usuario enviado por Sensor.

Cada instancia de Producción crea una sesión efímera con SesionId, propietario estable, conexión Sensor vinculada, configuración deseada/activa, versión monotónica y estado. SesionId sirve para correlación, no concede acceso. Dos páginas tienen sesiones independientes; no se coordinan sus generadores.

El registro concurrente en memoria solo conserva identificadores, DTO, estados y canales. No conserva servicios scoped, DbContext, componentes ni callbacks que capturen páginas. Su implementación puede ser singleton; los servicios de operación mantienen tiempos de vida compatibles.

Configuración: SesionId, VersionConfiguracion, ParqueId, NodoId y paneles con Id, PotenciaActual, Deshabilitado y RevisionEstado. La versión de configuración identifica la selección de parque/nodo; la revisión por panel identifica sus cambios de habilitación. El panel visible nunca cambia esta configuración. Repetir una configuración no reinicia el ciclo ni borra muestras; cambiar el flag actualiza el panel dentro del ciclo existente.

### Inicialización automática mediante raíces hermanas

Producción conserva su interfaz Mediciones.razor en SolarUy con InteractiveServer. La composición de esa interfaz y MotorSensor se realiza en un contenedor de renderizado estático, con dos raíces hermanas: una InteractiveServer y otra InteractiveWebAssembly sin contenido visual. No usar iframe, documento aislado, ruta propia de Sensor, nueva pantalla, enlace, pestaña o paso manual.

Mantener la composición exigida por el requerimiento: raíces interactivas hermanas bajo un contenedor estático, con árboles independientes. No convertir el contenedor o su ancestro Routes a InteractiveServer, porque MotorSensor dejaría de ser una raíz hermana independiente.

Propuesta de composición siguiendo las carpetas existentes:

- SolarUy/Components/Pages/ProduccionHost.razor: contenedor estático que declara /admin/mediciones y exige ADMIN. No lleva un modo interactivo propio. Conserva MainLayout mediante el enrutamiento actual.
- SolarUy/Components/Pages/Mediciones.razor: interfaz de Producción, raíz InteractiveServer dentro del contenedor. No declara otra ruta que compita con el host. Conserva selección, indicadores, Radzen, ficha y tabla.
- Sensor/Components/MotorSensor.razor: segunda raíz, InteractiveWebAssembly sin prerender ni marcado visible. Sin directiva @page, selectores o gráfica. Solo inicializa y dispone los servicios cliente del simulador.

El contenedor transmite únicamente parámetros serializables, principalmente un SesionId estable creado por el servidor para esa instancia. No transmite servicios, entidades, RenderFragment, delegados, callbacks, HubConnection ni estado mutable compartido entre raíces. El enlace operativo servidor–Sensor se realiza exclusivamente por SignalR, nunca mediante referencias entre componentes o un objeto singleton que pretenda compartir memoria entre runtimes.

Secuencia propuesta:

1. Al resolver la ruta autorizada, el contenedor estático solicita a Aplicación crear una sesión efímera asociada al principal autenticado. Genera una sola identidad de instancia y pasa su SesionId a ambas raíces. La sesión se crea antes de emitir sus descriptores de arranque; el identificador nunca autoriza por sí solo.
2. Desactivar prerender en las dos raíces propuestas para que la creación del ciclo y las suscripciones no se dupliquen entre prerender e interactividad. El contenedor estático puede mostrar un estado inicial de carga sin generar ni abrir conexiones. Toda operación interactiva se inicializa una vez por instancia, con guardas de ciclo de vida y claves estables.
3. Mediciones inicializa el lector local de notificaciones y carga catálogo mediante Aplicación y contrato local de Parques. Selecciona primer parque/nodo por Id y Todos los paneles y registra su configuración deseada en la sesión.
4. MotorSensor inicializa servicios/handlers una vez y abre una sola HubConnection. Invoca RegistrarSensor(SesionId); el servidor valida propietario y acceso de la conexión real y la vincula a esa sesión.
5. Catálogo y arranque WebAssembly pueden terminar en cualquier orden. El coordinador inicia automáticamente una sola vez cuando la sesión tiene configuración deseada válida y Sensor registrado. Si Sensor llega primero espera configuración; si el catálogo llega primero espera registro, sin pulsar Conectar ni añadir un paso del usuario.
6. El servidor reconsulta flags y envía configuración vigente solo al ConnectionId registrado; Sensor confirma versión e inicia su ciclo. Producción muestra Conectado únicamente después de la confirmación correspondiente.

Sin catálogo no enviar configuración ni producir datos; indicar el estado y deshabilitar selectores y Conectar. Si falla la carga de WebAssembly o la conexión, el coordinador comunica inicialización fallida o tiempo de espera por la interfaz de SolarUy. No generar sustitutos en el servidor ni crear un acceso manual a Sensor para resolverlo.

Reutilizar autenticación y cookies existentes del mismo origen. Los parámetros serializados no contienen credenciales ni un flag de autorización confiable del cliente. Si falta un mecanismo de acceso compatible, documentar el problema en su requerimiento y comunicar bloqueo; no quitar autorización, crear roles ni rediseñar autenticación para el arranque.

Una conexión adicional para la misma instancia se rechaza mientras la anterior siga siendo válida. Una reconexión legítima revalida propietario/sesión y sustituye la vinculación invalidada; no agrega otro motor o temporizador. Dos contenedores de Producción son instancias independientes, sin coordinación de generadores entre usuarios.

Al navegar fuera de Producción se retiran ambas raíces: Mediciones cancela lector y termina/invalida su sesión; MotorSensor cancela el ciclo y dispone HubConnection y handlers. El orden de sus disposiciones no está garantizado: las operaciones de cierre son idempotentes y una entrega posterior a la invalidación se rechaza. OnDisconnected limpia la vinculación si el cierre del navegador impide despedida ordenada. No depender solo de un mensaje final del cliente.

Una interrupción temporal del circuito recuperable de Mediciones no vuelve a crear el contenedor ni el motor hermano. Si el servidor reinicia y pierde las sesiones efímeras, distinguir esa situación y recuperarla coordinadamente mediante una nueva composición de ambas raíces; nunca reemplazar solo un SesionId dejando un motor viejo conectado. Validar también navegación mejorada, reentrada, recarga y renderizados: cada nueva instancia tiene una sesión y un motor, y cada instancia retirada queda liberada.

### Ajustes de renderizado y enrutamiento

Routes.razor actualmente se utiliza sin un modo interactivo global desde App.razor, y descubre el ensamblado Sensor. Mantener esa base estática para que el host emita raíces hermanas; conservar los modos propios de las demás páginas. No trasladar la interfaz de Producción al proyecto Sensor ni convertir toda la aplicación a WebAssembly.

Revisar el registro existente de AddInteractiveServerComponents, AddInteractiveWebAssemblyComponents y sus modos de endpoint, y reutilizarlo. Revisar HeadOutlet y la carga de estilos existente, incluida la condición residual para /sensor: Producción ahora está en /admin/mediciones y debe cargar allí Radzen y sus estilos, sin necesitar una ruta de Sensor. MotorSensor no escribe título ni contenido de cabecera. Conservar menú, barra superior y comportamiento de las otras rutas.

### Cambio de configuración y resultado incierto

Conectar valida parque/nodo pendientes y reconsulta paneles antes de enviar Configurar. Un cambio efectivo crea otra versión. Sensor cancela y espera el ciclo anterior, sustituye configuración y confirma. Solo entonces la página cambia selección activa, restablece Todos y limpia la serie.

Una validación fallida antes del envío conserva selección y ciclo activos. Si se pierde la confirmación después de enviar, el resultado es indeterminado: conservar los datos anteriores como desactualizados y mostrar sincronización pendiente. Consultar estado o reenviar idempotentemente la misma versión; no anunciar éxito ni afirmar que el ciclo viejo sigue activo sin confirmación. Ignorar versiones anteriores.

### Recepción y persistencia

La entrega identifica sesión, versión y entrega, más lecturas con parque, nodo, panel, valor y fecha. Rechazar sesión terminada/inexistente, conexión ajena y versión obsoleta. Validar asociaciones, duplicados, valores finitos entre cero y PotenciaActual y fechas.

Leer flags persistidos por el contrato local; no confiar en flags enviados por Sensor. Excluir lecturas de paneles deshabilitados antes de construir entidades, guardar o sumar. Una lectura tardía deshabilitada no crea filas ni invalida por sí sola las lecturas válidas restantes. Cada lectura identifica la RevisionEstado del panel usada al generarla: descartar una revisión anterior, incluso si el panel fue habilitado nuevamente, para no aceptar una lectura previa a ese cambio como generación nueva.

En operación sincronizada, exigir los habilitados de la configuración confirmada. Durante un cambio de flag aún no confirmado por Sensor, aceptar las lecturas vigentes de los otros paneles y mantener el afectado pendiente de su primera lectura válida; no completar su valor ni su serie con datos ficticios. Si falta un habilitado que Sensor ya confirmó o el catálogo no corresponde, informar y resincronizar. Una entrega sin lecturas aceptables no crea filas ni una muestra artificial.

Guardar el conjunto aceptado atómicamente por IMedicionRepository y publicar su instantánea/total después del éxito. Si falla, conservar últimos datos confirmados identificados como anteriores y comunicar error de guardado separado del estado de conexión. No reintentar escrituras inciertas sin idempotencia. El identificador de entrega correlaciona resultados; cualquier política de reenvío deberá definir deduplicación antes de habilitarla, sin prometer exactamente una vez después de reiniciar el servidor.

### Habilitación, deshabilitación y concurrencia

Aplicación de Mediciones coordina ambas acciones: invocar el cambio de flag local de Parques, esperar persistencia y después enviar CambiarHabilitacionPanel con PanelId, Deshabilitado y RevisionEstado a los Sensor vinculados cuyas configuraciones contienen ese panel. Obtener destinatarios del registro, nunca difundir indiscriminadamente.

Serializar recepción y cambios de flag por NodoId mediante un coordinador compartido en el servidor. Una recepción mantiene la exclusión desde la lectura de flags hasta confirmar el guardado; habilitar/deshabilitar usa la misma exclusión al persistir y actualizar la revisión. Una lectura aceptada antes de deshabilitar puede quedar en historial, pero ninguna recepción posterior al flag persistido la incorpora mientras siga deshabilitado. La propuesta cubre el monolito en un proceso; no añade coordinación distribuida de generadores.

Tras deshabilitar y persistir, notificar páginas afectadas: conservar panel visible, marcar Deshabilitado, poner generación actual en cero y excluirlo del total actual. No reescribir muestras ni totales antiguos. Cambiar el indicador no inventa una entrega para la gráfica: el siguiente punto corresponde a lecturas aceptadas reales.

Tras habilitar y persistir, mostrar Habilitado y Esperando nueva medición; no recuperar como actual el último valor histórico. El panel no aporta al total actual hasta recibir una lectura aceptada con su revisión vigente. Conservar los puntos históricos de su gráfica y agregar únicamente las nuevas lecturas reales, sin puntos sintéticos para el intervalo deshabilitado. Volver a guardar y sumar desde la siguiente entrega válida.

Para cada panel de cada sesión, asignar revisiones crecientes desde el servidor bajo la misma exclusión. Sensor y la proyección de UI aplican únicamente la revisión más reciente de su sesión/configuración y confirman esa revisión; repetirla no reinicia el temporizador. Ignorar órdenes y confirmaciones antiguas, incluidas las que lleguen después de una configuración completa más nueva. Antes de enviar una orden pendiente, verificar que aún corresponde al estado vigente. Las revisiones son estado efímero de sesión, no credenciales; las sesiones se invalidan al reiniciar el servidor y se reconstruyen desde los flags persistidos.

Si falla persistencia, conservar estado/revisión anteriores y no enviar la orden ni anunciar éxito. Si persiste pero falla comunicación, conservar el flag, mostrar el estado y advertir sincronización pendiente. Al reconectar, reconsultar todos los flags y enviar una configuración autoritativa actualizada con sus revisiones; no reproducir ciegamente una cola de órdenes antiguas.

Si todos están deshabilitados, Sensor no produce lecturas y la página muestra cero y esa situación. Conservar el único planificador activo, omitiendo ticks sin habilitados: habilitar uno lo incorpora desde el próximo tick de la misma cadencia, sin Conectar ni temporizador adicional. Se conserva la diferencia entre Sensor conectado, generación pausada por flags y sesión terminada; Detener sí cancela el ciclo.

### Infrastructure

Conservar MedicionesDbContext, fábrica, repositorio y migraciones propios en la misma base MySQL. El flag requiere migración en Parques, no trasladar mediciones a su contexto. No agregar claves foráneas restrictivas o cascadas sobre historial.

Implementar registro de sesiones, canales, exclusión y adaptador de órdenes con IHubContext conforme a contratos de Aplicación. No compartir contextos concurrentemente ni capturar scoped en singleton. Toda regla y secuencia de negocio sigue en Aplicación/Dominio; el adaptador solo transporta.

### Presentation: Hub y notificaciones locales

Adaptar MedicionesHub para RegistrarSensor, ConfirmarConfiguracion, confirmación/consulta de estado y RegistrarEntrega; delegar desconexión a Aplicación. Retirar su uso para el catálogo de Producción.

| Orden servidor a Sensor | Comportamiento |
| --- | --- |
| Configurar | Configuración completa de sesión/versión; sustituir el ciclo de forma idempotente. |
| CambiarHabilitacionPanel | PanelId, Deshabilitado y revisión creciente; incluir o excluir desde el próximo tick sin reiniciar, rechazando órdenes obsoletas. |
| Detener | Terminar solo el ciclo de la sesión destinataria y confirmar. |

Ampliar DTO existentes sin replicar entidades. Se pueden conservar mensajes de transporte en Sensor/Contratos/Mediciones con la referencia existente SolarUy hacia Sensor; solo el adaptador SignalR del servidor conoce esos tipos. Aplicación utiliza modelos propios. No enlazar fuentes de backend ni agregar dependencia inversa.

La página se suscribe a un contrato de Aplicación que devuelve un lector asíncrono de DTO por sesión y un recurso desechable. Usar canales, sin eventos globales que retengan componentes. Procesar con InvokeAsync, filtrar sesión/versión y cancelar/disponer lector y suscripción al salir. No requiere otro HubConnection en la página ni API REST.

## Sensor/WebAssembly

### Servicios y generación

Adaptar el PeriodicTimer existente de SimulacionMedicionesService a **5 segundos**. Recibir configuración y órdenes: no consultar catálogo ni generar antes de una configuración confirmada. En cada tick generar únicamente habilitados entre cero y PotenciaActual, aunque la pantalla muestre un solo panel. La misma configuración no duplica ni reinicia; otro parque/nodo sustituye y espera el ciclo previo. Los cambios de flag se aplican al ciclo existente sin sustituirlo. El panel visible y los selectores pendientes de SolarUy no afectan el generador.

CambiarHabilitacionPanel actualiza de forma atómica el flag y su revisión en la instantánea, sin cancelar ni recrear el temporizador. Cada tick captura una instantánea consistente y cada lectura incluye la revisión que utilizó. Las lecturas ya enviadas se filtran en el servidor. Si todos están deshabilitados, omitir la entrega manteniendo el planificador; al habilitar uno, generar desde el siguiente tick, sin Conectar. Detener o perder control cancela el ciclo; reconectar exige configuración vigente antes de reanudar y rechaza órdenes anteriores.

### Infrastructure y Presentation

Adaptar MedicionesCliente para registro, órdenes, confirmaciones y envío, con handlers registrados una vez y liberados al disponer. Mantener autenticación existente de mismo origen.

Conservar Sensor/Components/MotorSensor.razor como raíz WebAssembly hermana de Mediciones bajo ProduccionHost estático. Sin @page ni marcado visible; delega generación y transporte a sus servicios y gestiona únicamente inicialización/disposición. Retirar la antigua ruta de Producción WebAssembly al mover su interfaz a SolarUy. No crear otra ruta de Sensor para esta funcionalidad, ni siquiera como host técnico.

## SolarUy/Presentation

### Página, selección y sesión visual

Adaptar Mediciones.razor y código asociado a SolarUy/Components/Pages como raíz InteractiveServer de la pantalla, autorizada para ADMIN. La ruta /admin/mediciones la declara ProduccionHost estático y compone Mediciones y MotorSensor como hermanos, según la sección de inicialización. Cambiar Producción en el menú. Inyectar servicios de Aplicación, no repositorios/DbContext ni generador.

Separar catálogo, selección pendiente de controles (ParqueId, NodoId y PanelIdPendiente), selección activa confirmada (ParqueId, NodoId y PanelIdVisible), versión, muestras y estados de conexión/operación/guardado. Ningún campo de panel debe cumplir simultáneamente las dos responsabilidades mientras los nodos difieran. Carga inicial automática; sin catálogo, mensaje y controles deshabilitados; durante el arranque automático, estado inicializando; ante fallo de Sensor, error de conexión sin enlace o inicio manual.

Los selectores se resuelven inmediatamente desde el catálogo local ya cargado, sin invocar Configurar ni esperar Conectar:

1. Cambiar parque actualiza los nodos de ese parque por Id, selecciona su primer nodo, actualiza sus opciones de panel y pone PanelIdPendiente en Todos.
2. Cambiar nodo actualiza sus opciones de panel y pone PanelIdPendiente en Todos.
3. Las opciones del selector de panel proceden siempre del nodo pendiente. Si no hay nodo válido, dejar sus opciones vacías salvo Todos y deshabilitar los controles dependientes y Conectar.
4. Si el parque/nodo pendiente difiere del activo, cambiar PanelIdPendiente solo modifica el control. Conservar PanelIdVisible, indicadores, gráfica, ficha y tabla del activo; mostrar explícitamente el parque/nodo activo y el cambio pendiente. No cambiar la producción ni enviar órdenes.
5. Si parque/nodo de controles y activos coinciden, seleccionar Todos o un panel válido actualiza PanelIdVisible y su proyección inmediatamente, sin Conectar, spinner, consultas históricas u órdenes. Al volver con los controles al nodo activo, el reset a Todos puede reflejarse localmente sin reconfigurar Sensor.
6. Conectar toma una instantánea del parque/nodo pendiente, valida por Aplicación y solicita la configuración completa de todos sus paneles. El panel pendiente no reduce el conjunto generado ni se aplica como filtro inicial de un nodo nuevo.
7. Solo la confirmación de la versión correspondiente activa otro parque/nodo, restablece ambos estados de panel a Todos y comienza otra serie. Hasta entonces, la vista anterior permanece identificada como tal. Un error conserva los datos activos y no anuncia éxito. Repetir Conectar con la misma configuración conserva filtro activo, muestras y temporizador.

No acoplar el lector de notificaciones a los campos pendientes: una muestra, confirmación de flags o reconexión del nodo activo no debe sobrescribir los selectores de otro nodo preparado. Deshabilitar parque/nodo y Conectar durante la aplicación con espera o descartar resultados de operaciones superadas por su identificador/versión; una respuesta atrasada no puede reemplazar una elección posterior.

La actualización de opciones con el catálogo en memoria es local y no requiere spinner. Si es necesario refrescar datos, hacerlo por Aplicación y contrato local de Parques; mostrar indicador y deshabilitar solo los controles afectados durante esa consulta, resolver dependencias al completarla y restaurar controles en éxito/error. Ante fallo, mantener una combinación válida o informar indisponibilidad; no presentar opciones viejas como pertenecientes a la nueva selección ni exigir Conectar para cargar opciones.

Mantener muestras aceptadas de todos los paneles para cambiar vista localmente. Deshabilitar pone cero actual conservando historia; habilitar invalida solo la lectura actual del panel y muestra espera hasta recibir su nueva revisión. Ni alternar vista ni reconectar recuperan una lectura antigua como actual. No fabricar ceros retroactivos ni recalcular totales históricos.

### Diseño, Radzen y SVG

Reutilizar componentes y CSS tomando [vista total](../../Docs/SolarUyPantallaMedicionesTotal.png) y [vista individual](../../Docs/SolarUyPantallaMedicionIndividual.png). Mantener cabecera, fila de selectores con tarjeta verde, franja horizontal Potencia/Generando ahora, gráfica de aproximadamente dos tercios junto a ficha de un tercio y tabla inferior completa.

Ficha individual: Panel, Parque, Nodo, Orientación y Estado. Mostrar un único botón: Deshabilitar panel si está habilitado, Habilitar panel si está deshabilitado; desactivarlo solo mientras se procesa la operación correspondiente o falta acceso. Elegir texto y estado desde el flag confirmado, sin alternar optimistamente antes de persistir. Diferenciar el flag del Estado técnico.

Conservar RadzenChart/RadzenAreaSeries, SVG, referencia explícita y servicios/recursos en SolarUy. Verificar que Sensor no conserve dependencias o recursos de la pantalla retirada. Mantener línea amarilla, relleno suave, cuadrícula, tiempo real de sesión y W, sin curva fija, consultas históricas o reportes. No añadir biblioteca de iconos ni caracteres como rayo.

Spinner al cargar, consultar selectores dependientes si existe espera, confirmar parque/nodo y habilitar/deshabilitar, con restauración de controles en finally. Deshabilitar temporalmente los controles involucrados. Notificaciones periódicas y panel inmediato sin spinner. Mostrar por separado Sensor ausente, configuración pendiente, todos deshabilitados, habilitado esperando lectura nueva, error de guardado y flag guardado pendiente de sincronizar.

Adaptar a móvil conservando jerarquía y comparar ambas vistas con referencias: distribución, proporciones, tipografía, espacios, colores, bordes e iconos. Conservar menú y barra existentes.

## Acceso, verificación y documentación

Reutilizar sesión y ADMIN existentes. El propietario de la sesión procede de un identificador estable del principal, no de un dato arbitrario del cliente. Ante fallos de código, registrar bug vinculado al requerimiento responsable; si el requisito de autenticación es incorrecto/incompleto, documentar allí la corrección. Informar bloqueo antes de modificar esa funcionalidad, sin inventar credenciales o roles.

Agregar pruebas xUnit de Dominio por habilitación/deshabilitación y ejecutar las existentes según .agents/Reglas/6_flujo_trabajo_ia.md. No proponer pruebas unitarias nuevas de otras capas. Revisar dependencias manualmente; ejecutar controles estructurales/SonarQube solo si están configurados, sin instalarlos.

Al implementar desde la raíz: dotnet build SolarUy.sln y dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj. Verificar migración de Parques e historial de Mediciones conservado.

Matriz manual: página InteractiveServer y catálogo local; arranque automático de raíces hermanas una sola vez, ausencia de iframe/ruta Sensor/interfaz/pestañas/enlaces, fallo de carga/conexión y destinatario; dos sesiones aisladas; versiones y confirmaciones; opciones dependientes inmediatas de parque/nodo/panel, panel pendiente sin filtrar el activo, retorno a la selección activa, consultas lentas/fallidas sin Conectar, cambio confirmado de nodo y panel activo inmediato; ciclo único cada cinco segundos e idempotente; recepción/guardado/notificación; ambos botones según flag; habilitar desde todos deshabilitados conservando cadencia; espera de lectura nueva sin rescatar la histórica; lectura tardía concurrente y órdenes/confirmaciones fuera de orden tras varios cambios; fallo de persistencia y orden fallida posterior al guardado en ambos sentidos; recarga/reconexión; circuito y disposición; historial intacto; ADMIN/no ADMIN/anónimo; capturas nuevas de escritorio/móvil.

Los cambios, errores y verificaciones se documentarán en informe descriptivo separado. Tras implementar, README solo describirá funcionamiento, comunicación e inicialización automática de Sensor dentro de Producción. No recrear los informes eliminados ni presentar capturas antiguas como comprobación de la integración nueva.

## Correspondencia entre requisitos y tareas

La revisión mantiene el alcance del requerimiento actual. Los siguientes criterios permiten revisar el diseño y comprobar después su implementación; no representan verificaciones ya ejecutadas.

| Criterio del requerimiento | Decisión de diseño | Tareas relacionadas |
| --- | --- | --- |
| Producción en SolarUy InteractiveServer | Página en Components/Pages y catálogo por servicios locales de Mediciones/Parques. | T01, T02, T17, T19 |
| Sensor integrado y órdenes dirigidas | Raíces hermanas InteractiveServer/WebAssembly bajo contenedor estático, sin iframe o ruta Sensor; sesión autenticada y destinatario por versión. | T10, T17–T19 |
| Selección inicial y dependencias inmediatas | Primer parque/nodo por Id; opciones de nodo/panel siguen a los controles sin esperar Conectar y se restablece Todos. | T02, T03, T05, T06 |
| Pendiente separado de activo | Solo Conectar confirma otro nodo; panel pendiente no filtra el activo; panel inmediato cuando coinciden parque/nodo. | T03–T08, T14 |
| Un ciclo cada 5 segundos | Generación exclusiva WebAssembly; repetir configuración y cambiar flag no reinicia temporizador. | T09, T10, T15 |
| Guardado completo de lecturas válidas | Validar asociaciones, límites, flags y revisiones; persistir antes de notificar éxito. | T12–T14, T20, T21 |
| Habilitar y deshabilitar persistentemente | Flag separado de EstadoPanel, métodos explícitos y botón único según estado confirmado. | T11, T12, T16, T21 |
| Habilitado sin lectura antigua actual | Esperar primera lectura de la revisión vigente y reanudar en el próximo tick sin Conectar. | T10, T12–T14, T16, T21 |
| Fallos y órdenes fuera de orden | Conservar flag confirmado, marcar sincronización pendiente y aplicar solo revisiones vigentes. | T08, T09, T12, T13, T18, T21 |
| Historial y gráfica intactos | Muestras aceptadas por sesión, sin recalcular puntos pasados ni consultar historia en MySQL. | T11, T13, T14, T21 |
| Fidelidad visual y estados de espera | Radzen/SVG en SolarUy, comparación escritorio/móvil, spinner solo en operaciones con espera. | T16, T17, T22 |
| Verificación y documentación | Pruebas de Dominio, controles configurados e informe separado de cambios/errores. | T11, T20–T24 |

La revisión documental no compila, prueba ni implementa estas adaptaciones. La evidencia de cada criterio se registrará al ejecutar sus tareas.
