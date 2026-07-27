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
membership** in that academy (`IMembershipRepository.HasActiveMembershipAsync`), then talks to
Agendia machine-to-machine. SoundMate must **not** duplicate Agendia's bookings — only reference
them. More microservices will follow the same pattern.

## Commands

```
dotnet build                                       # build the whole solution (SoundMate.slnx)
dotnet run --project SoundMate.API                 # run the Web API
dotnet watch --project SoundMate.API run           # run with hot reload
dotnet test                                        # run all tests
dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"   # single test
```

EF Core (code-first). The design-time factory lets these run without starting the API:

```
dotnet ef migrations add <Name> --project SoundMate.Infrastructure --startup-project SoundMate.Infrastructure --output-dir Persistence/Migrations
dotnet ef database update      --project SoundMate.Infrastructure --startup-project SoundMate.Infrastructure
dotnet ef migrations has-pending-model-changes --project SoundMate.Infrastructure --startup-project SoundMate.Infrastructure
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
`SoundMate.slnx` (XML slnx format). Database is **SQL Server**.

## Domain conventions (follow these)

- **Rich domain model (DDD), NOT anemic.** Entities have **private setters** and are created only
  through **static factories** (`User.Register`, `Academy.Create`, `Membership.Create`, ...) that
  validate invariants, and mutated only through **behavior methods** (`membership.Leave()` sets
  status and date together, `academy.Cancel()`, ...). An aggregate can never exist in an invalid
  state — errors fail fast at construction, not late at `SaveChanges`. Use `Common.Guard` for
  guard clauses; throw `DomainException` on invariant violations.
- **Strongly-typed IDs** (`UserId`, `AcademyId`, ...): `readonly record struct` wrapping a `Guid`,
  with `New()`/`From()`. Backed by `uniqueidentifier` in the DB. Factories generate the Id, so it
  is never forgotten; `Entity.Id` is `protected set` (immutable from outside).
- **Value Objects** for things with rules: `Email` (validates + normalizes; equality is
  case-insensitive to keep one email = one global person) and `Slug`.
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
- **Time**: everything UTC, `datetime2`. Enums stored as `int` with **explicit values**
  (reordering must not corrupt data).
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

## Testing

`test/SoundMate.Domain.Tests` (xUnit + Shouldly). Domain tests are pure (no DB, no mocks) and must
cover **every invariant — both happy path and each guard/failure**. Keep them green when changing
the domain.

## Current state

Built: full domain model + EF configurations + one `InitialIdentity` migration (seeded catalogs)
+ repositories for all aggregates + the domain test suite.

Pending: register `DbContext`/repositories in DI (`AddInfrastructure` + API wiring), apply the
migration to SQL Server, clean the API template leftovers (`WeatherForecast*`, default `Program.cs`),
and build the Application use cases (which also need timestamps set — e.g. a `SaveChanges`
interceptor — and the Agendia integration).
