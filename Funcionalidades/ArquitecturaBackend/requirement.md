# Estructura de carpetas del backend

## Ampliación autorizada: conservar la estructura en Git

El equipo solicita archivos vacíos en las carpetas existentes de
`SolarUyBack/Modulos/` y `SolarUyBack/Compartido/` para poder versionarlas.
Crear un `.gitkeep` vacío en cada subcarpeta vacía, conservando las rutas actuales
y los archivos existentes. Verificar que Git detecte los marcadores y que todas
las carpetas queden representadas. No realizar commit ni push en esta tarea.

Esta solicitud sustituye, para esas dos rutas, la exclusión de archivos marcadores
de la entrega inicial descrita a continuación. SolarUyPruebas queda fuera de esta
ampliación. Las rutas actuales no incluyen el nivel intermedio Backend.

## Entrega inicial

Estado: implementado y verificado por solicitud del equipo. Cumplidos los criterios
de aceptación de la estructura de carpetas; no se implementó código de los módulos.

## Objetivo

Crear únicamente la estructura de carpetas solicitada para organizar el futuro
monolito modular de SolarUyBack. Esta entrega no genera archivos dentro de esas
carpetas ni implementa código.

## Alcance

- Crear Backend/Modulos dentro de SolarUyBack, con Usuarios, Parques, Tareas y
  Mediciones y sus subcarpetas, según el árbol de spec.md.
- Crear Backend/Compartido/Infraestructura/Persistencia dentro de SolarUyBack.
- Aplicar la misma estructura a Usuarios, Parques, Tareas y Mediciones:
  Controladores, DTOs, Interfaz, Aplicacion, Dominio e Infraestructura.
- Incluir en cada módulo Interfaz/Apis/Remota e Interfaz/Apis/Local para reservar
  los contratos de acceso remoto y de comunicación dentro del monolito.
- Incluir en cada módulo Interfaz/Eventos/IN e Interfaz/Eventos/OUT para reservar
  la recepción de eventos y los contratos de eventos publicados, respectivamente.
- Incluir Aplicacion/Servicios y Aplicacion/Implementaciones en los cuatro módulos.
- Incluir Infraestructura/Persistencia e Infraestructura/Identidad en los cuatro
  módulos. Su existencia no implica implementar ni configurar esas integraciones.
- Incluir Backend/Compartido/Aplicacion/Eventos y
  Backend/Compartido/Infraestructura/Eventos para separar las abstracciones comunes
  de publicación de eventos de su futura implementación técnica.
- Entender las APIs locales como llamadas a contratos dentro del mismo proceso
  del monolito. Esta propuesta no agrega comunicación HTTP entre módulos.
- Crear SolarUyPruebas y sus subcarpetas en la raíz, únicamente como directorios;
  no crear un proyecto .NET ni agregarlo a la solución.
- Respetar los nombres y las mayúsculas del árbol de spec.md, incluidos DTOs,
  Interfaz, Apis, Remota, Local, IN y OUT. No agregar otras subcarpetas a los módulos
  en esta etapa.
- Adaptar spec.md y task.md a este alcance durante la preparación de la propuesta.
- Actualizar Readme.md de la raíz únicamente después de que el equipo solicite
  implementar el requerimiento y se verifique su cumplimiento efectivo. No crear
  ni modificar Readme.md durante la preparación o revisión de la propuesta.

## Fuera de alcance

- Crear archivos .cs, .csproj, .gitkeep u otros archivos dentro de las carpetas
  solicitadas, incluso si son vacíos o contienen solamente comentarios.
- Crear clases, interfaces, enumeraciones, DTO, controladores o pruebas.
- Modificar o eliminar código, configuraciones o archivos existentes del backend
  o del frontend, incluidos los ejemplos de plantilla.
- Modificar SolarUy.sln, agregar paquetes o configurar herramientas de pruebas.
- Definir firmas o contenido de contratos, campos, reglas de negocio, persistencia,
  identidad o roles.
- Implementar llamadas entre módulos, publicación o manejo de eventos, o elegir
  bibliotecas, brokers y garantías de entrega. Solo se reservan las carpetas.
- Cambiar la versión de .NET o la configuración de despliegue.

## Criterios de aceptación

1. Existen todos los directorios del árbol de spec.md.
2. La creación de la estructura no genera ningún archivo dentro de esos directorios.
3. Se conservan los archivos preexistentes; no se eliminan archivos para vaciar carpetas.
4. La solución, los proyectos, las dependencias y el código mantienen su contenido.
5. Se verifica la estructura en el sistema de archivos y se informa el resultado
   de dotnet build, conforme a AGENTS.md.
6. Se informa que Git no registra carpetas vacías; no se crean archivos marcadores
   para evitar esa limitación.
7. Los cuatro módulos comparten exactamente la estructura indicada en el alcance,
   con APIs remotas y locales y eventos IN/OUT. Compartido dispone de las carpetas
   separadas de aplicación e infraestructura para eventos.

## Pendientes

- Definir los contratos concretos, emisores y consumidores, y el comportamiento
  ante errores y entrega de eventos al especificar las funcionalidades que los
  utilicen; no son decisiones necesarias para reservar las carpetas.
- La regla de documentar módulos y dependencias en Reglas/arquitectura.md sigue
  aplicando antes de una reorganización amplia del código. Esta etapa solamente
  crea directorios vacíos y no reorganiza código ni define nuevas dependencias.
- La implementación de funcionalidades y la configuración de pruebas,
  ArchUnitNET y SonarQube quedan fuera de esta entrega.
