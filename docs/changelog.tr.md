# Değişiklik günlüğü

[English](changelog.en.md) · [Deutsch](changelog.de.md)

## 2026-10-19 — Beni hatırla + müşteri profil alanları

- Login: “Beni hatırla” → uzun JWT (7 gün) + `localStorage`; işaretsiz → kısa JWT + `sessionStorage`; e-posta hatırlama.
- Müşteri profili: bireysel (doğum/anne/baba + salt okunur no/TCKN), kurumsal (vergi/sicil alanları).

## 2026-10-19 — E-posta doğrulama (RabbitMQ)

- Bireysel kayıt JWT vermez; `EmailConfirmed` + token (24 saat); giriş müşteri için onay ister.
- `POST /api/Auth/confirm-email`; client `/confirm-email?token=…`.
- RabbitMQ kuyruk (Compose); broker kapalıysa bağlantı Seq/console log’a yazılır.
- Mevcut / seed kullanıcılar onaylı; yeni kayıtlar onay bekler.

## 2026-10-19 — Datenschutz / çerez onayı

- İlk yüklemede gizlilik/çerez bandı (Kabul / Reddet); tercih `localStorage` (`bankapp-consent`).
- TR / EN / DE metinler; erişilebilirlik FAB ile çakışmayan yerleşim.

## 2026-10-18 — CI + entegrasyon testleri

- GitHub Actions CI: `dotnet build/test` (unit + integration) ve `client` npm build.
- `WebApi.IntegrationTests`: InMemory DB + memory cache factory; login smoke.
- Akışlar: create→onay, create→red, staff/müşteri iptal; müşteri onaylayamaz (401); tokensız create → 401.

## 2026-10-16 — Erişilebilirlik, döviz şeridi, Detay butonu

- Sol alt erişilebilirlik paneli: açık/koyu tema, yazı boyutu, yüksek kontrast (`localStorage`).
- Üst bantta kayan döviz pariteleri (USD/TRY, EUR/TRY vb.; open.er-api, yedek kurlar).
- Liste **Detay** butonu slate-mavi (`btn info`); aksiyon aralığı artırıldı.

## 2026-10-16 — Profil crop, ünvan/cinsiyet, sabit pozisyon, header avatar

- Avatar: çerçevede sürükle / yakınlaştır / ortala, sonra yükle.
- Akademik ünvan (`Dr`/`Prof`) ve cinsiyet (`Gender`) ayrıldı.
- Personel pozisyonu sabit SmartEnum listesinden (dil bazlı etiket).
- Üst bardaki kullanıcı alanında dairesel avatar; isim/resim → `/profile`.

## 2026-10-16 — Manager kredi türü yönetimi

- Create / Update / Delete yalnızca `BankManager`.
- `PUT /api/CreditTypes/{id}/active` ile aktif/pasif.
- Pasif üst kredi türünde yeni başvuru engellenir.
- UI: Manager için oluşturma formu + satır içi aktif/pasif.

## 2026-10-16 — Profil, avatar, Seq audit

- `ApplicationUser` profil alanları: FirstName, LastName, PersonTitle (SmartEnum), DateOfBirth, Position, ProfileImagePath.
- Personel kendi profilini güncelleyebilir; yönetici tüm personeli düzenler (e-posta/şifre/durum/rol dahil).
- Avatar yükleme: JPEG/PNG/WebP, en fazla 2 MB, magic-byte doğrulama; `wwwroot/uploads/profiles`.
- Seq’e yapılandırılmış audit: Auth / Credit / Staff / Profile (`EventCategory`, `EventAction`).
- E-posta doğrulamasında Türkçe karakter yasağı; JWT `nameidentifier` claim düzeltmesi (SOAP URI).

## 2026-10-16 — Dashboard istatistikleri (tüm roller)

- `GET /api/Dashboard/summary`: personel için aktif/pasif müşteri + başvuru kırılımı; müşteri için kendi başvuru sayıları.
- Tüm sayfalarda üst istatistik şeridi (müşteri sayısı personelde, başvuru/bekleyen herkese).
- Başvurular sayfasında rol bazlı dashboard paneli; sayılar API’den (sayfa verisi değil).

## 2026-10-16 — Müşteri yönetimi, BankManager, bireysel kayıt

- `BankManager` claim + demo `manager@bank.local` / `Manager123!` (BankStaff + BankManager).
- Soft-delete yalnızca BankManager; müşteri önce pasif olmalı ve Pending/Approved başvurusu olmamalı.
- `Gender` allow-list (`Female` / `Male` / `PreferNotToSay`); create / update / kayıt doğrulaması.
- Public `POST /api/Auth/register-individual` + `/register` sayfası.
- Personel `/customers` listesi (aktif / pasif / silinmiş, bireysel / kurumsal), oluşturma formu ve detay (IsActive, silme).

## 2026-10-16 — UX bugfix (boşluk, dil, kategori, toast)

- Kredi türleri Ad/Kod sütun boşluğu güçlendirildi.
- Login `displayName` / `customerCategory` artık JWT decode’a güvenmeden local override ile yazılıyor (Türkçe karakterler).
- Seed aylık gelir talep tutarına oranlandı (abartılı gelir azaltıldı).
- Müşteri başvuru formu yalnızca kendi kategorisindeki (bireysel/kurumsal) ürünleri listeler; API `category` filtresi.
- Create validasyonunda boş `CustomerId` engeli kaldırıldı; `react-toastify` + dil bazlı hata mesajları.

## 2026-10-16 — UX düzeltmeleri, profil, JWT UTF-8

- Kredi türleri listesinde sütun sıralama (`SortBy` / `SortDesc`) ve Ad/Kod sütun boşlukları.
- Başvuru detayında onaylanan vade; ödeme planı etkin tutar/vadeyi açıkça gösterir (Pending önizleme / Approved onay değerleri).
- Seed’de onay vadesi talep vadesini aşmaz; sıkça eşit seçilir.
- JWT `displayName` UTF-8 güvenli decode; `customerCategory` claim; personel için anlamlı displayName.
- `/profile` sayfası: müşteri ad/iletişim düzenleme; header’da müşteri türü.
- Roadmap İleride: e-posta doğrulama (RabbitMQ/Kafka), Elasticsearch, Tailwind admin dashboard.

## 2026-10-16 — Locale katalog, paket kısıtları, ödeme planı

- `CreditType.Locale` eklendi; TR→TRY, EN→USD, DE→EUR katalogları yeniden seed edildi.
- Müşteri UI diline göre yalnızca ilgili locale ürünlerini görür; tutar/vade paket seçeneklerinden gelir.
- Personel kredi türleri sayfasında TR/EN/DE filtresi.
- Sunucu tarafı paket doğrulaması (tutar adımı + izinli vadeler 3…60).
- Amortisman ödeme planı API + UI önizlemesi.
- Girişte `displayName` (ad soyad / firma); dil bazlı demo hesaplar.
- SQL volume + Redis sıfırlandı (temiz seed).
- Dokümanlar: changelog, test notları, roadmap/history güncellemesi.
