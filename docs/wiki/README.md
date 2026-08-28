# Wiki de SoundMate

El servicio principal de un **SaaS para profesores de música y academias**. Se encarga del
**negocio y la identidad**: quién es quién, qué academias existen y qué relación hay entre ellos.
Los horarios y las reservas viven en un microservicio aparte, **Agendia**.

La wiki se organiza **por versión**. Cada versión tiene **una sola página** con las dos
explicaciones dentro: una **funcional** (para cualquiera, con ejemplos) y una **técnica** (a fondo,
para desarrollo).

## Versiones

| Versión | Estado | Contenido |
|---|---|---|
| [**v0.2.0**](v0.2.0.md) | Actual | La capa de aplicación y los endpoints: usuarios y academias, con borrado lógico |
| [v0.1.0](v0.1.0.md) | Histórica | El modelo de dominio y la persistencia, **antes** de que hubiera endpoints de negocio |

> **Ojo con la v0.1.0.** No está mal, está **incompleta**: describe el dominio y la base de datos,
> pero da por pendiente todo lo que la v0.2.0 construyó encima. Para entender *qué son* las
> entidades sigue siendo la mejor página; para saber qué se puede hacer con ellas, la v0.2.0.

## Cómo se numera

**SemVer**, con el número en un único sitio: la propiedad `<Version>` de
[`Directory.Build.props`](../../Directory.Build.props). De ahí salen los ensamblados, el documento
OpenAPI y `GET /api/version`, así que lo que responde un entorno es siempre lo que dice el repo.

Mientras el MAJOR sea **0** el contrato HTTP no es estable: una subida de MINOR puede romperlo, y
decirlo es el trabajo del [CHANGELOG](../../CHANGELOG.md). Cada MINOR estrena página aquí.

## Documentos de referencia (repo)

- [`CLAUDE.md`](../../CLAUDE.md) — convenciones vivas del repo y estado actual, para quien programe.
- [`deploy/README.md`](../../deploy/README.md) — la infraestructura local en Docker y sus puertos.
- [`docs/tasks/agendia-m2m-auth.md`](../tasks/agendia-m2m-auth.md) — el contrato del token máquina-a-máquina con Agendia.
