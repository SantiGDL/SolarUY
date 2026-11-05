# Diseño: página principal de SolarUy

Basado en [requirements.md](requirements.md). Este documento define la
implementación; todavía no se modifica la página.

## Página y layout

- Adaptar `SolarUy/Components/Pages/Home.razor` y su CSS existente. Registrar
  las rutas `/` y `/Home`, y aplicar `HomeLayout` únicamente a esta página.
- Retirar la ruta `/` de HomeViejo para evitar duplicaciones. Conservar el
  archivo como referencia sin crear otra ruta pública.
- Renombrar Home-Layout.razor y Home-Layout.razor.css a HomeLayout.razor y
  HomeLayout.razor.css. El layout heredará de LayoutComponentBase y contendrá
  la navbar, `@Body` y el footer.
- Conservar MainLayout, NavMenu, sus estilos y el layout predeterminado de
  Routes.razor para las otras páginas y el futuro panel administrativo.

## Contenido y recursos

- Navbar blanca con el logo a la izquierda, Inicio, Plataforma y Contacto,
  y el botón amarillo «Ingresar al panel».
- Hero con solar-hero.png como fondo, texto a la izquierda y DashboardAdmin.png
  a la derecha. Mantener el título y botones aprobados, con una descripción
  breve de las funcionalidades previstas.
- Mostrar la imagen del dashboard completa y proporcionada, con bordes
  redondeados y sombra. Agregar el texto visible «Vista ilustrativa del futuro
  panel administrativo». Sus controles y cifras forman parte de la imagen.
- Debajo, cuatro tarjetas informativas: Parques solares, Producción, Tareas
  y Reportes. Footer con logo o nombre, descripción y navegación deshabilitada.
- Usar los recursos existentes de wwwroot. Copiar DashboardAdmin.png desde Docs
  y los cinco PNG de Docs/Iconos a `SolarUy/wwwroot/images/`, conservando los
  originales. Usar IconoConocer para «Conocer la plataforma» y los otros cuatro
  en sus tarjetas respectivas. No incorporar bibliotecas de íconos.
- Usar el logo como elemento visual sin enlace activo. Reservar solar-sidebar.png
  para el panel administrativo.

## Navegación y accesibilidad

El diseño inicial de esta sección fue actualizado por PanelAdministrativo: el logo e Inicio de la navbar enlazan a / y ambos ingresos al panel enlazan a /admin, sin Próximamente. El resto conserva el comportamiento descrito a continuación. Los elementos pendientes de navegación se representarán mediante botones
deshabilitados, sin href, manejadores ni desplazamientos. Las tarjetas serán
elementos informativos, sin enlace. En el diseño inicial, ambos botones «Ingresar al panel» tenían
la aclaración visible «Próximamente».

Usar header, nav, main y footer, un solo h1 y títulos ordenados. Incluir texto
alternativo para el logo y la vista previa; los íconos decorativos tendrán alt
vacío. Conservar contraste legible, incluso en controles deshabilitados.

## Estilos y adaptación

Distribuir los estilos entre HomeLayout.razor.css (navbar y footer) y
Home.razor.css (hero y tarjetas), respetando el aislamiento CSS de Blazor.
Reutilizar Bootstrap existente, sin dependencias adicionales ni cambios globales
innecesarios.

En escritorio, hero de dos columnas y cuatro tarjetas en fila. En tablet,
reorganizar el hero y mostrar dos tarjetas por fila cuando el espacio lo requiera.
En celular, apilar contenido, vista previa y tarjetas. La navbar podrá distribuirse
en varias filas sin necesitar un menú interactivo. Las imágenes no deben deformarse
ni provocar desplazamiento horizontal.

## Límites y verificación

No modificar Backend, Sensor, persistencia ni autenticación. No se requieren
servicios de aplicación, comunicación entre módulos ni lógica de negocio.

Compilar la solución y revisar en navegador ambas rutas, recursos, controles
deshabilitados, conservación de otras páginas y tamaños de 375, 768 y 1440 px.
Comprobar la ausencia de desbordes y de enlaces a rutas inexistentes. No generar
pruebas unitarias para esta presentación estática. Ejecutar verificaciones
existentes aplicables; SonarQube continúa pendiente según lo acordado.

