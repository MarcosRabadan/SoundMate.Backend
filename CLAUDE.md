# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What SoundMate is

SoundMate is the core service of a SaaS for **music teachers and academies**. It owns the
**business and identity**: users, academies, memberships, the discipline/genre catalogs, and the
teaching profile (specialty, reviews, education).

Scheduling is a **separate microservice, Agendia**, which owns calendars and bookings. The boundary:

- **SoundMate** answers *who is who* and *what relationship they have*.
- **Agendia** answers *when lessons happen* (recurring "fixed" lessons and one-off ones).

SoundMate validates before delegating: when a booking comes in, it checks the user has an **active
membership** in that academy (`IMembershipRepository.HasActiveMembershipAsync`) before calling
Agendia. SoundMate must **not** duplicate Agendia's bookings — only reference them. More
microservices will follow the same pattern.

**Which token goes on which call.** Two, and mixing them up is the mistake to avoid:

- **The teacher's own JWT, forwarded.** For anything done *on a person's behalf* — booking,
  moving or cancelling a lesson. Agendia reads the `sub` to decide what that person owns and
  writes it to its audit trail. SoundMate signs those tokens; Agendia only validates them.
- **The machine-to-machine token** (`POST /api/auth/service-token`, `AgendiaServiceTokenProvider`).
  Only for *provisioning* and service-level calls: creating the Agendia `Business` when an
  `Academy` is created, the `Employee` when a teacher joins.

Sending everything M2M would work and is tempting, but Agendia's M2M token carries the `Admin`
role and its `sub` is the **clientId**, not a person: every per-resource check there would pass
unconditionally, tenant isolation would rest entirely on SoundMate, and Agendia's audit log would
record `soundmate` instead of the teacher.

## Commands

```
dotnet build                                       # build the whole solution (SoundMate.slnx)
dotnet run --project SoundMate.API                 # run the Web API
dotnet watch --project SoundMate.API run           # run with hot reload
dotnet test                                        # run all tests
dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"   # single test
```

Local infrastructure runs in **Docker** (`deploy/`, see its README): PostgreSQL **5434**, Seq
**5342**, RabbitMQ **15673**. The ports are offset from Agendia's on purpose — both microservices
are developed at the same time, and 5432 is taken by a native PostgreSQL.

```
cd deploy && docker compose up -d                         # infra only; the API runs with dotnet run
cd deploy && docker compose --profile app up -d --build   # infra + the API in a container (8080)
cd deploy && docker compose down                          # stop, keeping the volumes
```

The API container sits behind a **profile**, so a plain `up -d` does not start it: the daily loop is
`dotnet run` against containerized infra. It applies **no migrations at startup** — you still run
`dotnet ef database update` from the host against 5434.

EF Core (code-first, PostgreSQL/Npgsql). Migrations use **the API as the startup project** (it holds
the `Microsoft.EntityFrameworkCore.Design` package; there is no design-time factory). The connection
string lives in **user-secrets**, never committed — `appsettings.json` only has a placeholder — so
commands that hit the DB need the Development environment for the secret to load:

```
dotnet ef migrations add <Name> --project SoundMate.Infrastructure --startup-project SoundMate.API --output-dir Persistence/Migrations
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet ef database update --project SoundMate.Infrastructure --startup-project SoundMate.API
dotnet ef migrations has-pending-model-changes --project SoundMate.Infrastructure --startup-project SoundMate.API
```

Set the connection string once per developer (stored outside the repo):
```
dotnet user-secrets set "ConnectionStrings:SoundMate" "Host=localhost;Port=5434;Database=soundmate;Username=soundmate;Password=soundmate" --project SoundMate.API
```

## Architecture

Clean Architecture across four projects, referenced one direction only (outer → inner):

- **SoundMate.Domain** — no project references. Entities, value objects, domain rules.
- **SoundMate.Application** — references Domain. Use-case logic and **repository interfaces**
  (`Abstractions/Persistence`).
