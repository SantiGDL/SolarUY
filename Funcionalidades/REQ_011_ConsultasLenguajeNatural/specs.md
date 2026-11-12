# REQ_011 — Especificación técnica: Consultas en lenguaje natural

## Estado y alcance

Este documento define la solución propuesta para la funcionalidad de consultas en lenguaje natural sobre el historial de mediciones. Está basado en [requirements.md](requirements.md) y en las reglas de arquitectura y calidad del proyecto.

La funcionalidad tiene dos accesos de consulta: un chat dentro de SolarUy y un servidor MCP externo con transporte HTTP. Ambos comparten el mismo conjunto de herramientas de solo lectura y reutilizan el histórico definido en REQ_010 sin duplicar reglas ni introducir nuevas escrituras.

No se implementa ni se autoriza código en esta fase. Las decisiones aquí descritas son un diseño orientativo para revisión previa a la implementación.

## 1. Vista general de la solución

La funcionalidad se estructura en dos canales de acceso a la misma lógica de negocio:

- Chat en aplicación: en la ruta /admin/consultas, con un administrador autenticado.
- Servidor MCP: en la ruta /mcp, protegido por JWT Bearer de Keycloak y con rol ADMIN.

Ambos canales consumen un único conjunto de herramientas de consulta que se resuelven sobre el historial de mediciones ya existente. El flujo de consulta es sempre de lectura:

1. El usuario formula una pregunta en español.
2. El canal de entrada (chat o MCP) valida autenticación y rol.
3. El cliente IA o el modelo conectado invoca herramientas de consulta.
4. La herramienta devuelve filas y metadatos del historial en formato estructurado.
5. El asistente o el cliente interpreta ese resultado y responde en español.
6. No se crean, modifican ni eliminan mediciones ni tareas.

## 2. Módulo Usuarios y seguridad

### Dominio
- Reutilizar la identidad actual de la aplicación y el rol ADMIN definido por Keycloak.
- Mantener la política de autenticación sin introducir nuevos roles ni nuevas credenciales.
- No incorporar reglas de negocio específicas del chat en Dominio de Usuarios.

### Aplicación
- Definir una abstracción de acceso para verificar permisos de consulta en ambos accesos.
- Reutilizar el mismo criterio de autorización para chat y MCP: únicamente usuarios ADMIN.
- Mantener la validación en la capa de aplicación, sin confiar en la UI para conceder acceso.

### Infraestructura
- Reutilizar el mecanismo de autenticación y tokens actuales de la aplicación.
- Para MCP, validar el token Bearer en Keycloak antes de ejecutar cualquier herramienta.
- No almacenar secretos o tokens en repositorio, ni en archivos versionados.

### Presentación
- El chat solo se ofrece a usuarios autenticados con rol ADMIN.
- La página /admin/consultas usa el mismo patrón de autoría y layout que el resto del sistema.
- El servidor MCP no define UI; su protección y respuesta se gestionan en transporte HTTP.

## 3. Módulo de Mediciones y histórico

### Dominio
- Reutilizar las entidades y reglas de `REQ_010` sobre mediciones, períodos, puntos de consulta y valores en W.
- Mantener la consulta como operación de solo lectura.
- No redefinir el dominio del historial con modelos específicos de IA ni de MCP.

### Aplicación
- Definir un contrato de consulta compartido para las herramientas de lenguaje natural.
- Reutilizar la lógica y restricciones del historial: Parque, Nodo, Panel, período y modo.
- Definir tipos de entrada y salida independientes de Blazor, EF, HTTP y proveedores de IA.
- Mantener la política de resultados con aviso de cobertura del historial, límite máximo de 100 filas y recorte informado.

### Infraestructura
- Resolver las consultas sobre el repositorio histórico ya existente.
- Evitar duplicación de reglas ni consultas paralelas con lógica distinta.
- Mantener el acceso a datos desacoplado del modelo de IA y del transporte MCP.

### Presentación
- La consulta por lenguaje natural debe consumir el mismo historial de resultados que la pantalla de historial.
- Las pantallas de consulta no inventan valores ni sustituyen la validación del backend.

## 4. Módulo de consultas en lenguaje natural (chat)

### Dominio
- No requiere nuevas entidades de negocio persistentes.
- La conversación del usuario se modela como estado efímero de presentación y sesión.
- Las reglas de negocio relevantes pertenecen a la validación de consultas y a la política de solo lectura.

### Aplicación
- Introducir un servicio coordinador para atender la conversación del chat.
- Este servicio debe:
  - validar si la pregunta está dentro del sistema,
  - detectar peticiones de modificación o datos ajenos,
  - preparar la herramienta adecuada para el modelo,
  - ejecutar máximo 6 llamadas a herramientas por pregunta,
  - aplicar límites de mensajes y longitud,
  - devolver una respuesta en español basada únicamente en el resultado de las herramientas.
- Definir un adaptador al proveedor de IA configurable, con interfaz de dependencias.
- El proveedor no debe ser una dependencia concreta del resto de la aplicación.

### Infraestructura
- Proveedor por defecto: Ollama, local y sin costear salidas a Internet.
- Proveedor opcional: Anthropic o cualquier modelo externo configurable, si el equipo lo habilita.
- Mantener la configuración en User Secrets, variables de entorno o appsettings de desarrollo.
- Si el proveedor no está disponible, no bloquear la aplicación: se registra el fallo y el chat responde con un mensaje claro.
- No versionar secretos ni claves en el repositorio.

