# Diseño de la estructura de carpetas del backend

Estado: estructura de carpetas implementada y verificada por solicitud del equipo.
Las carpetas están vacías; las responsabilidades descritas orientan su uso futuro.

## Alcance técnico

Crear exclusivamente directorios en el sistema de archivos. SolarUyBack mantiene
su proyecto y su código existentes. SolarUyPruebas será solamente una carpeta,
sin .csproj, paquetes, referencias ni incorporación a SolarUy.sln.

## Árbol de directorios

El árbol muestra únicamente las carpetas solicitadas. Usuarios, Parques, Tareas
y Mediciones comparten exactamente la estructura indicada por el equipo,
incluidos DTOs, Interfaz, Aplicacion/Implementaciones e Infraestructura/Identidad.

```text
SolarUyBack/
    Backend/
        Modulos/
            Usuarios/
                Controladores/
                DTOs/
                Interfaz/
                    Apis/
                        Remota/
                        Local/
                    Eventos/
                        IN/
                        OUT/
                Aplicacion/
                    Servicios/
                    Implementaciones/
                Dominio/
                Infraestructura/
                    Persistencia/
                    Identidad/
            Parques/
                Controladores/
                DTOs/
                Interfaz/
                    Apis/
                        Remota/
                        Local/
                    Eventos/
                        IN/
                        OUT/
                Aplicacion/
                    Servicios/
                    Implementaciones/
                Dominio/
                Infraestructura/
                    Persistencia/
                    Identidad/
            Tareas/
                Controladores/
                DTOs/
                Interfaz/
                    Apis/
                        Remota/
                        Local/
                    Eventos/
                        IN/
                        OUT/
                Aplicacion/
                    Servicios/
                    Implementaciones/
                Dominio/
                Infraestructura/
                    Persistencia/
                    Identidad/
            Mediciones/
                Controladores/
                DTOs/
                Interfaz/
                    Apis/
                        Remota/
                        Local/
                    Eventos/
                        IN/
                        OUT/
                Aplicacion/
                    Servicios/
                    Implementaciones/
                Dominio/
                Infraestructura/
                    Persistencia/
                    Identidad/
        Compartido/
            Aplicacion/
                Eventos/
            Infraestructura/
                Persistencia/
                Eventos/

SolarUyPruebas/
    Unitarias/
        Parques/
        Tareas/
        Mediciones/
    Integracion/
    Arquitectura/
```

## Comunicación entre módulos: organización propuesta

Las siguientes responsabilidades orientan el uso futuro de las carpetas. Esta
entrega no crea interfaces, clases, eventos ni registros de servicios.

| Carpeta | Responsabilidad prevista |
| --- | --- |
| Modulos/<Modulo>/Controladores | Entrada HTTP: recibe solicitudes, delega en aplicación y traduce resultados a respuestas HTTP. |
| Modulos/<Modulo>/DTOs | Datos públicos de entrada y salida, sin exponer entidades de dominio. |
| Modulos/<Modulo>/Interfaz/Apis/Remota | Contratos del acceso remoto expuesto por el módulo. La atención de HTTP corresponde a Controladores. |
| Modulos/<Modulo>/Interfaz/Apis/Local | Contratos públicos que el módulo ofrece a otros módulos dentro del mismo proceso. |
| Modulos/<Modulo>/Interfaz/Eventos/IN | Receptores o manejadores de entrada que delegan en los servicios de aplicación del módulo consumidor. |
| Modulos/<Modulo>/Interfaz/Eventos/OUT | Contratos de los eventos publicados por el módulo emisor. |
| Modulos/<Modulo>/Aplicacion/Servicios | Abstracciones de los servicios que coordinan los casos de uso del módulo. |
| Modulos/<Modulo>/Aplicacion/Implementaciones | Implementaciones de los servicios y de los contratos de APIs que correspondan, sin duplicar la lógica de los casos de uso. |
| Modulos/<Modulo>/Dominio | Entidades y reglas de negocio independientes de la comunicación y la infraestructura. |
| Modulos/<Modulo>/Infraestructura/Persistencia | Futura implementación del acceso a datos del módulo. |
| Modulos/<Modulo>/Infraestructura/Identidad | Reserva para futuras integraciones de identidad que el módulo necesite. No selecciona proveedor ni implica autenticación propia. |
| Compartido/Aplicacion/Eventos | Abstracciones técnicas comunes para publicar y manejar eventos, sin contratos de negocio de los módulos. |
| Compartido/Infraestructura/Eventos | Implementación técnica del mecanismo común de distribución de eventos. |

Una API local es una interfaz invocada dentro del proceso del monolito; no es un
controlador HTTP ni una llamada de red. El consumidor dependerá del contrato
público y la composición de la aplicación conectará su implementación. Los
controladores siguen reservados para la API HTTP externa. Las APIs locales
deberán delegar en los mismos casos de uso para conservar validaciones y permisos.

