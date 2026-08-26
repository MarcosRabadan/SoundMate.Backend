# Tarea (Agendia): Autenticación máquina-a-máquina (client-credentials) para servicios

## Contexto

Agendia es un microservicio de gestión de citas. Otro microservicio, **SoundMate** (identidad y
negocio de una plataforma para academias de música), necesita **llamar a la API de Agendia
servicio-a-servicio** (validar disponibilidad, crear/consultar citas, etc.).

Hoy Agendia **solo tiene autenticación de usuario** (JWT vía `POST /api/auth/login` / `register`,
con `accessToken` + `refreshToken` rotatorio). Ese modelo **no encaja para un servicio**: el refresh
rotatorio da problemas con varias instancias, y no queremos que un microservicio se loguee con la
contraseña de un usuario Admin.

**Objetivo:** añadir un flujo estándar **client-credentials (OAuth2 M2M)** para que servicios de
confianza se autentiquen con un `clientId` + `clientSecret` y reciban un **token de servicio**
(JWT), **sin refresh token** (el servicio vuelve a pedir token con su secreto cuando caduca).

> ⚠️ **Antes de empezar, localiza la implementación de auth actual.** En la rama principal `src`
> aparece una migración `RemoveIdentityTables` y no se encuentra la lógica de login/generación de
> JWT (puede estar en otra rama/worktree o en refactor). **Sitúa dónde vive hoy** la generación del
> JWT de usuario y **reutiliza esa misma infraestructura de firma** (misma clave, `Issuer`
> `MRC.Agendia`, `Audience` `MRC.Agendia.Clients`). No dupliques la firma ni toques el flujo de
> usuario existente.

## Qué hay que construir

### 1. Endpoint nuevo: obtener token de servicio

```
POST /api/auth/service-token
Content-Type: application/json

{ "clientId": "soundmate", "clientSecret": "<secreto>" }
```

Respuesta `200`:

```json
{
  "accessToken": "eyJhbGci...",
  "expiresAt": "2026-07-29T12:15:00",
  "tokenType": "Bearer"
}
```

- **Sin `refreshToken`.** Cuando el `accessToken` caduque, el servicio vuelve a llamar a este mismo
  endpoint con su secreto.
- `401` si el `clientId` no existe, el secreto no coincide o el cliente está deshabilitado.
- Endpoint **público** (no requiere Bearer previo), pero solo funciona con un `clientSecret` válido.
- Sigue las convenciones de la API: JSON **camelCase**, mismos formatos de fecha que el resto.

*(Alternativa válida si prefieres el estándar estricto OAuth2: `grant_type=client_credentials`
form-urlencoded devolviendo `access_token`/`expires_in`. Pero para ser consistente con el resto de
DTOs JSON de Agendia, se recomienda el cuerpo JSON de arriba.)*

### 2. Registro de clientes de servicio (dónde viven los `clientId`/`clientSecret`)

Una lista de clientes de confianza permitidos. **Recomendado para la primera versión: por
configuración** (sección en `appsettings` + secretos), sin migración de BD:

```json
// appsettings.json (el secreto va en user-secrets / variables de entorno, NUNCA en claro aquí)
"ServiceClients": [
  { "clientId": "soundmate", "clientSecretHash": "<hash>", "role": "Service", "enabled": true }
]
```

Requisitos:
- El **secreto se guarda hasheado** (p. ej. con el mismo mecanismo que las contraseñas de usuario, o
  un hash con sal). Nunca en texto plano.
- Comparación en **tiempo constante** al validar el secreto.
- Un cliente `enabled: false` no puede obtener token.

*(Alternativa: una tabla `ServiceClients` en BD con su repositorio, si prefieres gestionarlos sin
redeploy. Documenta cuál eliges.)*

### 3. Contenido del token de servicio (claims)

El JWT emitido se firma **igual que los de usuario** (misma clave/issuer/audience) para que la
validación existente (`AuthenticationSetup`) lo acepte sin cambios. Debe llevar:

- `sub` = el `clientId` (identifica al servicio).
- `client_id` = el `clientId`.
- Un claim que lo marque como token de servicio, p. ej. `token_use = service`.
- Un **rol / autorización** que le dé los permisos necesarios (ver punto 4).
- `exp` con la vida configurada (por defecto ~15–30 min; configurable en `Jwt`/`ServiceAuth`).

### 4. Autorización: qué puede hacer el token de servicio

SoundMate necesita operar **a través de varios negocios** (cada academia de SoundMate se
corresponderá con un negocio de Agendia), así que el token de servicio tiene que **saltarse el
filtrado por negocio** que tienen Owner/Employee.

Dos opciones (elige y documenta):