- **SoundMate.Infrastructure** — references Application. EF Core (`Persistence/`): `DbContext`,
  entity configurations, repository implementations, migrations.
- **SoundMate.API** — references Application + Infrastructure. ASP.NET Core Web API.

All projects target `net10.0` with `Nullable` and `ImplicitUsings` enabled. Solution file is
`SoundMate.slnx` (XML slnx format).

## Database (PostgreSQL)

- **PostgreSQL** via the **Npgsql** EF Core provider (`UseNpgsql`). Local dev DB is `soundmate`,
  in the **`soundmate-postgres` container on port 5434** (`deploy/docker-compose.yml`), user/password
  `soundmate`. A native PostgreSQL still holds 5432 and Agendia's container holds 5433.
- Email uniqueness relies on the **`citext`** type (case-insensitive) — Postgres is case-sensitive by
  default, unlike SQL Server. The `citext` extension is enabled in `OnModelCreating`
  (`HasPostgresExtension("citext")`).
- Tables/columns are created with **quoted PascalCase** names (`"Users"`, `"Disciplines"`), so raw SQL
  must double-quote them — `SELECT * FROM "Disciplines"`, not `FROM Disciplines` (Postgres lowercases
  unquoted identifiers).
- Check constraints in configurations use Postgres identifier quoting (`"Stars" >= 1 AND ...`).

## Domain conventions (follow these)

- **Rich domain model (DDD), NOT anemic.** Entities have **private setters** and are created only
  through **static factories** (`User.Register`, `Academy.Create`, `Membership.Create`, ...) that
  validate invariants, and mutated only through **behavior methods** (`membership.Leave()` sets
  status and date together, `academy.Cancel()`, ...). An aggregate can never exist in an invalid
  state — errors fail fast at construction, not late at `SaveChanges`. Use `Common.Guard` for
  guard clauses; throw `DomainException` on invariant violations.
- **Strongly-typed IDs** (`UserId`, `AcademyId`, ...): `readonly record struct` wrapping a `Guid`,
  with `New()`/`From()`. Backed by `uuid` in the DB. Factories generate the Id, so it is never
  forgotten; `Entity.Id` is `protected set` (immutable from outside).
- **Value Objects** for things with rules: `Email` (validates + normalizes; equality is
  case-insensitive to keep one email = one global person; stored as `citext`) and `Slug`.
- **Aggregates reference each other by identity**, never by navigation (e.g. `Membership` holds
  `UserId` + `AcademyId`). **No enforced cross-aggregate FKs** — only indexes — to keep a future
  DB-per-service split cheap.
- **Repository pattern**: one interface per aggregate root in `Application/Abstractions/Persistence`,
  implemented `internal` in `Infrastructure/Persistence/Repositories`. `IUnitOfWork` commits.
- **EF mapping lives only in Infrastructure** (`IEntityTypeConfiguration` per entity, picked up by
  `ApplyConfigurationsFromAssembly`). Domain stays persistence-ignorant. Value objects and typed
  IDs are mapped with `HasConversion`.
- **Language**: code and identifiers in English; XML `/// <summary>` on every entity, plus comments
  only where the *why* is non-obvious (not on trivial members).
- **Time**: everything UTC, `timestamp with time zone` (`DateTime.UtcNow` in factories). Enums stored
  as `int` with **explicit values** (reordering must not corrupt data).
- **Catalogs** (`Discipline`, `Genre`) are reference data seeded via `HasData` with **stable GUIDs**
  grouped by category. Never delete a catalog row — soft-hide with `IsActive`.

## Domain model

- **Identity**: `User` (unique, global person) · `Academy` (organization; `AcademyType.SoloTeacher`
  is a private teacher) · `Membership` (person↔academy with role; the "anchor" that always exists
  when there is any relationship).
- **Skill**: `MusicLevel` is **per discipline**, held in `UserDiscipline` (user studies discipline
  X at level Y). A teacher-only user simply has no rows. `Discipline` catalog spans instruments and
  music-theory subjects, grouped by `DisciplineCategory` (families + `MusicTheory`).
