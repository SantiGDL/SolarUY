# Reglas de Modelado de Dominio — SolarUY

Al generar, modificar o revisar entidades, modelar el dominio según los conceptos y reglas del negocio. Estas reglas complementan las de Clean Architecture y respetan los límites entre módulos.

## 1. Diseño orientado al negocio

- Modelar las entidades pensando en su significado, comportamiento y relaciones, no únicamente como un reflejo de tablas SQL.
- Utilizar el vocabulario acordado del negocio: parque, nodo, panel, inversor, medición y tarea de mantenimiento.
- No inventar reglas de negocio que los requisitos no establezcan.
- Mantener las decisiones específicas de Entity Framework y de la base de datos en Infraestructura.

## 2. Relaciones entre entidades

- Elegir referencias a objetos o identificadores según el significado de la relación y los límites del módulo.
- Dentro de un mismo módulo, utilizar referencias a entidades cuando ayuden a expresar y gestionar una relación del negocio.
- Se permite utilizar identificadores cuando no sea necesario disponer del objeto relacionado.
- No considerar incorrecta una propiedad como ParqueId únicamente por representar un identificador.
- Entre módulos, utilizar identificadores y los contratos acordados; no introducir referencias a entidades internas de otro módulo.
- No exigir propiedades de navegación únicamente para facilitar el trabajo del ORM.

## 3. Comportamiento y protección del estado

- Cuando una modificación de estado deba preservar una regla de negocio, concentrar esa regla en un método de la entidad.
- Si una regla involucra varios objetos y no corresponde naturalmente a una sola entidad, considerar un servicio de dominio.
- Los servicios de Aplicación coordinan los casos de uso y delegan las reglas propias del negocio en el Dominio.
- Advertir cuando una regla de una entidad esté dispersa o duplicada en servicios, componentes o repositorios.
- Se permiten entidades simples y propiedades con setters públicos durante las primeras etapas.
- No considerar incorrecta una entidad únicamente por tener propiedades y carecer de métodos.
- Proponer encapsular las modificaciones cuando sea necesario para evitar estados inválidos, sin exigir un modelo complejo para todas las entidades.

## 4. Tipos y simplicidad

- Utilizar tipos que expresen correctamente los datos del negocio.
- Considerar objetos de valor cuando permitan concentrar validaciones o expresar un concepto con significado propio.
- No reemplazar todos los tipos primitivos por clases sin una necesidad concreta.
- No introducir herencia, agregados, eventos de dominio ni otros patrones únicamente para aparentar un diseño orientado a objetos.