- **A (mínimo esfuerzo):** emitir el token con el **rol `Admin`** existente (Admin ya "ve todo sin
  filtro de negocio"). Reutiliza toda la autorización actual. Funciona ya.
- **B (más limpio):** un **rol nuevo `Service`** y que la capa de autorización lo trate con el acceso
  transversal necesario (como Admin para los endpoints que consuma SoundMate). Más preciso, permite
  acotarlo en el futuro.

**Recomendación:** opción **B** si es barato en su modelo de políticas; si no, **A** para no
bloquear.

**Ojo con `ICurrentUserContext` / `ICurrentBusinessScope` / `ResourceAuthorizationService`:** hoy
resuelven el usuario/negocio actual desde los claims de un **usuario**. Con un token de servicio **no
hay usuario** en el sentido normal. Hay que asegurarse de que:
- `ICurrentUserContext` refleje un **principal de servicio** (sin romper por no tener `userId` de
  persona).
- La autorización de recursos permita al servicio (como Admin) **sin** el filtro por negocio propio.

### 5. (Opcional, recomendado a futuro) "On-behalf-of" — usuario que actúa

Para atribuir correctamente las acciones (quién creó realmente la cita) y poder aplicar reglas por
usuario, SoundMate podría pasar el **usuario en cuyo nombre actúa** (p. ej. cabecera
`X-Acting-User: <id>` o un claim). Agendia, al ver un token de servicio válido + ese contexto,
fijaría el `ICurrentUserContext` a ese usuario.

**Para esta primera versión se puede DEJAR FUERA**: el token de servicio actúa con privilegios de
servicio y **SoundMate hace su propia validación por usuario** en su lado (SoundMate valida que el
usuario tiene membresía activa antes de delegar). Anótalo como mejora futura.

## Encaje en la arquitectura de Agendia (Clean Architecture + CQRS/MediatR)

Sigue los patrones existentes del proyecto:

- **Application**: un comando + handler (p. ej. `AuthenticateServiceCommand` →
  `AuthenticateServiceCommandHandler`) que valida el `clientId`/`clientSecret` y genera el token,
  reutilizando el servicio de firma JWT ya existente. Su **validador FluentValidation** (clientId y
  secret obligatorios), como el resto de comandos.
- **DTOs**: `ServiceTokenRequestDto` (`clientId`, `clientSecret`) y `ServiceTokenResponseDto`
  (`accessToken`, `expiresAt`, `tokenType`).
- **Api**: exponer el endpoint donde vivan hoy los demás endpoints de `/api/auth/*` (controlador o
  minimal API — usa el mismo mecanismo).
- **Config**: binding de `ServiceClients` y de los ajustes del token de servicio (vida, rol).
- **Infrastructure**: si eliges guardar clientes en BD, su entidad + repositorio + migración; si es
  por configuración, no hace falta migración.

## Seguridad (requisitos)

- `clientSecret` **hasheado** en reposo; secretos reales en **user-secrets / variables de entorno**,
  nunca commiteados.
- Validación del secreto en **tiempo constante**.
- El token de servicio **no** debe poder obtenerlo un usuario final (solo con el secreto).
- **Auditar** las autenticaciones de servicio (si hay `AuditLog`, registrar `client_id` + resultado).
- (Opcional) rate-limiting / bloqueo tras N fallos en el endpoint.

## Tests

- **Unitarios** del handler: cliente válido → token con los claims correctos; secreto incorrecto →
  falla; cliente deshabilitado → falla; cliente inexistente → falla.
- **Integración** del endpoint: `200` con token válido; `401` con secreto malo.
- Verificar que **un endpoint protegido acepta el token de servicio** (p. ej. `GET /api/auth/me` o
  un `GET` de negocio) y que puede operar sin filtro de negocio (crear/consultar una cita de
  cualquier negocio).

## Criterios de aceptación

- [ ] `POST /api/auth/service-token` devuelve un JWT válido con `clientId`+`clientSecret` correctos.
- [ ] El token se firma con la misma clave/issuer/audience que los de usuario y **lo aceptan** los
      endpoints protegidos existentes.
- [ ] El token de servicio puede operar **a través de negocios** (no queda atado a uno).
- [ ] Secreto guardado **hasheado**; comparación en tiempo constante; secretos fuera del repo.
- [ ] **No** se emite refresh token para el flujo de servicio.
- [ ] El **flujo de usuario existente sigue intacto** (login/register/refresh sin cambios).
- [ ] Tests unitarios + de integración en verde.

## Fuera de alcance (no tocar)

- La autenticación de **usuario** existente (login/register/refresh/logout) — no se modifica.
- No añadir refresh token al flujo de servicio.
- El "on-behalf-of" (punto 5) queda para una iteración futura salvo que sea trivial.

## Lo que Agendia debe devolver a SoundMate (para consumirlo)

Al terminar, documenta para SoundMate:

- **URL del endpoint** de token (`POST /api/auth/service-token`) y la **base URL** de Agendia.
- El **`clientId`** de SoundMate y **cómo se le entrega el `clientSecret`** (para meterlo en los
  user-secrets de SoundMate).
- **Vida** del token de servicio (para que SoundMate lo cachee y re-pida a tiempo).
- El **rol/permisos** que lleva el token (qué endpoints puede consumir).
- El JSON exacto de **request y response** del endpoint.
