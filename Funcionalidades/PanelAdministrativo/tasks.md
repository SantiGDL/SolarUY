# Tareas: panel administrativo de SolarUy

La interfaz visual del panel y la Home fueron implementadas previamente. La
autenticación y gestión inicial de parques amplían el alcance según los
requisitos aprobados en [GestionParques](../GestionParques/requirement.md).

## Interfaz visual

- [x] Copiar y reutilizar los recursos locales para Home y PanelAdmin.
- [x] Implementar layouts separados para Home y panel.
- [x] Implementar `/` y `/Home` con diseño responsive.
- [x] Implementar `/admin`, resumen provisional y recursos ilustrativos.
- [x] Habilitar navegación Home/panel y conservar deshabilitados los accesos
  a funcionalidades aún inexistentes.
- [x] Implementar navegación responsive y elementos accesibles.

## Integración de parques y autenticación

- [x] Proteger `/admin` y `/admin/parques` con autenticación OIDC.
- [x] Mostrar identidad de sesión y habilitar cierre de sesión con protección
  antifalsificación.
- [x] Enlazar el panel con la consulta y el registro de parques.
- [x] Mostrar el total de parques registrados.
- [x] Registrar cambios detallados y validaciones en
  [GestionParques](../GestionParques/task.md).

## Verificación

- [x] Compilar `dotnet build SolarUy.sln` sin errores ni advertencias.
- [x] Ejecutar las pruebas unitarias: 9 aprobadas.
- [ ] Verificar en navegador login/logout, protección de rutas y menú responsive
  con Keycloak en ejecución.
- [ ] Revisar la Home, el panel y las páginas de plantilla tras los cambios.

Las verificaciones de Data Protection en `/counter` y `/weather` habían
reportado errores locales de DPAPI antes de esta ampliación. ArchUnitNET y
SonarQube siguen sin estar configurados.
