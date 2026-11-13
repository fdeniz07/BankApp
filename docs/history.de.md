# Projektverlauf

[Türkçe](history.tr.md) · [English](history.en.md)

Chronologische Zusammenfassung der BankApp-Schulungs- / Unternehmensdemo.

## Phase 1 — Kern-API

- Clean-Architecture-Schichten (Core, Domain, Application, Persistence, WebApi)
- Kunden-CRUD (Privat / Unternehmen, TPT)
- Kreditarten, Untertypen, Anträge; Genehmigung / Ablehnung; Soft-Delete
- FluentValidation, Business Rules, AutoMapper, MediatR-Pipeline
- SmartEnum (Status, Kategorie, Risiko)
- Development-Bogus-Seed

## Phase 2 — Sicherheit und Observability

- ASP.NET Identity + JWT
- Rollen Customer / BankStaff (`ISecuredRequest`)
- ApplicationUser ↔ Customer-Verknüpfung
- Serilog + Seq
- Redis-Cache (Kreditart- / Untertyp-Listen)

## Phase 3 — Infrastruktur

- Docker Compose: Seq, Redis, SQL Server (externes Volume), WebApi
- `infra/.env.example`, README
- `bin` / `obj` ignorieren / entfernnen
- Application-Unit-Tests

## Phase 4 — React-Client

- Vite + React + TypeScript-Client
- TR / EN / DE i18n (`react-i18next`)
- Login, Antragsliste / Detail, Genehmigen / Ablehnen für Personal
- Client-Docker (nginx) in Compose
- Kundename / Kategorie, Währungsspalten
- EN- / DE-Seed-Kunden und USD- / EUR-Produkte; TRY für TR-Produkte
- Kunden-Dashboard, neuer Antrag, Storno
- Listensortierung und Statusfilter

## Phase 5 — Datenqualität und UX

- Untertyp-Namen an Produkt-Locale angepasst (TR / EN / DE)
- Runde Beträge (z. B. 50.000 ₺, 250.000 €), Laufzeiten 3–6–9–12–15–18–24–32
- Seed-Sync für bestehende Daten (kein SQL-Wipe nötig)
- Tabellenlayout stabilisiert; Button-Größen vereinheitlicht
- Roadmap und Verlauf unter `docs/` (TR / EN / DE)
- CI, Integrationstests, Datenschutz-Einwilligung, E-Mail-Verifizierung (RabbitMQ), Healthchecks, Kundennummer / Formular-Parität

## Phase 6 — Architekturtiefe (in Arbeit)

Diskussionsnotizen: [architecture-notes.de.md](architecture-notes.de.md)

- [x] Domain Events + Transactional Outbox (RabbitMQ, Kreditanträge)
- [x] Idempotency-Key (kritische Schreib-APIs; Redis, 24h TTL)
- [x] Saga / Process Manager (Orchestrierung; `CreditApplicationSagas`)
- [x] SignalR-Benachrichtigungen (Outbox → Hub; vor Modular)
- [x] Modular Monolith (Identity / Customers / Credit / Notification)
- [x] OpenTelemetry + Grafana / Prometheus (OTLP → Collector → Tempo/Prometheus/Grafana)
- [x] Rate-Limit Umgebungs-Split (strenges Base + Development-Lab) + Vault-Rotations-/Raft-Notizen
- [x] Antrags-Dokumenten-Upload-Lab (App_Data + documents-API; noch nicht Pflicht)
- [x] Filialmodell (Branches + Personal-BranchId + Entscheidungs-Snapshot)
- [x] Manager/Admin ES-Facettenpanel (Terms + Betrags-Range-Aggregationen; SQL-Fallback)
- [x] Admin-Tailwind-Dashboard (`/dashboard`) + Vite manualChunks + nginx gzip_static
- [x] Filial-Obergrenze mit Währung (`MaxApprovalCurrencyCode` + `ApprovalAuthority:CurrencyCode`); nur bei gleicher Währung
- [x] Manager-Personal-RBAC + StaffPage-Modal; ES-Indizes Personal/Kunden/Kreditarten (`bankapp-staff` / `customers` / `credit-types`)
- [x] Keycloak-Lab: Compose (`:8180`) + Realm; WebApi Dual-JWT + Claim-Enrich
- [x] Keycloak-SPA-Login (PKCE, `oidc-client-ts`, `/login/callback`); lokaler Login bleibt
- [x] Keycloak Social-IdP Lab-Skript (Google/GitHub; ohne Secrets aus)
- [x] Vault JWT Dual-Key Lab-Rotation (`jwt_security_key_previous` + Script)
- [x] Vault Raft-HA Lab-Overlay (`docker-compose.vault-raft.yml` + Init-Skript; Ports 8210+)
- [x] Vault AppRole + getrennte KV-Pfade (`jwt` / `database` / `messaging` / `cache` / `observability`); kein Root-Token auf WebApi; `FailClosed`
- [x] Redis `requirepass` + Vault `secret/bankapp/cache` → WebApi-Connection-Overlay (`Redis=True` im Log)
- [x] Täglicher Lab-Smoke: `infra/scripts/lab-daily-check.ps1` (+ Redis-Auth / Vault-Policy / ES-Index)
- [x] Learning Library: Vault Kap.19 + Redis 08 + ELK 31; Screenshot-Einbettungen Kap. 02–25 (TR/EN/DE)
