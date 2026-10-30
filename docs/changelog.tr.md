# Değişiklik günlüğü

[English](changelog.en.md) · [Deutsch](changelog.de.md)

## 2026-10-30 — Evrak soft-kapı + karar audit + doküman SignalR

- Onay öncesi kimlik/gelir eksikse soft confirm (hard-block yok).
- `DecisionTemplateName` + liste/detay karar audit satırı (kim · şube · şablon).
- `CreditApplicationDocumentUploaded` → Outbox/SignalR toast; liste/detay kırmızı “evrak eksik” rozeti + filtre.

## 2026-10-30 — Karar şablonları + demo giriş matrisi + katalog locale

- `DecisionLetterTemplates` + API CRUD; başvuru detayında sistem/kullanıcı şablon seçici (UI diline göre otomatik).
- Liste üzerinden hızlı Onayla/Reddet kaldırıldı (yalnızca detay).
- Personel kredi kataloğu şube ülkesine kilitli (`CatalogLocaleService`); SuperAdmin tüm locale’leri görür.
- Demo hesaplar: Personel / Yönetici / Müşteri × TR/DE/EN; ortak lab şifresi `Demo123!` (seed backfill).
- Learning: `29-decision-letter-templates.tr.md`.

## 2026-10-29 — Şube / branch modeli

- `Branches` tablosu + seed (IST-01, ANK-01, BER-01); demo staff `BranchId`.
- Onay/red: `DecisionBranchCode` / `DecisionBranchName` snapshot.
- `GET /api/Branches`; personel listesi / profil / başvuru detayında şube.
- Learning: `27-branch-model.tr.md`.
- Şube RBAC + SuperAdmin + Customer/Application `BranchId` (ayrı dilim; learning `28`).

## 2026-10-28 — Başvuru evrak yükleme (lab)

- `CreditApplicationDocuments` + disk `App_Data/credit-documents` (wwwroot değil; JWT ile download).
- API: `GET/POST …/documents`, `GET …/documents/{id}/download`; Pending + ≤5 MB PDF/JPEG/PNG; max 10.
- React başvuru detayı: yükleme formu + liste; Compose volume `credit-document-uploads`.
- Learning: `26-credit-application-documents.tr.md`.

## 2026-10-28 — Güvenlik sertleştirme lab (rate limit ortamı)

- `appsettings.json`: sıkı prod-benzeri limitler (Auth 10 / Write 30 / Fixed 60 per 60s).
- `appsettings.Development.json`: lab gevşek limitler (20 / 60 / 100) — Compose varsayılanı Development.
- Learning: `23-rate-limiting` ortam tablosu + checklist; `19-vault-lab` secret rotasyonu / Raft “sonra” notu.
- Raft Vault ve üretim secret rotasyonu hâlâ ayrı dilim.

## 2026-10-25 — Ödeme planı PDF / yazdır + toast renkleri

- `GET /api/CreditApplications/payment-plan/export?format=pdf` (pluggable exporter; CSV/XLSX için hazır).
- Müşteri + personel: PDF indir / yazdır; taksit tablosu açılır-kapanır.
- SignalR toast: onay success, red error, iptal warning.

## 2026-10-25 — OpenTelemetry + Grafana / Prometheus

- WebApi: OTLP traces + metrics (`OpenTelemetry` config; Compose’ta açık).
- Compose: `otel-collector`, Tempo, Prometheus, Grafana (`:3001`).
- Serilog `TraceId` / `SpanId` enricher (Seq ↔ trace korelasyonu).
- Seq log için kalır; OTel metrik/trace laboratuvarı.

## 2026-10-24 — Modular Monolith (Identity / Customers / Credit / Notification)

- Application: `Modules/{Identity,Customers,Credit,Notification}` + `AddApplicationModules()`.
- WebApi: controller / Idempotency / Messaging / SignalR aynı sınırlara taşındı; host module kayıtları.
- Tek deploy, tek DB; `Customers` adı entity `Customer` ile çakışmayı önler.
- Faz 6 ana dilimler tamam.

## 2026-10-23 — SignalR (kredi başvurusu anlık bildirim)

- Hub `/hubs/credit-applications` (JWT `access_token`); gruplar `staff` + `customer:{id}`.
- Domain events consumer → Process Manager sonrası hub publish.
- React `@microsoft/signalr`: toast + başvuru listesi/detay yenileme.
- Modular Monolith’ten önce uygulandı (pedagojik sıra).

## 2026-10-22 — Saga / Process Manager (kredi başvurusu)

- `CreditApplicationSaga` + SmartEnum status/step; tablo `CreditApplicationSagas`.
- `CreditApplicationProcessManager`: Created → AwaitingDecision; Approved → Risk/Contract/Notify → Completed; Rejected/Cancelled → Terminated.
- Domain events consumer Process Manager’a bağlandı; unit testler.

## 2026-10-22 — HashiCorp Vault lab (Docker Compose)

- Compose: `vault` (dev mode, :8200) + one-shot `vault-seed`.
- KV `secret/bankapp`: JWT / SQL / RabbitMQ örnek secret’lar.
- WebApi henüz Vault okumuyor; `infra/README` lab adımları.

## 2026-10-21 — Idempotency-Key (kredi yazma API’leri)

- Header `Idempotency-Key` zorunlu: create / approve / reject / cancel.
- Redis’te yanıt önbelleği (24s); aynı key+body → replay; farklı body → 409.
- React client UUID header gönderir; entegrasyon testleri + lab notu.

## 2026-10-21 — Domain Events + Transactional Outbox

- `AggregateRoot` + kredi başvurusu olayları (Created / Approved / Rejected / Cancelled).
- `OutboxMessages` tablosu; `BankingDbContext.SaveChanges` ile aynı transaction’da yazım.
- `OutboxDispatcherHostedService` → RabbitMQ kuyruk `domain-events`; consumer Seq/log.
- Unit + entegrasyon testleri (outbox satırları).

## 2026-10-21 — Faz 6 mimari yol haritası + tartışma notları

- Yol haritasına **Faz 6 — Mimari derinleşme** eklendi (Outbox, Idempotency, Saga, Modular Monolith, SignalR).
- Tartışma arşivi: [architecture-notes.tr.md](architecture-notes.tr.md) (EN/DE karşılıkları).
- History ve README doküman linkleri güncellendi.

## 2026-10-19 — Docker Compose healthcheck’ler

- Redis / Seq / WebApi (`/health`) / Client healthcheck; `depends_on: service_healthy` zinciri.
- WebApi Dockerfile’a `curl`; liveness only (DB/Redis probe yok).

## 2026-10-19 — Müşteri create/detay alan hizası + katalog önerileri

- Bireysel/kurumsal create formları detaydaki alanlarla hizalandı (şifre sadece create).
- Kurumsal tür/sektör/iş kolu/alt iş kolu: datalist (seç veya yeni yaz); `GET …/catalog`.

## 2026-10-19 — Müşteri numarası otomatik / salt okunur

- Personel create: numara sistem üretir (`IND|CORP-{TR|EN|DE}-…`); formda alan yok.
- Detay/güncelleme: numara görünür, değiştirilemez (API ignore).

## 2026-10-19 — Müşteri no locale + personel detay alanları

- Kayıt müşteri no: `IND-{TR|EN|DE}-{yyyyMMdd}-{hex}` (UI dili).
- Personel müşteri detayı: bireysel doğum/anne/baba; kurumsal şirket e-posta/adres/tür/sektör/iş kolu/kuruluş.

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
