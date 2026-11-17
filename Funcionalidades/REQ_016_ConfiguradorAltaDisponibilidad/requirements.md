# REQ_016 — Configurador de alta disponibilidad

## Objetivo

Se necesita un configurador independiente para preparar la alta disponibilidad
de SolarUY en `http://localhost:7000`. Un compañero que ya tenga configurado el
entorno habitual deberá poder ejecutarlo sin obtener manualmente la subred de
Docker ni editar las direcciones del cliente de Keycloak.

Se conservarán sin cambios `Scripts/Configurar-EntornoLocal.cmd` y
`Scripts/Configurar-EntornoLocal.ps1`. El despliegue seguirá utilizando la
arquitectura activo-pasivo definida en REQ_015.

## Archivos y ejecución

- Se crearán `Scripts/Configurar-AltaDisponibilidad.cmd` y
  `Scripts/Configurar-AltaDisponibilidad.ps1`.
- El `.cmd` será un lanzador sencillo de PowerShell, siguiendo el estilo del
  configurador local existente.
- El configurador será compatible con Windows PowerShell 5.1 y no requerirá
  dependencias adicionales a las del entorno habitual.
- Preparará la configuración sin levantar, detener ni recrear servicios.
  El despliegue HA se iniciará después mediante Docker Compose.

## Configuración de la red

Se detectará automáticamente la red utilizada por el proyecto Compose existente
y se obtendrá su subred para guardar o actualizar `HA_PROXY_NETWORK` en `.env`.
No se utilizará una subred fija ni se asumirá un nombre de proyecto idéntico en
todas las computadoras.

Las demás variables, comentarios y credenciales de `.env` deberán conservarse.
Si el valor ya es correcto, no se reescribirá el archivo. Si no se puede
identificar una red válida de forma inequívoca, se informará cómo resolverlo
sin modificar la configuración ni crear otra red o proyecto.

## Configuración de Keycloak

Se comprobará que Keycloak esté disponible y que existan el realm `solaruy` y
su cliente `solaruy`. Mediante la administración de Keycloak se agregarán
únicamente las entradas que falten:

| Configuración | Entrada |
| --- | --- |
| Redirect URI | `http://localhost:7000/signin-oidc` |
| Web origin | `http://localhost:7000` |
| Post logout redirect URI | `http://localhost:7000/*` |

Se conservarán todas las entradas anteriores, incluidos los callbacks de Rider,
los atributos del cliente y las demás configuraciones. No se reemplazará el
realm ni se modificarán usuarios, roles o contraseñas. Si las tres entradas ya
existen, no se actualizará el cliente.

La autenticación administrativa reutilizará de forma segura las credenciales
locales existentes. No se imprimirán credenciales ni tokens, ni se guardarán en
nuevos archivos. Si las credenciales iniciales ya no permiten administrar
Keycloak, se informará el problema y los pasos para continuar sin restablecerlas.

## Restricciones y errores

- No borrar ni recrear volúmenes, reinicializar servicios o modificar datos de
  MySQL y Keycloak ajenos a las entradas del cliente indicadas.
- No modificar Nginx, la arquitectura activo-pasivo, la lógica de negocio ni las
  validaciones OIDC o protecciones de ASP.NET Core.
- Mantener los documentos de REQ_015 sin modificaciones. Documentar esta
  funcionalidad en REQ_016 y actualizar `Docs/AltaDisponibilidad.md` con las
  instrucciones del nuevo configurador.
- Si falta `.env`, Docker no está disponible o Keycloak no está preparado,
  mostrar instrucciones claras y finalizar con un resultado de error.
- Informar qué pasos se completaron y cuáles quedaron pendientes si ocurre un
  error, sin mostrar información sensible.
- Mantener los cambios acotados y no interrumpir el despliegue existente.

## Criterios de aceptación

1. Ejecutar el nuevo `.cmd` en un entorno habitual ya preparado y detectar la
   red del proyecto sin comandos manuales ni una subred predeterminada.
2. Guardar el valor correcto de `HA_PROXY_NETWORK`, conservando el resto de
   `.env` y el funcionamiento del configurador local.
3. Comprobar que el cliente de Keycloak contiene las tres entradas HA y mantiene
   las entradas anteriores y sus atributos de seguridad.
4. Ejecutar nuevamente el configurador sin duplicar entradas, reescribir `.env`
   ni actualizar innecesariamente el cliente.
5. Verificar la ejecución con Windows PowerShell 5.1 y mensajes claros ante
   prerrequisitos faltantes o fallos de autenticación administrativa.
6. Comprobar que el despliegue existente sigue disponible en
   `http://localhost:7000`, sin detener servicios ni alterar sus volúmenes.
7. Registrar las verificaciones realizadas y sus límites. No borrar ni alterar
   destructivamente el entorno actual para simular una instalación nueva.
