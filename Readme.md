# SolarUY

Prueba de concepto académica del Taller .NET para gestionar parques solares, nodos,
paneles y mediciones simuladas. La solución es un monolito modular con Usuarios,
Parques, Tareas y Mediciones.

## Funcionalidades

- Home en `/` y panel administrativo en `/admin`, con menú lateral y barra superior.
- Gestión de parques, nodos y paneles, con persistencia MySQL mediante Entity Framework Core.
- Autenticación con Keycloak/OpenID Connect y autorización por roles en las operaciones protegidas.
- Producción en `/admin/mediciones`, disponible para administradores autenticados.
  Al abrirla se seleccionan el primer parque y nodo con paneles y se inicia
  automáticamente el simulador. Cada 5 segundos genera en el navegador una
  lectura por panel habilitado, entre 0 y su PotenciaActual, en W. La pantalla
  muestra el total del nodo o un panel individual y una gráfica con las muestras
  guardadas durante la sesión. Los cambios de parque o nodo se confirman con
  Conectar. Los selectores de nodo y panel actualizan sus opciones inmediatamente
  al preparar otro parque o nodo, mientras los datos conservan la selección activa.
  Cambiar el panel del nodo activo actualiza la vista inmediatamente.
- Habilitación y deshabilitación persistente desde el detalle del panel. Un panel
  deshabilitado permanece visible con generación actual cero. Al habilitarlo
  vuelve a generar desde el próximo ciclo, sin Conectar ni borrar su historial.

Los valores de producción son simulados; no provienen de hardware real.

## Arquitectura y comunicación

SolarUy es una Blazor Web App .NET 10 con páginas Interactive Server. Sensor es
el proyecto WebAssembly .NET 10 hospedado por SolarUy y ejecuta únicamente el
motor no visual de simulación. Producción compone la pantalla Interactive Server
y el motor WebAssembly como raíces hermanas dentro de la misma página.

La pantalla consulta el catálogo mediante Aplicación de Mediciones y el contrato
local C# de Parques. El servidor envía órdenes dirigidas al motor de cada sesión
mediante SignalR en `/hubs/mediciones`. Sensor confirma la configuración y envía
las lecturas al Hub, que delega su validación y guardado a Aplicación de Mediciones
mediante su repositorio y MedicionesDbContext. Cada registro conserva parque,
nodo, panel, valor y fecha UTC. El contexto de Mediciones utiliza la misma base
MySQL y su propio historial de migraciones. Las versiones de configuración y de
habilitación permiten descartar lecturas atrasadas. La gráfica Radzen se ejecuta
en SolarUy con muestras de sesión en memoria, sin consultas históricas a la base
de datos. Cada pestaña tiene su ciclo independiente; al salir se libera su motor.

Los componentes Interactive Server llaman a Aplicación mediante C# e inyección
de dependencias dentro del servidor. Las entidades y contratos de persistencia
pertenecen a Dominio; las implementaciones EF están en Infraestructura.

## Organización

| Ruta | Contenido |
| --- | --- |
| `SolarUy/Components` | Páginas, layouts y navegación del servidor. |
| `SolarUy/Backend/Modulos` | Usuarios, Parques, Tareas y Mediciones, por responsabilidades. |
| `SolarUy/Backend/Infraestructura` | Persistencia existente de Parques. |
| `Sensor/Components` | Motor WebAssembly no visual de Producción. |
| `Sensor/Pages` | Página de plantilla Counter. |
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
