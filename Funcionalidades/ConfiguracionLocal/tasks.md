# Tareas: configuración local interactiva

Estado: implementación de Windows creada; verificación integral pendiente.
Todas las tareas corresponden a herramientas de desarrollo o documentación,
sin cambios en las capas de los módulos de negocio.

- [x] 1 Sincronizar diseño y tareas con los requisitos aceptados de Windows,
  incluyendo el bloqueo ante `.env` existente y la reutilización de credenciales
  MySQL en User Secrets. Finalizado al reflejar ambos comportamientos en estos
  documentos.
- [x] 2 Crear `Scripts/Configurar-EntornoLocal.ps1` con rutas relativas
  a `$PSScriptRoot` y localizar la raíz del repositorio en `..`
  y comprobaciones previas. Finaliza cuando se detecten requisitos faltantes y,
  si existe `.env`, se explique que no se puede continuar hasta eliminarlo
  manualmente o seguir el README, se espere una tecla antes de cerrar y no se
  alteren `.env` ni User Secrets.
- [x] 3 Implementar presentación de los nombres de `.env.example`, pregunta
  de cambio y conservación al responder que no; captura y confirmación visibles
  de contraseñas y validaciones. Finaliza cuando entradas inválidas permitan
  reintentar y cancelar antes de escribir no cambie los destinos.
- [x] 4 Implementar serialización de `.env`, cadena MySQL y JSON para
  User Secrets por stdin. Finaliza cuando ambos destinos representen las mismas
  credenciales ya ingresadas, sin solicitarlas nuevamente ni repetirlas en
  salida de diagnóstico o argumentos de proceso.
- [x] 5 Gestionar creación exclusiva, errores y resultados parciales.
  Finaliza cuando los fallos informen qué se guardó y cómo completar la
  configuración, sin borrar configuración preexistente; la confirmación previa
  debe avisar si se reemplazará la clave de User Secrets.
- [ ] 6 Verificar en entorno temporal con credenciales ficticias los casos
  de aceptación y caracteres especiales, Compose y el proveedor MySQL.
  Registrar resultados para PowerShell 5.1 y 7 y limitaciones del entorno.
- [x] 7 Compilar `SolarUy.sln`, localizar y ejecutar verificaciones existentes
  aplicables y revisar que no haya cambios de dependencias entre capas.
  Registrar resultados y bloqueos; no agregar pruebas de Dominio porque esta
  tarea no modifica su comportamiento ni configurar ArchUnitNET o SonarQube.
- [x] 8 Actualizar la preparación inicial del README con el asistente de
  Windows ya verificado, requisitos, límites y alternativa manual.
- [ ] 9 Diseñar y planificar el equivalente Linux después de Windows.
  Acordar intérprete y requisitos; su implementación corresponde a la segunda etapa.

## Verificaciones de esta etapa

Se revisaron AGENTS.md, las siete reglas existentes en `.agents/Reglas/`, el
flujo `.agents/skills/sdd-generar-spec.md`, `.env.example`, `compose.yaml`,
el README y la configuración de arranque del proyecto.
La ruta `.agents/rules/` mencionada por AGENTS.md no existe en este repositorio;
las reglas se localizaron en `.agents/Reglas/`.

Se creó el script y se actualizó el README. La solución compiló sin advertencias
y pasaron las 35 pruebas existentes de Parques. Se comprobó que un `.env`
existente provoca un aviso y espera una tecla antes de salir, sin pedir credenciales
ni modificar User Secrets.
Quedan pendientes las pruebas integrales del asistente en un entorno aislado,
incluidas las dos versiones de PowerShell, Compose y caracteres especiales.
En este entorno no están disponibles Docker ni `dotnet-sonarscanner`; no se
ejecutó SonarQube ni se obtuvo un resultado de Quality Gate.