Los eventos entre módulos comunican hechos mediante contratos ubicados en
Interfaz/Eventos/OUT del módulo emisor. El consumidor alojará los receptores en
su propia Interfaz/Eventos/IN; estos delegarán el caso de uso en aplicación. IN y
OUT se interpretan desde la perspectiva del módulo que contiene la carpeta.
No se copiará el contrato del emisor en IN de cada consumidor. El emisor no
deberá conocer los manejadores ni las implementaciones de los consumidores.
Estos eventos son independientes de las notificaciones de SignalR al frontend.

Las referencias entre módulos se limitarán a los contratos públicos de
Interfaz/Apis/Local, Interfaz/Eventos/OUT y los DTO necesarios. La carpeta
Interfaz/Eventos/IN contiene receptores internos del consumidor y no es una API
pública para otros módulos. No se expondrán entidades, repositorios ni servicios
internos. El dominio no dependerá del mecanismo de comunicación entre módulos.

La estructura es uniforme: en esta etapa no se agregan subcarpetas específicas
para repositorios de dominio, simulación o tiempo real. Cuando sus funcionalidades
se definan, se propondrá su ubicación sin anticipar archivos o integraciones.
Compartido y SolarUyPruebas conservan las carpetas previstas fuera de los módulos.

No se elige una biblioteca de mediación, un broker ni un mecanismo de persistencia
de eventos. Tampoco se presume ejecución en segundo plano, entrega garantizada,
orden, reintentos o transacciones entre módulos. Esas decisiones requieren los
requisitos de las funcionalidades que utilicen eventos. La estructura solamente
reserva ubicaciones y no garantiza todavía ningún comportamiento.

## Procedimiento

- Crear los directorios faltantes sin modificar los que ya existen.
- No generar archivos individuales, ni siquiera archivos vacíos o .gitkeep.
- No borrar ni mover archivos preexistentes, incluido WeatherForecast.cs.
- No modificar Program.cs, appsettings.json, proyectos, solución ni dependencias.
- No definir espacios de nombres, tipos, firmas de contratos, rutas, servicios o pruebas.

Las carpetas indican la organización prevista, pero no implementan ni verifican
límites entre módulos. AGENTS.md mantiene las reglas de responsabilidades para
cuando se incorpore código. Antes de una reorganización amplia deberá documentarse
el diseño en Reglas/arquitectura.md con la autorización correspondiente; crear
estos directorios vacíos no requiere modificar ese archivo ni AGENTS.md.

## Verificación

1. Comparar el árbol del sistema de archivos con los directorios enumerados aquí.
   Verificar que los cuatro módulos tengan la misma estructura, incluidos DTOs,
   Interfaz/Apis/Remota, Interfaz/Apis/Local, Interfaz/Eventos/IN,
   Interfaz/Eventos/OUT, Aplicacion/Servicios, Aplicacion/Implementaciones e
   Infraestructura/Identidad, y las carpetas comunes de eventos de Compartido.
2. Comprobar que no se generaron archivos dentro de las carpetas solicitadas y
   que los archivos preexistentes conservaron su contenido.
3. Ejecutar dotnet build desde la raíz, como establece AGENTS.md.
4. No ejecutar dotnet test si siguen sin existir proyectos de pruebas.
   ArchUnitNET y SonarQube no están configurados y no se configuran en esta entrega.
5. Actualizar Readme.md únicamente después de implementar por solicitud del equipo
   y verificar que el requerimiento quedó cumplido. No crear ni modificar ese
   archivo durante la preparación o revisión de la propuesta. Marcar en task.md
   solamente lo realizado.

Git no registra directorios vacíos. La estructura existirá localmente, pero no
se conservará por sí sola en un commit o un clon. No agregar archivos marcadores,
porque el requisito exige únicamente carpetas.

## Resultado de la verificación

- Se crearon los 83 directorios faltantes y se comparó el árbol real con este diseño.
- Los cuatro módulos tienen exactamente la misma estructura; Backend y
  SolarUyPruebas no contienen archivos, incluidos archivos .gitkeep.
- Se verificó mediante hashes que los 75 archivos preexistentes conservaron su
  contenido durante la creación de carpetas y la compilación, excluyendo los
  artefactos generados en bin/obj. Después se actualizaron únicamente los tres
  documentos de esta funcionalidad y se creó Readme.md para registrar el resultado.
- dotnet build desde la raíz terminó con 0 errores y 0 advertencias.
- No se ejecutó dotnet test porque no existen proyectos de pruebas.
  ArchUnitNET y SonarQube siguen sin configurar y no se ejecutaron.
