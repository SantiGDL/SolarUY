# Convenciones de código — SolarUY

## Nombres

- Usar nombres descriptivos en español para conceptos de negocio.
- Conservar los nombres exigidos por frameworks y bibliotecas.
- Usar PascalCase para tipos, métodos y propiedades públicas.
- Usar camelCase para parámetros y variables locales.
- Usar _camelCase para campos privados de instancia.
- Las interfaces comienzan con I.
- Las excepciones personalizadas terminan en Exception.
- Los controladores terminan en Controller.
- Los repositorios concretos terminan en Repository y sus interfaces
  utilizan el mismo nombre precedido por I.
- Aplicar el sufijo Repository únicamente a clases que desempeñen
  esa responsabilidad.
- Los servicios de aplicación y cliente terminan en Service,
  salvo otra convención expresamente acordada.
- Los componentes Blazor usan nombres descriptivos en PascalCase.
- Mantener una convención uniforme para acrónimos e identificadores,
  como IdUsuario, UsuarioId y XmlDocument.
- No incorporar CQRS, MediatR ni otros patrones por una excepción
  de nomenclatura; requieren una decisión independiente.

## Responsabilidad y legibilidad

- Cada método debe tener un propósito claro y un nombre que lo describa.
- No ocultar operaciones de negocio inesperadas dentro de un método.
- Un servicio de aplicación puede coordinar las acciones necesarias
  para completar un único caso de uso.
- No considerar una violación de responsabilidad única únicamente
  porque un método tenga varias llamadas o use "Y" en su nombre.
- Si mezcla responsabilidades independientes, proponer su separación.
- Extraer lógica cuando facilite comprenderla, reutilizarla o probarla.
- No dividir métodos únicamente para cumplir un número arbitrario
  de líneas.
- Evitar abstracciones que solo agreguen complejidad.

## Comunicación y acceso a datos

- Preferir operaciones asíncronas para entrada/salida.
- Evitar bloquear operaciones asíncronas mediante .Result o .Wait.
- No usar async void salvo en situaciones exigidas por el framework.
- Propagar CancellationToken cuando la operación y los contratos
  utilizados lo permitan.
- Evitar peticiones o consultas repetidas innecesariamente.
- No introducir reintentos de operaciones que modifican datos
  sin considerar el riesgo de duplicación.

## Errores y comentarios

- No capturar excepciones para ignorarlas o devolver un éxito falso.
- Diferenciar errores esperados de negocio de fallos inesperados.
- Mostrar mensajes comprensibles sin revelar información interna.
- No registrar secretos, tokens ni contraseñas.
- Comentar decisiones poco evidentes.
- Evitar comentarios que repitan literalmente el código.