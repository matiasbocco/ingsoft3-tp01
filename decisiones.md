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

## TP2 — Contenedores

### Qué app elegí y por qué

Elegí un inventario básico (backend .NET 8 Minimal API + EF Core + PostgreSQL, frontend React/Vite). Los criterios de la guía pedían una app que buildee localmente, tenga tests, sea comprensible y sea chica. Esta los cumple todos: el backend compila con `dotnet publish` sin dependencias externas al `.csproj`, tiene tests xUnit sobre la lógica de upsert y tests Vitest sobre los componentes, el dominio (item con nombre/cantidad/ubicación/categoría) es trivial de entender, y el total de código propio es menos de 300 líneas.

### Decisiones de contenerización

**Imágenes base elegidas:**
- Backend build: `mcr.microsoft.com/dotnet/sdk:8.0` — incluye el SDK completo para compilar. Runtime: `mcr.microsoft.com/dotnet/aspnet:8.0` — solo el runtime, sin SDK ni herramientas de build.
- Frontend build: `node:20-alpine` — Node LTS en variante Alpine para minimizar tamaño. Serve: `nginx:alpine` — servidor estático liviano; el frontend es un bundle estático después del build.
- Base de datos: `postgres:16-alpine` — PostgreSQL 16 en Alpine, imagen oficial.

**Por qué multi-stage:**
El stage de build del backend pesa ~900 MB (SDK + fuentes + cache de restore). El stage de runtime pesa ~220 MB. Sin multi-stage, cada deploy arrastraría el SDK completo. Con multi-stage, la imagen final solo contiene el binario publicado y el runtime. Lo mismo en el frontend: `node_modules` durante el build suma cientos de MB que no tienen que estar en producción; nginx sirve solo el `dist/`.

**Qué persiste en el volumen y qué no:**
El volumen `pgdata` persiste los datos de PostgreSQL en `/var/lib/postgresql/data`. Todo lo demás es efímero: los contenedores de backend y frontend son stateless y se pueden recrear sin perder datos. Si se hace `docker compose down` (sin `-v`) los datos sobreviven. Si se agrega `-v` el volumen se borra y la base queda vacía en el próximo `up`.

### Problemas encontrados

El principal problema fue que `npm ci` fallaba en el build del frontend porque no había `package-lock.json` en el repositorio (el agente generó `package.json` pero no corrió `npm install`). El error en Docker era claro: `npm ci` requiere un lockfile existente. Lo resolví corriendo `npm install` en `frontend/` localmente para generar el `package-lock.json` y commitearlo junto con el resto.

Otro problema: la tabla `Items` no se creaba aunque EF Core decía "no migrations were applied". Las migraciones fueron generadas manualmente por el agente y no quedaron registradas en el assembly compilado. La solución fue reemplazar `db.Database.Migrate()` por `db.Database.EnsureCreated()`, que crea el schema directamente del modelo sin depender de archivos de migración.

### Declaración de uso de IA

La estructura inicial del proyecto (archivos `.csproj`, `Program.cs`, `Dockerfile`, `docker-compose.yml`, componentes React) fue generada con asistencia de Claude Code. El código fue revisado, los errores de contenerización fueron diagnosticados y corregidos por mí corriendo la app de punta a punta: levanté el stack con `docker compose up`, verifiqué cada endpoint desde el browser y con `curl`, y confirmé la persistencia y el borrado del volumen manualmente.
