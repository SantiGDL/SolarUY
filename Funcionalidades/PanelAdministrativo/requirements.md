# Requisitos: panel administrativo de SolarUy

## 1. Objetivo

Crear la primera versión visual del panel administrativo tomando como referencia
[DashboardAdmin.png](../../Docs/DashboardAdmin.png), manteniendo su distribución,
colores y estilo. En esta etapa será una interfaz provisional, sin funcionalidades
de gestión ni conexión con datos reales.

## 2. Alcance

Incluye una página de inicio del panel en `/admin`, barra lateral, cabecera,
presentación, tarjetas de resumen, accesos rápidos, actividad reciente y gráfico
de producción. Reutilizar y adaptar la barra lateral existente y mantener el
panel separado de HomeLayout. La Home conservará su diseño actual. La navegación
entre ambas pantallas estará habilitada sin verificación de acceso. El uso de
la navbar general dentro del panel queda pendiente de definición con el equipo.

Quedan fuera autenticación, permisos, búsqueda real, cierre de sesión, gestión
de parques y tareas, reportes, persistencia e integración con el simulador.
Esta página provisional no representa un área protegida por autenticación.

## 3. Requisitos funcionales

- Barra lateral con logo y opciones Inicio, Parques, Producción, Tareas y
  Reportes. Tanto Inicio como el logo de SolarUy llevarán a la Home.
- Cabecera con búsqueda ilustrativa, identificación genérica «Admin» y botón
  «Cerrar sesión», sin representar una sesión real.
- Presentación con fondo solar, título «Panel de gestión SolarUy» y descripción
  breve.
- Tarjetas de Parques activos, Producción actual y Tareas pendientes.
- Accesos rápidos para gestionar parques, consultar producción, gestionar tareas
  y consultar reportes.
- Secciones de actividad reciente y producción de energía. Mostrar estados
  vacíos o marcadores sin datos, evitando inventar registros o estadísticas.
  Para el gráfico utilizar provisionalmente [GraficaMockup.png](../../Docs/GraficaMockup.png),
  identificada como imagen ilustrativa, sin datos reales ni interacción.
- Mantener deshabilitados los controles de gestión, búsqueda, filtros y cierre
  de sesión. No enlazar a páginas inexistentes.
- Habilitar «Ingresar al panel» en navbar y hero de la Home para
  abrir `/admin`, retirando su indicación «Próximamente».
- En la Home, habilitar Inicio y el logo de la navbar para volver al inicio de
  la página. Plataforma y Contacto conservarán su estado deshabilitado. No se
  requiere autenticación para navegar entre Home y el panel en esta versión.
- Utilizar el logo, solar-hero.png, solar-sidebar.png y los íconos disponibles.
  La imagen de referencia guía el diseño; el panel se construirá con Razor y CSS.

## 4. Requisitos no funcionales

- Mantener azul marino, azul eléctrico, blanco y amarillo solar, tarjetas
  redondeadas y sombras suaves.
- Adaptar el panel a computadoras, tablets y celulares; permitir mostrar y
  ocultar el menú lateral en pantallas pequeñas.
- Cuidar contraste, textos alternativos, navegación por teclado y señalización
  de controles deshabilitados y contenido provisional.
- Conservar Blazor Interactive Server, Bootstrap y la arquitectura existente,
  sin dependencias adicionales ni cambios del backend.

## 5. Criterios de aceptación

- [ ] `/admin` muestra las secciones previstas con una estética acorde a la imagen.
- [ ] Los dos ingresos de la Home abren el panel; Inicio y el logo del panel
  vuelven a Home, y los de la navbar de Home también funcionan.
- [ ] La sección de producción muestra GraficaMockup.png como imagen ilustrativa.
- [ ] No se presentan datos ficticios como reales ni sesiones autenticadas.
- [ ] Los controles pendientes están deshabilitados y el menú móvil funciona.
- [ ] La Home conserva su layout y el panel se adapta sin desbordes.
- [ ] La solución compila sin incorporar dependencias ni modificar el backend.
