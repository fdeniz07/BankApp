# Architecture and training discussion notes

[Türkçe](architecture-notes.tr.md) · [Deutsch](architecture-notes.de.md)

**Date:** 2026-10-21  
**Context:** Educational BankApp — notes to revisit before starting the next phase.

Roadmap: [roadmap.en.md](roadmap.en.md) (Phase 6).  
Deep learning (why + examples): [tech-learning.en.md](tech-learning.en.md).

---

## Current stack (summary)

Clean Architecture + CQRS (MediatR), JWT/RBAC, Redis, Serilog+Seq, RabbitMQ (email verification), Docker Compose, React i18n (TR/EN/DE), CI + integration tests.

Next steps are not “missing features” but **high-learning modern layers**.

---

## Chosen focus

**Architectural deepening** (not platform / product-AI first).

Out of scope for now: Keycloak, OAuth, K8s, AI chatbot code. (OTel/Grafana done 2026-10-25.)

---

## Suggested technology groups (discussion summary)

### 1) Architecture / domain

| Idea | Learning value |
|------|----------------|
| Domain Events + Outbox | Post-aggregate events, reliable publish |
| Saga / Process Manager | Long-running credit orchestration |
| Modular Monolith | Bounded contexts; single deploy |
| Idempotency-Key | Double-submit / retry safety |
| Vertical slice hardening | Scaling CQRS like a product |

### 2) Data & search

| Idea | Note |
|------|------|
| Elasticsearch / OpenSearch | Staff lists; better after Outbox/read-model |
| CDC (Debezium) → ES | Advanced sync |
| Queryable audit table | Complement to Seq |

### 3) Real-time & UX

| Idea | Note |
|------|------|
| SignalR | Application status; visible proof of Outbox events |
| Notification hub | In-app + email |
| BFF / TanStack Query | Modern client data layer |

### 4) Identity & security

| Idea | Note |
|------|------|
| Keycloak / OIDC first, social OAuth later | Enterprise auth |
| Refresh token + rotation | Production-like JWT |
| Secrets, rate limits | Hardening |

### 5) Observability & platform

| Idea | Note |
|------|------|
| OpenTelemetry → Prometheus + Grafana | Alongside Seq |
| .NET Aspire | Local orchestration |
| k3d/kind + Helm + ArgoCD | GitOps lab |
| Feature flags, Pact | Progressive delivery / contract tests |

### 6) AI (kept modest)

| Idea | Note |
|------|------|
| Support chatbot (RAG + tools) | Product FAQ + application queries |
| Semantic Kernel / Extensions.AI | .NET AI integration |

### 7) Banking domain scenarios

Credit scorecard, contract PDF, installments/late fees, IBAN/transfer (idempotent), fraud alerts, multi-tenant branch — make technology tangible.

Application **document upload** (ID, payslip, etc.): on the Later roadmap; not required now, may become mandatory later for KYC/verification.

---

## Comments on the existing “Later” list

- **Elasticsearch** — good; Outbox/event story first is more pedagogical.
- **Admin Tailwind dashboard** — visual; low architecture learning.
- **K8s/ArgoCD** — heavy before modularizing the monolith.
- **OTel/Grafana** — **done** (2026-10-25); traces/metrics beside Seq.
- **OAuth + Keycloak** — Keycloak/OIDC first, social login second.
- **Chatbot** — prefer RAG + tools as a separate lab, not UI-only chat.

---

## Phase 6 — chosen order

1. Domain Events + Transactional Outbox (RabbitMQ) — **done** (2026-10-21): `AggregateRoot`, `OutboxMessages`, dispatcher + Seq/log consumer; credit create/approve/reject/cancel events
2. Idempotency-Key (critical write APIs) — **done** (2026-10-21): `Idempotency-Key` header, Redis 24h, create/approve/reject/cancel; same key replay / different body → 409
3. Saga / Process Manager (credit application lifecycle) — **done** (2026-10-22): `CreditApplicationSagas`, orchestrating Process Manager (AwaitingDecision → Onboarding → Completed / Terminated / Compensated)
4. SignalR notifications (driven by Outbox events) — **done** (2026-10-23): hub `/hubs/credit-applications`; consumer → staff + customer groups; React toast + list refresh (before Modular)
5. Modular Monolith (Identity / Customers / Credit / Notification) — **done** (2026-10-24): `Application.Modules.*` + `WebApi.Modules.*`; `AddApplicationModules` / host module registration; single deploy, single DB
6. OpenTelemetry + Grafana / Prometheus — **done** (2026-10-25): WebApi OTLP → collector → Tempo (traces) + Prometheus (metrics) + Grafana `:3001`; Seq stays for logs; `TraceId`/`SpanId` enricher
7. Decision letter email (lab) — **done** (2026-10-27): approve/reject → `decision-letter` queue + Seq log; no SMS yet

```mermaid
flowchart LR
  A[Outbox_DomainEvents] --> B[Idempotency]
  B --> C[Saga]
  C --> D[SignalR]
  D --> E[Modular_Monolith]
  E --> F[OTel_Grafana]
```

Broader training sequence: Outbox → SignalR → OTel → Keycloak → ES → AI RAG.

---

## Lab — Idempotency-Key

1. Via Scalar/curl: `POST /api/CreditApplications` with header `Idempotency-Key: <uuid>` and a body.
2. Repeat with the **same** key and **same** body → same `id`; Seq shows `Idempotency replay` on the second call; one `CreditApplicationCreated` outbox row for that id.
3. Same key + **different** amount → `409 Conflict`.
4. Missing key → `400`.

## Lab — HashiCorp Vault (Compose)

- UI: http://localhost:8200 — token from `.env` → `VAULT_DEV_ROOT_TOKEN_ID`
- KV path: `secret/bankapp` (`jwt_security_key`, `mssql_sa_password`, `rabbitmq_password`)
- Seed: Compose `vault-seed` or `infra/scripts/vault-seed.sh`
- **Note:** WebApi still reads `.env`; Vault is a side lab (SDK wiring in a later slice)

## Lab — Saga / Process Manager

1. Create an application → SQL row in `CreditApplicationSagas` with `AwaitingDecision` (Seq: `Saga started`).
2. Approve → steps `RiskChecked` → `ContractPrepared` → `CustomerNotified` → `Completed` + `ContractReference` (Seq step logs).
3. Reject or cancel a separate application → `Terminated` (no onboarding).

## Lab — SignalR (credit applications)

1. WebApi + client running; two browsers (or an incognito window): staff `staff@bank.local` + customer `customer@bank.local`.
2. Keep **Applications** open in both sessions (hub connects).
3. Staff approve/reject (or customer create/cancel).
4. Expect: toast (`realtime.creditChanged`), list refreshes; Seq: `Domain event consumed` + `Realtime notified`.
5. DevTools → Network → WS: `/hubs/credit-applications` should be connected.

## Revisit before coding

Phase 6 main slices + OTel complete. Optional next: **Keycloak**, Elasticsearch, **Vault → WebApi** (Later list).
