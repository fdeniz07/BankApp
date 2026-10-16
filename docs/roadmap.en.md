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

## Todo

- [ ] Inline approve / reject shortcuts on staff list
- [ ] Staff management UI
- [ ] API integration / e2e tests
- [ ] CI (GitHub Actions) basic pipeline

## Later

- [ ] Post-registration email verification (RabbitMQ or Kafka)
- [ ] Elasticsearch for staff GET endpoints
- [ ] Admin welcome dashboard (Tailwind CSS + bundle compression / performance)
- [ ] Kubernetes / ArgoCD deployment
- [ ] OpenTelemetry + Grafana / Prometheus
- [ ] Production hardening (secrets, rate-limit policies)
