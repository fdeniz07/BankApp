# Roadmap

[Türkçe](roadmap.tr.md) · [Deutsch](roadmap.de.md)

## Done

- [x] Clean Architecture + CQRS (MediatR) .NET 10 API
- [x] Individual / corporate customers (TPT), credit types / sub-types / applications
- [x] SmartEnum status and category fields
- [x] JWT auth, Customer / BankStaff RBAC
- [x] Serilog + Seq, Redis cache
- [x] Docker Compose (SQL, Redis, Seq, WebApi, React client)
- [x] Application unit tests
- [x] React client (Vite + TypeScript), TR / EN / DE i18n
- [x] Currency display (TRY / EUR / USD), customer dashboard
- [x] Application cancel (not soft-delete)
- [x] List sorting / status filter, credit type column
- [x] Customer new-application form
- [x] Locale-aware seed names, round amounts, standard terms
- [x] Locale catalog (TR/EN/DE) + staff filter + constrained amount/term
- [x] Payment schedule (amortization)
- [x] displayName + locale demo accounts
- [x] Changelog / test notes
- [x] Credit types sorting + table spacing
- [x] Payment plan / approved term consistency
- [x] JWT UTF-8 + customerCategory + profile page
- [x] BankManager role; customer delete rules (inactive + no open credit)
- [x] Gender allow-list; public individual register; staff customer UI
- [x] Role-based dashboard summary + stats strip on all pages
- [x] Inline approve / reject shortcuts on staff list
- [x] Staff management UI
- [x] Staff/customer profile fields + avatar (≤2 MB) + Seq audit categories
- [x] BankManager credit type create / activate-deactivate
- [x] Profile avatar crop + academic title/gender + fixed positions + header avatar
- [x] Accessibility toolbar (theme / font size / contrast) + FX ticker band
- [x] CI (GitHub Actions): build + unit/integration tests + client build
- [x] WebApi integration test skeleton (login smoke)
- [x] Integration: staff create application → approve flow
- [x] Integration: reject / cancel / customer create+cancel (+ customer cannot approve)
- [x] Privacy / cookie consent banner on first load (persist preference)
- [x] Post-registration email verification (RabbitMQ queue + Seq/log link; no JWT on register)
- [x] Domain Events + Transactional Outbox (credit applications; queue `domain-events`)
- [x] Idempotency-Key (credit create/approve/reject/cancel; Redis)
- [x] Saga / Process Manager (credit application lifecycle; `CreditApplicationSagas`)
- [x] SignalR notifications (Outbox-driven; hub `/hubs/credit-applications`)
- [x] Modular Monolith (Identity / Customers / Credit / Notification)
- [x] OpenTelemetry + Grafana / Prometheus (collector + Tempo; Seq stays for logs)

## Todo

### Phase 6 — Architectural deepening

Discussion notes: [architecture-notes.en.md](architecture-notes.en.md)

- [x] Domain Events + Transactional Outbox (reliable publish via RabbitMQ)
- [x] Idempotency-Key (critical write APIs)
- [x] Saga / Process Manager (credit application lifecycle)
- [x] SignalR notifications (live status driven by Outbox events) — done before Modular
- [x] Modular Monolith (Identity / Customers / Credit / Notification)

## Documentation (ongoing)

- [x] Beginner tech learning library (`docs/learning/`) + Cursor rule `tech-learning-library` — updated with every new feature

## Later

- [ ] Elasticsearch for staff GET endpoints
- [ ] Admin welcome dashboard (Tailwind CSS + bundle compression / performance)
- [ ] Kubernetes / ArgoCD deployment
- [x] OpenTelemetry + Grafana / Prometheus (collector + Tempo; Seq stays for logs)
- [x] HashiCorp Vault lab (Compose dev mode + `secret/bankapp` seed; WebApi not wired yet)
- [x] Rate-limit policies (auth / write / fixed; 429)
- [x] Vault → WebApi JWT key overlay (lab; SQL still from Compose env)
- [x] Rate-limit environment split (strict `appsettings.json` + loose Development; learning 23)
- [ ] Raft Vault / secret rotation (production; beyond Compose dev-mode — see learning 19)
- [ ] OAuth (social sign-up / login)
- [ ] Keycloak support
- [ ] Credit application document upload (ID, payslip, etc.; may become mandatory later)
- [x] Send decision letters by email (lab: `decision-letter` queue + Seq log; UI templates / Outbox)
- [ ] Send decision letters by SMS
- [ ] Branch model + decision-maker branch on credit applications
- [ ] Chatbot (customer / staff support assistant)
