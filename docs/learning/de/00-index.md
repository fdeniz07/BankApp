# BankApp Lernbibliothek (DE)

Null-zu-Stack-Leitfaden — Technologien, die **in diesem Repo wirklich genutzt** werden, in **Bau-Reihenfolge**.

[tech-learning Überblick](../../tech-learning.de.md) · [Historie](../../history.de.md) · [Roadmap](../../roadmap.de.md) · [Architektur-Notizen](../../architecture-notes.de.md)

Auch: [Türkçe](../tr/00-index.md) · [English](../en/00-index.md)

---

## Was ist diese Bibliothek?

| | |
|--|--|
| **Für wen?** | Alle, die ein Paket im Repo zum ersten Mal sehen |
| **Inhalt** | Definition, Rolle in BankApp, Phase, Begriffe, CLI, Klassen/Pakete, Pfade, Mini-Ablauf, Diagramme |
| **Nicht** | Kopie der offiziellen Docs; Dump jeder Quellzeile |
| **Updates** | Nach jeder relevanten Stack-Änderung (Cursor-Regel: `tech-learning-library`) |

---

## So liest du

1. Folge der **Phasen-Reihenfolge** unten (nicht springen).
2. Nach einem Kapitel die Pfade unter **Wo im Repo?** in der IDE öffnen.
3. Wenn möglich einen kleinen Befehl selbst ausführen.
4. Produkt-„Warum?“ → `architecture-notes` / `tech-learning`.

---

## Phase → Kapitel

| Phase | Thema | Kapitel |
|-------|-------|---------|
| 1 | .NET / C# Grundlagen | [01](01-dotnet-csharp.md) |
| 1 | Clean Architecture | [02](02-clean-architecture.md) |
| 1 | EF Core + SQL Server | [03](03-ef-core-sql.md) |
| 1 | CQRS + MediatR | [04](04-cqrs-mediatr.md) |
| 1 | FluentValidation + SmartEnum | [05](05-validation-smartenum.md) |
| 2 | JWT + RBAC | [06](06-jwt-rbac.md) |
| 2 | Serilog + Seq | [07](07-serilog-seq.md) |
| 2–3 | Redis | [08](08-redis.md) |
| 3 | Docker Compose | [09](09-docker-compose.md) |
| 3–5 | xUnit / Integrationstests | [10](10-testing.md) |
| 4 | React + Vite + TypeScript | [11](11-react-vite.md) |
| 4–5 | i18n + UX | [12](12-i18n-ux.md) |
| 5 | RabbitMQ | [13](13-rabbitmq.md) |
| 6 | Domain Events + Outbox | [14](14-outbox-events.md) |
| 6 | Idempotency-Key | [08](08-redis.md) + [14](14-outbox-events.md) |
| 6 | Saga / Process Manager | [15](15-saga.md) |
| 6 | SignalR | [16](16-signalr.md) |
| 6 | Modular Monolith | [17](17-modular-monolith.md) |
| 6 | OpenTelemetry / Grafana / Prometheus | [18](18-otel-observability.md) |
| Lab | HashiCorp Vault (AppRole, Split-KV, Cache, FailClosed; JWT Dual-Key; optional Raft) | [19](19-vault-lab.md) |
| Lab / Security | Redis requirepass + Vault Cache; täglich `lab-daily-check.ps1` | [08](08-redis.md) · [24](24-lab-tools.md) |
| UX | QuestPDF-Export | [20](20-pdf-export.md) |
| DevOps | GitHub Actions CI | [21](21-ci-github-actions.md) |
| Notification | Entscheidungsbrief E-Mail | [22](22-decision-letter-email.md) |
| Security | Rate limiting | [23](23-rate-limiting.md) |
| Lab / DX | Lab-Tools | [24](24-lab-tools.md) |
| Notification | Entscheidungsbrief SMS | [25](25-decision-letter-sms.md) |
| Credit / UX | Antragsunterlagen | [26](26-credit-application-documents.md) |
| Domain | Filialmodell | [27](27-branch-model.md) |
| Security | Filial-RBAC + SuperAdmin | [28](28-branch-rbac-superadmin.md) |
| Credit / UX | Briefvorlagen | [29](29-decision-letter-templates.md) |
| Credit / Security | Freigabe-Tor + DoA | [30](30-approve-gate-authority.md) |
| Search | ELK + Suche/Facets | [31](31-elk-elasticsearch.md) |
| UX / DevOps | Admin-Tailwind-Dashboard | [32](32-admin-tailwind-dashboard.md) |
| Security / Lab | Keycloak | [33](33-keycloak-lab.md) |
| UX / Lab | Portables a11y-Kit | [34](34-a11y-portable-kit.md) |

Anforderungen (BRD/FRD): [requirements/de](../../requirements/de/00-index.md)

Changelog: [99-changelog-learning.md](99-changelog-learning.md)

---

## Schnellbefehle

```powershell
dotnet build BankApp.slnx
dotnet test BankApp.slnx

cd infra
powershell -NoProfile -File infra/scripts/lab-daily-check.ps1
cd infra
docker compose --env-file .env up -d --build

cd client
npm install
npm run dev
```

Details stehen im jeweiligen Kapitel.
