# Technology & architecture learning guide

[Türkçe](tech-learning.tr.md) · [Deutsch](tech-learning.de.md)

**Goal:** Grow BankApp as a product while learning *why* each piece of the stack exists.  
**Related:** [roadmap.en.md](roadmap.en.md) · [architecture-notes.en.md](architecture-notes.en.md) · [README.md](../README.md)

> **Beginner deep-dive (commands, classes, packages, build order):** [learning/00-index.tr.md](learning/00-index.tr.md) (Turkish library; kept up to date until the project ends).

Not a dump of “what we installed” — each item covers **what / why here / benefits / trade-offs / repo example**.

---

## How to read this

1. Start with **Architecture** — everything else hangs off it.
2. Trace one feature end-to-end (e.g. credit approve → Outbox → Saga → SignalR).
3. Open the **Example** paths in the repo.
4. “When not to use” notes help avoid over-engineering even in a learning project.

Suggested order: Auth/JWT → CQRS handler → Domain Event + Outbox → Idempotency → Saga → SignalR → Modular Monolith.

---

## 1) Architecture

### Clean Architecture

| | |
|--|--|
| **What** | Dependencies point inward: `Domain` → `Application` → `Persistence` / `WebApi`. |
| **Why** | Make “where does the business rule live?” obvious while learning. |
| **Benefit** | Testability; domain survives framework churn. |
| **Trade-off** | More projects/folders; heavy for tiny CRUD. |
| **Example** | `src/Domain`, `src/Application`, `src/Persistence`, `src/WebApi`, `src/Core` |

### CQRS + MediatR

| | |
|--|--|
| **What** | Separate write (Command) and read (Query). MediatR routes controller → handler. |
| **Why** | Keep approve/reject/cancel distinct from list/detail; easy pipelines (validation, auth, cache). |
| **Benefit** | Single-purpose handlers; cross-cutting via `IPipelineBehavior`. |
| **Trade-off** | Extra indirection on trivial endpoints. |
| **Example** | `ApproveCreditApplicationRequest` under `Application/Modules/Credit/Features/...` · wiring: `ApplicationServiceRegistration.cs` |

### Modular Monolith

| | |
|--|--|
| **What** | One deploy / one DB; code split by modules: Identity, Customers, Credit, Notification. |
| **Why** | Learn bounded contexts without microservice ops. |
| **Benefit** | Clear ownership; path toward future extraction. |
| **Trade-off** | Soft boundaries — discipline required. |
| **Example** | `Application/Modules/*` · `WebApi/Modules/*` · `AddApplicationModules()` |

### Aggregate + Domain Events

| | |
|--|--|
| **What** | Aggregate is the consistency boundary; important changes raise `IDomainEvent`. |
| **Why** | Connect side effects (saga, SignalR) to the same business fact. |
| **Benefit** | Domain stays free of RabbitMQ/SignalR details. |
| **Trade-off** | Event design takes care; too many events = noise. |
| **Example** | Domain entities/events · Persistence harvest into Outbox |

### Transactional Outbox

| | |
|--|--|
| **What** | Persist the event in `OutboxMessages`, then a worker publishes to RabbitMQ. |
| **Why** | Avoid dual-write: DB committed but message never sent. |
| **Benefit** | Solid base for at-least-once delivery and retries. |
| **Trade-off** | Small delay; consumers must be idempotent. |
| **Example** | `OutboxMessages` · `OutboxDispatcherHostedService` · queue `domain-events` |

```text
Handler → SaveChanges (aggregate + outbox row)
       → Dispatcher publish → RabbitMQ
       → Consumer → Saga + SignalR
```

### Saga / Process Manager

| | |
|--|--|
| **What** | State machine for a long-running flow (credit application lifecycle). |
| **Why** | Track Created → decision → onboarding stubs in one place. |
| **Benefit** | Queryable process state (`CreditApplicationSagas`). |
| **Trade-off** | This sample is **orchestration** (process manager), not choreography. |
| **Example** | `CreditApplicationProcessManager` · called from the domain-events consumer |

### Idempotency-Key

| | |
|--|--|
| **What** | Client sends a unique key on critical writes; server stores/replays via Redis. |
| **Why** | Double-click and retry-after-timeout must not double-approve. |
| **Benefit** | Safe writes on unreliable networks. |
| **Trade-off** | Client must generate keys; same key + different body → 409. |
| **Example** | Header `Idempotency-Key` · `IdempotencyKeyFilter` · React credit write calls |

---

## 2) Backend stack

### .NET 10 + ASP.NET Core

Host, DI, middleware, JWT, SignalR, health checks — enterprise patterns on a current runtime.

**Example:** `WebApi` · `ServiceCollectionExtensions` · `ApplicationBuilderExtensions`

### EF Core + SQL Server

Relational model, migrations, TPT customers, outbox/saga tables.

**Example:** `Persistence` · Compose service `bankapp-sql`

### FluentValidation / AutoMapper / SmartEnum

Input rules before handlers; DTO mapping in feature `Profiles`; named enums instead of magic ints.

### JWT + RBAC

Roles: Customer, BankStaff, BankManager. Keycloak/OIDC later (roadmap).

### Serilog → Seq + ProblemDetails

Structured logs for local investigation; consistent API errors for the client.

---

## 3) Infrastructure & realtime

| Tech | Role in BankApp |
|------|------------------|
| **Redis** | Cache + idempotency store |
| **RabbitMQ** | `email-confirmation`, `domain-events` |
| **SignalR** | Hub `/hubs/credit-applications` after Outbox consume |
| **OpenTelemetry** | OTLP traces + metrics → collector → Tempo / Prometheus / Grafana `:3001` |
| **Vault (lab)** | Dev mode + `secret/bankapp` seed (WebApi not wired yet) |
| **Docker Compose** | Full local stack with healthchecks — `infra/docker-compose.yml` |

**Client hook:** `client/src/realtime/useCreditApplicationRealtime.ts`  
**OTel code:** `WebApi/Extensions/OpenTelemetryExtensions.cs` · Seq gets `TraceId`/`SpanId` via `ActivityEnricher`

---

## 4) Frontend

React + Vite + TypeScript SPA; react-i18next (TR/EN/DE); SignalR client; UUID `Idempotency-Key` on credit writes — so backend patterns are visible in the UI too.

---

## 5) Quality & CI

xUnit unit tests, WebApi integration tests, GitHub Actions (build + tests + client). Tests are the safety net for architectural refactors.

---

## 6) End-to-end: credit approve

```text
[React] Approve + Idempotency-Key
    → [WebApi] CreditApplicationsController
    → [MediatR] Approve handler
    → [Domain] status + domain event
    → [EF] application + OutboxMessages (one SaveChanges)
    → [Dispatcher] RabbitMQ domain-events
    → [Consumer] ProcessManager + SignalR groups
    → [React] toast + list refresh
```

One flow shows CQRS, domain events, outbox, saga, idempotency, and SignalR together — the highest-value learning path.

---

## 7) When not to use (short)

| Pattern | Skip early if… |
|---------|----------------|
| Outbox / Saga | Single-table CRUD, sync is enough |
| Modular folders | Tiny 2–3 feature toy API |
| Idempotency | Read-only or naturally idempotent GETs |
| SignalR | Rare updates; polling is fine |
| Vault | Pure `.env` demo (lab still useful) |

---

## 8) Later (roadmap)

Keycloak, Elasticsearch, K8s/ArgoCD, chatbot — see [architecture-notes.en.md](architecture-notes.en.md) and [roadmap.en.md](roadmap.en.md). OTel is already in the stack; walk it in Grafana, then apply the same what/why/example template to the next layer.
