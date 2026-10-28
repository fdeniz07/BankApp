# Project history

[Türkçe](history.tr.md) · [Deutsch](history.de.md)

Chronological summary of the BankApp training / corporate sample.

## Phase 1 — Core API

- Clean Architecture layers (Core, Domain, Application, Persistence, WebApi)
- Customer CRUD (individual / corporate, TPT)
- Credit types, sub-types, applications; approve / reject; soft delete
- FluentValidation, business rules, AutoMapper, MediatR pipeline
- SmartEnum (status, category, risk)
- Development Bogus seeding

## Phase 2 — Security and observability

- ASP.NET Identity + JWT
- Customer / BankStaff roles (`ISecuredRequest`)
- ApplicationUser ↔ Customer linkage
- Serilog + Seq
- Redis cache (credit type / sub-type lists)

## Phase 3 — Infrastructure

- Docker Compose: Seq, Redis, SQL Server (external volume), WebApi
- `infra/.env.example`, README
- Ignore / untrack `bin` / `obj`
- Application unit tests

## Phase 4 — React client

- Vite + React + TypeScript client
- TR / EN / DE i18n (`react-i18next`)
- Login, application list / detail, staff approve-reject
- Client Docker (nginx) added to Compose
- Customer name / category, currency columns
- EN / DE seed customers and USD / EUR products; TRY for TR products
- Customer dashboard, new application, cancel
- List sorting and status filter

## Phase 5 — Data quality and UX

- Sub-type names aligned with product locale (TR / EN / DE suffixes)
- Round amounts (e.g. 50,000 ₺, 250,000 €), term steps 3–6–9–12–15–18–24–32
- Seed sync for existing rows (no SQL wipe required)
- Table layout fixed; approve / reject and Details / Cancel button sizes unified
- Roadmap and history under `docs/` (TR / EN / DE)
- CI, integration tests, privacy consent, email verification (RabbitMQ), health checks, customer number / form parity

## Phase 6 — Architectural deepening (in progress)

Discussion notes: [architecture-notes.en.md](architecture-notes.en.md)

- [x] Domain Events + Transactional Outbox (RabbitMQ, credit applications)
- [x] Idempotency-Key (critical write APIs; Redis, 24h TTL)
- [x] Saga / Process Manager (orchestration; `CreditApplicationSagas`)
- [x] SignalR notifications (Outbox → hub; before Modular)
- [x] Modular Monolith (Identity / Customers / Credit / Notification)
- [x] OpenTelemetry + Grafana / Prometheus (OTLP → collector → Tempo/Prometheus/Grafana)
- [x] Rate-limit environment split (strict base + Development lab) + Vault rotation/Raft notes
