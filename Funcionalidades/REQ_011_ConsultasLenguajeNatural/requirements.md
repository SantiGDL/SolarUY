# REQ_011 — Consultas en lenguaje natural sobre el histórico (chat y servidor MCP)

## Objetivo
Permitir consultar el histórico de mediciones con preguntas en español,
por ejemplo "¿cómo estuvo la generación en las últimas 24 horas?", desde
un chat dentro de SolarUy y desde un cliente MCP externo.
Cumple el requerimiento no funcional de lenguaje natural de la letra, que
aplica solo a funcionalidades de consulta de información.
La solución no requiere servicios de pago ni claves de terceros.

## Decisiones de tecnología
- Proveedor del modelo del chat: configurable. Por defecto, un modelo local
  mediante Ollama, sin costo, sin clave y sin enviar datos fuera de la máquina.
- El modelo local debe soportar llamadas a herramientas. Se documenta un modelo
  recomendado y los requisitos de hardware aproximados.
- Alternativa opcional: la API de Anthropic, seleccionable solo por
  configuración. No se necesita para cumplir este requisito.
- El código de la aplicación no depende de un proveedor concreto: cambiar de
  proveedor modifica únicamente la configuración y el registro de dependencias.
- Cliente MCP de referencia (obligatorio): MCP Inspector, en su modo gráfico
  o de línea de comandos, que requiere Node.js.
- Cliente MCP adicional (opcional): Claude Code, si se dispone de cuenta.
- Fuera de alcance: los conectores personalizados de Claude (claude.ai y
  Desktop) y el conector MCP de la Messages API. Ambos realizan la llamada
  desde la nube de Anthropic, por lo que exigen una URL pública con HTTPS
  y autenticación OAuth. Se documentan como limitación.

## Alcance
Incluye:
1. Un chat en SolarUy, en la ruta /admin/consultas.
2. Un servidor MCP expuesto por SolarUy en la ruta /mcp con transporte HTTP.
3. Un único conjunto de herramientas de solo lectura, compartido por ambos
   accesos, que reutiliza el historial de REQ_010 sin duplicar sus reglas.

Fuera de alcance: crear, modificar o eliminar datos; estado de paneles y tareas
(ampliación futura mediante nuevas herramientas); interacción con el inversor
o el sensor; guardar conversaciones; exportar respuestas; exponer el servidor
MCP en una URL pública.

## Herramientas de consulta
- **Listar opciones:** parques, nodos y paneles con datos históricos, con sus
  identificadores y nombres.
- **Consultar histórico:** por parque, con nodo y panel opcionales, un período
  (todos los datos, últimas 24 horas, últimos 30 días, últimos 365 días) y un
  modo (promedio, diario o crudos), con las mismas restricciones de REQ_010.
- Los resultados indican selección, período, modo, unidad (W) y el aviso de
  cobertura del historial.
- Se devuelve un máximo de 100 filas e indica cuándo hubo recorte.
- Los errores esperados (período inválido, selección inexistente) se devuelven
  como mensaje comprensible, no como falla del servidor.

## Chat en SolarUy
- Solo usuarios autenticados con rol ADMIN, igual que el historial.
- Conversación de varios turnos mantenida solo mientras la página está abierta.
- Máximo 20 mensajes por conversación, 1000 caracteres por mensaje y 6 llamadas
  a herramientas por pregunta.
- El asistente responde en español, usa solo datos devueltos por las
  herramientas y no inventa cifras, parques, nodos ni paneles.
- Valores en W; un promedio es potencia media por panel, no energía (kWh)
  ni potencia total del parque.
- Los períodos son móviles; "hoy" se interpreta como las últimas 24 horas
  y el asistente lo aclara.
- Un período sin datos no se presenta como generación cero.
- Las preguntas ajenas al sistema reciben una negativa breve y cortés.
- Los pedidos de modificar datos se rechazan: el asistente solo consulta.
- Un modelo local pequeño puede ser menos preciso al redactar; por eso las
  cifras provienen siempre de las herramientas y se contrastan con
  /admin/historial.
