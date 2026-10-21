# Diseño: panel administrativo de SolarUy

Basado en [requirements.md](requirements.md) y
[DashboardAdmin.png](../../Docs/DashboardAdmin.png). Diseño pendiente de implementación.

## Página y layout

- Crear `Components/Pages/PanelAdmin.razor` con ruta `/admin` y su CSS aislado.
- Adaptar `Components/Layout/MainLayout.razor`, NavMenu y sus estilos existentes
  para la estructura administrativa: barra lateral y área principal con cabecera.
  No duplicar la barra lateral ni modificar HomeLayout para contener el panel.
- Aplicar MainLayout al panel. Las páginas de plantilla que ya usan ese layout
  compartirán su nueva apariencia; conservar sus rutas y contenido.
- No agregar la navbar horizontal de Home dentro del panel: su uso allí sigue
  pendiente de decisión. Esto no impide implementar la cabecera del dashboard.

## Contenido visual

- NavMenu mostrará logo, Inicio, Parques, Producción, Tareas y Reportes sobre
  fondo azul marino, con solar-sidebar.png como imagen decorativa inferior.
  Inicio y el logo enlazarán a `/`; no mostrar Inicio como página activa mientras
  se está en `/admin`, porque su destino es la Home.
- La cabecera incluirá búsqueda deshabilitada, identidad genérica «Admin» y
  «Cerrar sesión» deshabilitado. Indicar que es una vista provisional sin sesión real.
- En PanelAdmin, construir el hero con solar-hero.png, título aprobado y descripción.
- Crear tres tarjetas de resumen y cuatro accesos rápidos siguiendo la referencia.
  Los valores se mostrarán como «Sin datos», sin números ficticios ni ceros que
  puedan interpretarse como resultados reales. Los accesos estarán deshabilitados.
- Actividad reciente mostrará un estado vacío. La sección de producción contendrá
  GraficaMockup.png, proporcionada y con texto visible «Gráfica ilustrativa, sin
  datos reales». El filtro temporal permanecerá deshabilitado.

## Recursos y estilos

Reutilizar los recursos de wwwroot y los íconos locales ya disponibles en
`wwwroot/images/`. Copiar `Docs/GraficaMockup.png` a esa carpeta conservando el
original. No usar DashboardAdmin.png como página completa ni incorporar una
biblioteca de gráficas en esta etapa.

Usar CSS aislado de MainLayout para estructura y cabecera, de NavMenu para el
menú y de PanelAdmin para secciones y tarjetas. Mantener Bootstrap existente,
la paleta aprobada, bordes redondeados y sombras suaves.

En escritorio, mostrar barra lateral y contenido con tarjetas en filas. En
tablet y móvil, reorganizar las tarjetas y permitir abrir/cerrar el menú mediante
un control accesible. Mantener la solución simple usando el mecanismo existente
si puede adaptarse, sin nuevas dependencias. El contenido y las imágenes no
deben provocar desplazamiento horizontal.

## Navegación Home ↔ panel

- En HomeLayout, convertir logo e Inicio en enlaces a `/`, e «Ingresar al panel»
  en enlace a `/admin`.
- En Home.razor, habilitar el ingreso del hero con destino `/admin`.
- Retirar «Próximamente» de ambos ingresos. Mantener Plataforma, Contacto y
  «Conocer la plataforma» deshabilitados, así como las tarjetas de Home.
- En NavMenu, logo e Inicio volverán a `/`. Las otras opciones serán controles
  deshabilitados, sin href ni rutas ficticias.
- No agregar autenticación, autorización ni una sesión simulada.

## Accesibilidad y límites

Usar navegación semántica, un h1 en la página, títulos ordenados, foco visible y
textos alternativos. Marcar íconos decorativos con alt vacío. El control del menú
móvil tendrá nombre accesible y comunicará su estado abierto/cerrado.

Esta tarea afecta Presentación y recursos estáticos. Dominio, Aplicación e
Infraestructura de todos los módulos quedan sin cambios. No se incorpora
comunicación con backend, simulador ni servicios externos.

## Verificación

Compilar `SolarUy.sln`. Comprobar `/`, `/Home` y `/admin`, ambos ingresos al panel
y el retorno mediante Inicio y logo. Revisar carga de recursos, controles
deshabilitados, teclado y menú móvil. Comprobar distribución a 375, 768 y 1440 px
y que las páginas de plantilla conserven rutas y contenido.

Ejecutar verificaciones existentes aplicables; no crear pruebas unitarias de
presentación ni instalar herramientas. Registrar limitaciones del entorno,
incluidos los errores locales de Data Protection si reaparecen. SonarQube sigue
pendiente según lo acordado.
