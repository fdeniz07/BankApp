# Teknoloji ve mimari öğrenme rehberi

[English](tech-learning.en.md) · [Deutsch](tech-learning.de.md)

**Amaç:** BankApp’i geliştirirken hem ürünü ilerletmek hem de modern bir tech stack’i *neden* kullanıldığını anlayarak öğrenmek.  
**İlişkili:** [roadmap.tr.md](roadmap.tr.md) · [architecture-notes.tr.md](architecture-notes.tr.md) · [README.tr.md](../README.tr.md)

> **Sıfırdan / komut / sınıf / paket derinliği:** [learning/00-index.tr.md](learning/00-index.tr.md) (inşa sırasına göre MD kitaplığı; proje bitene kadar güncellenir).

Bu belge “ne kurduk?” listesi değil; her madde için **ne / neden burada / avantaj / trade-off / repoda örnek** verir.

---

## Nasıl okunur?

1. Önce **Mimari** bölümünü oku — tüm seçimler buraya bağlanır.
2. Sonra bir özelliği uçtan uca takip et (ör. kredi onayı → Outbox → Saga → SignalR).
3. Kodda `Örnek` yollarına bak; dosyayı açıp akışı izle.
4. “Ne zaman kullanma?” notları, eğitim projesinde bile aşırı mühendislik tuzağını gösterir.

Önerilen sıra: Auth/JWT → CQRS handler → Domain Event + Outbox → Idempotency → Saga → SignalR → Modular Monolith → OTel.

---

## 1) Mimari

### Clean Architecture

| | |
|--|--|
| **Ne** | Bağımlılıklar içeriye akar: `Domain` → `Application` → `Persistence` / `WebApi`. UI ve DB domain’i bilmez; domain dış dünyayı bilmez. |
| **Neden** | Eğitimde katmanları ayırarak “iş kuralı nerede?” sorusunu netleştirmek. |
| **Avantaj** | Test edilebilirlik; framework değişince domain’in korunması. |
| **Trade-off** | Daha fazla proje/klasör; küçük CRUD’ta ağır gelebilir. |
| **Örnek** | `src/Domain`, `src/Application`, `src/Persistence`, `src/WebApi`, `src/Core` |

### CQRS + MediatR

| | |
|--|--|
| **Ne** | Yazma (Command) ve okuma (Query) isteklerini ayırır. MediatR, controller’dan handler’a mesaj taşır. |
| **Neden** | Onay/red/iptal gibi işlemler ile liste/detay sorgularını ayrı tutmak; pipeline (validation, auth, cache) eklemek kolay. |
| **Avantaj** | Tek sorumluluklu handler’lar; çapraz kesen davranışlar `IPipelineBehavior` ile. |
| **Trade-off** | Basit endpoint’lerde “indirection” artar. |
| **Örnek** | `ApproveCreditApplicationRequest` + Handler → `Application/Modules/Credit/Features/...` · kayıt: `ApplicationServiceRegistration.cs` |

**Mini senaryo:** Personel “Onayla” der → `CreditApplicationsController` MediatR’a command gönderir → Handler aggregate’i günceller → domain event çıkar.

### Modular Monolith

| | |
|--|--|
| **Ne** | Tek deploy / tek DB; kod **modül** sınırlarıyla ayrılır: Identity, Customers, Credit, Notification. |
| **Neden** | Mikroservise gitmeden bounded context öğrenmek; ileride ayrıştırmaya hazır iskelet. |
| **Avantaj** | Dağıtık sistem karmaşası yok; yine de klasör/namespace disiplini var. |
| **Trade-off** | Modül sınırları “yumuşak”; disiplin bozulursa yine büyük yığın olur. |
| **Örnek** | `Application/Modules/*` · `WebApi/Modules/*` · `AddApplicationModules()` |

### Aggregate + Domain Events

| | |
|--|--|
| **Ne** | Aggregate tutarlılık sınırıdır. Önemli durum değişince `IDomainEvent` üretir (ör. başvuru onaylandı). |
| **Neden** | “Onaylandı” bilgisini e-posta, saga, SignalR gibi yan etkilere **aynı transaction hikâyesi** ile bağlamak. |
| **Avantaj** | Domain, altyapıyı (RabbitMQ/SignalR) bilmez. |
| **Trade-off** | Event modeli tasarımı zaman alır; aşırı event = gürültü. |
| **Örnek** | `Domain` entity + events · harvest: Persistence `DbContext` / Outbox |