### Presentación
- Crear la página /admin/consultas con Blazor Interactive Server.
- Mantener la conversación en memoria mientras la página permanece abierta.
- Limitar:
  - 20 mensajes por conversación,
  - 1000 caracteres por mensaje,
  - 6 llamadas a herramientas por pregunta.
- Mostrar mensajes de error claros cuando el proveedor no responde o no hay modelo disponible.
- Mostrar la respuesta con formato legible y conservar el mismo idioma del sistema: español.

## 5. Módulo del servidor MCP

### Dominio
- No requiere entidad ni agregado de negocio.
- Se definen contratos de herramienta y validación de permisos, pero no se introducen reglas de dominio nuevas.

### Aplicación
- Exponer el mismo contrato de consulta utilizado por el chat.
- Las herramientas pueden listar opciones y consultar el historial, pero no modifican datos.
- Las descripciones de cada herramienta deben ser claras y orientadas a los clientes MCP.
- Las herramientas deben devolver errores comprensibles y no fallar por parámetros de entrada ambiguos.

### Infraestructura
- Exponer el endpoint /mcp usando transporte HTTP Streamable HTTP.
- Aplicar autenticación por Bearer token y validación de rol ADMIN en Keycloak antes de ejecutar cualquier herramienta.
- No depender de un modelo concreto ni de una clave de proveedor para que el cliente pueda listar y ejecutar herramientas directamente.
- Las herramientas de MCP son un conjunto compartido con el chat; la implementación es la misma lógica de consulta, no una segunda versión del historial.

### Presentación
- El servidor MCP no tiene UI; su cliente de referencia es MCP Inspector, con Node.js.
- Está diseñado para consumo por herramientas externas, no por navegadores.
- La documentación debe incluir cómo obtener un token válido, renovarlo y utilizarlo en MCP Inspector.

## 6. Comunicación entre módulos y con el simulador

- El módulo de IA/consulta no modifica ni invoca al simulador ni a los sensores.
- La lógica de consulta reutiliza únicamente el historial de mediciones y las operaciones de lectura definidas en REQ_010.
- El chat y MCP comparten la misma capa de servicios y herramientas, evitando duplicación de lógica y de validaciones.
- La interpretación final de datos la hace el modelo o el cliente que conecta al servidor, nunca la herramienta por sí misma.

## 7. Seguridad y configuración

- El proveedor de IA es configurable por User Secrets, entorno o appsettings de desarrollo.
- Las claves de proveedor, si se usan, no se versionan ni se incluyen en artefactos del repositorio.
- El token de Keycloak para MCP también se maneja como secreto y no se versiona.
- Con modelo local, las consultas y resultados no salen de la máquina.
- Con un proveedor externo, la aplicación documenta el alcance y la responsabilidad de compartir datos de producción con un servicio remoto.
- El cliente MCP se configura con una cabecera Authorization fija, sin flujo OAuth interactivo.

## 8. Reglas de negocio clave

1. El usuario debe ser ADMIN para usar el chat o el servidor MCP.
2. La respuesta solo puede usar datos obtenidos del historial; no se inventan cifras ni nombres.
3. La funcionalidad es de consulta y solo lectura; no permite crear, actualizar ni borrar entidades.
4. Si no hay datos para el período, el sistema informa la ausencia sin convertirla en cero.
5. Si la pregunta es ajena al sistema o pide modificar datos, el sistema responde negativamente de forma breve y cortés.
6. Si el proveedor falla o no está disponible, la aplicación sigue funcionando y muestra un error claro.
7. Los resultados se limitan a 100 filas con indicación cuando se recortan.
8. El historial se consulta con las mismas restricciones y unidades del módulo de mediciones.

## 9. Criterios de validación esperados

- Un ADMIN puede consultar un período y obtener una respuesta coherente con /admin/historial.
- Un usuario sin rol ADMIN queda bloqueado en chat y en MCP.
- El chat y MCP devuelven la misma respuesta auxiliar cuando la selección es inválida o el período no tiene datos.
- Si el proveedor no está activo, la app no cae y informa claramente el problema.
- Una herramienta MCP sin token o con token inválido es rechazada antes de ejecutar.
- Las consultas no insertan ni modifican mediciones.

## 10. Dependencias técnica relevantes

- Blazor Web App con Interactive Server.
- Keycloak para autenticación y autorización.
- Repositorio y servicio del historial de mediciones de REQ_010.
- Proveedor de IA configurable, con Ollama como opción local recomendada.
- MCP Inspector como cliente de referencia para pruebas manuales.

## 11. Riesgos y limitaciones conocidas

- Un modelo local pequeño puede ser menos preciso al redactar, por lo que la solución exige que las cifras vengan de herramientas y no de inferencias libres.
- El servidor MCP exige token estático y no un flujo OAuth interactivo.
- Los conectores de nube de terceros no se implementan por no ser compatibles con el alcance del requisito.
- Las decisiones de agregación y resumen quedan sujetas a la implementación final de REQ_010, y el diseño actual evita redefinirlas en esta funcionalidad.

## 12. Resultado esperado

El resultado final de esta funcionalidad será una experiencia de consulta inteligente y segura sobre el histórico de mediciones, accesible tanto desde la propia aplicación como desde clientes externos compatibles con MCP, sin afectar la integridad ni la operación normal del sistema.
