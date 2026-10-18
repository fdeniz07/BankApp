# BankApp

[English README](README.md) · [Türkçe README](README.tr.md)

BankApp ist ein **full-stack Bildungsbeispiel** für **Privat- und Firmenkreditanträge**.  
Backend: **.NET 10** Clean Architecture + CQRS. Frontend: **React (Vite + TypeScript)** mit **TR / EN / DE**-i18n. Lokal: **Docker Compose** (SQL Server, Redis, Seq, WebApi, Client).

**Dokumentation:** [Roadmap](docs/roadmap.de.md) · [Verlauf](docs/history.de.md) · [Changelog](docs/changelog.de.md) · [Testnotizen](docs/test-notes.de.md) · [Infra](infra/README.md)

---

## Inhaltsverzeichnis

1. [Demo-Logins](#demo-logins)
2. [Funktionen](#funktionen)
3. [Tech-Stack](#tech-stack)
4. [Lösungsstruktur](#lösungsstruktur)
5. [Architekturdiagramme](#architekturdiagramme)
6. [Domänenmodell](#domänenmodell)
7. [Geschäftsregeln](#geschäftsregeln)
8. [API-Überblick](#api-überblick)
9. [SmartEnum](#smartenum)
10. [Daten-Seed](#daten-seed)
11. [Schnellstart](#schnellstart)
12. [Beispielanfragen](#beispielanfragen)

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
- JWT-Auth + RBAC; öffentliche Privatkunden-Registrierung
- Genehmigen / Ablehnen / **Stornieren** (Statuswechsel — Storno ≠ Soft-Delete)
- Zahlungsplan, Dashboard-Summary, Personalverwaltung
- Profilfelder + Avatar (≤ 2 MB), Seq-Audit-Kategorien
- Kreditarten anlegen / ändern / löschen / aktiv-passiv (**BankManager**)
- Serilog → Seq, Redis-Cache, FluentValidation, ProblemDetails

### Frontend (`client/`)

- Login / Registrierung, Anträge, Kunden, Kreditarten, Personal, Profil
- Statistikleiste, FX-Ticker, Barrierefreiheits-Panel (Thema / Schrift / Kontrast)
- Sprachumschalter TR / EN / DE

---

## Tech-Stack

| Bereich | Technologie |
|---------|-------------|
| API | .NET 10, ASP.NET Core, MediatR, FluentValidation, AutoMapper, EF Core 10 |
| DB / Cache / Logs | SQL Server, Redis, Serilog + Seq |
| Domain-Enums | Ardalis SmartEnum (als **Name**) |
| Auth | JWT + Rollen-Claims |
| Client | React, Vite, TypeScript, react-i18next, react-router |
| Docs (Dev) | OpenAPI + Scalar (`/scalar/v1`) |
| Seed | Bogus (+ fester Kreditkatalog) |
| Infra | Docker Compose (`infra/`) |

---

## Lösungsstruktur

```
BankApp/
├── BankApp.slnx
├── README.md / README.tr.md / README.de.md
├── client/              # React-SPA
├── docs/
├── infra/
└── src/
    ├── WebApi/
    ├── Application/
    ├── Domain/
    ├── Persistence/
    └── Core/
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

| Docker Dev | URL |
|------------|-----|
| WebApi | http://localhost:8080 |
| Scalar | http://localhost:8080/scalar/v1 |
| Client | http://localhost:3000 |
| Seq | http://localhost:5341 |

Die meisten Endpunkte brauchen JWT (außer Login / öffentliche Registrierung).

| Controller | Highlights |
|------------|------------|
| **Auth** | `login`, `register`, `register-individual` |
| **IndividualCustomers** / **CorporateCustomers** | CRUD + inactive / deleted |
| **CreditTypes** | Liste; Mutation + `PUT {id}/active` (Manager) |
| **CreditSubTypes** | CRUD |
| **CreditApplications** | `approve` / `reject` / **`cancel`**; `payment-plan` |
| **Dashboard** | `GET summary` |
| **StaffUsers** | Liste / anlegen / ändern (Manager) |
| **Profile** | Eigenes Profil; Avatar |

Details: Scalar-UI.

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
