# BRD — BankApp (Business Requirements Document)

| Alan | Değer |
|------|--------|
| Ürün | BankApp — eğitim / kurumsal lab bankacılık uygulaması |
| Durum | Canlı (lab); Continuous documentation |
| Dil | Türkçe (birincil) |
| Son güncelleme | 2026-11-02 |
| İlgili | [roadmap](../roadmap.tr.md) · [history](../history.tr.md) · [FRD kredi kararı](frd-credit-decision.tr.md) |

---

## 1. Amaç ve kapsam

### 1.1 İş amacı

BankApp, gerçek banka süreçlerine yakın bir **kredi başvuru ve karar** lab’idir: müşteri başvurusu, personel incelemesi, şube yetkisi, evrak, bildirim ve gözlemlenebilirlik katmanlarını tek modular monolith’te gösterir.

### 1.2 Kapsam içi

- Bireysel / kurumsal müşteri ve kredi ürün kataloğu
- Başvuru yaşam döngüsü (oluştur / güncelle / iptal / onay / red)
- Personel ve yönetici RBAC + şube izolasyonu
- Evrak yükleme ve onay kapısı (yapılandırılabilir Soft/Hard)
- Pozisyon bazlı onay yetkisi (delegation of authority)
- Karar mektubu (şablon, e-posta/SMS lab kuyrukları)
- React istemci (TR/EN/DE), Compose lab, CI

### 1.3 Kapsam dışı (şimdilik / yapılacak)

- Elasticsearch personel araması
- Üretim Kubernetes / ArgoCD
- Raft Vault / tam secret rotasyonu
- OAuth / Keycloak
- AI destek sohbeti (roadmap; sonra tartışılacak)
- Gerçek SMS/e-posta gateway’leri

---

## 2. Paydaşlar ve roller

| Rol | İş ihtiyacı |
|-----|-------------|
| Müşteri | Kayıt, e-posta doğrulama, başvuru, evrak, durum takibi, iptal |
| BankStaff | Kendi şube kapsamında inceleme, onay/red (limit + evrak kurallarına bağlı) |
| BankManager | Şube yönetimi, daha geniş yetki, kredi türü yönetimi, müşteri kuralları |
| SuperAdmin | Tüm şubeler, katalog locale serbestisi, sınırsız onay yetkisi (config) |
| Lab eğitmeni / geliştirici | Compose ile ayağa kalkma, Seq/OTel, yapılandırılabilir kapılar |

---

## 3. İş gereksinimleri (BR)

Durum: **Yapıldı** | **Kısmi** | **Planlı**

### 3.1 Kimlik ve erişim

| ID | Gereksinim | Durum |
|----|------------|--------|
| BR-ID-01 | Kullanıcılar JWT ile kimlik doğrular; roller Customer / BankStaff / BankManager / SuperAdmin | Yapıldı |
| BR-ID-02 | Kamu bireysel kayıt; JWT kayıtta verilmez; e-posta doğrulama (kuyruk + log) | Yapıldı |
| BR-ID-03 | Demo hesap matrisi (Personel/Yönetici/Müşteri × TR/DE/EN); ortak lab şifresi | Yapıldı |
| BR-ID-04 | Datenschutz / çerez onayı ilk ziyarette | Yapıldı |
| BR-ID-05 | Rate limiting (auth/write/fixed) ortam bazlı | Yapıldı |
| BR-ID-06 | Vault lab + JWT key overlay (Compose) | Yapıldı |
| BR-ID-07 | OAuth / sosyal giriş | Planlı |
| BR-ID-08 | Keycloak / kurumsal IdP | Planlı |

### 3.2 Müşteri ve ürün

| ID | Gereksinim | Durum |
|----|------------|--------|
| BR-CU-01 | Bireysel ve kurumsal müşteri (TPT) | Yapıldı |
| BR-CU-02 | Kredi tipi / alt tip; locale katalog; paket tutar/vade ızgarası | Yapıldı |
| BR-CU-03 | Katalog şube locale’ine kilitli (SuperAdmin hariç) | Yapıldı |
| BR-CU-04 | BankManager kredi türü ekleme / aktif-pasif | Yapıldı |
| BR-CU-05 | Müşteri silme: pasif + açık kredi yok | Yapıldı |

### 3.3 Kredi başvurusu ve karar

| ID | Gereksinim | Durum |
|----|------------|--------|
| BR-CR-01 | Müşteri başvuru oluşturur; personel onay/red/iptal kuralları | Yapıldı |
| BR-CR-02 | Ödeme planı (amortisman) + PDF export | Yapıldı |
| BR-CR-03 | Liste filtre/sıralama; eksik evrak rozeti ve preset | Yapıldı |
| BR-CR-04 | Karar audit: kim, şube, şablon adı snapshot | Yapıldı |
| BR-CR-05 | Kullanıcı karar mektubu şablon CRUD + sistem şablonları (UI dili) | Yapıldı |
| BR-CR-06 | Evrak yükleme (kimlik, gelir vb.; PDF/JPEG/PNG) | Yapıldı |
| BR-CR-07 | Onayda evrak kapısı: None / Soft / Hard (ortam config) | Yapıldı |
| BR-CR-08 | Pozisyon bazlı onay tutarı tavanı; yönetici rolleri sınırsız (config) | Yapıldı |
| BR-CR-09 | Idempotent kritik yazmalar (Idempotency-Key) | Yapıldı |

