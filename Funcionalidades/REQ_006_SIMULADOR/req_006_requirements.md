Crear la página Mediciones.razor dentro de Pages del proyecto Sensor, ejecutada con InteractiveWebAssembly. Se accederá desde la opción Producción del menú de SolarUy.

Basar el diseño en las imágenes de referencia guardadas en Docs osea SolarUyPantallaMedicionesTotal y SolarUyPantallaMedicionIndividual, correspondientes a todos los paneles y a un panel individual. Reutilizar el menú lateral y la barra superior existentes.

Al acceder, establecer una conexión con el Hub de SignalR de SolarUy y solicitar los parques disponibles. El servidor debe devolver únicamente parques que tengan al menos un nodo con paneles. Para cada parque, ofrecer únicamente los nodos que tengan paneles.

Si no existe ningún parque que cumpla estas condiciones, mostrar un mensaje explicativo y deshabilitar los selectores y el botón Conectar.

En la carga inicial, seleccionar automáticamente el primer parque disponible, su primer nodo disponible y la opción Todos los paneles. Aplicar esa selección automáticamente para comenzar a mostrar las mediciones

Al cambiar de parque, actualizar sus nodos, seleccionar el primero disponible y cargar sus paneles. Al cambiar de nodo, actualizar los paneles. En ambos casos, restablecer la selección de panel a Todos los paneles.

Después de la carga inicial, los cambios de selección se aplicarán al pulsar Conectar. Mantener separadas la selección pendiente y la selección activa, indicando claramente a qué parque, nodo y panel corresponden los datos visibles.

Al seleccionar todos los paneles, mostrar la suma de su generación actual y una tabla con los valores individuales. Al seleccionar uno, mostrar sus datos y generación actual.

Mantener la arquitectura del monolito modular: el Hub debe delegar las consultas y operaciones a los servicios de aplicación correspondientes, sin acceder directamente al DbContext ni contener lógica de negocio. Revisar y reutilizar las implementaciones existentes antes de crear otras nuevas.