- **Teaching profile**: `UserProfile` (bio/avatar, 1:1, anyone) · `UserEducation` (diplomas, 1:N,
  open year range) · `TeacherDiscipline` + `TeacherGenre` (specialty, **global** to the teacher) ·
  `Genre` catalog · `TeacherReview` (rating **per academy**, 1–5 stars, no self-review). The star
  rating shown is the **average of reviews**, computed on the fly (compute now, cache later only if
  reads hurt) — never a hand-set field.

## Application conventions (follow these)

- **Controller → Service → Repository. No MediatR.** Agendia has one, but its 61 handlers are
  1–3 line pass-throughs to 30 services and it has exactly one behavior, so 122 files buy a hook
  for FluentValidation that a 30-line action filter gives for free. Do not add a Command/Handler
  pair per operation here; the service *is* the use case. Agendia will be revisited separately.
- **No AutoMapper. Map by hand** (`Users/UserMapper.cs`, static extension methods). Its DoS
  advisory GHSA-rvv3-g6hj-g44x is only fixed from 15.1.1/16.1.1, past the point where it stopped
  being MIT — every free version is affected. It also earned nothing here: typed ids and value
  objects force every member to be declared explicitly anyway.
- **Response DTOs use `required` on everything that is not optional.** That is what replaces
  AutoMapper's `AssertConfigurationIsValid`: a forgotten field is `error CS9035` at build time.
- **Validation is FluentValidation, run by `API/Filters/ValidationFilter.cs`**, registered globally
  so no endpoint has to remember. Not `FluentValidation.AspNetCore` — it stopped at 11.3.1 and
  never followed FluentValidation 12.
- **One rule, one place.** A validator must not restate a domain rule in its own words: expose the
  check on the value object (`Email.IsValid`) and call it. FluentValidation's `.EmailAddress()`
  accepts `missing@domain`, `Email.Create` does not, and that gap sent malformed input past the
  400 and into a thrown invariant.
- **Errors:** `DomainException` → 400, conflicts over existing state → 409, everything else → 500
  with a **generic** detail (`GlobalExceptionHandler`). Never put an unexpected exception's message
  on the wire.
- **Persistence errors get translated in Infrastructure, never sniffed in Application.** `UnitOfWork`
  turns Postgres' 23505 into `UniqueConstraintViolationException`; Application references neither
  EF Core nor Npgsql. A uniqueness check followed by a save is not atomic, so any use case that
  does one must also handle losing the race — otherwise it is a 500.

## Testing

`test/SoundMate.Domain.Tests` (xUnit + Shouldly). Domain tests are pure (no DB, no mocks) and must
cover **every invariant — both happy path and each guard/failure**. Keep them green when changing
the domain.

`test/SoundMate.Application.Tests` uses **hand-written fakes, no mocking library** — the same style
as `SoundMate.Infrastructure.Tests`. `FakeUnitOfWork.FailWithUniqueViolationOn` exists to reach the
lost-race path, which no amount of in-memory set-up reproduces on its own.

## Versioning and docs

- **One version, one place**: `<Version>` in `Directory.Build.props`. The SDK stamps it onto every
  assembly, `API/BuildInfo.cs` reads it back from the attributes, and from there it reaches the
  OpenAPI document, the Scalar title and `GET /api/version`. **Never hardcode a version anywhere
  else** — two places to change is one place to forget.
- **SemVer, and MAJOR is 0**, so the HTTP contract is not stable: a MINOR bump may break it. Saying
  so is `CHANGELOG.md`'s job, under **Cambios incompatibles**.
- **Every MINOR gets a page in `docs/wiki/`** (`vX.Y.Z.md`), with a functional half and a technical
  half, and a row in `docs/wiki/README.md`. Old pages are kept and marked historical rather than
  edited — the point of a version page is what was true then. Same shape as Agendia's wiki.
- `CHANGELOG.md` is for what a user or a developer would notice. Refactors that change nothing
  observable do not belong in it; they belong here.

