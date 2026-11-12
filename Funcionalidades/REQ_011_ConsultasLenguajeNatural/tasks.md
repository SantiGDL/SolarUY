# REQ_011 — Plan de implementación: consultas en lenguaje natural

Todas las tareas están pendientes. Este plan no inicia la implementación ni ejecuta cambios de código.

## A) Chat en SolarUy

- [x] **A01 — Definición de permisos y acceso.** Módulo: Usuarios / Seguridad / Presentación. Definir la política de acceso para la ruta /admin/consultas, reutilizando Keycloak y el rol ADMIN. **Criterio de finalización:** el chat queda restringido a usuarios autenticados con el rol correcto; la aplicación no expone la funcionalidad a usuarios no autorizados.
- [x] **A02 — Contrato compartido de consulta.** Módulo: Mediciones / Aplicación. Definir el contrato de uso para listar opciones y consultar el historial con filtros de parque, nodo, panel, período y modo. **Criterio de finalización:** el contrato reutiliza la lógica y restricciones de REQ_010 sin duplicar reglas ni copiar persistencia.
- [x] **A03 — Proveedor configurable de IA.** Módulo: Consultas / Infraestructura. Crear la abstracción del proveedor de IA y registrar la implementación local por defecto con Ollama. **Criterio de finalización:** la configuración del proveedor se realiza por entorno o User Secrets; el chat puede fallar de forma controlada si el servicio no está disponible.
- [x] **A04 — Servicio del chat.** Módulo: Consultas / Aplicación. Implementar el caso de uso que recibe una pregunta, valida si pertenece al sistema, limita llamadas a herramientas y usa solo resultados del historial. **Criterio de finalización:** la lógica rechaza peticiones fuera de tema, detecta intentos de modificación y no inventa valores ni nombres.
- [x] **A05 — Estado y validación de conversación.** Módulo: Consultas / Presentación. Construir la página /admin/consultas con estado de sesión, mensajes, longitud máxima, límite de conversación y manejo de errores. **Criterio de finalización:** la UI mantiene hasta 20 mensajes por conversación, aplica límites de 1000 caracteres y presenta errores claros si falla el proveedor.
- [x] **A06 — Verificación funcional del chat.** Módulo: Consultas / Presentación + Aplicación. Probar preguntas válidas, vacías, fuera de tema, de modificación y con servicio de IA caído. **Criterio de finalización:** los resultados son coherentes con /admin/historial; si no hay datos, se informa la ausencia y no se reemplaza por cero; la aplicación sigue funcionando.

## B) Servidor MCP

- [x] **B01 — Configuración del endpoint /mcp.** Módulo: Consultas / Infraestructura + Presentación. Exponer la ruta /mcp con transporte HTTP Streamable HTTP. **Criterio de finalización:** el endpoint está disponible para clientes MCP y no depende de un modelo ni de una clave del proveedor.
- [x] **B02 — Autenticación y autorización.** Módulo: Usuarios / Seguridad / Infraestructura. Validar JWT Bearer de Keycloak y exigir rol ADMIN antes de ejecutar cualquier herramienta. **Criterio de finalización:** una solicitud sin token, con token vencido o sin rol ADMIN queda rechazada antes de ejecutar herramientas.
- [x] **B03 — Herramientas de solo lectura compartidas.** Módulo: Mediciones + Consultas / Aplicación + Infraestructura. Registrar funciones MCP de listado y consulta histórica reutilizando el mismo contrato del chat. **Criterio de finalización:** las herramientas expuestas son solo lectura, comparten la misma lógica de historial y respetan los avisos, límites y restricciones del sistema.
- [x] **B04 — Seguridad y documentación del cliente.** Módulo: Consultas / Infraestructura + Presentación. Documentar cómo obtener y renovar el token de Keycloak, cómo conecterse con MCP Inspector y cómo listar/ejecutar herramientas. **Criterio de finalización:** el README explica la configuración de conexión, la cabecera Authorization y la limitación del cliente estático.
- [x] **B05 — Verificación funcional del servidor MCP.** Módulo: Consultas + Seguridad. Validar conexión del MCP Inspector, listados de herramientas, ejecución de consultas y rechazo de accesos no autorizados. **Criterio de finalización:** el cliente ejecuta una consulta válida y obtiene resultados consistentes con el chat y con /admin/historial; los accesos no autorizados son bloqueados.

## Verificación final recomendada

- [x] Validar con usuarios ADMIN y no ADMIN en la ruta del chat.
- [x] Validar con token correcto e incorrecto en /mcp.
- [x] Comparar resultados del chat y MCP con el historial y con los datos reales disponibles.
- [x] Revisar los casos de proveedor no activo y de falta de modelo descargado.
- [x] Confirmar que la funcionalidad no modifica ni crea mediciones, tareas, nodos ni paneles.