### Transactional Outbox

| | |
|--|--|
| **Ne** | Event önce DB’de `OutboxMessages` tablosuna yazılır; sonra worker RabbitMQ’ya yayınlar. |
| **Neden** | “DB commit oldu ama mesaj gitmedi” yarışını önlemek (dual-write problemi). |
| **Avantaj** | En az bir kez teslimat için sağlam temel; yeniden denenebilir. |
| **Trade-off** | Gecikme (poll/dispatcher); consumer idempotent olmalı. |
| **Örnek** | Tablo `OutboxMessages` · `OutboxDispatcherHostedService` · kuyruk `domain-events` |

```text
Handler → SaveChanges (aggregate + outbox satırı)
       → Dispatcher publish → RabbitMQ
       → Consumer → Saga + SignalR + decision-letter (lab)
```

### Saga / Process Manager

| | |
|--|--|
| **Ne** | Uzun süren süreci adım adım yöneten durum makinesi (burada kredi başvuru yaşam döngüsü). |
| **Neden** | Created → Approved/Rejected/Cancelled → onboarding stub’ları tek yerde izlensin. |
| **Avantaj** | “Şu an süreç neresinde?” sorgulanabilir (`CreditApplicationSagas`). |
| **Trade-off** | Dağıtık saga (choreography) ile karıştırılmamalı; bu örnek **orchestration** (process manager). |
| **Örnek** | `CreditApplicationProcessManager` · consumer içinde çağrı |

### Idempotency-Key

| | |
|--|--|
| **Ne** | İstemci her kritik yazmaya benzersiz anahtar gönderir; sunucu Redis’te yanıtı saklar / çakışmayı yakalar. |
| **Neden** | Çift tıklama, timeout + retry ile çift onay riskini kesmek. |
| **Avantaj** | Ağ güvenilir değilken yazma API’leri güvenli. |
| **Trade-off** | Client anahtar üretmeli; aynı key + farklı body = 409. |
| **Örnek** | Header `Idempotency-Key` · `IdempotencyKeyFilter` · React create/approve/reject/cancel |

---

## 2) Backend yığını

### .NET 10 + ASP.NET Core

Minimal API / controller host, DI, middleware, JWT, SignalR, health check. Eğitim değeri: modern .NET ile enterprise kalıplar.

**Örnek:** `WebApi` host · `ServiceCollectionExtensions` · `ApplicationBuilderExtensions`

### EF Core + SQL Server

İlişkisel model, migration, TPT müşteri hiyerarşisi, outbox/saga tabloları. Avantaj: güçlü sorgular ve transaction; trade-off: şema evrimi disiplin ister.

**Örnek:** `Persistence` · migration’lar · Compose’ta `bankapp-sql`

### FluentValidation

Handler’a girmeden input kuralları. Pipeline’da `RequestValidationBehavior`.

### AutoMapper

Entity ↔ DTO eşlemesi; feature `Profiles` klasörleri. Trade-off: “gizli” mapping — kritik alanlarda bilinçli kullan.

### Ardalis SmartEnum

Kredi durumu / kategori gibi değerler **isim** ile API’de; int magic number yok.

### JWT + RBAC

Roller: Customer, BankStaff, BankManager. Claim’lerle endpoint yetkisi. İleride Keycloak/OIDC (roadmap).

**Öğrenim ipucu:** Önce “kim ne yapabilir?” matrisini çiz, sonra attribute/policy ekle.

### Serilog → Seq

Yapısal log + Seq UI. Audit kategorileri ile iş olaylarını arama. OTel/Grafana ileride; Seq yerel öğrenim için hafif.

### ProblemDetails

Hataları RFC 7807 tarzı JSON ile dönmek; client tutarlı parse eder.

---

## 3) Altyapı ve gerçek zamanlı

### Redis

Cache (MediatR caching behaviors) + Idempotency store. Hızlı, TTL’li veri.

### RabbitMQ

Kuyruklar: `email-confirmation`, `domain-events`, `decision-letter`. Async iş; API’yi yavaşlatmadan yan etki.

### Karar mektubu e-posta (lab)

