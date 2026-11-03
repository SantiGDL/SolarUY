# Configuración local interactiva

## Objetivo

Preparar las credenciales del entorno local mediante un asistente que solicite
los datos una sola vez y configure tanto Docker Compose como SolarUy.

## Alcance solicitado

- Primera etapa: script interactivo para Windows.
- Segunda etapa: equivalente para Linux, después de completar Windows.
- Crear el `.env` de la raíz y configurar `ConnectionStrings:SolarUy` mediante
  User Secrets del proyecto `SolarUy/SolarUy.csproj`.
- Pedir las contraseñas de root de MySQL, del usuario de la aplicación y del
  administrador de Keycloak. Mostrar los nombres de usuario predeterminados de
  `.env.example` (`solaruy_app` y `admin`) y preguntar si se quieren cambiar.
  Si la respuesta es no, conservar esos valores sin pedir que se reingresen.
- Usar la misma contraseña y usuario de MySQL en ambos destinos. Generar la
  conexión de User Secrets a partir de los datos ya ingresados, sin volver a
  pedir credenciales. La conexión utiliza el usuario de la aplicación, no root
  ni el administrador de Keycloak.

## Comportamiento propuesto para revisión

- Mostrar las contraseñas en texto visible durante su ingreso, sin puntos ni
  asteriscos, y solicitar confirmación para
  detectar errores de escritura; rechazar contraseñas vacías.
- Comprobar los requisitos antes de escribir configuración.
- Si ya existe `.env`, avisar que el script no puede continuar por seguridad y
  terminar sin modificar `.env` ni User Secrets. Explicar las dos opciones:
  eliminar `.env` manualmente y volver a ejecutar el script, o completar la
  configuración manual siguiendo el README. El script no elimina ni sobrescribe
  `.env`.
- Informar que la operación configura la conexión local de User Secrets y puede
  reemplazar el valor existente de esa clave. Pedir confirmación antes de escribir.
- La visibilidad de contraseñas se limita al ingreso interactivo; no repetirlas
  en resúmenes ni incluirlas en mensajes de diagnóstico.
- Informar por separado los resultados de `.env` y de User Secrets; no anunciar
  éxito si alguno falla.

## Criterios de aceptación

1. En Windows, el script está en `Scripts/` y se puede invocar desde cualquier
   directorio. Localiza la raíz del repositorio una carpeta arriba de la suya
   para acceder a `.env`, `.env.example` y `SolarUy/SolarUy.csproj`.
2. Al completar el asistente, `.env` contiene las cinco variables de
   `.env.example` y la conexión de User Secrets usa `localhost`, puerto `3306`,
   base `solaruy` y las credenciales ingresadas para la aplicación.
3. Los caracteres especiales admitidos conservan su valor en Compose y en la
   cadena de conexión; las entradas no admitidas se rechazan antes de escribir.
4. Las contraseñas y su confirmación se escriben en texto visible. Una
   confirmación incorrecta permite volver a ingresarlas. Los nombres
   predeterminados se muestran antes de preguntar si se quieren cambiar;
   responder que no conserva los valores de `.env.example`.
5. La falta del SDK requerido o de los archivos del proyecto produce un mensaje
   claro sin crear `.env` ni modificar User Secrets.
6. Si existe `.env`, el script explica por qué no puede continuar, indica las
   dos opciones manuales y termina sin modificar `.env` ni User Secrets.
   Cancelar antes de guardar tampoco altera ninguno de los destinos.
7. Un fallo de User Secrets no publica credenciales ni se presenta como éxito;
   si ya se creó `.env`, se informa ese estado y la recuperación manual.
8. El README explica cómo ejecutar el asistente una vez implementado y conserva
   el procedimiento manual como alternativa.

## Fuera de alcance

- Iniciar Docker, instalar herramientas o ejecutar SolarUy desde el asistente.
- Cambiar credenciales en volúmenes ya inicializados, borrar datos o volúmenes.
- Hacer que SolarUy cargue `.env` o cambiar su mecanismo de configuración.
- Configuración de producción y cambios en los módulos de negocio.
- Implementación de Linux en la primera etapa.

## Estado

Requisitos redactados a partir de la solicitud. Las políticas de confirmación,
validación y tratamiento de archivos existentes son propuestas para revisión.
