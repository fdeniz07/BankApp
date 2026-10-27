# BankApp

[English README](README.md) · [Türkçe README](README.tr.md)

BankApp ist ein **full-stack Bildungsbeispiel** für **Privat- und Firmenkreditanträge**.  
Backend: **.NET 10** Clean Architecture + CQRS (MediatR), **Modular Monolith** (Identity / Customers / Credit / Notification), Domain Events + Outbox, Idempotency-Key, Saga, SignalR.  
Frontend: **React (Vite + TypeScript)** mit **TR / EN / DE**-i18n und Live-Kreditbenachrichtigungen (SignalR).  
Lokal: **Docker Compose** (SQL Server, Redis, Seq, RabbitMQ, Vault-Lab, WebApi, Client).

**Dokumentation:** [Roadmap](docs/roadmap.de.md) · [Tech-Lernen](docs/tech-learning.de.md) · [Lernbibliothek (TR)](docs/learning/00-index.tr.md) · [Architekturnotizen](docs/architecture-notes.de.md) · [Verlauf](docs/history.de.md) · [Changelog](docs/changelog.de.md) · [Testnotizen](docs/test-notes.de.md) · [Infra](infra/README.md)

---

## Inhaltsverzeichnis

1. [Demo-Logins](#demo-logins)
2. [Funktionen](#funktionen)
3. [Screenshots (EN)](#screenshots-en)
4. [Tech-Stack](#tech-stack)
5. [Lösungsstruktur](#lösungsstruktur)
6. [Architekturdiagramme](#architekturdiagramme)
7. [Domänenmodell](#domänenmodell)
8. [Geschäftsregeln](#geschäftsregeln)
9. [API-Überblick](#api-überblick)
10. [Lab-Tools](#lab-tools)
11. [SmartEnum](#smartenum)
12. [Daten-Seed](#daten-seed)
13. [Schnellstart](#schnellstart)
14. [Beispielanfragen](#beispielanfragen)

---

## Demo-Logins

| Rolle | E-Mail | Passwort |
|-------|--------|----------|
| Bankmitarbeiter | `staff@bank.local` (auch `staff.tr` / `.en` / `.de`) | `Staff123!` |
| Bankmanager | `manager@bank.local` | `Manager123!` |
| Kunde | `customer@bank.local` (auch `customer.tr` / `.en` / `.de`) | `Customer123!` |

Rollen: **Customer**, **BankStaff**, **BankManager** (Manager hat auch BankStaff).

---

## Funktionen

### Backend

- Privat-/Firmenkunden (TPT), Kreditarten & Untertypen, Anträge
- JWT-Auth + RBAC; öffentliche Privatkunden-Registrierung + E-Mail-Verifizierung (RabbitMQ)
- Genehmigen / Ablehnen / **Stornieren** (Statuswechsel — Storno ≠ Soft-Delete)
- Bearbeitbare **Kunden-Entscheidungsbriefe** (Genehmigungs-/Ablehnungsvorlagen; in `notes`)
- **Domain Events + Transactional Outbox** (Kredit-Lebenszyklus → Queue `domain-events`)
- **Idempotency-Key** (create / approve / reject / cancel; Redis, 24h)
- **Saga / Process Manager** (`CreditApplicationSagas`; Created → Entscheidung → Onboarding-Stubs)
- **SignalR** (`/hubs/credit-applications`; Staff- + Kundengruppen; Publish nach Outbox-Consumer)
- **Entscheidungsbrief-E-Mail (Lab)** — nach Genehmigung/Ablehnung Queue `decision-letter` → Seq/Log (kein SMTP; SMS später)
- **Rate limiting** — Policies `auth` / `write` / `fixed` (429); Health + SignalR ausgenommen
- **Vault-Lab** — JWT-`SecurityKey`-Overlay aus `secret/bankapp` (Compose); Env-Fallback
- Zahlungsplan (Amortisation) mit **erweiterbarem Export** (`payment-plan/export?format=pdf`; später CSV/XLSX) und Druck
- Dashboard-Summary, Personalverwaltung, Profil + Avatar (≤ 2 MB)
- Kreditarten anlegen / ändern / löschen / aktiv-passiv (**BankManager**)
- Serilog → Seq, Redis-Cache, FluentValidation, ProblemDetails
- OpenTelemetry → Collector → Tempo / Prometheus / Grafana

### Frontend (`client/`)

- Login / Registrierung / E-Mail-Bestätigung, Anträge, Kunden, Kreditarten, Personal, Profil
- Statistikleiste, FX-Ticker, Barrierefreiheits-Panel (Thema / Schrift / Kontrast)
- Datenschutz- / Cookie-Banner; Sprachumschalter TR / EN / DE
- `Idempotency-Key` (UUID) bei Kredit-Schreibanfragen
- SignalR: Antragsstatus-Toast (success / error / warning je Event) + Listen-/Detail-Refresh
- Zahlungsplan: einklappbare Ratenliste, **PDF-Download** und **Drucken** (Kunde + Personal)
- Entscheidungsformulare: neu ladbare Genehmigungs-/Ablehnungsvorlagen

### CI/CD

- GitHub Actions: WebApi Restore/Build, Unit- + Integrationstests, React-Client-Build
- Docker Compose (`infra/`) für lokalen Multi-Service-Betrieb (Healthcheck-Kette)

### Test

- xUnit **Application.UnitTests** (Domain-Regeln, Saga, Handler)
- xUnit **WebApi.IntegrationTests** (API + Auth-/Kreditflüsse)
- Client-Production-Build (`tsc` + Vite) in CI

---

## Screenshots (EN)

UI-Aufnahmen unter [`docs/assets/screenshots/en/`](docs/assets/screenshots/en/) (vorerst nur englische Oberfläche).  
Rollen-Galerien: [Kunde](docs/screenshots/customer.en.md) · [Mitarbeiter](docs/screenshots/bank-employee.en.md) · [Manager](docs/screenshots/bank-manager.en.md)

### Auth, Datenschutz & Barrierefreiheit

**Login & Datenschutz-Banner** — Anmeldung mit TR/EN/DE-Umschalter, „Angemeldet bleiben“, Demo-Zugangsdaten auf der Karte und Cookie-/Datenschutz-Zustimmung vor der SPA.

<img src="docs/assets/screenshots/en/Login%20Page%20and%20Privacy.png" alt="Login und Datenschutz-Banner" width="720" />

**Barrierefreiheits-Panel** — Hell/Dunkel, Schriftgröße und hoher Kontrast (im Browser persistiert).

<img src="docs/assets/screenshots/en/Accessiblity%20Settings.png" alt="Barrierefreiheitseinstellungen" width="360" />

### Dashboards (nach Rolle)

**Kunde** — Begrüßung mit Anzeigename; Zähler der eigenen Anträge. Mehr: [Kunden-Galerie](docs/screenshots/customer.en.md).

<img src="docs/assets/screenshots/en/Customer/Customer%20Page%20-%20Dashboard-1.png" alt="Kunden-Dashboard" width="720" />

**Bankmitarbeiter** — Statistikleiste, FX-Ticker, SignalR-Lebenszyklus-Toasts. Mehr: [Mitarbeiter-Galerie](docs/screenshots/bank-employee.en.md).

<img src="docs/assets/screenshots/en/Bank%20Employee/Bank%20Employee%20Page%20-%20Dashboard.png" alt="Mitarbeiter-Dashboard" width="720" />

**Bankmanager** — Übersicht mit Personal- und Kreditarten-Admin. Mehr: [Manager-Galerie](docs/screenshots/bank-manager.en.md).

<img src="docs/assets/screenshots/en/Bank%20Manager/Bank%20Manager%20Page%20-%20Dashboard.png" alt="Manager-Dashboard" width="720" />

### Infrastruktur & Observability

**Docker Compose** — Vollständiger Lab-Stack (SQL, Redis, Seq, RabbitMQ, Vault, OTel, Prometheus, Grafana, WebApi, Client).

<img src="docs/assets/screenshots/en/Containers%20on%20Docker.png" alt="Docker-Container" width="720" />

**Scalar API-Referenz** — Interaktive OpenAPI-UI für WebApi (`/scalar/v1`).

<img src="docs/assets/screenshots/en/Api%20Documents%20with%20Scalar.png" alt="Scalar API-Dokumentation" width="720" />

**Seq** — Strukturierte Anwendungslogs (Serilog), mit TraceId korrelierbar wenn OTel aktiv ist.

<img src="docs/assets/screenshots/en/Logs%20on%20Seq.png" alt="Seq-Logs" width="720" />

**Prometheus & Grafana** — Metrics-Scrape + Dashboards/Traces (`:9090` / `:3001`).

<img src="docs/assets/screenshots/en/Metrics%20on%20Prometheus.png" alt="Prometheus" width="720" />

<img src="docs/assets/screenshots/en/Metrics%20on%20Grafana.png" alt="Grafana-Metriken" width="720" />

<img src="docs/assets/screenshots/en/Metrics%20are%20also%20tracked%20on%20Grafana.png" alt="Grafana weitere Metriken" width="720" />

**RabbitMQ** — Queues für E-Mail-Bestätigung, Domain-Events (Outbox) und decision-letter (Lab).

<img src="docs/assets/screenshots/en/Queues%20on%20RabbitMQ.png" alt="RabbitMQ-Queues" width="720" />

**Vault (Lab)** — Dev-Mode Secrets-Seed (`secret/bankapp`); WebApi liest Vault noch nicht.

<img src="docs/assets/screenshots/en/Secrets%20on%20Hashicorp%20Vault.png" alt="HashiCorp Vault Secrets" width="720" />


---

## Tech-Stack

| Bereich | Technologie |
|---------|-------------|
| API | .NET 10, ASP.NET Core, MediatR, FluentValidation, AutoMapper, EF Core 10 |
| Domain | AggregateRoot, Domain Events (`CreditApplication*`), Ardalis SmartEnum (**Name**) |
| DB / Cache / Logs | SQL Server, Redis, Serilog + Seq |
| Observability | OpenTelemetry (OTLP) → Collector → Tempo + Prometheus + Grafana `:3001` |
| Messaging | RabbitMQ (`email-confirmation`, `domain-events`, `decision-letter`) + Outbox |
| Idempotency | Header `Idempotency-Key` + Redis (Kredit-Schreib-APIs, 24h) |
| Saga | `CreditApplicationProcessManager` + Tabelle `CreditApplicationSagas` |
| Realtime | SignalR-Hub `/hubs/credit-applications` (JWT `access_token`) |
| Secrets (Lab) | HashiCorp Vault **dev** (`secret/bankapp`; WebApi liest Vault noch nicht) |
| Auth | JWT + Rollen-Claims (Customer / BankStaff / BankManager) |
| Client | React, Vite, TypeScript, react-i18next, react-router, `@microsoft/signalr` |
| Docs (Dev) | OpenAPI + Scalar (`/scalar/v1`) |
| Test / CI | xUnit (Application + WebApi.IntegrationTests), GitHub Actions |
| Seed | Bogus (+ fester Kreditkatalog) |
| Infra | Docker Compose (`infra/`) — Healthcheck-Servicekette |

---

## Lösungsstruktur

```
BankApp/
├── BankApp.slnx
├── README.md / README.tr.md / README.de.md
├── client/              # React-SPA
├── docs/                # Roadmap, Tech-Lernen, Architekturnotizen, Changelog, Verlauf
├── infra/               # docker-compose, .env.example, scripts/vault-seed.sh
├── tests/               # Application.UnitTests, WebApi.IntegrationTests
└── src/
    ├── WebApi/          # Host + Modules/{Identity,Customers,Credit,Notification}
    ├── Application/     # Modules/{Identity,Customers,Credit,Notification} + shared Services
    ├── Domain/          # Entities, Events, SmartEnums, CreditApplicationSaga
    ├── Persistence/     # DbContext (Outbox-Harvest), Migrations, Seed
    └── Core/            # Entity, AggregateRoot, Security, Pipelines
```

**Abhängigkeitsrichtung:** `WebApi` → `Application` / `Persistence` → `Domain` / `Core`.

---

## Architekturdiagramme

### Schichtabhängigkeiten

```mermaid
flowchart TB
    Client[React client]
    WebApi[WebApi]
    Application[Application]
    Persistence[Persistence]
    Domain[Domain]
    Core[Core]

    Client --> WebApi
    WebApi --> Application
    WebApi --> Persistence
    Application --> Domain
    Application --> Core
    Persistence --> Application
    Persistence --> Domain
    Persistence --> Core
    Domain --> Core
```

### Anfragefluss (CQRS)

```mermaid
sequenceDiagram
    participant Client
    participant Controller
    participant MediatR
    participant Validation as RequestValidationBehavior
    participant Handler
    participant Rules as BusinessRules
    participant Repo as Repository / EF Core
    participant Db as SQL Server

    Client->>Controller: HTTP + JWT
    Controller->>MediatR: Send(Request)
    MediatR->>Validation: FluentValidation
    Validation->>Handler: Handle
    Handler->>Rules: Domänenprüfungen
    Handler->>Repo: Query / Command
    Repo->>Db: SQL
    Db-->>Repo: Daten
    Repo-->>Handler: Entity / Seite
    Handler-->>Controller: Response-DTO
    Controller-->>Client: JSON / ProblemDetails
```

### Lebenszyklus Kreditantrag

```mermaid
stateDiagram-v2
    [*] --> Pending: Erstellen
    Pending --> Approved: Genehmigen
    Pending --> Rejected: Ablehnen
    Pending --> Cancelled: Stornieren
    Pending --> Pending: Aktualisieren (nur Pending)
    Approved --> [*]
    Rejected --> [*]
    Cancelled --> [*]
```

---

## Domänenmodell

### Kunden (TPT)

| Entity | Hinweis |
|--------|---------|
| **Customer** | Gemeinsam: Nummer, E-Mail, Telefon, Adresse, `IsActive`, Soft-Delete |
| **IndividualCustomer** | Name, Ausweis, Geburtsdatum, Geschlecht, … |
| **CorporateCustomer** | Firma, Steuer, Ansprechpartner, … |

### Kreditkatalog & Anträge

| Entity | Rolle |
|--------|-------|
| **CreditType** | Produkt (Kategorie, Risiko, Währung, Locale, aktiv) |
| **CreditSubType** | Paket (Betrags-/Laufzeitspannen, Zinssätze) |
| **CreditApplication** | Antrag + Status + Entscheidungsfelder |

### Soft-Delete vs. Aktiv

| Flag | Bedeutung |
|------|-----------|
| **`IsActive`** | Geschäftlich nutzbar für neue Vorgänge |
| **`IsDeleted`** | Technisches Soft-Delete (`DeletedDate`) |

**Storno** setzt den Status auf `Cancelled` (kein Soft-Delete).

---

## Geschäftsregeln

### Kunden

- Eindeutigkeit (Ausweis / Steuer / Register / Kundennummer / E-Mail).
- Soft-Delete nur **BankManager**; Kunde inaktiv, keine offenen Pending/Approved-Anträge.

### Kreditarten & Untertypen

- Eindeutige Codes; konsistente Min/Max-Spannen.
- Neue Anträge brauchen **aktive** Mutter-Kreditart und Untertyp.
- Kategorie-Match Privat ↔ Individual, Firma ↔ Corporate.
- Mutierende CreditType-APIs nur Manager.

### Kreditanträge

- Betrag/Laufzeit im Paketrahmen.
- Nur **Pending** aktualisierbar / genehmigbar / ablehnbar / stornierbar.

---

## API-Überblick

| Dienst (Docker-Lab) | URL / Endpoint | Lab-Zugangsdaten / Hinweis |
|---------------------|----------------|----------------------------|
| WebApi | http://localhost:8080 | JWT nach Login (siehe [Demo-Logins](#demo-logins)) |
| Scalar | http://localhost:8080/scalar/v1 | Dev-OpenAPI-UI |
| Client | http://localhost:3000 | Demo-Konten auf der Login-Karte |
| SQL Server | `localhost,11433` | `sa` / `MSSQL_SA_PASSWORD` (Default in `.env.example`: `1q2w3e4r!`) |
| Redis | `localhost:6379` | Kein Passwort (Lab) |
| Redis Insight (UI) | http://localhost:5540 | Kein Login; DB-Host **`redis`**, Port **6379** (Compose-DNS) |
| Seq (Logs) | http://localhost:5341 | Kein Login (Lab; UI direkt) |
| Grafana (Metriken/Traces-UI) | http://localhost:3001 | `admin` / `admin` (`GRAFANA_ADMIN_*`) |
| Prometheus | http://localhost:9090 | Kein Login (Lab) |
| OTLP gRPC (Collector) | `localhost:4317` | App → Collector (keine Browser-UI) |
| RabbitMQ UI | http://localhost:15672 | `guest` / `guest` |
| Vault UI (Lab) | http://localhost:8200 | Methode **Token** = `VAULT_DEV_ROOT_TOKEN_ID` (Default `bankapp-vault-root-token`); KV `secret/bankapp` |

Die meisten Endpunkte brauchen JWT (außer Login / öffentliche Registrierung / E-Mail-Bestätigung).  
Kredit-**Schreib**-APIs (`POST` create/approve/reject/cancel): Pflicht-Header **`Idempotency-Key`**.

| Controller | Highlights |
|------------|------------|
| **Auth** | `login`, `register`, `register-individual`, `confirm-email` |
| **IndividualCustomers** / **CorporateCustomers** | CRUD + inactive / deleted |
| **CreditTypes** | Liste; Mutation + `PUT {id}/active` (Manager) |
| **CreditSubTypes** | CRUD |
| **CreditApplications** | `approve` / `reject` / **`cancel`** (+ Idempotency-Key); `payment-plan`; `payment-plan/export?format=pdf` |
| **Dashboard** | `GET summary` |
| **StaffUsers** | Liste / anlegen / ändern (Manager) |
| **Profile** | Eigenes Profil; Avatar |

Details: Scalar-UI.

## Lab-Tools

Stack beobachten (Scalar, Seq, Redis Insight, RabbitMQ-UI, Grafana, Vault, SQL-Client, Docker) — Setup, Compose-DNS (`redis` vs. `localhost`), Hinweise und Best Practices → **[docs/learning/24-lab-tools.tr.md](docs/learning/24-lab-tools.tr.md)** (TR-Kapitel; URLs wie in dieser README).

Kurze Debug-Kette: Scalar-Request → Redis Insight / RabbitMQ → Seq-Logs → (optional) Grafana-Traces.

---

## SmartEnum

Werte als **Name** (z. B. `"Pending"`), nicht als int.

| Typ | Beispielwerte |
|-----|---------------|
| `CreditCategory` | Individual, Corporate |
| `CreditRiskLevel` | Low, Medium, High |
| `CreditApplicationStatus` | Pending, Approved, Rejected, Cancelled |
| `Gender` | Female, Male, PreferNotToSay |
| `PersonTitle` | Dr, Prof |
| `StaffPosition` | CreditAnalyst, RelationshipManager, BranchOfficer, RiskOfficer, OperationsSpecialist, BranchManager |

---

## Daten-Seed

Bei `Database__RunSeed` (typisch Docker / Development). Top-up bis Zielzahlen.

| Seed | Ca. Ziel |
|------|----------|
| Privatkunden | 30 (10 × tr/en/de) |
| Firmenkunden | 30 (10 × tr/en/de) |
| Kreditarten | 16 (TRY/USD/EUR-Kataloge) |
| Untertypen | Pakete je Typ |
| Anträge | 48 |
| Identity | Demo Staff / Manager / Kunden |

---

## Schnellstart

### A — Docker (empfohlen)

```bash
copy infra\.env.example infra\.env
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --build
```

Siehe [infra/README.md](infra/README.md).

### B — Lokal API + Client

**Voraussetzungen:** .NET 10 SDK, SQL Server, Node.js 20+.

```bash
dotnet ef database update --project src/Persistence/Persistence.csproj --startup-project src/WebApi/WebApi.csproj
dotnet run --project src/WebApi/WebApi.csproj
```

```bash
cd client
copy .env.example .env
npm install
npm run dev
```

Connection String und `TokenOptions__SecurityKey` über appsettings / User Secrets / Env setzen.

### Tests & CI

```bash
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj
dotnet test tests/WebApi.IntegrationTests/WebApi.IntegrationTests.csproj
```

GitHub Actions (`.github/workflows/ci.yml`) führt bei Push/PR auf `main`/`master` Backend-Build, beide Testprojekte und den React-Client-Build aus.

---

## Beispielanfragen

### Login

```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "staff@bank.local",
  "password": "Staff123!"
}
```

Token: `Authorization: Bearer <token>`.

### Antrag erstellen

```http
POST /api/CreditApplications
Authorization: Bearer <token>
Content-Type: application/json

{
  "creditSubTypeId": "<guid>",
  "requestedAmount": 50000,
  "requestedTermMonths": 24,
  "monthlyIncome": 45000,
  "existingDebtAmount": 5000,
  "purpose": "Privatbedarf",
  "notes": null
}
```

### Genehmigen

```http
POST /api/CreditApplications/{id}/approve
Authorization: Bearer <token>
Content-Type: application/json

{
  "approvedAmount": 45000,
  "approvedTermMonths": 24,
  "approvedInterestRate": 2.45,
  "notes": "Bedingte Genehmigung"
}
```

### Stornieren (Pending)

```http
POST /api/CreditApplications/{id}/cancel
Authorization: Bearer <token>
Content-Type: application/json

{}
```

---

## Lizenz / Zweck

Bildungsbeispiel für Clean Architecture und full-stack Kreditworkflow.  
Vor Produktion Secrets, Connection Strings und Härtung anpassen.
