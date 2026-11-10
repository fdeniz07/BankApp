# BankApp learning library (EN)

Zero-to-stack guide — technologies **actually used in this repo**, in **build order**.

[tech-learning overview](../../tech-learning.en.md) · [history](../../history.en.md) · [roadmap](../../roadmap.en.md) · [architecture notes](../../architecture-notes.en.md)

Also: [Türkçe](../tr/00-index.md) · [Deutsch](../de/00-index.md)

---

## What is this library?

| | |
|--|--|
| **Who?** | Anyone seeing a package in the repo for the first time |
| **Contains** | Definition, BankApp role, phase order, concepts, CLI, classes/packages, paths, mini flow, diagrams |
| **Not** | A copy of official docs; a dump of every source line |
| **Updates** | After each meaningful stack change (Cursor rule: `tech-learning-library`) |

---

## How to read

1. Follow the **phase order** below (do not skip; later chapters build on earlier ones).
2. After a chapter, open the **Where in the repo?** paths in the IDE.
3. Run a small command when you can (`dotnet build`, `docker compose ps`, …).
4. For product “why?”, see `architecture-notes` / `tech-learning`.

---

## Phase → chapter map

| Phase | Topic | Chapter |
|-------|-------|---------|
| 1 | .NET / C# basics | [01](01-dotnet-csharp.md) |
| 1 | Clean Architecture | [02](02-clean-architecture.md) |
| 1 | EF Core + SQL Server | [03](03-ef-core-sql.md) |
| 1 | CQRS + MediatR | [04](04-cqrs-mediatr.md) |
| 1 | FluentValidation + SmartEnum | [05](05-validation-smartenum.md) |
| 2 | JWT + RBAC | [06](06-jwt-rbac.md) |
| 2 | Serilog + Seq | [07](07-serilog-seq.md) |
| 2–3 | Redis | [08](08-redis.md) |
| 3 | Docker Compose | [09](09-docker-compose.md) |
| 3–5 | xUnit / integration tests | [10](10-testing.md) |
| 4 | React + Vite + TypeScript | [11](11-react-vite.md) |
| 4–5 | i18n + UX | [12](12-i18n-ux.md) |
| 5 | RabbitMQ | [13](13-rabbitmq.md) |
| 6 | Domain Events + Outbox | [14](14-outbox-events.md) |
| 6 | Idempotency-Key | [08](08-redis.md) + [14](14-outbox-events.md) |
| 6 | Saga / Process Manager | [15](15-saga.md) |
| 6 | SignalR | [16](16-signalr.md) |
| 6 | Modular Monolith | [17](17-modular-monolith.md) |
| 6 | OpenTelemetry / Grafana / Prometheus | [18](18-otel-observability.md) |
| Lab | HashiCorp Vault | [19](19-vault-lab.md) |
| UX | QuestPDF export | [20](20-pdf-export.md) |
| DevOps | GitHub Actions CI | [21](21-ci-github-actions.md) |
| Notification | Decision letter e-mail | [22](22-decision-letter-email.md) |
| Security | Rate limiting | [23](23-rate-limiting.md) |
| Lab / DX | Lab tools | [24](24-lab-tools.md) |
| Notification | Decision letter SMS | [25](25-decision-letter-sms.md) |
| Credit / UX | Application documents | [26](26-credit-application-documents.md) |
| Domain | Branch model | [27](27-branch-model.md) |
| Security | Branch RBAC + SuperAdmin | [28](28-branch-rbac-superadmin.md) |
| Credit / UX | Decision letter templates | [29](29-decision-letter-templates.md) |
| Credit / Security | Approve gate + DoA | [30](30-approve-gate-authority.md) |
| Search | ELK + search/facets | [31](31-elk-elasticsearch.md) |
| UX / DevOps | Admin Tailwind dashboard | [32](32-admin-tailwind-dashboard.md) |
| Security / Lab | Keycloak | [33](33-keycloak-lab.md) |
| UX / Lab | Portable a11y kit | [34](34-a11y-portable-kit.md) |

Requirements (BRD/FRD): [requirements/en](../../requirements/en/00-index.md)

Changelog: [99-changelog-learning.md](99-changelog-learning.md)

---

## Quick commands

```powershell
dotnet build BankApp.slnx
dotnet test BankApp.slnx

cd infra
docker compose up -d --build

cd client
npm install
npm run dev
```

Details live in each chapter.
