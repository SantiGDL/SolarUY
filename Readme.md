# SolarUY

Prueba de concepto académica del Taller .NET para la gestión de parques solares,
sus paneles, mantenimiento y mediciones. La solución adopta un monolito modular
con los módulos Usuarios, Parques, Tareas y Mediciones.

## Estado actual

- **Home:** presentación en `/` y `/Home`, con tarjetas informativas e imagen
  ilustrativa del futuro dashboard.
- **Panel administrativo:** vista en `/admin` protegida por autenticación,
  con consulta de parques y secciones visuales todavía provisionales.
  Los dos botones «Ingresar al panel» de Home abren esta página;
  Inicio y el logo del panel permiten regresar a Home.
- **Dominio inicial:** clases Usuario, UsuarioTecnico y UsuarioAdmin en Usuarios;
  Parque, Nodo y Panel en Parques; enumerados en `Dominio/Enums/` de sus módulos.
  Medicion y TareaMantenimiento todavía no están implementadas.
- **Sensor:** proyecto separado con páginas de plantilla, sin captura de datos
  ni comunicación con SolarUy implementadas.

El panel requiere autenticación mediante Keycloak y la gestión de parques,
nodos y paneles utiliza MySQL mediante Entity Framework Core. No hay mediciones
reales, monitoreo, reportes ni integración con hardware. Las cifras incluidas
en las imágenes son ilustrativas.

## Proyectos y tecnologías

| Proyecto real | Tecnología | Función actual |
| --- | --- | --- |
| [SolarUy](SolarUy/SolarUy.csproj) | ASP.NET Core Blazor Web App, .NET 10 | Aplicación principal con páginas Razor, layouts, recursos y estructura modular del backend. |
| [Sensor](Sensor/Sensor.csproj) | Blazor WebAssembly, .NET 10 | Base destinada al sensor/simulador, referenciada por SolarUy; conserva contenido de plantilla. |

SolarUy tiene habilitado **Interactive Server**. La interactividad configurada
se ejecuta en el servidor y utiliza SignalR para comunicarse con el navegador.
Home presenta el proyecto; PanelAdmin consulta los parques mediante servicios de aplicación.
Bootstrap y CSS definen la apariencia; los íconos son imágenes locales.

La presentación y el backend de SolarUy pertenecen al mismo proyecto. Los
componentes llamarán a los servicios de aplicación mediante C# e inyección de
dependencias dentro del proceso; no hay proyectos separados de frontend y API
para esta comunicación interna.

Los módulos separan responsabilidades de Dominio, Aplicación e Infraestructura,
con carpetas de Interfaz para contratos de comunicación. Gran parte de esta
estructura está reservada para trabajo futuro. Las reglas vigentes están en
[AGENTS.md](AGENTS.md) y [.agents/Reglas/](.agents/Reglas/).

## Organización del repositorio

```text
SolarUy.sln
SolarUy/
    Components/
        Pages/                 Home, PanelAdmin y páginas de plantilla
        Layout/                HomeLayout, MainLayout y NavMenu
    Backend/
        Modulos/
            Usuarios/
            Parques/
            Tareas/
            Mediciones/
        Infraestructura/
    wwwroot/                   CSS, Bootstrap, imágenes e íconos
Sensor/                        Aplicación WebAssembly independiente
Funcionalidades/               Requisitos, diseños, tareas y verificaciones
Docs/
    Diagramas/                 Diagramas exportados como imágenes
    Iconos/                    Recursos originales de íconos
    IA/                        Prompts e instrucciones históricas
.agents/                       Reglas y flujo de trabajo del agente
AGENTS.md                      Instrucciones vigentes del proyecto
```

Los documentos del desarrollo asistido por IA se agrupan en
[Funcionalidades/](Funcionalidades/), por funcionalidad, con `requirements.md`,
`specs.md` y `tasks.md`. Los prompts e instrucciones anteriores están en
[Docs/IA/](Docs/IA/); son material histórico y pueden describir una arquitectura
anterior. Las instrucciones activas permanecen en AGENTS.md y .agents.

Docs también contiene la letra del taller, el boceto de casos de uso y las
imágenes de referencia. Las carpetas vacías no equivalen a funcionalidades
implementadas y Git no las conserva si no contienen archivos.

## Diagramas de diseño

