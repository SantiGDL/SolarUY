# SolarUY

Prueba de concepto académica del Taller .NET para gestionar parques solares, nodos,
paneles y mediciones simuladas. La solución es un monolito modular con Usuarios,
Parques, Tareas y Mediciones.

## Funcionalidades

- Home en `/` y panel administrativo en `/admin`, con menú lateral y barra superior.
- Gestión de parques, nodos y paneles, con persistencia MySQL mediante Entity Framework Core.
- Autenticación con Keycloak/OpenID Connect y autorización por roles en las operaciones protegidas.
- Producción en `/sensor/mediciones`, disponible para administradores autenticados:
  selecciona parque, nodo y todos los paneles o uno individual. Cada instancia de
  la página genera en el navegador una lectura por panel del nodo activo cada
  10 segundos, entre 0 y su PotenciaActual, en W. Muestra el total del nodo o el
  detalle del panel y envía siempre las lecturas de todos los paneles para guardar
  su historial. Los cambios de parque o nodo se confirman con Conectar; cambiar
el panel actualiza la vista inmediatamente. La gráfica de área muestra las
muestras de la sesión del nodo activo.

Los valores de producción son simulados; no provienen de hardware real. El botón
Deshabilitar panel queda identificado como pendiente y sin efectos.

## Arquitectura y comunicación

SolarUy es una Blazor Web App .NET 10 con páginas Interactive Server. Sensor es
el proyecto WebAssembly .NET 10 hospedado por SolarUy y ejecuta la página de
Mediciones con InteractiveWebAssembly.

Sensor consulta el catálogo y envía entregas mediante SignalR a
`/hubs/mediciones`. El Hub delega a los servicios de Aplicación de Mediciones.
Estos consultan Parques mediante un contrato local C# y guardan las lecturas
mediante su repositorio y MedicionesDbContext. Cada registro conserva parque,
nodo, panel, valor y fecha UTC. El contexto de Mediciones utiliza la misma base
MySQL y su propio historial de migraciones. La gráfica Radzen se ejecuta en Sensor con muestras en memoria, sin consultas históricas a la base de datos, y sin cambiar las reglas de eliminación
de Parques. Cada pestaña tiene su ciclo independiente.

Los componentes Interactive Server llaman a Aplicación mediante C# e inyección
de dependencias dentro del servidor. Las entidades y contratos de persistencia
pertenecen a Dominio; las implementaciones EF están en Infraestructura.

## Organización

| Ruta | Contenido |
| --- | --- |
| `SolarUy/Components` | Páginas, layouts y navegación del servidor. |
| `SolarUy/Backend/Modulos` | Usuarios, Parques, Tareas y Mediciones, por responsabilidades. |
| `SolarUy/Backend/Infraestructura` | Persistencia existente de Parques. |
| `Sensor/Pages` | Página de Mediciones y páginas de plantilla. |
| `Sensor/Servicios` | Simulación local y cliente SignalR. |
| `Sensor/Contratos/Mediciones` | Mensajes de transporte entre Sensor y el Hub. |
| `SolarUyPruebas/Unitarias/Parques` | Proyecto xUnit existente, incluidas pruebas de Dominio de Mediciones. |
| `Funcionalidades` | Requisitos, diseños, tareas e informes por funcionalidad. |
| `Docs` | Referencias visuales y diagramas de diseño. |

## Ejecución local

Requiere SDK .NET 10, acceso a NuGet, MySQL y Keycloak configurados.
La configuración local de servicios está en [compose.yaml](compose.yaml) y
[.env.example](.env.example). Proporcionar ConnectionStrings:SolarUy y las opciones
Keycloak mediante la configuración del servidor; no guardar credenciales reales
en el repositorio. El procedimiento de base de datos está en [LevantarDB.md](LevantarDB.md).

Desde la raíz:

```powershell
dotnet build SolarUy.sln
dotnet run --project SolarUy/SolarUy.csproj --launch-profile http
```

SolarUy hospeda Sensor; no es necesario iniciar un segundo servidor. En desarrollo,
el arranque aplica las migraciones de Parques y Mediciones. Acceder a Producción
desde el menú tras iniciar sesión como ADMIN.

## Verificación

```powershell
dotnet test SolarUyPruebas/Unitarias/Parques/SolarUyParquesPruebas.csproj
```

Las reglas del proyecto están en [AGENTS.md](AGENTS.md). El diseño, el avance y
las verificaciones de Mediciones están en
[REQ_008_SIMULADOR](Funcionalidades/REQ_008_SIMULADOR/).
