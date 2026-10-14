# Verificación y controles — SolarUY

## Comprobaciones al implementar

- Ejecutar dotnet build desde la carpeta de la solución
  al completar un conjunto coherente de cambios de código.
- Ejecutar las pruebas pertinentes para el comportamiento modificado.
- Antes de finalizar, ejecutar dotnet test sobre la solución
  cuando existan proyectos de pruebas.
- No ejecutar controles de código por cambios exclusivamente documentales,
  salvo que afecten comandos o configuración que deban comprobarse.

## Pruebas de comportamiento

- Incorporar pruebas para reglas de dominio nuevas o modificadas.
- Cubrir casos válidos, límites y errores relevantes para los requisitos.
- No crear pruebas que solo repitan la implementación.
- Utilizar el framework de pruebas acordado por el equipo.
- Incorporar pruebas de otras capas cuando sean necesarias para verificar
  criterios de aceptación, permisos, contratos o integraciones.
- No introducir herramientas de pruebas nuevas sin justificarlo.

## Pruebas de arquitectura

- Cuando ArchUnitNET esté configurado, ejecutar el proyecto
  de pruebas estructurales.
- Localizar el proyecto real; no asumir que CodeQuality.Tests existe.
- Las pruebas deben reflejar dependencias y límites acordados.
- Comprobar que las reglas seleccionen los tipos esperados.
  Una regla que no analiza ningún tipo no demuestra cumplimiento.
- No afirmar que ArchUnitNET verifica por sí solo la intención
  de negocio o todos los principios SOLID.

## Resolución de fallos

- Leer y analizar los errores antes de modificar código.
- Corregir los problemas introducidos por los cambios de la tarea.
- Volver a ejecutar los controles afectados después de corregirlos.
- No ampliar el alcance para arreglar problemas ajenos sin explicarlo.
- No ocultar fallos previos, problemas del entorno ni controles
  que no se pudieron ejecutar.
- Si existe un bloqueo, informar su causa y las verificaciones pendientes.

## Integridad de los controles

- No eliminar, omitir ni debilitar pruebas o reglas para hacer pasar
  una implementación.
- No modificar pruebas estructurales existentes sin autorización explícita.
- Si una regla parece incorrecta o desactualizada, explicar el conflicto
  y proponer el ajuste para revisión del equipo.
- Detener únicamente los cambios que dependan de resolver ese conflicto.
- Se pueden agregar pruebas de comportamiento como parte de una tarea
  autorizada, respetando los criterios de aceptación.
- No cambiar el resultado esperado de una prueba existente
  sin una justificación respaldada por los requisitos.

## SonarQube

- Ejecutar el análisis mediante el procedimiento documentado,
  cuando esté configurado.
- Revisar los problemas pertinentes a los cambios realizados.
- No desactivar reglas ni agregar exclusiones para ocultar problemas.
- No afirmar que el análisis fue satisfactorio solo porque
  se ejecutó el comando; revisar su resultado y el estado
  del Quality Gate cuando esté configurado.
- Si SonarQube no está disponible, registrar la verificación pendiente.

## Informe final

Informar:
- Qué comportamiento cambió.
- Qué controles se ejecutaron y sus resultados.
- Qué controles no pudieron ejecutarse y por qué.
- Qué pendientes o limitaciones permanecen.

No afirmar cumplimiento de controles que no se realizaron.