### Arquitectura prevista

![Diseño de arquitectura de SolarUY](Docs/Diagramas/Arquitectura.png)

El diagrama muestra la dirección del diseño, no el estado completo de la
implementación. La aplicación principal corresponde a `SolarUy/` y el proyecto
WebAssembly a `Sensor/`. Entity Framework Core, MySQL y la autenticación con
Keycloak ya están configurados. Los endpoints externos y la integración con
el inversor siguen pendientes.
El transporte entre Sensor y SolarUy también requiere definición e implementación.

Las próximas versiones del diagrama deben incorporar explícitamente el proveedor
externo de identidad Keycloak, compatible con OpenID Connect. El componente de
consultas mediante lenguaje natural y la estrategia de despliegue para alta
disponibilidad siguen pendientes.

### Modelo de clases propuesto

![Diagrama de clases de SolarUY, versión 2](Docs/Diagramas/Diagrama%20de%20Clases%20v2.png)

Este esquema es una referencia de trabajo. Las clases implementadas se enumeran
en «Estado actual»; el modelo completo, sus relaciones y las automatizaciones
anotadas en el dibujo todavía no están implementados. Los ajustes acordados,
como Imagen de tipo string para guardar una ruta relativa, están documentados
en [ModeloDominioInicial](Funcionalidades/ModeloDominioInicial/specs.md).

## Ejecución local

Requisitos: SDK de .NET 10 para ambos proyectos, acceso a NuGet para restaurar
paquetes y Docker con Compose y soporte para contenedores Linux. En Windows
se puede usar Docker Desktop o Docker Engine con Compose dentro de Ubuntu en
WSL 2. El motor de Docker debe estar iniciado antes de ejecutar Compose.

### Para qué se utiliza Docker

[compose.yaml](compose.yaml) prepara los servicios externos del entorno local:

| Servicio | Función | Puerto local |
| --- | --- | --- |
| MySQL 8.4 | Persistencia de parques, nodos y paneles. | 3306 |
| Keycloak 26.8.1 | Inicio de sesión y roles mediante OpenID Connect. | 8080 |
| BusyBox 1.36 | Preparación de permisos del volumen de Keycloak; termina al completar esa tarea. | Ninguno |

Docker descarga automáticamente las imágenes que falten y crea los contenedores.
No hay que descargarlas manualmente. SolarUy se ejecuta desde Rider o con
`dotnet run`, fuera de los contenedores. No es necesario iniciar Sensor por separado.
Docker facilita la preparación del entorno; también se pueden instalar MySQL y
Keycloak por otros medios y adaptar la configuración de conexión.

MySQL debe estar disponible al arrancar SolarUy: en `Development` se aplican
las migraciones antes de que el servidor comience a escuchar. Keycloak debe
estar disponible para iniciar sesión en las páginas protegidas.

### Preparación inicial

Los siguientes comandos PowerShell se ejecutan desde la raíz del repositorio.
Si se utiliza Docker Engine en WSL 2, los comandos `docker compose` se ejecutan
en Ubuntu, desde la misma carpeta del proyecto; los comandos `dotnet` se ejecutan
en Windows, donde se ejecutará SolarUy.

1. Crear la configuración local, si todavía no existe:

   ```powershell
   Copy-Item .env.example .env
   ```

   Editar `.env` y reemplazar los valores de ejemplo de `MYSQL_ROOT_PASSWORD`,
   `MYSQL_PASSWORD` y `KEYCLOAK_BOOTSTRAP_PASSWORD`. `MYSQL_USER` tiene como valor
   inicial `solaruy_app` y `KEYCLOAK_BOOTSTRAP_ADMIN`, `admin`.
   `.env` está excluido de Git y sus credenciales no deben publicarse.

2. Iniciar los servicios y comprobar su estado:

   ```powershell
   docker compose up -d
   docker compose ps -a
   docker compose logs --tail 50 mysql keycloak
   ```

   Esperar a que MySQL aparezca como `healthy` y a que Keycloak termine de
   iniciar. La finalización exitosa de `preparar-volumen-keycloak` es esperada.
   El realm `solaruy` y el cliente OIDC se importan desde
   [solaruy-realm.json](SolarUyKeycloak/solaruy-realm.json) al crear el entorno.