## Current state

Done: full rich domain model + EF configurations + `InitialIdentity` migration (seeded catalogs) +
repositories for **all 11 aggregates** + the domain test suite (129 tests). DI is **wired**
(`AddInfrastructure` registers the `DbContext`, all repositories and `IUnitOfWork`; called from the
API's `Program.cs`). The migration has been **applied to a real PostgreSQL database**.

Done, on the **Agendia integration**: the M2M client (`Infrastructure/Agendia/`) authenticates with
client-credentials, caches the short-lived token, and checks the connection through Agendia's
`/api/ping`, which echoes back the identity it read. `GET /api/agendia/connection` exposes it in
**Development only** — SoundMate has no authentication yet, so it would otherwise publish our
clientId anonymously; gate it behind an admin policy once auth lands rather than deleting it.

Done, on **Docker** (`deploy/`): `docker-compose.yml` with PostgreSQL, Seq and RabbitMQ, plus
`SoundMate.API/Dockerfile` (multi-stage, non-root, HTTP-only on 8080) behind the `app` profile.
The compose project is named `soundmate` explicitly — Docker would derive it from the folder, and
Agendia's compose also lives in a folder called `deploy`, so they would share a project and a
`down` in one repo would reach into the other. Seq and RabbitMQ are up but **not wired** yet: no
Serilog, no event transport.

From inside the container Agendia is `https://host.docker.internal:7097` — `localhost` there is the
container itself. That hop needs `Agendia:DangerousAcceptAnyServerCertificate`, off by default and
set **only** in `deploy/docker-compose.yml`: Agendia's HTTP port answers `307` to the HTTPS one, and
the ASP.NET dev certificate fails twice over — issued for `localhost`, and its CA is not in the
container's trust store. The problem is the local certificate, not Agendia. Verified end to end:
`GET /api/agendia/connection` from the container returns `succeeded: true`, `subject: soundmate`,
`tokenUse: service`.

Done, on the **Application layer** (issue #6): `AddApplication()`, a global `ValidationFilter`, a
`GlobalExceptionHandler`, `Pbkdf2PasswordHasher`, and the full `User` surface — register, read by
id, read by email, update, change password, verify email, suspend, reactivate, delete, restore and
purge. `test/SoundMate.Application.Tests` is a new project (hand-written fakes, no mocking library,
matching `SoundMate.Infrastructure.Tests`). 260 tests green.

**A user has two independent states, and conflating them is the mistake to avoid.** `Status`
(`Active`/`Suspended`) is a moderation decision about somebody who is still here. `DeletedAtUtc`
(the `UserSoftDelete` migration) is a lifecycle fact about the record. Deleting never touches
`Status`, so restoring a suspended user brings the suspension back exactly — which is why deletion
is **not** a third `UserStatus` value: folded into the enum, the suspension would be lost and
restore would have to guess.

Soft-deleted users are invisible to every read and every mutation (`UserNotFoundException`); only
`RestoreAsync` and `PurgeAsync` see past it, and `User` itself refuses every behaviour method while
deleted. Their **email stays reserved**, on purpose: the row still holds it in the unique index,
and eight tables still point at that `UserId`, so handing the address to somebody new would create
a second person wearing the first one's identity.

`DELETE /api/users/{id}` is the soft one. **`DELETE /api/users/{id}/permanent` really removes the
row** — a separate route rather than a flag so it cannot be reached by forgetting a default. It
refuses while a `Membership` exists (the anchor relationship) but still orphans `UserProfile`,
`UserEducation`, `UserDiscipline`, `TeacherDiscipline`, `TeacherGenre` and `TeacherReview` rows,
and Agendia keeps its `Employee`. It wants a real cascade before it is used in anger.

Done, on **academies** (issue #8): the same shape one layer over — `AcademiesController`,
`AcademyService`, `AcademyMapper`, validators, and `Academy` given the same soft delete as `User`
(`AcademySoftDelete` migration). 387 tests green.

**`Cancel` used to be a one-way door**, and combined with the soft delete it was a dead end: a
cancelled academy that was also deleted came back cancelled from `Restore`, and nothing could move
it out of that state — its slug stayed held, its history stranded. `Academy.Reopen()` /
`POST /api/academies/{id}/reopen` is the way out. It is deliberately narrow: it only undoes a
cancellation, so a **suspended** academy stays suspended — lifting a suspension is `Activate`'s
job, and doing both here would let a reopen quietly wave away a moderation decision.

Three things were fixed in the domain on the way. **`Slug.IsValid`** now exists, for the reason
`Email.IsValid` does. **`Academy`'s guard was incomplete**: `EnsureNotCancelled` only covered
`Suspend`/`Activate`, so a cancelled academy could still be renamed, re-slugged and moved between
plans — it now guards every mutator, deleted included. And **two indexes existed for queries
nothing could make**: `IX_Academies_OwnerId` and `IX_Memberships_AcademyId` now have
`ListByOwnerAsync` and `ListByAcademyAsync` behind them.

**Creating an academy also creates its owner's `Owner` membership, in the same `SaveChanges`.**
Not a convenience: `Membership` is the anchor, and `HasActiveMembershipAsync` is the gate every
booking passes, so an academy without it would be born claiming its owner does not belong to it.

Enums cross the wire **by name** (`"SoloTeacher"`, not `2`) via `JsonStringEnumConverter` in
`Program.cs`; numbers are still accepted inbound. The numeric values are a storage detail and the
HTTP contract should not inherit them.

**Response DTOs therefore hold real enums, not strings.** With that converter registered the wire
format is identical either way, so the string bought nothing and cost the type, the OpenAPI enum
documentation, and a `.ToString()` per field. It does mean the contract depends on a registration
in another project that nothing else would miss — so `JsonContractTests` asserts the serialized
shape directly. Remove the converter and those tests fail, instead of every consumer.

Done, on the **user profile** (issue #11): `GET`/`PUT`/`DELETE` on
`/api/users/{userId}/profile` — a singleton sub-resource, because the profile is only ever reached
through its owner and a `UserProfileId` never leaves the database. 440 tests green.

**`PUT` is an upsert, and that is the interesting part.** It creates the profile when there is none,
so a caller never has to find out first whether a row exists. Which means losing the race against
`IX_UserProfiles_UserId` must **not** answer 409 — a PUT promises idempotence and the resource now
exists — so the service re-reads the winning row and applies to it. That retry only works because
`UnitOfWork` now **detaches the entries a failed save left behind**: they stay `Added` otherwise and
the next `SaveChanges` replays the very insert the index rejected.

`PUT` also replaces the whole profile: a body with only a description **clears the avatar**. An
empty profile is a legitimate state, and distinct from having none — hence two different 404s,
`UserNotFoundException` and `UserProfileNotFoundException`.

`UserProfile.IsValidAvatarUrl` follows `Email.IsValid` and `Slug.IsValid`: absolute http(s) only,
enforced by the aggregate and *called* by the validator rather than restated in it. Deleting a
profile is a plain delete — nothing references a `UserProfileId`, so there is nothing to orphan, and
it is content rather than identity.

And **nothing is authenticated**, so every route is open — `GET /api/users?email=` is a
user-enumeration oracle, anyone can open an academy in somebody else's name, and anyone can rewrite
anyone's bio. All want an admin/self policy the moment auth lands, same treatment as
`/api/agendia/connection`.

Pending: the remaining use cases (create academy, memberships, teaching profile), a **`SaveChanges`
interceptor** to fill `CreatedAtUtc`/`UpdatedAtUtc`, **signing the user JWTs** Agendia expects
(HS256 with the shared key, `iss` `SoundMate`, `aud` `MRC.Agendia.Clients`, and the SHORT `sub`
and `role` claims — the long claim URIs authenticate but then fail every authorization check in
Agendia), and the **provisioning bridge** `Academy`→`Business` / `Membership`→`Employee`.
