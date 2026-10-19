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

## Yapılacak

_(Şimdilik boş — sonraki maddeler “İleride” listesinden seçilebilir.)_

## İleride

- [ ] Personel GET’leri için Elasticsearch
- [ ] Admin karşılama dashboard (Tailwind CSS + paket sıkıştırma / performans)
- [ ] Kubernetes / ArgoCD dağıtımı
- [ ] OpenTelemetry + Grafana / Prometheus
- [ ] Üretim güvenlik sertleştirme (secret yönetimi, rate limit politikaları)
- [ ] OAuth (sosyal medya ile kayıt olma) mekanizması
- [ ] Keycloak desteği
- [ ] Sohbet robotu (müşteri / personel destek asistanı)
