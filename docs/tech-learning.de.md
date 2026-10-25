# Technologie- & Architektur-Lernleitfaden

[Türkçe](tech-learning.tr.md) · [English](tech-learning.en.md)

**Ziel:** BankApp als Produkt weiterbauen und gleichzeitig verstehen, *warum* der Stack so gewählt ist.  
**Verwandt:** [roadmap.de.md](roadmap.de.md) · [architecture-notes.de.md](architecture-notes.de.md) · [README.de.md](../README.de.md)

Kein Installationskatalog — jeder Punkt: **Was / Warum hier / Vorteil / Trade-off / Repo-Beispiel**.

---

## So liest du den Leitfaden

1. Zuerst **Architektur** — alles andere hängt daran.
2. Ein Feature End-to-End verfolgen (z. B. Kreditgenehmigung → Outbox → Saga → SignalR).
3. Die **Beispiel**-Pfade im Repo öffnen.
4. „Wann nicht?“ schützt vor Over-Engineering.

Empfohlene Reihenfolge: Auth/JWT → CQRS-Handler → Domain Event + Outbox → Idempotency → Saga → SignalR → Modular Monolith.

---

## 1) Architektur

### Clean Architecture

| | |
|--|--|
| **Was** | Abhängigkeiten zeigen nach innen: `Domain` → `Application` → `Persistence` / `WebApi`. |
| **Warum** | Klarheit: Wo lebt die Geschäftsregel? |
| **Vorteil** | Testbarkeit; Domain überlebt Framework-Wechsel. |
| **Trade-off** | Mehr Projekte/Ordner; für Mini-CRUD schwer. |
| **Beispiel** | `src/Domain`, `src/Application`, `src/Persistence`, `src/WebApi`, `src/Core` |

### CQRS + MediatR

| | |
|--|--|
| **Was** | Schreiben (Command) und Lesen (Query) trennen; MediatR routet Controller → Handler. |
| **Warum** | Approve/Reject/Cancel vs. Listen/Details; Pipelines (Validation, Auth, Cache). |
| **Vorteil** | Fokussierte Handler; Querschnitt über `IPipelineBehavior`. |
| **Trade-off** | Indirection bei trivialen Endpoints. |
| **Beispiel** | `ApproveCreditApplicationRequest` unter `Application/Modules/Credit/Features/...` · `ApplicationServiceRegistration.cs` |

### Modular Monolith

| | |
|--|--|
| **Was** | Ein Deploy / eine DB; Module: Identity, Customers, Credit, Notification. |
| **Warum** | Bounded Contexts ohne Microservice-Ops. |
| **Vorteil** | Klare Zuständigkeit; späterer Split vorbereitet. |
| **Trade-off** | Weiche Grenzen — Disziplin nötig. |
| **Beispiel** | `Application/Modules/*` · `WebApi/Modules/*` · `AddApplicationModules()` |

### Aggregate + Domain Events

| | |
|--|--|
| **Was** | Aggregate = Konsistenzgrenze; wichtige Änderungen erzeugen `IDomainEvent`. |
| **Warum** | Side Effects (Saga, SignalR) an denselben Geschäftsfakt koppeln. |
| **Vorteil** | Domain kennt RabbitMQ/SignalR nicht. |
| **Trade-off** | Event-Design kostet Zeit; zu viele Events = Rauschen. |
| **Beispiel** | Domain-Entities/Events · Persistenz → Outbox |

### Transactional Outbox

| | |
|--|--|
| **Was** | Event zuerst in `OutboxMessages`, Worker publiziert nach RabbitMQ. |
| **Warum** | Dual-Write vermeiden (DB ok, Message weg). |
| **Vorteil** | Basis für at-least-once und Retries. |
| **Trade-off** | Leichte Verzögerung; Consumer idempotent. |
| **Beispiel** | `OutboxDispatcherHostedService` · Queue `domain-events` |

```text
Handler → SaveChanges (Aggregate + Outbox-Zeile)
       → Dispatcher → RabbitMQ
       → Consumer → Saga + SignalR
```

### Saga / Process Manager

