# Diseño propuesto: configuración local

## Herramientas de desarrollo

Crear `Scripts/Configurar-EntornoLocal.ps1`, compatible con Windows PowerShell
5.1 y PowerShell 7 en Windows. Tomar `$PSScriptRoot` como base y localizar la
raíz del repositorio con `..`. Desde allí acceder a `.env`, `.env.example` y
`SolarUy/SolarUy.csproj`, independientemente del directorio desde el que se
invoque el script. `Scripts/` agrupa las automatizaciones del proyecto.
No incorporar dependencias adicionales.

### Responsabilidades y capas

Este cambio corresponde a herramientas de desarrollo externas a los módulos.
No modifica Dominio, Aplicación, Infraestructura ni Presentación de Usuarios,
Parques, Tareas o Mediciones. No hay comunicación con el simulador.
La consola recoge los datos; el script serializa la configuración de Compose
y delega el almacenamiento de la conexión a `dotnet user-secrets`.

## Secuencia

1. Verificar Windows, archivos necesarios, ausencia de `.env`, disponibilidad
   de `dotnet` y SDK de .NET 10. Comprobar el `UserSecretsId` del proyecto. Si
   existe `.env`, avisar que no se puede continuar por seguridad. Indicar las
   dos opciones: eliminarlo manualmente y volver a ejecutar el script, o
   completar la configuración manual siguiendo el README. Salir sin modificar
   `.env` ni User Secrets y sin solicitar credenciales.
2. Explicar los destinos y el alcance: entorno local, sin arrancar servicios.
   Advertir que cambiar `.env` no cambia una base previamente inicializada.
3. Leer de `.env.example` los nombres predeterminados de `MYSQL_USER`
   (`solaruy_app`) y `KEYCLOAK_BOOTSTRAP_ADMIN` (`admin`). Mostrar cada uno
   y preguntar si se quiere cambiar; si la respuesta es no, conservarlo.
   Solicitar un nombre nuevo solamente cuando se responda que sí.
4. Pedir y confirmar las tres contraseñas usando `Read-Host` con texto visible,
   sin enmascaramiento. Reutilizar el usuario y contraseña de la aplicación
   MySQL para construir la conexión de User Secrets, sin volver a pedirlos.
5. Validar todos los datos y solicitar confirmación de escritura, indicando
   que se establecerá la clave de conexión de User Secrets y que puede
   reemplazarse un valor existente, sin imprimir credenciales. Si se cancela,
   salir antes de escribir cualquiera de los destinos.
6. Crear `.env` con semántica de creación exclusiva para evitar sobrescrituras
   incluso si otro proceso lo crea después de la comprobación inicial.
7. Configurar exclusivamente `ConnectionStrings:SolarUy` del proyecto mediante
   `dotnet user-secrets set --project <ruta>`, enviando un objeto JSON por stdin.
   No pasar la conexión como argumento de proceso ni invocar un shell adicional.
8. Comprobar el código de salida y mostrar los resultados y próximos pasos.

## Formatos y validaciones

- `.env`: UTF-8 sin BOM, cinco variables explícitas, sin interpolar los valores
  como código PowerShell. Serializar con comillas simples de Compose para
  conservar `$`, `#`, espacios y comillas dobles; escapar apóstrofes conforme
  al formato de Compose y verificar su lectura real.
- Rechazar saltos de línea, NUL y caracteres de control; permitir contraseñas
  con caracteres especiales sin recortar espacios significativos.
- Validar usuarios con un conjunto sencillo documentado de letras ASCII,
  números y guion bajo; MySQL no permite usar `root` como usuario de aplicación.
  Definir límites compatibles con los servicios al implementar.
- Cadena de conexión: servidor `localhost`, puerto `3306`, base `solaruy`;
  entrecomillar y escapar usuario y contraseña para que `;`, comillas y espacios
  no alteren la estructura. Verificarla con el proveedor MySQL del proyecto.
- JSON de stdin: usar el serializador de PowerShell para escapar la conexión.
  Garantizar UTF-8 al transmitir caracteres no ASCII en ambas versiones de
  PowerShell, sin modificar permanentemente la configuración de la sesión.
- Capturar la salida del comando de secretos sin reproducir diagnósticos que
  pudieran contener credenciales. Informar el fallo con un mensaje propio.
- No listar User Secrets ni leer o registrar credenciales reales en las pruebas.

## Fallos y recuperación

No hay transacción compartida entre el archivo y User Secrets. Crear `.env`
antes de actualizar la clave para que un error al crear el archivo no cambie
User Secrets. Si falla el segundo paso, conservar el archivo y explicar cómo
completar manualmente User Secrets siguiendo el README. No eliminar secretos
preexistentes ni intentar restaurarlos sin conocer su estado anterior.

User Secrets y `.env` almacenan secretos localmente; no son almacenes cifrados.
No generar copias adicionales con credenciales. Mantener `.env` excluido de Git.

## Verificación prevista

Ejecutar el asistente con credenciales ficticias en una copia temporal aislada,
con proyecto y `UserSecretsId` de prueba, sin modificar la configuración real.
Comprobar ingreso y confirmación visibles en consola, conservación o cambio de
los nombres predeterminados y ausencia de pedidos adicionales para User Secrets.
Comprobar también cancelación, archivo existente (mensaje con ambas opciones y
ningún cambio en los dos destinos), requisitos faltantes y fallos de escritura
o del comando `dotnet`.

Validar caracteres especiales y Unicode: comparar valores resueltos por
`docker compose config --format json` con las entradas ficticias sin imprimir
el documento completo; analizar la conexión con el proveedor MySQL y verificar
la clave almacenada solamente en el almacén de prueba. No iniciar contenedores.
Verificar Windows PowerShell 5.1 y PowerShell 7 si están disponibles, declarando
las comprobaciones que queden pendientes.

Compilar `SolarUy.sln` según las reglas del proyecto. Ejecutar las pruebas
existentes aplicables si se encuentran. No hay comportamiento de Dominio nuevo
que requiera pruebas xUnit. ArchUnitNET y SonarQube no están configurados según
el README; confirmar su estado al implementar, sin instalar herramientas.

## Segunda etapa: Linux

Preparar un diseño específico después de validar Windows, conservando las claves,
el comportamiento y los criterios funcionales. No asumir que PowerShell está
instalado en Linux. El intérprete y los requisitos del script Linux quedan
pendientes para esa etapa.
