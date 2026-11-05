# Requisitos: página principal de SolarUy

## 1. Objetivo

Desarrollar la página de presentación de SolarUy con la estética y distribución de [ReferenciaHome.png](../../Docs/ReferenciaHome.png), mostrando sus funcionalidades previstas de forma clara y sencilla.

## 2. Alcance

Incluye navbar, hero, vista previa estática del futuro dashboard, cuatro tarjetas informativas y footer. Se aprovecharán Home.razor y sus estilos existentes. La página utilizará HomeLayout, adaptando los archivos actuales Home-Layout, y estará disponible en `/` y `/Home`. La Home no mostrará la barra lateral del template; se conservarán MainLayout, NavMenu y sus estilos para el futuro panel administrativo.

El panel administrativo, la autenticación, los datos reales y las conexiones al backend quedan fuera de esta etapa. No se crearán specs.md ni tasks.md hasta las aprobaciones correspondientes.

## 3. Requisitos funcionales

- Navbar con logo, Inicio, Plataforma, Contacto y el botón «Ingresar al panel».
- Hero con fondo de paneles solares, título «Gestioná tu energía solar de forma simple», descripción breve y botones «Ingresar al panel» y «Conocer la plataforma».
- Usar provisionalmente [DashboardAdmin.png](../../Docs/DashboardAdmin.png) como imagen de vista previa a la derecha del hero, en el lugar señalado en la captura de referencia. Identificarla como diseño ilustrativo, sin interacción ni datos reales; reemplaza al mockup construido con Razor y CSS.
- Tarjetas de Parques solares, Producción, Tareas y Reportes, con imagen, título y descripción breve.
- Footer con nombre o logo, descripción breve y enlaces internos representados visualmente.
- Por el alcance posterior de [PanelAdministrativo](../PanelAdministrativo/requirements.md), ambos ingresos al panel enlazan a `/admin` y el logo e Inicio de la navbar a `/`. Los demás controles permanecen deshabilitados.
- Usar solaruy-logo.png y solar-hero.png de wwwroot. Reservar solar-sidebar.png para el futuro panel administrativo.
- Utilizar temporalmente las imágenes de Docs/Iconos: IconoConocer.png, IconoPanelSolar.png, IconoProduccion.png, IconoReportes.png e IconoTareas.png. No incorporar Bootstrap Icons ni generar o descargar imágenes nuevas.

## 4. Requisitos no funcionales

- Mantener azul marino, azul eléctrico, blanco y amarillo solar, tarjetas redondeadas y sombras suaves.
- Adaptar la distribución a computadoras, tablets y celulares.
- Cuidar contraste, textos alternativos y señalización accesible de controles deshabilitados.
- Conservar Bootstrap y la arquitectura existente, sin dependencias nuevas ni modificaciones del backend.
- Evitar componentes duplicados, redes sociales y contactos inventados. No agregar estadísticas fuera de la imagen ilustrativa; sus cifras forman parte del diseño y no representan datos reales.

## 5. Criterios de aceptación

- [ ] La Home se visualiza en ambas rutas sin barra lateral y se conservan los componentes de la barra existente.
- [ ] Incluye todas las secciones y utiliza los recursos indicados.
- [ ] El mockup y las tarjetas son informativos; solo navegan los enlaces habilitados por PanelAdministrativo.
- [ ] El contenido es legible y no desborda en móvil, tablet y escritorio.
- [ ] La solución compila y no incorpora dependencias ni cambios del backend.