Onay/red sonrası müşteriye notes gövdesi: domain-events consumer → `IDecisionLetterPublisher` → `decision-letter` → Seq log (SMTP yok). Ayrıntı: [learning/22-decision-letter-email.tr.md](learning/22-decision-letter-email.tr.md).

### SignalR

Outbox consumer sonrası anlık UI: toast + liste yenileme. Hub: `/hubs/credit-applications` (JWT `access_token` query).

**Örnek:** `CreditApplicationsHub` · `CreditApplicationRealtimeNotifier` · `client/src/realtime/useCreditApplicationRealtime.ts`

### OpenTelemetry + Grafana / Prometheus

| | |
|--|--|
| **Ne** | Trace (istek zinciri) + metrik (süre, rate); OTLP ile dışarı aktarım. |
| **Neden** | Seq = log; OTel = “hangi istek yavaş / nerede kırıldı?” |
| **Avantaj** | Standart vendor-neutral; Tempo/Prometheus/Grafana ile lab. |
| **Trade-off** | Ek Compose servisleri; dashboard metrik adları sürüme göre değişebilir. |
| **Örnek** | `OpenTelemetryExtensions.cs` · Grafana `:3001` · [infra/README.md](../infra/README.md) lab |

```text
WebApi → otel-collector → Tempo (traces)
                       → Prometheus (metrics) → Grafana
Seq: yapısal log (+ TraceId/SpanId)
```

### HashiCorp Vault (lab)

Compose **dev** mode + `secret/bankapp` seed. WebApi henüz Vault’tan okumaz — amaç secret yönetimini tanımak.

### Docker Compose

Tek komutla Seq, Redis, RabbitMQ, SQL, Vault, OTel yığını, WebApi, Client. Healthcheck zinciri ile “hazır olunca ayağa kalk”.

**Örnek:** `infra/docker-compose.yml` · [infra/README.md](../infra/README.md)

---

## 4) Frontend

### React + Vite + TypeScript

Hızlı DX (Vite), tip güvenliği (TS), bileşen modeli (React). BankApp SPA: auth, başvurular, personel, profil.

### react-i18next (TR / EN / DE)

Ürün dili + doküman dili paralelliği; i18n’i “sonradan” değil baştan öğrenmek.

### SignalR client + Idempotency-Key

Yazma isteklerinde UUID key; hub ile canlı durum. Böylece backend’deki iki kalıp UI’da da görünür.

**Örnek:** `client/src/realtime/useCreditApplicationRealtime.ts`

---

## 5) Kalite ve CI

| Parça | Neden |
|--------|--------|
| xUnit (Application.UnitTests) | Handler / rules / saga birim testleri |
| WebApi.IntegrationTests | HTTP + gerçek pipeline’a yakın senaryolar |
| GitHub Actions | Her push’ta build + test + client build |

Test, mimariyi “bozulmadan refaktör” için güvenlik ağıdır.

---

## 6) Uçtan uca örnek: kredi onayı

```text
[React] Onayla + Idempotency-Key
    → [WebApi] CreditApplicationsController
    → [MediatR] ApproveCreditApplication handler
    → [Domain] status + domain event
    → [EF] CreditApplications + OutboxMessages (aynı SaveChanges)
    → [Dispatcher] RabbitMQ domain-events
    → [Consumer] ProcessManager (saga adımı)
                 + SignalR staff/customer grupları
    → [React] toast + liste refresh
```

Bu tek akışta CQRS, domain event, outbox, saga, idempotency ve SignalR bir arada görünür — öğrenirken en değerli senaryo budur.

---

## 7) Ne zaman kullanma? (kısa)

| Kalıp | Erken kullanma |
|--------|----------------|
| Outbox / Saga | Tek tablolu CRUD, senkron yeterliyse |
| Modular Monolith klasörleri | 2–3 feature’lık deneme API |
| Idempotency | Salt okuma veya doğal idempotent GET |
| SignalR | Nadiren değişen, poll’un yeterli olduğu ekranlar |
| Vault | Sadece `.env` ile giden lokal demo (yine de lab faydalı) |

---

## 8) İleride (roadmap)

Keycloak, Elasticsearch, K8s/ArgoCD, chatbot — [architecture-notes.tr.md](architecture-notes.tr.md) ve [roadmap.tr.md](roadmap.tr.md). OTel yığında; Grafana’da dolaş, sonra bir sonraki katmana aynı “neden / örnek” şablonunu uygula.
