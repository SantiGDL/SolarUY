# Tareas: panel administrativo de SolarUy

Implementación realizada. La verificación completa de las páginas de plantilla está limitada por errores locales de Data Protection.

- [x] T01 — Presentación / Recursos: copiar GraficaMockup.png a `SolarUy/wwwroot/images/` conservando el original y reutilizar los recursos locales. Finaliza sin descargas ni dependencias nuevas.
- [x] T02 — Presentación / Layout: adaptar MainLayout y NavMenu con barra lateral, cabecera y estilos administrativos. Finaliza sin duplicar la barra ni incorporar la navbar horizontal de Home.
- [x] T03 — Presentación / Página: crear PanelAdmin.razor y su CSS, registrar `/admin` y aplicar MainLayout. Finaliza con hero, tres tarjetas de resumen y cuatro accesos rápidos según la referencia, con marcadores «Sin datos» y controles pendientes deshabilitados.
- [x] T04 — Presentación / Actividad y gráfica: agregar estado vacío de actividad, GraficaMockup.png con aclaración ilustrativa y filtro deshabilitado. Finaliza sin registros ficticios ni biblioteca de gráficas.
- [x] T05 — Presentación / Navegación: habilitar los dos ingresos de Home hacia `/admin`, retirar «Próximamente» y enlazar Inicio y logo de Home y panel hacia `/`. Finaliza sin autenticación y con los demás controles pendientes deshabilitados.
- [x] T06 — Presentación / Responsive y accesibilidad: adaptar tarjetas, imágenes y menú móvil, con control accesible de apertura/cierre. Finaliza con teclado y foco utilizables, textos alternativos y sin desbordes a 375, 768 y 1440 px.
- [ ] T07 — Verificación: compilar con `dotnet build SolarUy.sln`, ejecutar verificaciones existentes aplicables y revisar en navegador rutas, ida y vuelta, recursos y páginas de plantilla. Corregir errores introducidos y registrar comprobaciones bloqueadas. No agregar pruebas unitarias de presentación ni instalar herramientas.
- [x] T08 — Documentación: actualizar el README y los documentos de PaginaPrincipal afectados por la navegación habilitada, distinguiendo el diseño anterior del comportamiento actual. Finaliza con documentación coherente con lo implementado.
- [x] T09 — Cierre: registrar resultados y marcar únicamente tareas completadas. Declarar SonarQube pendiente y cualquier limitación de revisión visual o del entorno.

## Resultados

- dotnet build SolarUy.sln --no-restore: 0 errores y 0 advertencias, incluida la corrección de contraste de logo e íconos laterales.
- HTTP 200 en /, /Home, /admin, solar-sidebar.png, GraficaMockup.png y la hoja CSS compilada.
- HTML: dos ingresos de Home hacia /admin y logo e Inicio del panel con destino /. No se agregó autenticación.
- Edge headless mediante protocolo de depuración: revisión visual a 375, 768 y 1440 px, sin desborde horizontal ni imágenes faltantes. Se verificó cierre y reapertura del menú nativo en móvil. El contraste lateral se mejoró después de esa revisión.
- T07 queda parcialmente pendiente: /counter y /weather responden HTTP 500 por errores locales de Data Protection/DPAPI, también observados antes de esta tarea. Sus archivos y rutas se conservaron.
- No existen proyectos de pruebas ni ArchUnitNET configurado. No se agregaron pruebas unitarias de presentación. SonarQube sigue pendiente.