### 3.4 Şube

| ID | Gereksinim | Durum |
|----|------------|--------|
| BR-BR-01 | Şube entity; personel/müşteri/başvuru BranchId | Yapıldı |
| BR-BR-02 | Liste ve modify şube guard; SuperAdmin tüm şubeler | Yapıldı |
| BR-BR-03 | Demo şubeler (IST/ANK/NYC/CHI/BER vb.) | Yapıldı |
| BR-BR-04 | Şube tavan onay tutarı (Branch.MaxApprovalAmount); efektif = min(pozisyon, şube) | Tamamlandı |

### 3.5 Bildirim ve gerçek zamanlı

| ID | Gereksinim | Durum |
|----|------------|--------|
| BR-NT-01 | Domain events + transactional outbox → RabbitMQ | Yapıldı |
| BR-NT-02 | SignalR başvuru/evrak olayları | Yapıldı |
| BR-NT-03 | Karar mektubu e-posta kuyruğu (lab) | Yapıldı |
| BR-NT-04 | Karar mektubu SMS kuyruğu (lab; gateway yok) | Yapıldı |
| BR-NT-05 | Destek sohbet robotu | Planlı |

### 3.6 UX / istemci

| ID | Gereksinim | Durum |
|----|------------|--------|
| BR-UX-01 | React + Vite; TR/EN/DE i18n | Yapıldı |
| BR-UX-02 | Rol bazlı dashboard + istatistik şeridi | Yapıldı |
| BR-UX-03 | Profil / avatar; erişilebilirlik; döviz şeridi | Yapıldı |
| BR-UX-04 | Admin karşılama dashboard (Tailwind / performans) | Planlı |

### 3.7 Platform / kalite

| ID | Gereksinim | Durum |
|----|------------|--------|
| BR-PL-01 | Clean Architecture + CQRS; Modular Monolith | Yapıldı |
| BR-PL-02 | Docker Compose lab stack | Yapıldı |
| BR-PL-03 | Serilog+Seq; Redis; OTel→Grafana/Prometheus | Yapıldı |
| BR-PL-04 | Unit + integration testler; GitHub Actions CI | Yapıldı |
| BR-PL-05 | Saga / process manager kredi yaşam döngüsü | Yapıldı |
| BR-PL-06 | Elasticsearch personel/arama (ELK lab + canlı kutu; Seq kalır) | Yapıldı |
| BR-PL-06b | Manager/Admin gelişmiş ES filtre paneli | Planlı |
| BR-PL-07 | Kubernetes / ArgoCD | Planlı |
| BR-PL-08 | Raft Vault / secret rotasyonu (üretim) | Planlı |

### 3.8 Dokümantasyon

| ID | Gereksinim | Durum |
|----|------------|--------|
| BR-DOC-01 | Learning kitaplığı + Cursor kuralı | Yapıldı |
| BR-DOC-02 | Roadmap (kategori) + history (faz) | Yapıldı |
| BR-DOC-03 | Bu BRD + ilgili FRD’ler (canlı tutulan) | Yapıldı |

---

## 4. İş kuralları (özet)

1. Yalnızca **Pending** başvurular onay/red/iptal edilir.
2. Personel yalnızca yetkili olduğu **şube** kaynaklarını değiştirir (SuperAdmin hariç).
3. Onay tutarı / vade **ürün paket ızgarası** içinde olmalıdır.
4. **Hard** evrak kapısında kimlik + gelir belgesi olmadan onay yok.
5. **Soft** kapıda uyarı vardır; API yine de onaylayabilir (lab).
6. **ApprovalAuthority** açıkken pozisyon limiti aşımı onaylanamaz; BankManager/SuperAdmin (config) muaf.
7. Kritik yazmalar **Idempotency-Key** ile güvenli tekrar edilir.

Ayrıntılı davranış: [frd-credit-decision.tr.md](frd-credit-decision.tr.md).

---

## 5. Başarı ölçütleri (lab)

- Compose ile API + client + bağımlılıklar ayağa kalkar.
- Demo matris ile üç dilde personel/müşteri akışı tamamlanır.
- Development’ta Soft kapı + kapalı DoA; base config’te Hard + açık DoA doğrulanır.
- CI yeşil; learning/BRD güncel tutulur.

---

## 6. Varsayımlar ve riskler

| Varsayım / risk | Not |
|-----------------|-----|
| Lab ≠ üretim bankası | Gateway, KVKK süreçleri, gerçek Vault HA yok |
| Tutar limitleri ürün para birimiyle aynı kabul | Çok para birimli DoA yok |
| Hard kapı zorunlu tipleri sabit | Identity + IncomeProof (`RequiredCreditDocuments`) |

---

## 7. Değişiklik günlüğü (BRD)

| Tarih | Değişiklik |
|-------|------------|
| 2026-11-01 | İlk tam BRD; BR-CR-07/08 (evrak kapısı + DoA) eklendi |
| 2026-11-02 | BR-BR-04 şube tavanı tamamlandı; efektif = min(pozisyon, şube) |
