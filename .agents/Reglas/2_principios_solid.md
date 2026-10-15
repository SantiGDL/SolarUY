Principios SOLID y buenas prácticas de código — SolarUY
Al generar, modificar o revisar código, aplicar los principios SOLID,
con especial énfasis en la responsabilidad única y en la claridad.
Estas reglas complementan las de Clean Architecture; no definen
una estructura de carpetas ni autorizan nuevas funcionalidades.

1. Responsabilidad única (SRP)
- Mantener clases y métodos centrados en una responsabilidad coherente.
- Los métodos deben cumplir lo que indica su nombre, sin operaciones
  de negocio adicionales ni efectos secundarios inesperados.
- Marcar como incorrecto un método de una operación específica que
  incorpore otra operación de negocio distinta de forma oculta.
- Por ejemplo, un método CrearParque no debe crear automáticamente
  nodos y paneles si ese comportamiento no forma parte de su contrato
  y de los requisitos acordados.
- Separar las operaciones específicas en métodos o servicios con
  responsabilidades claras.
- El caso de uso puede coordinar varias operaciones cuando los requisitos
  lo contemplen. Esa coordinación debe ser explícita: invocar las operaciones
  correspondientes, sin esconderlas dentro de otra operación.
- Consultar datos, validar condiciones y guardar cambios puede formar parte
  de una misma operación. No separar cada instrucción en un método por rutina.
- No mezclar en una misma clase interacción visual, reglas de negocio,
  persistencia y comunicación técnica con el simulador.
- Mantener distinguibles la recepción de mediciones, su procesamiento
  y su presentación, con responsabilidades claras.
- No duplicar reglas de negocio entre componentes, servicios y repositorios.

2. Abierto/cerrado (OCP)
- Diseñar los puntos de variación conocidos para permitir nuevas
  implementaciones sin modificar los casos de uso que los consumen.
- Encapsular el acceso al mock o simulador mediante un contrato adecuado,
  para que la lógica de negocio no dependa de su implementación técnica.
- No repartir condiciones sobre proveedores o implementaciones concretas
  por todo el código.
- No anticipar variaciones hipotéticas ni crear jerarquías, fábricas
  o estrategias sin una necesidad concreta.
- Este principio no prohíbe modificar código existente para corregirlo
  o incorporar requisitos.

3. Sustitución de Liskov (LSP)
- Toda implementación debe respetar el comportamiento prometido
  por su interfaz o clase base.
- Mantener coherentes los resultados, errores esperados y condiciones
  de uso entre implementaciones del mismo contrato.
- El mock o simulador debe respetar el contrato acordado, aunque produzca
  datos simulados. No debe exigir condiciones adicionales que los
  consumidores del contrato desconozcan.
- No implementar miembros obligatorios con NotImplementedException
  o resultados ficticios para forzar una clase a encajar en una abstracción.
- Preferir composición cuando la herencia no represente una relación
  de sustitución válida.

4. Segregación de interfaces (ISP)
- Crear interfaces centradas en las necesidades de sus consumidores.
- No obligar a una clase a depender de operaciones que no utiliza
  ni a implementar operaciones que no le corresponden.
- Revisar los contratos que mezclen responsabilidades independientes,
  como mantenimiento, lectura de mediciones y administración de parques.
- Las funcionalidades de consulta mediante lenguaje natural no deben
  depender de contratos de escritura o control si no los necesitan.
- No dividir interfaces mecánicamente hasta tener una por cada método.
  Mantener juntas las operaciones que formen un contrato coherente.

5. Inversión de dependencias (DIP)
- Los servicios de aplicación deben depender de contratos para
  persistencia e integraciones, no de implementaciones concretas.
- Definir las interfaces de repositorios en Dominio y las interfaces
  de integraciones requeridas por los casos de uso en Aplicación,
  conforme a las reglas de Clean Architecture del proyecto.
- Implementar esos contratos en Infraestructura.
- Inyectar las dependencias por constructor en servicios y repositorios.
  En componentes, utilizar los mecanismos de inyección de Blazor.
- No usar localizadores de servicios ni instanciar manualmente
  repositorios o integraciones administrados por el contenedor.
- No crear una interfaz para cada clase sin una utilidad concreta.

6. Complejidad y tamaño
- Mantener métodos breves y fáciles de comprender. Tomar como referencia
  poder leerlos sin desplazarse excesivamente, sin imponer un límite
  rígido de líneas.
- Si un método contiene bloques if/else que resuelven tareas no relacionadas
  con su propósito principal, sugerir extraerlas a métodos o clases
  con responsabilidades claras.
- Reducir anidamientos innecesarios; utilizar retornos tempranos
  cuando mejoren la lectura.
- No extraer métodos únicamente para reducir el número de líneas
  si eso fragmenta la lógica y dificulta entenderla.

7. Nombres y comportamiento explícito
- Usar nombres que expresen la intención y respeten el vocabulario
  acordado: parque, nodo, panel, inversor, medición y tarea de mantenimiento.
- Si un comportamiento combinado está justificado, su nombre y contrato
  deben reflejarlo; por ejemplo, ObtenerOCrear.
- Un nombre con Y o And es una señal para revisar posibles responsabilidades
  mezcladas. Advertir y sugerir una separación cuando corresponda.
- No corregir una mezcla de responsabilidades únicamente cambiando el nombre.
- Distinguir en los nombres y contratos las consultas de las operaciones
  que modifican datos o envían órdenes.
- No introducir modificaciones de negocio como efecto oculto de una consulta.

8. Aplicación de las reglas
- Justificar las observaciones indicando qué responsabilidad o contrato
  se está incumpliendo y proponer una corrección concreta.
- Respetar el alcance acordado: las consultas mediante lenguaje natural
  son de consulta de información; no añadirles operaciones de modificación
  ni órdenes al inversor.
- No inventar reglas de negocio que la letra o los requisitos de la
  funcionalidad no establezcan.
- Refactorizar dentro del alcance de la tarea, conservando el comportamiento
  requerido. No realizar reorganizaciones amplias por una preferencia de estilo.