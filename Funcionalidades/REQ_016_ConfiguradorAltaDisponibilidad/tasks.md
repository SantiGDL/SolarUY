# REQ_016 — Tareas del configurador HA

Implementación y verificaciones realizadas el 17/11/2026. REQ_015 y los
configuradores locales se conservaron sin cambios.

## Implementación

- [x] **CFG-01 — Validaciones y detección (herramientas de entorno).** Crear
  `Scripts/Configurar-AltaDisponibilidad.ps1` con validación de prerrequisitos,
  detección del proyecto existente, red default y subred IPv4 inequívoca.
  Finalización: no depende de nombres o subredes fijos y falla claramente ante
  ausencia o ambigüedad, sin modificar servicios.
- [x] **CFG-02 — Administración de Keycloak (herramientas de entorno).** Obtener
  credenciales locales en memoria, comprobar disponibilidad, autenticar, localizar
  realm y cliente y agregar solo las entradas faltantes. Preservar las listas y
  atributos existentes y verificar el resultado. Depende de CFG-01.
  Finalización: sin PUT cuando no hay cambios, sin secretos impresos o escritos
  y sin restablecer credenciales si falla la autenticación.
- [x] **CFG-03 — Actualización de .env (herramientas de entorno).** Validar la
  asignación antes de cambios remotos y guardar únicamente HA_PROXY_NETWORK tras
  comprobar Keycloak. Conservar formato y contenido restante; no escribir si el
  valor coincide. Detectar cambios concurrentes y explicar resultados parciales.
  Depende de CFG-01 y CFG-02. Finalización: variable correcta sin duplicados ni
  copias nuevas de credenciales.
- [x] **CFG-04 — Lanzador Windows.** Crear
  `Scripts/Configurar-AltaDisponibilidad.cmd` siguiendo el estilo existente,
  conservar el código de salida y permitir leer el resultado. Depende de CFG-03.
  Finalización: ejecutar mediante doble clic sin depender del directorio actual
  y sin tocar los scripts locales.

## Verificación y documentación

- [x] **CFG-05 — Compatibilidad y casos controlados.** Validar sintaxis y ejecución
  en Windows PowerShell 5.1. Comprobar con datos ficticios entradas ausentes,
  listas múltiples, conservación de atributos, formatos de .env, red ambigua y
  errores de autenticación. Depende de CFG-04. Finalización: resultados registrados
  sin modificar destructivamente el entorno real ni afirmar integración donde
  solamente se usaron respuestas controladas.
- [x] **CFG-06 — Integración e idempotencia.** Ejecutar el configurador dos veces
  sobre la instalación preparada. Comparar internamente .env y cliente antes y
  después, verificar que no se emiten actualizaciones innecesarias y que las
  entradas y atributos previos permanecen. Comprobar HTTP 7000 y contenedores sin
  reiniciarlos. Depende de CFG-05. Finalización: entorno conservado y resultados
  claros, indicando si la adición real quedó pendiente por existir ya las entradas.
- [x] **CFG-07 — Guía de uso.** Actualizar `Docs/AltaDisponibilidad.md` para indicar
  configuración habitual → nuevo configurador → arranque HA. Documentar errores
  y límites. Depende de CFG-06. Finalización: no exigir al compañero obtener la
  subred o agregar manualmente las tres entradas en el recorrido normal.
- [x] **CFG-08 — Cierre y controles del proyecto.** Compilar en Release, ejecutar
  las pruebas de arquitectura existentes y las verificaciones funcionales
  aplicables; revisar diff y registrar resultados en REQ_016. Seguir el
  procedimiento SonarQube existente si hay acceso, o informar el bloqueo.
  Confirmar que los scripts locales y documentos de REQ_015 están intactos y que
  no cambió código de negocio, Compose o Nginx. Depende de CFG-07.
  Finalización: resumen de archivos y resultados, sin commit ni push.

## Resultados — 17/11/2026

### Integración real en la instalación existente

- Ejecutado con Windows PowerShell 5.1 real y finalización correcta. El lanzador
  .cmd también funcionó desde un directorio diferente de la raíz, con entrada
  vacía para cerrar la pausa; no se realizó una prueba gráfica de doble clic.
- Detectados el proyecto, la red default y su subred sin nombres o valores fijos.
- La instalación ya contenía HA_PROXY_NETWORK y las tres entradas de Keycloak.
  Las ejecuciones repetidas informaron que no era necesario escribir.
- Comparación en memoria antes y después de dos ejecuciones independientes:
  representación completa del cliente idéntica; hash y fecha de .env idénticos;
  identificadores y StartedAt de los contenedores conservados. No se guardaron
  credenciales ni tokens en archivos de comprobación.
- SolarUY respondió HTTP 200 en localhost:7000. Ambas instancias permanecieron
  healthy. No se reiniciaron servicios ni se recrearon contenedores o volúmenes.
- Hashes de configuradores locales, documentos de REQ_015, Compose y Nginx
  conservados. No se modificó código C# ni lógica de negocio.

### Comprobaciones aisladas con datos ficticios

- Entorno temporal con Docker y API Keycloak sustituidos por respuestas
  controladas, ejecutando el mismo script con Windows PowerShell 5.1.
- Adición de las tres entradas faltantes, conservación de callbacks, múltiples
  destinos logout, PKCE y otro atributo ajeno; un solo PUT inicial y ninguno en
  la segunda ejecución. Listas previamente vacías no agregan valores nulos.
- Escritura de HA_PROXY_NETWORK ausente y sustitución de valor diferente,
  conservando variables, comentarios y saltos CRLF. Valores entre comillas
  reconocidos sin reescritura; comprobados UTF-8 sin BOM, UTF-8 con BOM y UTF-16.
- Rechazos seguros por .env ausente, asignación duplicada, múltiples subredes
  IPv4 y autenticación administrativa fallida: código 1, sin PUT ni cambios de
  .env. No se detuvo Docker o Keycloak para simular errores.
- Los archivos auxiliares de estas comprobaciones contienen únicamente datos
  ficticios y quedan en .local-ha, excluido de Git.

### Controles del proyecto y límites

- Compilación Release: correcta, cero errores y tres avisos NU1903 preexistentes
  de Microsoft.AspNetCore.Components.WebAssembly.Server 10.0.13.
- Arquitectura: 22/22 aprobadas; funcionales existentes: 113/113 aprobadas.
  ArchUnitNET comprueba la arquitectura C# existente, no la lógica PowerShell.
- SonarQube/Quality Gate pendiente: no hay token disponible. No se instalaron
  herramientas ni se cambiaron reglas de análisis.
- La adición de entradas se probó con respuestas controladas, no eliminando
  entradas del Keycloak real. Queda pendiente verificar esa ruta en otra
  instalación real que todavía no tenga HA; no se afirma equivalencia entre
  esa integración y la prueba aislada.
- No se repitió login interactivo en Brave ni pruebas de caída: no se cambió el
  despliegue o la autenticación de SolarUY y no se autorizaron reinicios para
  este requisito. La disponibilidad se comprobó mediante HTTP y estado Docker.
- No se realizó commit ni push.