# Mimari ve eğitim tartışma notları

[English](architecture-notes.en.md) · [Deutsch](architecture-notes.de.md)

**Tarih:** 2026-10-21  
**Bağlam:** Eğitim amaçlı BankApp — sonraki fazlara geçmeden önce tekrar bakılacak notlar.

Yol haritası: [roadmap.tr.md](roadmap.tr.md) (Faz 6).

---

## Mevcut yığın (özet)

Clean Architecture + CQRS (MediatR), JWT/RBAC, Redis, Serilog+Seq, RabbitMQ (e-posta doğrulama), Docker Compose, React i18n (TR/EN/DE), CI + entegrasyon testleri.

Bundan sonrası “eksik feature” değil; **öğrenim değeri yüksek modern katmanlar** eklemek.

---

## Seçilen odak

**Mimari derinleşme** (platform / ürün-AI değil).

Kapsam dışı (şimdilik): OTel/Grafana uygulaması, Keycloak, OAuth, K8s, AI chatbot kodu.

---

## Önerilen teknoloji grupları (tartışma özeti)

### 1) Mimari / domain

| Öneri | Ne öğretir |
|--------|------------|
| Domain Events + Outbox | Aggregate sonrası olay, tutarlı yayın |
| Saga / Process Manager | Uzun kredi süreci orkestrasyonu |
| Modular Monolith | Bounded context; tek deploy |
| Idempotency-Key | Çift tıklama / retry güvenliği |
| Vertical Slice sertleştirme | CQRS’i ürün gibi ölçekleme |

### 2) Veri & arama

| Öneri | Not |
|--------|-----|
| Elasticsearch / OpenSearch | Personel listeleri; Outbox/read-model sonrası daha anlamlı |
| CDC (Debezium) → ES | İleri seviye senkron |
| Sorgulanabilir audit tablosu | Seq’e ek |

### 3) Gerçek zamanlı & UX

| Öneri | Not |
|--------|-----|
| SignalR | Başvuru durumu; Outbox olaylarının görünür kanıtı |
| Notification hub | In-app + e-posta |
| BFF / TanStack Query | Modern client veri katmanı |

### 4) Kimlik & güvenlik

| Öneri | Not |
|--------|-----|
| Keycloak / OIDC önce, sosyal OAuth sonra | Enterprise auth |
| Refresh token + rotation | Üretim benzeri JWT |
| Secret yönetimi, rate limit | Hardening |

### 5) Observability & platform

| Öneri | Not |
|--------|-----|
| OpenTelemetry → Prometheus + Grafana | Seq ile yan yana |
| .NET Aspire | Local orchestration |
| k3d/kind + Helm + ArgoCD | GitOps lab |
| Feature flags, Pact | Progressive delivery / contract test |

### 6) AI (abartmadan)

| Öneri | Not |
|--------|-----|
| Destek chatbot (RAG + tool) | Ürün FAQ + başvuru sorgulama |
| Semantic Kernel / Extensions.AI | .NET AI entegrasyonu |

### 7) Banka domain senaryoları

Kredi skor kartı, sözleşme PDF, taksit/gecikme, IBAN/transfer (idempotent), fraud alert, multi-tenant şube — teknolojiyi görünür kılar.

---

## Mevcut “İleride” listesine yorum

- **Elasticsearch** — iyi; önce Outbox veya event hikâyesi daha pedagojik.
- **Admin Tailwind dashboard** — görsel; mimari öğrenim düşük.
- **K8s/ArgoCD** — monolit modülerleşmeden ağır.
- **OTel/Grafana** — Seq yanına güçlü; Faz 6 sonrası doğal aday.
- **OAuth + Keycloak** — Keycloak/OIDC önce, sosyal login sonra.
- **Chatbot** — “sadece UI chat” yerine RAG + tool ayrı lab gibi düşünülmeli.

---

## Faz 6 — seçilen sıra

1. Domain Events + Transactional Outbox (RabbitMQ) — **uygulandı** (2026-10-21): `AggregateRoot`, `OutboxMessages`, dispatcher + Seq/log consumer; kredi create/approve/reject/cancel olayları
2. Idempotency-Key (kritik yazma API’leri)
3. Saga / Process Manager (kredi başvuru yaşam döngüsü)
4. Modular Monolith (Customer / Credit / Identity / Notification)
5. SignalR bildirimleri (Outbox olaylarına bağlı)

```mermaid
flowchart LR
  A[Outbox_DomainEvents] --> B[Idempotency]
  B --> C[Saga]
  C --> D[Modular_Monolith]
  D --> E[SignalR]
```

Daha geniş eğitim sırası (Faz 6 dışı referans): Outbox → SignalR → OTel → Keycloak → ES/modül → AI RAG.

---

## Tekrar bakılacak

Sonraki kod adımı: **Idempotency-Key** (ayrı plan ve onay). Outbox uygulaması: `BankingDbContext` harvest, kuyruk `domain-events`.