3. Configurar la conexión de la aplicación mediante User Secrets:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:SolarUy" "Server=localhost;Port=3306;Database=solaruy;User=solaruy_app;Password=REEMPLAZAR_PASSWORD;" --project SolarUy/SolarUy.csproj
   ```

   Sustituir `REEMPLAZAR_PASSWORD` por el valor de `MYSQL_PASSWORD` de `.env`.
   Si se cambió `MYSQL_USER`, ajustar también `User`. SolarUy no lee `.env`
   automáticamente: ese archivo configura Compose, mientras que User Secrets
   configura la aplicación. Como alternativa se puede proporcionar la variable
   de entorno `ConnectionStrings__SolarUy` al proceso de SolarUy.

4. Compilar e iniciar SolarUy, o ejecutarlo en Rider con el perfil `http`:

   ```powershell
   dotnet build SolarUy.sln
   dotnet run --project SolarUy/SolarUy.csproj --launch-profile http
   ```

Abrir `http://localhost:5058` cuando la terminal indique `Now listening on`.
El perfil HTTPS utiliza `https://localhost:7196` y requiere un certificado de
desarrollo confiable. La configuración local de Keycloak ya está en
[appsettings.Development.json](SolarUy/appsettings.Development.json).

Para acceder al panel, crear un usuario en el realm `solaruy` desde la consola
de Keycloak en `http://localhost:8080/admin/`, utilizando las credenciales
administrativas de `.env`. Asignar una contraseña al usuario y el rol de realm
`ADMIN` si necesita realizar operaciones restringidas a administradores. El
administrador de Keycloak no es automáticamente un usuario de SolarUy.
Esta configuración de Keycloak utiliza `start-dev` y está destinada al desarrollo local.

### Uso diario y problemas de arranque

Iniciar los servicios con `docker compose up -d` antes de ejecutar SolarUy.
Al terminar se pueden detener con `docker compose stop`.
Los volúmenes conservan los datos de MySQL y Keycloak entre arranques;
`docker compose down -v` elimina esos volúmenes y sus datos.
Cambiar las contraseñas de `.env` no actualiza las credenciales ya almacenadas
en una base de datos inicializada.

- **`Falta ConnectionStrings:SolarUy`:** completar el paso de User Secrets.
- **Error de conexión a MySQL:** comprobar que Docker esté iniciado, MySQL esté
  `healthy` y que el usuario y la contraseña coincidan con los de la base existente.
- **Error al iniciar sesión:** comprobar que Keycloak esté disponible en el
  puerto 8080 y que el usuario pertenezca al realm `solaruy`.
- **`ERR_CONNECTION_REFUSED` en el navegador:** revisar primero la salida de
  Rider o de `dotnet run`; el servidor puede haberse cerrado antes de escuchar.

## Verificación y pendientes

La compilación comprueba que los proyectos pueden construirse. Todavía no hay
proyectos de pruebas con xUnit ni reglas automatizadas con ArchUnitNET.
SonarQube no está configurado en el repositorio; existe una
[guía de configuración](Docs/ReadmeSonarQube.md). Ejecutar `dotnet test` no realiza
un análisis de SonarQube.

Los resultados y limitaciones de cada tarea se registran en sus documentos:
[Home](Funcionalidades/PaginaPrincipal/tasks.md) y
[panel administrativo](Funcionalidades/PanelAdministrativo/tasks.md).
En el entorno de verificación se detectaron errores locales de Data Protection
con claves DPAPI al abrir las páginas interactivas de plantilla `/counter` y
`/weather`; esa revisión sigue pendiente. Las pantallas Home y panel son
provisionales y no incluyen esas operaciones interactivas.

## ¿Cómo utilizamos la IA?

Durante este Sprint estuvimos probando utilizar ChatGPT Desktop en lugar del CLI,
principalmente por la comodidad que ofrece para adjuntar imágenes y documentos.
Esto resulta especialmente útil con archivos PDF, ya que la extensión de Rider
no permite enviarlos directamente, siendo necesario dejarlos en una carpeta e
indicarle al agente dónde encontrarlos.

Por otro lado, para consultas más generales seguimos utilizando el chat
convencional, ya sea desde la propia aplicación de escritorio o desde la versión
web, dado que cuenta con un contexto más personalizado sobre nuestras
preferencias, como el estilo, formato y nivel de detalle de las respuestas.
