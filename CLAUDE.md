# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

Harmony is a freshly scaffolded .NET solution. All four projects currently contain only template
placeholder code (`Class1.cs` stubs, the default `WeatherForecastController`) — there is no
real domain logic, no tests, and no README yet. Expect to be building this up from near-zero.

## Commands

```
dotnet build                                    # build the whole solution (Harmony.slnx)
dotnet run --project Harmony.API                # run the Web API
dotnet watch --project Harmony.API run          # run with hot reload
```

There is no test project yet (the `Harmony.slnx` solution has an empty `/test/` folder reserved
for one). Once test projects exist, run a single test with:
```
dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"
```

## Architecture

Clean Architecture layering across four projects, referenced in one direction only
(outer layers depend on inner ones, never the reverse). Per the `.csproj` `ProjectReference`s:
- **Harmony.Domain** — no project references. Innermost layer; entities/domain logic go here.
- **Harmony.Application** — references `Harmony.Domain`. Application/use-case logic goes here.
- **Harmony.Infrastructure** — references `Harmony.Application`. External concerns (persistence,
  I/O, third-party integrations) go here.
- **Harmony.API** — references `Harmony.Application` and `Harmony.Infrastructure` directly (ASP.NET
  Core minimal-hosting Web API, `Program.cs` top-level statements). Controllers live under
  `Harmony.API/Controllers`.

Keep new code consistent with this dependency direction: Domain has no outward dependencies,
Application depends only on Domain, Infrastructure depends only on Application, and API composes
Infrastructure + Application.

All four projects target `net10.0` with `<Nullable>enable</Nullable>` and
`<ImplicitUsings>enable</ImplicitUsings>`.

The solution file is `Harmony.slnx` (the new XML-based slnx format, not a `.sln`).
