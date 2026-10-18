# Test notları

[English](test-notes.en.md) · [Deutsch](test-notes.de.md)

Bu sürümde doğrulananlar (uygulama sonrası):

- [x] `dotnet build` başarılı
- [x] Unit testler (Application) geçti
- [x] Client `npm run build` başarılı
- [x] Compose rebuild (webapi + client) başarılı
- [x] Seed sonrası kredi tipi sayısı = katalog boyutu (16: 6 TR + 5 EN + 5 DE)
- [x] Personel kredi listesinde locale filtresi (TR / EN / DE / Tümü)
- [x] DE UI’de müşteri formu yalnızca DE + EUR alt türleri listeler
- [x] Tutar ve vade `<select>`; serbest 60 ay / küsuratlı tutar girilemez
- [x] Paket dışı create isteği business exception döner
- [x] Ödeme planı satır sayısı = seçilen vade
- [x] Welcome / üst bar `displayName` (e-posta değil)
- [x] Demo hesaplar: staff/customer + `.tr` / `.en` / `.de`
- [x] SQL wipe + Redis boş cache ile tam liste görünür (eski 6 TR cache sorunu giderildi)
- [x] `WebApi.IntegrationTests` login smoke (3 test) geçti
- [x] Entegrasyon: approve / reject / cancel / müşteri akışları (toplam 9 test)
- [x] GitHub Actions CI workflow eklendi (`.github/workflows/ci.yml`)
