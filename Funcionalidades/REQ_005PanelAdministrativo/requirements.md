# Requisitos: panel administrativo de SolarUy

## 1. Objetivo

Crear el panel administrativo tomando como referencia
[DashboardAdmin.png](../../Docs/DashboardAdmin.png), manteniendo su distribución,
colores y estilo. El panel muestra información real de parques y permite su
consulta y registro según [GestionParques](../GestionParques/requirement.md).

## 2. Alcance

Incluye una página de inicio del panel en `/admin`, barra lateral, cabecera,
presentación, tarjetas de resumen, accesos rápidos, actividad reciente y gráfico
de producción. El panel utiliza MainLayout y requiere autenticación con Keycloak.
La Home conserva HomeLayout y permite acceder al panel luego de iniciar sesión.

Quedan fuera la gestión de tareas, reportes, persistencia de esos módulos e
integración con el simulador. La funcionalidad de parques y sus permisos se
definen en [GestionParques](../GestionParques/requirement.md).

## 3. Requisitos funcionales

- Barra lateral con logo y opciones Inicio, Parques, Producción, Tareas y
  Reportes. Tanto Inicio como el logo de SolarUy llevarán a la Home.
- Cabecera con búsqueda ilustrativa, identificación del usuario autenticado y
  cierre de sesión.
- Presentación con fondo solar, título «Panel de gestión SolarUy» y descripción
  breve.
- Tarjetas de Parques activos, Producción actual y Tareas pendientes.
- Accesos rápidos para gestionar parques, consultar producción, gestionar tareas
  y consultar reportes.
- Secciones de actividad reciente y producción de energía. Mostrar estados
  vacíos o marcadores sin datos, evitando inventar registros o estadísticas.
  Para el gráfico utilizar provisionalmente [GraficaMockup.png](../../Docs/GraficaMockup.png),
  identificada como imagen ilustrativa, sin datos reales ni interacción.
- Habilitar la gestión de parques en `/admin/parques`; mantener deshabilitados la
  búsqueda, filtros y las secciones aún no implementadas.
- Habilitar «Ingresar al panel» en navbar y hero de la Home para
  abrir `/admin`, retirando su indicación «Próximamente».
- En la Home, habilitar Inicio y el logo de la navbar para volver al inicio de
  la página. Plataforma y Contacto conservarán su estado deshabilitado.
- Exigir autenticación para el panel; permitir consulta a usuarios autenticados
  y registro solo a usuarios con rol `ADMIN`.
- Utilizar el logo, solar-hero.png, solar-sidebar.png y los íconos disponibles.
  La imagen de referencia guía el diseño; el panel se construirá con Razor y CSS.

## 4. Requisitos no funcionales

- Mantener azul marino, azul eléctrico, blanco y amarillo solar, tarjetas
  redondeadas y sombras suaves.
- Adaptar el panel a computadoras, tablets y celulares; permitir mostrar y
  ocultar el menú lateral en pantallas pequeñas.
- Cuidar contraste, textos alternativos, navegación por teclado y señalización
  de controles deshabilitados y contenido provisional.
- Conservar Blazor Interactive Server, Bootstrap y la arquitectura monolítica
  modular existente.

## 5. Criterios de aceptación

- [ ] `/admin` muestra las secciones previstas con una estética acorde a la imagen.
- [ ] Los dos ingresos de la Home abren el panel; Inicio y el logo del panel
  vuelven a Home, y los de la navbar de Home también funcionan.
- [ ] La sección de producción muestra GraficaMockup.png como imagen ilustrativa.
- [ ] No se presentan datos ficticios como reales; se muestra la identidad real.
- [ ] Los usuarios autenticados consultan parques y solo ADMIN puede
  registrarlos.
- [ ] Los controles pendientes están deshabilitados y el menú móvil funciona.
- [ ] La Home conserva su layout y el panel se adapta sin desbordes.
- [ ] La solución compila y pasan las pruebas de la funcionalidad.
