# BankApp

[English README](README.md) · [Deutsche README](README.de.md)

BankApp, **bireysel ve kurumsal kredi başvuruları** için eğitim amaçlı **full-stack** bir bankacılık örneğidir.  
Backend: **.NET 10** Clean Architecture + CQRS (MediatR), **Modular Monolith** (Identity / Customers / Credit / Notification), Domain Events + Outbox, Idempotency-Key, Saga, SignalR.  
Frontend: **React (Vite + TypeScript)**, **TR / EN / DE** i18n, anlık kredi bildirimleri (SignalR).  
Yerel yığın: **Docker Compose** (SQL Server, Redis, Seq, RabbitMQ, Vault lab, WebApi, client).

**Dokümanlar:** [Yol haritası](docs/roadmap.tr.md) · [Tech öğrenme](docs/tech-learning.tr.md) · [Mimari notlar](docs/architecture-notes.tr.md) · [Geçmiş](docs/history.tr.md) · [Changelog](docs/changelog.tr.md) · [Test notları](docs/test-notes.tr.md) · [Altyapı](infra/README.md)

---

## İçindekiler

1. [Demo girişler](#demo-girişler)
2. [Özellikler](#özellikler)
3. [Teknoloji yığını](#teknoloji-yığını)
4. [Çözüm yapısı](#çözüm-yapısı)
5. [Mimari diyagramlar](#mimari-diyagramlar)
6. [Alan modeli](#alan-modeli)
7. [İş kuralları](#iş-kuralları)
8. [API özeti](#api-özeti)
9. [SmartEnum](#smartenum)
10. [Veri seed](#veri-seed)
11. [Kurulum](#kurulum)
12. [Örnek istekler](#örnek-istekler)

---

## Demo girişler

| Rol | E-posta | Şifre |
|-----|---------|-------|
| Banka personeli | `staff@bank.local` (ayrıca `staff.tr` / `.en` / `.de`) | `Staff123!` |
| Banka yöneticisi | `manager@bank.local` | `Manager123!` |
| Müşteri | `customer@bank.local` (ayrıca `customer.tr` / `.en` / `.de`) | `Customer123!` |

Roller: **Customer**, **BankStaff**, **BankManager** (yönetici aynı zamanda BankStaff).

---

## Özellikler

### Backend

- Bireysel / kurumsal müşteri (TPT), kredi tipi & alt tip, başvuru
- JWT + RBAC; kamuya açık bireysel kayıt + e-posta doğrulama (RabbitMQ)
- Onay / red / **iptal** (durum değişimi — iptal soft-delete değildir)
- **Domain Events + Transactional Outbox** (kredi yaşam döngüsü → kuyruk `domain-events`)
- **Idempotency-Key** (create / approve / reject / cancel; Redis, 24 saat)
- **Saga / Process Manager** (`CreditApplicationSagas`; Created → karar → onboarding stub’ları)
- **SignalR** (`/hubs/credit-applications`; staff + müşteri grupları; Outbox consumer sonrası yayın)
- Ödeme planı, dashboard özeti, personel yönetimi
- Profil alanları + avatar (≤ 2 MB), Seq audit kategorileri
- Kredi tipi oluşturma / güncelleme / silme / aktif-pasif (**BankManager**)
- Serilog → Seq, Redis önbellek, FluentValidation, ProblemDetails
- CI: GitHub Actions (build + unit/integration + client)

### Frontend (`client/`)

- Giriş / kayıt / e-posta onay, başvurular, müşteriler, kredi türleri, personel, profil
- İstatistik şeridi, döviz ticker, erişilebilirlik paneli (tema / yazı / kontrast)
- Datenschutz / çerez bandı; dil seçici TR / EN / DE
- Kredi yazma isteklerinde `Idempotency-Key` (UUID)
- SignalR: başvuru durumu toast + liste/detay yenileme

---

## Teknoloji yığını

| Alan | Teknoloji |
|------|-----------|
| API | .NET 10, ASP.NET Core, MediatR, FluentValidation, AutoMapper, EF Core 10 |
| Domain | AggregateRoot, Domain Events (`CreditApplication*`), Ardalis SmartEnum (**isim**) |
| DB / önbellek / log | SQL Server, Redis, Serilog + Seq |
| Observability | OpenTelemetry (OTLP) → collector → Tempo + Prometheus + Grafana `:3001` |
| Messaging | RabbitMQ (`email-confirmation`, `domain-events`) + Transactional Outbox |
| Idempotency | Header `Idempotency-Key` + Redis (kredi yazma API’leri, 24 saat) |
| Saga | `CreditApplicationProcessManager` + tablo `CreditApplicationSagas` |
| Realtime | SignalR hub `/hubs/credit-applications` (JWT `access_token`) |
| Secrets (lab) | HashiCorp Vault **dev** (`secret/bankapp`; WebApi henüz Vault okumaz) |
| Auth | JWT + rol claim’leri (Customer / BankStaff / BankManager) |
| Client | React, Vite, TypeScript, react-i18next, react-router, `@microsoft/signalr` |
| Dokümantasyon (Dev) | OpenAPI + Scalar (`/scalar/v1`) |
| Test / CI | xUnit (Application + WebApi.IntegrationTests), GitHub Actions |
| Seed | Bogus (+ sabit kredi kataloğu) |
| Altyapı | Docker Compose (`infra/`) — healthcheck’li servis zinciri |

---

## Çözüm yapısı

```
BankApp/
├── BankApp.slnx
├── README.md / README.tr.md / README.de.md
├── client/              # React SPA
├── docs/                # yol haritası, tech öğrenme, mimari notlar, changelog, geçmiş
├── infra/               # docker-compose, .env.example, scripts/vault-seed.sh
├── tests/               # Application.UnitTests, WebApi.IntegrationTests
└── src/
    ├── WebApi/          # Host + Modules/{Identity,Customers,Credit,Notification}
    ├── Application/     # Modules/{Identity,Customers,Credit,Notification} + shared Services
    ├── Domain/          # Entities, Events, SmartEnums, CreditApplicationSaga
    ├── Persistence/     # DbContext (outbox harvest), migrations, seed
    └── Core/            # Entity, AggregateRoot, security, pipelines
```

**Bağımlılık yönü:** `WebApi` → `Application` / `Persistence` → `Domain` / `Core`.

---

## Mimari diyagramlar

### Katman bağımlılıkları

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

### İstek akışı (CQRS)

```mermaid
sequenceDiagram
    participant Client as İstemci
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
    Handler->>Rules: İş kuralları
    Handler->>Repo: Sorgu / komut
    Repo->>Db: SQL
    Db-->>Repo: Veri
    Repo-->>Handler: Entity / sayfa
    Handler-->>Controller: Response DTO
    Controller-->>Client: JSON / ProblemDetails
```

### Kredi başvurusu yaşam döngüsü

```mermaid
stateDiagram-v2
    [*] --> Pending: Oluştur
    Pending --> Approved: Onayla
    Pending --> Rejected: Reddet
    Pending --> Cancelled: İptal
    Pending --> Pending: Güncelle (yalnızca Pending)
    Approved --> [*]
    Rejected --> [*]
    Cancelled --> [*]
```

---

## Alan modeli

### Müşteriler (TPT)

| Entity | Not |
|--------|-----|
| **Customer** | Ortak: numara, e-posta, telefon, adres, `IsActive`, soft-delete |
| **IndividualCustomer** | Ad, TCKN, doğum tarihi, cinsiyet, … |
| **CorporateCustomer** | Şirket, vergi, yetkili kişi, … |

### Kredi kataloğu ve başvurular

| Entity | Rol |
|--------|-----|
| **CreditType** | Ürün (kategori, risk, para birimi, locale, aktif) |
| **CreditSubType** | Paket (tutar/vade aralığı, oranlar) |
| **CreditApplication** | Talep + durum + karar alanları |

### Soft delete ve aktiflik

| Bayrak | Anlam |
|--------|-------|
| **`IsActive`** | İş: yeni işlemlerde kullanılabilir mi |
| **`IsDeleted`** | Teknik soft delete (`DeletedDate`) |

Başvuru **iptali** `Cancelled` durumuna özel komutla geçer (soft-delete değil).

---

## İş kuralları

### Müşteriler

- Benzersizlik kuralları (TCKN / vergi / sicil / müşteri no / e-posta).
- Soft delete: yalnızca **BankManager**; müşteri pasif olmalı ve açık Pending/Approved başvurusu olmamalı.

### Kredi tipleri ve alt tipler

- Kod benzersizliği; tutar/vade aralığı tutarlılığı.
- Yeni başvuru için üst tip ve alt tip **aktif** olmalı.
- Kategori eşleşmesi bireysel ↔ Individual, kurumsal ↔ Corporate.
- Kredi tipi mutasyonları yalnızca yönetici.

### Kredi başvuruları

- Tutar/vade paket aralığında (ve paket kısıtları).
- Yalnızca **Pending** güncellenebilir / onaylanabilir / reddedilebilir / iptal edilebilir.

---

## API özeti

| Ortam (Docker Dev) | URL |
|--------------------|-----|
| WebApi | http://localhost:8080 |
| Scalar | http://localhost:8080/scalar/v1 |
| Client | http://localhost:3000 |
| Seq | http://localhost:5341 |
| RabbitMQ UI | http://localhost:15672 |
| Vault UI (lab) | http://localhost:8200 |

Login / kamu kayıt / e-posta onay dışında çoğu endpoint JWT ister.  
Kredi **yazma** (`POST` create/approve/reject/cancel): zorunlu header **`Idempotency-Key`**.

| Controller | Öne çıkanlar |
|------------|--------------|
| **Auth** | `login`, `register`, `register-individual`, `confirm-email` |
| **IndividualCustomers** / **CorporateCustomers** | CRUD + inactive / deleted |
| **CreditTypes** | Liste; mutasyon + `PUT {id}/active` (manager) |
| **CreditSubTypes** | CRUD |
| **CreditApplications** | `approve` / `reject` / **`cancel`** (+ Idempotency-Key); `payment-plan` |
| **Dashboard** | `GET summary` |
| **StaffUsers** | Liste / oluştur / güncelle (manager) |
| **Profile** | Profilim; personel güncelleme; avatar |

Detaylar: Scalar UI.

---

## SmartEnum

Değerler **isim** olarak saklanır (`"Pending"`), int değil.

| Tip | Örnek değerler |
|-----|----------------|
| `CreditCategory` | Individual, Corporate |
| `CreditRiskLevel` | Low, Medium, High |
| `CreditApplicationStatus` | Pending, Approved, Rejected, Cancelled |
| `Gender` | Female, Male, PreferNotToSay |
| `PersonTitle` | Dr, Prof |
| `StaffPosition` | CreditAnalyst, RelationshipManager, BranchOfficer, RiskOfficer, OperationsSpecialist, BranchManager |

---

## Veri seed

`Database__RunSeed` açıkken (Docker / Development tipik). Top-up: eksik kalanlar tamamlanır.

| Seed | Yaklaşık hedef |
|------|----------------|
| Bireysel müşteri | 30 (10 × tr/en/de) |
| Kurumsal müşteri | 30 (10 × tr/en/de) |
| Kredi tipi | 16 (TRY/USD/EUR katalogları) |
| Kredi alt tipi | Tip başına paketler |
| Başvuru | 48 |
| Identity | Demo staff / manager / customer |

---

## Kurulum

### A — Docker (önerilen)

```bash
copy infra\.env.example infra\.env
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --build
```

Ayrıntılar: [infra/README.md](infra/README.md).

### B — Yerel API + client

**Önkoşul:** .NET 10 SDK, SQL Server, Node.js 20+.

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

Connection string ve `TokenOptions__SecurityKey` için `appsettings` / User Secrets / ortam değişkenleri kullanın.

### Testler ve CI

```bash
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj
dotnet test tests/WebApi.IntegrationTests/WebApi.IntegrationTests.csproj
```

GitHub Actions (`.github/workflows/ci.yml`) `main`/`master` push/PR’da backend build + her iki test projesi ve React client build çalıştırır.

---

## Örnek istekler

### Giriş

```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "staff@bank.local",
  "password": "Staff123!"
}
```

Token: `Authorization: Bearer <token>`.

### Başvuru oluşturma

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
  "purpose": "İhtiyaç",
  "notes": null
}
```

### Onay

```http
POST /api/CreditApplications/{id}/approve
Authorization: Bearer <token>
Content-Type: application/json

{
  "approvedAmount": 45000,
  "approvedTermMonths": 24,
  "approvedInterestRate": 2.45,
  "notes": "Koşullu onay"
}
```

### İptal (Pending)

```http
POST /api/CreditApplications/{id}/cancel
Authorization: Bearer <token>
Content-Type: application/json

{}
```

---

## Lisans / amaç

Eğitim amaçlı Clean Architecture + full-stack kredi iş akışı örneği.  
Üretim öncesi secret, connection string ve güvenlik sertleştirmesini uyarlayın.
