Lee AGENTS.md y las reglas de .agents/rules/.

Quiero preparar la estructura interna del backend de SolarUY
en el proyecto Blazor Web App con Interactive Server existente.

Organizalo como un monolito modular dentro de Backend/Modulos,
con los módulos Usuarios, Parques, Tareas y Mediciones.

Dentro de cada módulo, crea estas carpetas:

- Dominio/Entidades
- Dominio/Repositorios
- Aplicacion/Contratos
- Aplicacion/Implementaciones
- Aplicacion/DTOs
- Infraestructura/Repositorios
- Interfaz/Apis/Local/Contratos
- Interfaz/Apis/Local/Implementaciones
- Interfaz/Eventos/IN
- Interfaz/Eventos/OUT
- Excepciones

Crea también:
- Backend/Infraestructura/Persistencia

Mantén los componentes Blazor existentes fuera de Backend.

Inspecciona primero el proyecto para evitar duplicaciones.
Crea únicamente las carpetas y los archivos .gitkeep necesarios
para conservarlas en Git.

No generes código, crees proyectos .NET adicionales, instales
paquetes ni configures integraciones.
No actualices README.md todavía.

Al finalizar, muestra las rutas creadas e informa cualquier
conflicto con las reglas del proyecto.