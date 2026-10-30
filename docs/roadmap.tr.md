# Yol haritası

[English](roadmap.en.md) · [Deutsch](roadmap.de.md)

## Yapıldı

- [x] Clean Architecture + CQRS (MediatR) .NET 10 API
- [x] Bireysel / kurumsal müşteri (TPT), kredi tipi / alt tip / başvuru
- [x] SmartEnum durum ve kategori alanları
- [x] JWT kimlik doğrulama, Customer / BankStaff RBAC
- [x] Serilog + Seq, Redis önbellek
- [x] Docker Compose (SQL, Redis, Seq, WebApi, React client)
- [x] Unit testler (Application)
- [x] React istemci (Vite + TypeScript), TR / EN / DE i18n
- [x] Para birimi gösterimi (TRY / EUR / USD), müşteri paneli
- [x] Başvuru iptali (Cancel, soft-delete değil)
- [x] Liste sıralama / durum filtresi, kredi türü kolonu
- [x] Müşteri yeni başvuru formu
- [x] Dile uygun seed ürün adları, yuvarlak tutar ve standart vadeler
- [x] Locale katalog (TR/EN/DE) + personel filtre + paket kısıtlı tutar/vade
- [x] Ödeme planı (amortisman)
- [x] displayName + dil bazlı demo hesaplar
- [x] Changelog / test notları
- [x] Kredi türleri sıralama + tablo boşlukları
- [x] Ödeme planı / onay vadesi tutarlılığı
- [x] JWT UTF-8 + customerCategory + profil sayfası
- [x] BankManager rolü; müşteri silme kuralları (pasif + açık kredi yok)
- [x] Gender allow-list; kamu bireysel kayıt; personel müşteri UI
- [x] Rol bazlı dashboard özeti + tüm sayfalarda istatistik şeridi
- [x] Personel listesinde satır içi hızlı onay / red kısayolları
- [x] Personel yönetimi UI
- [x] Personel/müşteri profil alanları + avatar (≤2 MB) + Seq audit kategorileri
- [x] BankManager kredi türü ekleme / aktif-pasif
- [x] Profil avatar crop + akademik ünvan/cinsiyet + sabit pozisyon + header avatar
- [x] Erişilebilirlik paneli (tema / yazı boyutu / kontrast) + üst döviz şeridi
- [x] CI (GitHub Actions): build + unit/integration tests + client build
- [x] WebApi entegrasyon test iskeleti (login smoke)
- [x] Entegrasyon: personel başvuru oluştur → onay akışı
- [x] Entegrasyon: red / iptal / müşteri create+cancel (+ müşteri onaylayamaz)
- [x] Datenschutz / çerez onayı (ilk yüklemede banner, tercih saklama)
- [x] Kayıt sonrası e-posta doğrulama (RabbitMQ kuyruk + Seq/log bağlantısı; JWT kayıtta verilmez)
- [x] Domain Events + Transactional Outbox (kredi başvurusu; kuyruk `domain-events`)
- [x] Idempotency-Key (kredi create/approve/reject/cancel; Redis)
- [x] Saga / Process Manager (kredi başvuru yaşam döngüsü; `CreditApplicationSagas`)
- [x] SignalR bildirimleri (Outbox olaylarına bağlı; hub `/hubs/credit-applications`)
- [x] Modular Monolith (Identity / Customers / Credit / Notification)
- [x] OpenTelemetry + Grafana / Prometheus (collector + Tempo; Seq log kalır)

## Yapılacak

### Faz 6 — Mimari derinleşme

Ayrıntılı tartışma: [architecture-notes.tr.md](architecture-notes.tr.md)

- [x] Domain Events + Transactional Outbox (RabbitMQ ile güvenilir yayın)
- [x] Idempotency-Key (kritik yazma API’leri)
- [x] Saga / Process Manager (kredi başvuru yaşam döngüsü)
- [x] SignalR bildirimleri (Outbox olaylarına bağlı anlık durum) — Modular’dan önce uygulandı
- [x] Modular Monolith (Identity / Customers / Credit / Notification)

## Dokümantasyon (sürekli)

- [x] Sıfırdan tech öğrenme kitaplığı (`docs/learning/`) + Cursor kuralı `tech-learning-library` — her yeni özellikte güncellenir

## İleride

- [ ] Personel GET’leri için Elasticsearch
- [ ] Admin karşılama dashboard (Tailwind CSS + paket sıkıştırma / performans)
- [ ] Kubernetes / ArgoCD dağıtımı
- [x] OpenTelemetry + Grafana / Prometheus (collector + Tempo; Seq log kalır)
- [x] HashiCorp Vault lab (Compose dev mode + `secret/bankapp` seed; WebApi henüz bağlı değil)
- [x] Rate limit politikaları (auth / write / fixed; 429)
- [x] Vault → WebApi JWT key overlay (lab; SQL hâlâ Compose env)
- [x] Rate limit ortam ayrımı (sıkı `appsettings.json` + gevşek Development; learning 23)
- [ ] Raft Vault / secret rotasyonu (üretim; Compose dev-mode dışı — learning 19 notu)
- [ ] OAuth (sosyal medya ile kayıt olma) mekanizması
- [ ] Keycloak desteği
- [x] Kredi başvurusunda evrak yükleme (lab: PDF/JPEG/PNG, App_Data; henüz onay için zorunlu değil)
- [x] Şube / branch modeli + karar veren personelde şube snapshot (lab)
- [x] Şube RBAC + SuperAdmin (liste filtreleri, modify guard, NYC/CHI seed, şube CRUD)
- [x] Karar mektubunu e-posta ile gönderme (lab: `decision-letter` kuyruk + Seq log; UI şablonları / Outbox)
- [x] Karar mektubunu SMS ile gönderme (lab: `decision-letter-sms` kuyruk + Seq log; gateway yok)
- [ ] Sohbet robotu (müşteri / personel destek asistanı)
