# 02 — Clean Architecture

[← Previous](01-dotnet-csharp.md) · [Contents](00-index.md) · [Next →](03-ef-core-sql.md)

## What is this?

**Clean Architecture** splits code into layers: **business rules** stay near the center, **UI / DB / framework** stay on the outside.  
Dependency arrows point **inward**: outer layers know inner ones; inner layers do not know outer ones.

**Analogy:** The company **core** (Domain) is inside; the counter (WebApi) and archive (Persistence) are outside. The core does not know the counter’s brand; the counter follows the core’s rules.

```mermaid
flowchart TB
  subgraph outer["Outer (framework / I/O)"]
    WEB["WebApi\nHTTP, JWT, SignalR"]
    PERS["Persistence\nEF Core, repositories"]
  end
  subgraph middle["Application"]
    APP["Use cases\nCommands, queries, DTOs"]
  end
  subgraph core["Domain"]
    DOM["Entities, SmartEnum\ndomain events"]
  end
  WEB --> APP
  PERS --> APP
  APP --> DOM
  WEB -.->|implements| PERS
```

## Role in BankApp

BankApp layers:

| Layer | Responsibility |
|--------|--------|
| **Domain** | Entity, enum/SmartEnum, domain event — independent of frameworks |
| **Application** | Use cases (command/query handlers), rules, DTOs |
| **Persistence** | EF Core, repository, seed |
| **WebApi** | HTTP, JWT, SignalR, hosted services |
| **Core** | Shared pipeline / exception / security helpers |

## When added?

**Phase 1**.

## Core concepts

| Concept | Meaning |
|--------|--------|
| **Dependency Inversion** | High-level code depends on abstractions (`interface`) |
| **DTO** | Plain data object for API input/output |
| **Business rule** | Domain/application rule such as “Only Pending can be approved” |
| **Repository** | Interface that abstracts data access |

```mermaid
sequenceDiagram
  participant UI as WebApi
  participant App as Application
  participant Dom as Domain
  participant Per as Persistence
  UI->>App: Mediator.Send command
  App->>Dom: Business rule
  App->>Per: Repository
  Per-->>App: Entity
  App-->>UI: DTO response
```

## Commands

Clean Architecture is not a CLI tool; layers are wired with project references:

```xml
<!-- Application references Domain (example idea) -->
<ProjectReference Include="..\Domain\Domain.csproj" />
```

Wrong direction: `Domain` → `WebApi` reference **must not** exist.

## Classes, methods, packages

- Registration: `ApplicationServiceRegistration.cs`, `PersistenceServiceRegistration.cs`
- WebApi `Program.cs` wires layers with `builder.Services.Add…()`.
- Module split (later): `Application/Modules/*`, `WebApi/Modules/*` → [17](17-modular-monolith.md)

## Where in the repo?

```
src/Domain/
src/Application/
src/Persistence/
src/WebApi/
src/Core/
```

## Mini flow

**Approve button** → `WebApi` controller → `Application` handler → `Domain` aggregate method → `Persistence` `SaveChanges` → (later) Outbox / SignalR.

Screen (example):

<img src="../../assets/screenshots/en/Api%20Documents%20with%20Scalar.png" alt="Scalar API" width="720" />

## Beginner lab

1. Open `src/Domain/Domain.csproj` — it must **not** reference `WebApi`.
2. Check `src/Application/Application.csproj` — references should point at `Domain` (and `Core` if needed).
3. Find a command handler under `Features/.../Commands/` — no raw SQL/HTTP, only repository interfaces.
4. Trace `AddApplication` + `AddPersistence` in `src/WebApi/Program.cs`.
5. Try adding a forbidden reference (Domain→WebApi) — the compiler should block it.

## Common mistakes

- Putting EF or HTTP inside `Domain` → layer violation.  
- Using `DbContext` directly in controllers → move logic to handlers.  
- Confusing DTOs with domain entities → keep API DTOs separate.  

## Where it helps

- [ ] Dependency graph always points inward  
- [ ] New features start in Application/Domain  
- [ ] Persistence changes barely touch WebApi  
