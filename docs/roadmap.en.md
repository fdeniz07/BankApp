# Roadmap

[Türkçe](roadmap.tr.md) · [Deutsch](roadmap.de.md)

For chronological phases, see [history.en.md](history.en.md). Architecture discussion: [architecture-notes.en.md](architecture-notes.en.md).

## Done

### Architecture / infrastructure

- [x] Clean Architecture + CQRS (MediatR) .NET 10 API
- [x] Docker Compose (SQL, Redis, Seq, RabbitMQ, WebApi, React client)
- [x] Modular Monolith (Identity / Customers / Credit / Notification)
- [x] Domain Events + Transactional Outbox (credit applications; queue `domain-events`)
- [x] Idempotency-Key (credit create/approve/reject/cancel; Redis)
- [x] Saga / Process Manager (credit application lifecycle; `CreditApplicationSagas`)
- [x] SignalR notifications (Outbox → hub `/hubs/credit-applications`)

### Security / identity

- [x] JWT auth; Customer / BankStaff / BankManager / SuperAdmin RBAC
- [x] Branch RBAC (list filters, modify guard; NYC/CHI seed, branch CRUD)
- [x] Post-registration email verification (RabbitMQ + Seq/log; no JWT on register)
- [x] Privacy / cookie consent (banner + persisted preference)
- [x] Rate-limit policies (auth / write / fixed; 429) + environment split (strict base, loose Development)
- [x] HashiCorp Vault lab (Compose dev mode + `secret/bankapp` seed)
- [x] Vault → WebApi JWT key overlay (lab; SQL still from Compose env)
- [x] Demo login matrix Staff/Manager/Customer × TR/DE/EN; shared lab password `Demo123!`

### Domain / business logic

- [x] Individual / corporate customers (TPT), credit types / sub-types / applications
- [x] SmartEnum status and category fields
- [x] Application cancel, approve / reject, payment schedule (amortization)
- [x] Locale catalog (TR/EN/DE) + constrained amount/term; catalog locked to branch locale (except SuperAdmin)
- [x] Branch model + decision snapshot (`DecisionBy` / branch / template name)
- [x] Credit application document upload (lab: PDF/JPEG/PNG, App_Data)
- [x] Soft approve gate config (`SoftApproveGateEnabled`; off in Development) + missing-docs badge/filter/list preset
- [x] Decision letter template CRUD (user); system templates follow UI language
- [x] BankManager credit type create / activate-deactivate; customer delete rules

### UX / client

- [x] React client (Vite + TypeScript), TR / EN / DE i18n
- [x] Currency display (TRY / EUR / USD), customer panel, new-application form
- [x] List sorting / status filter; staff inline approve / reject shortcuts
- [x] Role-based dashboard summary + stats strip
- [x] Staff management UI; staff/customer profile + avatar crop + header avatar
- [x] Accessibility toolbar (theme / font size / contrast) + FX ticker band
- [x] Gender allow-list; public individual register; staff customer UI

### Observability / quality

- [x] Serilog + Seq; Redis cache
- [x] OpenTelemetry + Grafana / Prometheus (collector + Tempo; Seq stays for logs)
- [x] Application unit tests
- [x] WebApi integration tests (login smoke; create→approve; reject/cancel; customer rules)
- [x] CI (GitHub Actions): build + tests + client build (`ubuntu-24.04`, Actions v5)

### Notification channels (lab)

- [x] Decision letter email (queue `decision-letter` + Seq log)
- [x] Decision letter SMS (queue `decision-letter-sms` + Seq log; no gateway)

### Documentation

- [x] Beginner tech learning library (`docs/learning/`) + Cursor rule `tech-learning-library`
- [x] Roadmap / history / architecture notes (TR / EN / DE)

## Todo

- [ ] Elasticsearch for staff GET endpoints
- [ ] Admin welcome dashboard (Tailwind CSS + bundle compression / performance)
- [ ] Kubernetes / ArgoCD deployment
- [ ] Raft Vault / secret rotation (production; beyond Compose dev-mode — learning 19)
- [ ] OAuth (social sign-up / login)
- [ ] Keycloak support
- [ ] Chatbot (customer / staff support assistant)