| | |
|--|--|
| **Was** | Zustandsmaschine für den Kredit-Lebenszyklus. |
| **Warum** | Created → Entscheidung → Onboarding-Stubs nachvollziehbar. |
| **Vorteil** | Abfragbarer Prozessstand (`CreditApplicationSagas`). |
| **Trade-off** | Hier **Orchestrierung** (Process Manager), keine Choreographie. |
| **Beispiel** | `CreditApplicationProcessManager` |

### Idempotency-Key

| | |
|--|--|
| **Was** | Client sendet eindeutigen Key; Server speichert/antwortet über Redis. |
| **Warum** | Doppelklick / Retry ohne Doppel-Approve. |
| **Vorteil** | Sichere Writes bei unsicherem Netz. |
| **Trade-off** | Client muss Keys erzeugen; gleicher Key + anderer Body → 409. |
| **Beispiel** | Header `Idempotency-Key` · `IdempotencyKeyFilter` |

---

## 2) Backend-Stack

### .NET 10 + ASP.NET Core

Host, DI, Middleware, JWT, SignalR, Health Checks.

**Beispiel:** `WebApi` · `ServiceCollectionExtensions` · `ApplicationBuilderExtensions`

### EF Core + SQL Server

Relationales Modell, Migrationen, TPT-Kunden, Outbox/Saga-Tabellen — Compose: `bankapp-sql`.

### FluentValidation / AutoMapper / SmartEnum

Eingaberegeln, DTO-Mapping, benannte Enums statt Magic Numbers.

### JWT + RBAC

Rollen: Customer, BankStaff, BankManager. Später Keycloak/OIDC (Roadmap).

### Serilog → Seq + ProblemDetails

Strukturierte Logs lokal; einheitliche API-Fehler für den Client.

---

## 3) Infrastruktur & Echtzeit

| Technik | Rolle |
|---------|--------|
| **Redis** | Cache + Idempotency-Store |
| **RabbitMQ** | `email-confirmation`, `domain-events` |
| **SignalR** | Hub `/hubs/credit-applications` nach Outbox-Consume |
| **OpenTelemetry** | OTLP Traces + Metrics → Collector → Tempo / Prometheus / Grafana `:3001` |
| **Vault (Lab)** | Dev-Mode + Seed `secret/bankapp` (WebApi noch nicht angebunden) |
| **Docker Compose** | Kompletter Lokalstack — `infra/docker-compose.yml` |

**Client:** `client/src/realtime/useCreditApplicationRealtime.ts`  
**OTel:** `WebApi/Extensions/OpenTelemetryExtensions.cs` · Seq erhält `TraceId`/`SpanId` über `ActivityEnricher`

---

## 4) Frontend

React + Vite + TypeScript; react-i18next (TR/EN/DE); SignalR-Client; UUID-`Idempotency-Key` bei Kredit-Writes — Backend-Muster auch in der UI sichtbar.

---

## 5) Qualität & CI

xUnit, WebApi-Integrationstests, GitHub Actions (Build + Tests + Client). Tests sichern Architektur-Refactors ab.

---

## 6) End-to-End: Kreditgenehmigung

```text
[React] Genehmigen + Idempotency-Key
    → [WebApi] CreditApplicationsController
    → [MediatR] Approve-Handler
    → [Domain] Status + Domain Event
    → [EF] Antrag + OutboxMessages (ein SaveChanges)
    → [Dispatcher] RabbitMQ domain-events
    → [Consumer] ProcessManager + SignalR-Gruppen
    → [React] Toast + Listen-Refresh
```

Ein Flow zeigt CQRS, Domain Events, Outbox, Saga, Idempotency und SignalR zusammen — der wertvollste Lernpfad.

---

## 7) Wann nicht? (kurz)

| Muster | Früh weglassen, wenn… |
|--------|------------------------|
| Outbox / Saga | Einfaches CRUD, Sync reicht |
| Modular-Ordner | Mini-API mit 2–3 Features |
| Idempotency | Nur Lesen / idempotente GETs |
| SignalR | Seltene Updates; Polling ok |
| Vault | Reine `.env`-Demo (Lab trotzdem nützlich) |

---

## 8) Später (Roadmap)

Keycloak, Elasticsearch, K8s/ArgoCD, Chatbot — [architecture-notes.de.md](architecture-notes.de.md) und [roadmap.de.md](roadmap.de.md). OTel ist bereits im Stack; in Grafana ansehen, dann beim nächsten Layer dasselbe Was/Warum/Beispiel-Schema nutzen.