- Si el proveedor configurado no está disponible (por ejemplo, Ollama apagado
  o el modelo no descargado) o falla, el chat muestra un mensaje claro y el
  resto de la aplicación sigue funcionando.

## Servidor MCP externo
- Endpoint en /mcp, con transporte HTTP (Streamable HTTP).
- Expone las mismas herramientas de consulta, con descripciones claras.
- Exige un token Bearer emitido por Keycloak, con el rol ADMIN. Sin token,
  con token inválido o sin el rol, se rechaza la solicitud antes de ejecutar
  cualquier herramienta.
- Solo lectura: no expone herramientas, recursos ni prompts que modifiquen datos.
- Aplica las mismas autorizaciones, límites y avisos que el chat.
- No depende de ningún modelo ni clave: las herramientas se pueden listar y
  ejecutar directamente desde el cliente, y el modelo, si lo hay, lo aporta
  quien se conecta.
- El cliente se configura con un token estático en la cabecera Authorization,
  sin flujo OAuth interactivo.
- Se documenta en el README cómo obtener el token y su duración, cómo conectar
  el MCP Inspector, cómo listar y ejecutar herramientas, y cómo agregar el
  servidor en Claude Code si se dispone de cuenta.

## Keycloak
- Se define en el realm versionado la forma de obtener tokens para un cliente
  de línea de comandos. El cliente actual de SolarUy tiene deshabilitado el
  acceso directo, por lo que no sirve para ese fin.
- Los tokens son de corta duración; el README indica cómo renovarlos.
- El token debe incluir el rol ADMIN con el mismo formato que usa el login
  actual.
- Los cambios en el realm se documentan y no debilitan el login existente.

## Configuración y seguridad
- Proveedor, modelo y dirección del servicio de IA se configuran mediante
  User Secrets, variables de entorno o configuración de desarrollo sin secretos.
- Si se usa un proveedor con clave, esta nunca se versiona ni se incluye en
  archivos entregados. Los tokens de Keycloak tampoco.
- Con el proveedor local, las consultas y resultados no salen de la máquina.
  Con un proveedor externo opcional, pueden incluir valores de producción y
  nombres de parques; se documenta en la entrega.
- Los datos devueltos por las herramientas se tratan como datos, no como
  instrucciones para el modelo.

## Criterios de aceptación
1. Un ADMIN pregunta por un período y el chat responde con cifras coherentes
   con la pantalla de historial.
2. Un usuario sin rol ADMIN no puede usar el chat ni su caso de uso.
3. Si no hay datos en el período, el chat lo informa y no los reemplaza por cero.
4. Una pregunta ajena al sistema y un pedido de modificar datos son rechazados
   con cortesía.
5. Con el servicio de IA apagado o sin el modelo descargado, la aplicación
   arranca y el chat avisa.
6. Si el proveedor falla, se muestra un error comprensible sin caer la aplicación.
7. Con un token válido de un ADMIN, el MCP Inspector se conecta a /mcp por
   Streamable HTTP, lista las herramientas, ejecuta una consulta y obtiene los
   mismos resultados que el chat.
8. Una solicitud a /mcp sin token, con token vencido o sin rol ADMIN es rechazada.
9. Ninguna consulta, por el chat o por MCP, genera ni modifica mediciones.
10. La respuesta de una consulta acotada respeta el límite de 100 filas y
    señala el recorte.
11. El servidor MCP funciona sin ningún modelo ni clave configurados.
12. El README explica la instalación de Ollama y del modelo, cómo obtener el
    token y cómo conectar el MCP Inspector.
13. El login con cookie de la aplicación sigue funcionando sin cambios.
14. La solución compila y las pruebas existentes pasan.
15. (Opcional) Con una cuenta disponible, Claude Code se conecta a /mcp con el
    mismo token y obtiene los mismos resultados.

## Verificación
Comparar respuestas del chat y del Inspector con /admin/historial; probar
los casos negativos (sin token, token vencido, sin rol, servicio de IA apagado,
fuera de tema, pedido de modificación); ejecutar build, pruebas existentes y
análisis de SonarQube sobre el código nuevo.