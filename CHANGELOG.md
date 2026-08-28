# Changelog

Cambios que le importan a alguien que use SoundMate o programe sobre él. Formato basado en
[Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/); versionado
[SemVer](https://semver.org/lang/es/).

El número vive en un único sitio, la propiedad `<Version>` de `Directory.Build.props`, y de ahí
salen los ensamblados, el documento OpenAPI y `GET /api/version`.

> **MAJOR es 0: el contrato HTTP no es estable.** Una subida de MINOR puede romperlo. Cuando lo
> haga, sale aquí bajo **Cambios incompatibles**.

Cada MINOR tiene además su página en [`docs/wiki/`](docs/wiki/README.md), con la explicación
funcional y la técnica.

---

## [0.2.0] — 2026-08-28

La versión en la que SoundMate empieza a hacer cosas: la 0.1.0 tenía el modelo de dominio pero ni
un solo endpoint de negocio. Ahora hay **25**.

Detalle completo en [`docs/wiki/v0.2.0.md`](docs/wiki/v0.2.0.md).

### Añadido

- **Capa de aplicación**: `AddApplication()`, `ValidationFilter` global (FluentValidation),
  `GlobalExceptionHandler` con `ProblemDetails`, y `Pbkdf2PasswordHasher`.
- **Usuarios** — 11 endpoints: registro, consulta por id y por email, edición de nombre y teléfono,
  cambio de contraseña (exigiendo la actual), verificación de email, suspender/reactivar, y baja,
  recuperación y borrado permanente.
- **Academias** — 14 endpoints: creación, consulta por id, por slug y por dueño, renombrado, cambio
  de slug, cambio de plan, suspender/activar, cerrar/reabrir, y baja, recuperación y borrado
  permanente.
- **Borrado lógico** en `User` y `Academy` (`DeletedAtUtc`), independiente del estado de negocio, con
  índice parcial sobre el conjunto vivo. Migraciones `UserSoftDelete` y `AcademySoftDelete`.
- **`Academy.Reopen()`**: hasta ahora una academia cancelada no podía volver a operar nunca, y con
  el borrado lógico quedaba un callejón sin salida.
- **`Email.IsValid` y `Slug.IsValid`**, para que los validadores usen la regla del dominio en vez de
  reescribirla.
- **`GET /api/version`**, y la versión también en el documento OpenAPI y en el título de Scalar.
- Repositorios `IAcademyRepository.ListByOwnerAsync` e `IMembershipRepository.ListByAcademyAsync`,
  cuyos índices ya existían sin que nada pudiera usarlos.
- Wiki versionada en `docs/wiki/` y este changelog.

### Cambiado

- **Crear una academia crea también la membresía de dueño**, en el mismo `SaveChanges`.
- **Los enums viajan por nombre** (`"SoloTeacher"`, no `2`) mediante `JsonStringEnumConverter`. Los
  números se siguen aceptando de entrada.
- **Los DTO de respuesta llevan enums en vez de `string`.** Cambia el tipo en C#; **el JSON no
  cambia**. Afecta a `UserDto.Status` y a `AcademyDto.Type/Plan/Status`.
- La wiki pasa de una única `Home.md` a una página por versión.

### Corregido

- **El validador y el dominio no coincidían sobre qué es un email válido.** `missing@domain` pasaba
  el validador de FluentValidation y luego reventaba en `Email.Create`: el caller recibía una
  invariante lanzada en vez de un 400 con el campo señalado.
- **Una academia cancelada se podía renombrar, re-sluguear y cambiar de plan.** La guarda solo cubría
  `Suspend` y `Activate`, al contrario de lo que decía su propia documentación.
- **Perder la carrera contra un índice único salía como 500.** El `23505` de Postgres se traduce
  ahora en `UnitOfWork` y los servicios devuelven el mismo 409 que la comprobación previa. Aplica a
  emails y a slugs duplicados.
- **`reopen` sobre algo dado de baja respondía un 404 sin información.** Ahora es un 409 que dice
  que hay que usar `restore`.

### Seguridad

- Las contraseñas se guardan con **PBKDF2-HMAC-SHA256 y 600.000 iteraciones**, formato
  autodescriptivo y comparación en tiempo constante. Seis veces las iteraciones que usa Agendia para
  secretos de máquina, porque una contraseña humana tiene mucha menos entropía.
- **`UserDto` no puede llevar credenciales**, y hay un test que lo impide en vez de un comentario.
- **No se usa AutoMapper.** Su advisory de DoS (GHSA-rvv3-g6hj-g44x) solo está parcheado a partir de
  15.1.1/16.1.1, que ya no son MIT: **todas las versiones libres están afectadas**.
- El correo de una persona dada de baja y el slug de una academia dada de baja **siguen reservados**,
  para que nadie herede una identidad ajena ni un enlace público lleve a otro sitio.

### Pendiente conocido

- **Nada está autenticado.** Todas las rutas están abiertas: `GET /api/users?email=` es un oráculo
  de enumeración de usuarios y cualquiera puede abrir una academia a nombre de otro.
- El **borrado permanente deja huérfanas** filas en otras tablas. Se niega mientras queden
  relaciones, pero quiere una cascada de verdad antes de usarse en serio.
- El **aprovisionamiento hacia Agendia** (`Academy` → `Business`) no está.

---

## [0.1.0] — 2026-07-27

Los cimientos. Sin endpoints de negocio todavía; ver [`docs/wiki/v0.1.0.md`](docs/wiki/v0.1.0.md).

### Añadido

- Modelo de dominio rico completo: 11 agregados con factories, invariantes, IDs tipados y value
  objects (`Email`, `Slug`).
- Persistencia con EF Core sobre PostgreSQL: configuraciones, migración `InitialIdentity` y
  catálogos sembrados (48 disciplinas, 33 géneros).
- Repositorios para los 11 agregados, `IUnitOfWork` y la inyección de dependencias.
- Conexión máquina-a-máquina con Agendia (`client-credentials`) y `GET /api/agendia/connection`.
- Infraestructura local en Docker (`deploy/`): PostgreSQL, Seq y RabbitMQ, más el `Dockerfile` de la
  API tras un perfil de compose.
- 129 tests de dominio y CI en GitHub Actions.
