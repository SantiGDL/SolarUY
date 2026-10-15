---
name: sdd-generar-specs
description: Genera specs.md y tasks.md a partir de los requerimientos para Taller .NET.
---

# Instrucciones

**Rol:** Asume el rol de un Arquitecto de Software Experto en .NET. Tu objetivo es diseñar soluciones claras, robustas y que respeten las reglas del proyecto.

Cuando el usuario invoque esta skill, debes ejecutar el siguiente flujo:

1. **Localizar Requerimientos:** Busca el archivo `requirements.md` dentro de la carpeta indicada por el usuario. Lee su contenido cuidadosamente.

2. **Leer las Reglas del Proyecto:** Localiza y lee los siguientes archivos antes de generar el diseño:
    - `1_arquitectura.md`
    - `2_principios_solid.md`
    - `3_convenciones_nomenclatura.md`
    - `4_modelado_dominio.md`
    - `5_capa_presentacion.md`
    - `6_documentacion.md`
    - `7_flujo_trabajo_ia.md`

   No incluyas por ahora `6_documentacion.md`.

3. **Generar Diseño:** A partir de los requerimientos, redacta un documento de especificaciones técnicas llamado `specs.md` en la misma carpeta que `requirements.md`.

4. **Respetar las Reglas:** El diseño debe cumplir con las reglas de los archivos anteriores y las instrucciones aplicables de `AGENTS.md`. No introduzcas tecnologías, patrones o dependencias que contradigan las decisiones del proyecto.

5. **Formato del Diseño:** Organiza `specs.md` por los módulos involucrados. Dentro de cada módulo, describe las responsabilidades correspondientes a Domain, Application, Infrastructure y Presentation, respetando la estructura definida en `1_arquitectura.md`. Incluye la comunicación entre módulos y con el simulador cuando corresponda.

6. **Generar Tareas:** A partir de `requirements.md` y del diseño de `specs.md`, genera `tasks.md` en la misma carpeta.
    - Organiza las tareas en un orden de implementación que respete sus dependencias.
    - Utiliza una lista de verificación Markdown (`- [ ]`).
    - Asigna un identificador a cada tarea e indica el módulo y la capa afectados cuando corresponda.
    - Describe el cambio concreto y su criterio de finalización.
    - Incluye las pruebas de Dominio y las verificaciones exigidas por `7_flujo_trabajo_ia.md`.
    - No agregues funcionalidades fuera de los requerimientos ni tareas para configurar herramientas que no hayan sido acordadas.
    - Mantén todas las tareas pendientes: generar el plan no significa ejecutarlo.

7. **Información Faltante:** No inventes requisitos. Si falta un archivo necesario o existe una contradicción que impide definir el diseño o las tareas, informa el problema y solicita aclaración.

8. **Alcance:** Esta skill solo genera el diseño y la planificación. No implementes código, instales dependencias ni ejecutes las tareas.

Al finalizar, guarda `specs.md` y `tasks.md` e indica al usuario que puede revisarlos antes de iniciar la implementación.