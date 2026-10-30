# Yol haritası

[English](roadmap.en.md) · [Deutsch](roadmap.de.md)

Kronolojik faz özeti için: [history.tr.md](history.tr.md). Mimari tartışma: [architecture-notes.tr.md](architecture-notes.tr.md).

## Yapıldı

### Mimari / altyapı

- [x] Clean Architecture + CQRS (MediatR) .NET 10 API
- [x] Docker Compose (SQL, Redis, Seq, RabbitMQ, WebApi, React client)
- [x] Modular Monolith (Identity / Customers / Credit / Notification)
- [x] Domain Events + Transactional Outbox (kredi başvurusu; kuyruk `domain-events`)
- [x] Idempotency-Key (kredi create/approve/reject/cancel; Redis)
- [x] Saga / Process Manager (kredi başvuru yaşam döngüsü; `CreditApplicationSagas`)
- [x] SignalR bildirimleri (Outbox → hub `/hubs/credit-applications`)

### Güvenlik / kimlik

- [x] JWT kimlik doğrulama; Customer / BankStaff / BankManager / SuperAdmin RBAC
- [x] Şube RBAC (liste filtreleri, modify guard; NYC/CHI seed, şube CRUD)
- [x] Kayıt sonrası e-posta doğrulama (RabbitMQ + Seq/log; JWT kayıtta verilmez)
- [x] Datenschutz / çerez onayı (banner + tercih saklama)
- [x] Rate limit politikaları (auth / write / fixed; 429) + ortam ayrımı (sıkı base, gevşek Development)
- [x] HashiCorp Vault lab (Compose dev mode + `secret/bankapp` seed)
- [x] Vault → WebApi JWT key overlay (lab; SQL hâlâ Compose env)
- [x] Demo giriş matrisi Personel/Yönetici/Müşteri × TR/DE/EN; ortak lab şifresi `Demo123!`

### Domain / iş mantığı

- [x] Bireysel / kurumsal müşteri (TPT), kredi tipi / alt tip / başvuru
- [x] SmartEnum durum ve kategori alanları
- [x] Başvuru iptali (Cancel), onay / red, ödeme planı (amortisman)
- [x] Locale katalog (TR/EN/DE) + paket kısıtlı tutar/vade; katalog şube locale’ine kilitli (SuperAdmin hariç)
- [x] Şube / branch modeli + karar snapshot (`DecisionBy` / şube / şablon adı)
- [x] Kredi başvurusunda evrak yükleme (lab: PDF/JPEG/PNG, App_Data)
- [x] Soft onay kapısı config (`SoftApproveGateEnabled`; Development kapalı) + eksik evrak rozeti/filtre/liste preset
- [x] Karar mektubu şablon CRUD (kullanıcı); UI diline göre sistem şablonu
- [x] BankManager kredi türü ekleme / aktif-pasif; müşteri silme kuralları

### UX / istemci

- [x] React istemci (Vite + TypeScript), TR / EN / DE i18n
- [x] Para birimi (TRY / EUR / USD), müşteri paneli, yeni başvuru formu
- [x] Liste sıralama / durum filtresi; personel satır içi hızlı onay / red
- [x] Rol bazlı dashboard özeti + istatistik şeridi
- [x] Personel yönetimi UI; personel/müşteri profil + avatar crop + header avatar
- [x] Erişilebilirlik paneli (tema / yazı boyutu / kontrast) + üst döviz şeridi
- [x] Gender allow-list; kamu bireysel kayıt; personel müşteri UI

### Gözlemlenebilirlik / kalite

- [x] Serilog + Seq; Redis önbellek
- [x] OpenTelemetry + Grafana / Prometheus (collector + Tempo; Seq log kalır)
- [x] Application unit testleri
- [x] WebApi entegrasyon testleri (login smoke; create→approve; reject/cancel; müşteri kuralları)
- [x] CI (GitHub Actions): build + testler + client build (`ubuntu-24.04`, Actions v5)

### Bildirim kanalları (lab)

- [x] Karar mektubu e-posta (kuyruk `decision-letter` + Seq log)
- [x] Karar mektubu SMS (kuyruk `decision-letter-sms` + Seq log; gateway yok)

### Dokümantasyon

- [x] Sıfırdan tech öğrenme kitaplığı (`docs/learning/`) + Cursor kuralı `tech-learning-library`
- [x] Yol haritası / geçmiş / mimari notlar (TR / EN / DE)

## Yapılacak

- [ ] Personel GET’leri için Elasticsearch
- [ ] Admin karşılama dashboard (Tailwind CSS + paket sıkıştırma / performans)
- [ ] Kubernetes / ArgoCD dağıtımı
- [ ] Raft Vault / secret rotasyonu (üretim; Compose dev-mode dışı — learning 19)
- [ ] OAuth (sosyal medya ile kayıt / giriş)
- [ ] Keycloak desteği
- [ ] Sohbet robotu (müşteri / personel destek asistanı)
