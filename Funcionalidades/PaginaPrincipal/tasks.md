# Tareas: página principal de SolarUy

Implementación realizada; la revisión visual en navegador está pendiente.

- [x] T01 — Presentación / Recursos: copiar DashboardAdmin.png y los cinco íconos a `SolarUy/wwwroot/images/`, conservando los originales. Finaliza con todos los recursos disponibles localmente, sin descargas ni dependencias nuevas.
- [x] T02 — Presentación / Layout: adaptar y renombrar Home-Layout a HomeLayout con navbar, contenido y footer. Finaliza conservando MainLayout, NavMenu y sus estilos, sin barra lateral en Home.
- [x] T03 — Presentación / Rutas: aplicar HomeLayout a Home, habilitar `/` y `/Home` y retirar la ruta duplicada de HomeViejo. Finaliza cuando ambas rutas muestran la nueva Home y las otras páginas conservan su layout.
- [x] T04 — Presentación / Hero: adaptar el título, descripción, fondo y botones; reemplazar el mockup actual por DashboardAdmin.png con su aclaración ilustrativa. Finaliza con la imagen proporcionada y sin estadísticas adicionales.
- [x] T05 — Presentación / Tarjetas: adaptar las cuatro tarjetas con sus PNG, títulos y descripciones. Finaliza con tarjetas informativas sin enlaces ni comportamiento.
- [x] T06 — Presentación / Controles y accesibilidad: deshabilitar toda navegación, incluidos logo, navbar, footer y botones; mostrar «Próximamente» en ambos ingresos al panel. Revisar estructura semántica, textos alternativos y contraste. Finaliza sin redirecciones, desplazamientos ni controles aparentemente activos.
- [ ] T07 — Presentación / CSS: ajustar estilos aislados y distribución responsive según specs.md. Finaliza sin imágenes deformadas ni desbordes a 375, 768 y 1440 px.
- [ ] T08 — Verificación: ejecutar `dotnet build SolarUy.sln` y las verificaciones existentes aplicables. Revisar ambas rutas y el resultado visual en navegador, los recursos y las páginas que conservan MainLayout. Corregir errores introducidos y registrar cualquier comprobación no disponible. No agregar pruebas unitarias de presentación ni instalar herramientas.
- [x] T09 — Documentación y cierre: revisar si corresponde actualizar la información de uso del README, marcar únicamente tareas completadas y registrar resultados. Declarar SonarQube pendiente y cualquier limitación de la revisión visual.

## Resultado de verificación

- Compilación: dotnet build SolarUy.sln --no-restore, 0 errores y 0 advertencias.
- HTTP: / y /Home devuelven 200, igual que el logo, fondo, dashboard y los cinco íconos. La Home no contiene barra lateral ni enlaces activos.
- CSS responsive implementado. T07 y T08 quedan pendientes de revisión visual a 375, 768 y 1440 px: Edge headless falló por indisponibilidad de GPU en el entorno.
- Al iniciar aparecieron errores de Data Protection con claves locales DPAPI; /counter no pudo validarse correctamente. No se modificó la configuración de claves ni esa página.
- No existen proyectos de pruebas ni ArchUnitNET configurado. No se agregaron pruebas unitarias de presentación. SonarQube continúa pendiente.



## Navegación posterior
La funcionalidad PanelAdministrativo habilitó ambos ingresos a /admin y el logo e Inicio de la navbar a /. El resto de los controles conserva su estado deshabilitado. Se revisó el panel con Edge headless a 375, 768 y 1440 px; los errores de Data Protection de las páginas de plantilla siguen pendientes.

