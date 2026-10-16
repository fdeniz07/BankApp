# BankApp

[English README](README.md) · [Türkçe README](README.tr.md)

BankApp ist eine **.NET 10**-Banking-API mit **Clean Architecture** und **CQRS**. Sie verwaltet Privat-/Firmenkunden sowie den Kreditprozess (Kreditarten, Untertypen, Anträge, Genehmigung/Ablehnung, Soft-Delete, Validierung und Development-Seed).

## Dokumentation

| Dokument | Link |
|----------|------|
| Roadmap (Erledigt / Offen) | [docs/roadmap.de.md](docs/roadmap.de.md) |
| Projektverlauf | [docs/history.de.md](docs/history.de.md) |
| Changelog | [docs/changelog.de.md](docs/changelog.de.md) |
| Testnotizen | [docs/test-notes.de.md](docs/test-notes.de.md) |
| Infrastruktur (Compose) | [infra/README.md](infra/README.md) |

## Schnellstart (Docker)

```bash
copy infra\.env.example infra\.env
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --build
```

- WebApi: http://localhost:8080  
- Client: http://localhost:3000  
- Seq: http://localhost:5341  

Demo-Logins: `staff@bank.local` / `Staff123!`, `customer@bank.local` / `Customer123!`  
(auch `staff.{tr|en|de}@bank.local`, `customer.{tr|en|de}@bank.local`)

Ausführliche Architektur- und API-Beschreibung: [README.md](README.md) (Englisch) oder [README.tr.md](README.tr.md) (Türkisch).
