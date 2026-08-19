# Decisiones — TP1

## 1. Por qué Git no pudo resolver el conflicto solo

### Qué pasó

Las dos ramas (feature/titulo-a y feature/titulo-b) nacieron del mismo commit de main. Las dos cambiaron la línea 1 del README.md: una puso "versión A" y la otra puso "versión B".

Cuando mergeé primero la rama A, main pasó a tener "versión A". Después, al querer mergear la rama B, Git comparó las dos puntas contra el ancestro común y vio que las dos habían cambiado la misma línea pero de forma distinta.

Git no puede saber cuál es la correcta porque solo compara texto, no entiende significado. Entonces hace lo único que puede: marca las dos versiones en el archivo y le deja la decisión a una persona.

La prueba de que el criterio es "misma línea" y no "mismo archivo" está en la captura 3: la sección de Instalación, que estaba en el mismo archivo, se fusionó sola porque ninguna de las dos ramas la había tocado.

### Cómo se podría haber evitado

- **Integrar más seguido.** Si la rama B se hubiera creado después de mergear la rama A, habría partido de un main que ya tenía "versión A" y no habría conflicto. Ramas cortas = conflictos chicos.
- **No tocar la misma línea.** Si cada rama toca una zona distinta del archivo, Git fusiona sin preguntar.
- **Ponerse de acuerdo antes.** El conflicto de Git es el síntoma; la causa real es que dos personas tomaron decisiones incompatibles sin hablar.

En este TP el conflicto se fabricó a propósito para practicarlo en un entorno controlado.

## 2. Problemas que encontré y cómo los solucioné

### a) El README no tenía salto de línea final

Cuando agregué la sección de instalación, el diff mostraba `\ No newline at end of file` y la sección nueva quedaba pegada al título. En Markdown eso rompe el encabezado.

Lo vi leyendo el diff antes de mergear. Lo arreglé con un segundo commit en la misma rama separando bien las líneas.

### b) GitHub tarda en detectar el conflicto

Justo después de mergear el PR #2, el PR #3 todavía aparecía como mergeable. GitHub calcula la mergeabilidad en background y tarda unos segundos. Tuve que esperar y refrescar antes de sacar la captura del conflicto.

### c) El push rechazado (que no es un error)

`git push` devolviendo `! [remote rejected]` en rojo parece un error, pero es el resultado buscado: es la prueba de que la protección funciona. El commit local quedó descartado con `git reset --hard HEAD~1`.

### d) Las aprobaciones obligatorias en un TP individual

La protección se creó con 0 aprobaciones requeridas. GitHub no permite que el autor de un PR apruebe su propio PR, así que en un TP individual pedir 1 aprobación deja los PRs imposibles de mergear. En un equipo real ese número iría en 1 o más; acá va en 0 y la revisión la hago yo leyendo el diff.
