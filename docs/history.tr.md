# Proje geçmişi

[English](history.en.md) · [Deutsch](history.de.md)

Kronolojik özet (eğitim / kurumsal BankApp çalışması).

## Faz 1 — Temel API

- Clean Architecture katmanları (Core, Domain, Application, Persistence, WebApi)
- Müşteri CRUD (bireysel / kurumsal, TPT)
- Kredi tipi, alt tip, başvuru; onay / red; soft delete
- FluentValidation, iş kuralları, AutoMapper, MediatR pipeline
- SmartEnum (durum, kategori, risk)
- Development Bogus seed

## Faz 2 — Güvenlik ve gözlemlenebilirlik

- ASP.NET Identity + JWT
- Customer / BankStaff rolleri (`ISecuredRequest`)
- ApplicationUser ↔ Customer eşlemesi
- Serilog + Seq
- Redis cache (kredi tipi / alt tip listeleri)

## Faz 3 — Altyapı

- Docker Compose: Seq, Redis, SQL Server (external volume), WebApi
- `infra/.env.example`, README
- `bin` / `obj` gitignore ve temizleme
- Application unit testleri

## Faz 4 — React istemci

- Vite + React + TypeScript istemci
- TR / EN / DE i18n (`react-i18next`)
- Giriş, başvuru listesi / detay, personel onay-red
- Client Docker (nginx) Compose’a eklendi
- Müşteri adı / kategori, para birimi kolonları
- EN / DE seed müşteri ve USD / EUR ürünler; TR ürünlerde TRY
- Müşteri paneli, yeni başvuru, iptal (Cancel)
- Liste sıralama ve durum filtresi

## Faz 5 — Veri kalitesi ve UX

- Alt tür adları ürüne göre TR / EN / DE eklerle uyumlu
- Yuvarlak tutarlar (örn. 50.000 ₺, 250.000 €), vade adımları 3–6–9–12–15–18–24–32
- Mevcut kayıtlar için seed sync (SQL wipe gerekmez)
- Tablo kayması giderimi; onay / red ve Detay / İptal buton boyutu eşitlendi
- `docs/` altında yol haritası ve geçmiş (TR / EN / DE)
- CI, entegrasyon testleri, Datenschutz, e-posta doğrulama (RabbitMQ), healthcheck’ler, müşteri no / form hizası

## Faz 6 — Mimari derinleşme (devam ediyor)

Ayrıntılı tartışma: [architecture-notes.tr.md](architecture-notes.tr.md)

- [x] Domain Events + Transactional Outbox (RabbitMQ, kredi başvurusu)
- [x] Idempotency-Key (kritik yazma API’leri; Redis, 24 saat TTL)
- [x] Saga / Process Manager (orkestrasyon; `CreditApplicationSagas`)
- [x] SignalR bildirimleri (Outbox → hub; Modular’dan önce)
- [x] Modular Monolith (Identity / Customers / Credit / Notification)
- [x] OpenTelemetry + Grafana / Prometheus (OTLP → collector → Tempo/Prometheus/Grafana)
- [x] Rate limit ortam ayrımı (sıkı base + Development lab) + Vault rotasyon/Raft notları
- [x] Başvuru evrak yükleme lab (App_Data + documents API; henüz zorunlu değil)
- [x] Şube modeli (Branches + staff BranchId + karar snapshot)
- [x] Şube RBAC + SuperAdmin (Customer/Application BranchId, NYC/CHI seed, şube CRUD UI)
- [x] Karar mektubu şablon CRUD + UI diline göre sistem şablonu; katalog ürünleri şube locale’ine kilitli (SuperAdmin hariç)
- [x] Demo giriş matrisi Personel/Yönetici/Müşteri × TR/DE/EN; ortak lab şifresi `Demo123!`
- [x] Onay soft-evrak uyarısı; karar audit (şube/şablon); evrak yükleme SignalR + eksik evrak rozeti/filtresi
