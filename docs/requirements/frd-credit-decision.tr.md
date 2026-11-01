# FRD — Kredi kararı: evrak kapısı + onay yetkisi

| Alan | Değer |
|------|--------|
| Üst BRD | [brd.tr.md](brd.tr.md) — BR-CR-07, BR-CR-08 |
| Modüller | Credit (Application / WebApi / client) |
| Son güncelleme | 2026-11-01 |

---

## 1. Amaç

Personelin kredi onayında:

1. Zorunlu evrakların (kimlik + gelir) **ortama göre** Soft veya Hard uygulanması  
2. **Pozisyon bazlı** onay tutarı tavanının (DoA) uygulanması  

Her iki davranış da **appsettings** ile açılıp kapanır / seçilir; kod yolları her zaman mevcuttur.

---

## 2. Yapılandırma

### 2.1 Evrak kapısı

```json
"CreditDocuments": {
  "ApproveGateMode": "Hard"
}
```

| Değer | UI | API Approve |
|--------|-----|-------------|
| `None` | Soft confirm yok; sarı band yok | Blok yok |
| `Soft` | Eksik evrakta confirm + sarı band | Blok yok |
| `Hard` | Onay butonu disabled + kırmızı mesaj | `MissingRequiredDocuments` |

Ortam önerisi:

- `appsettings.Development.json` → `Soft`
- `appsettings.json` (base / prod-benzeri) → `Hard`

Liste rozeti, “Eksik zorunlu evrak” filtresi ve “Bekleyen + eksik evrak” preset **tüm modlarda** kalır.

Zorunlu tipler: `IdentityDocument`, `IncomeProof` (`RequiredCreditDocuments`).

### 2.2 Onay yetkisi (DoA)

```json
"ApprovalAuthority": {
  "Enabled": true,
  "LimitsByPosition": {
    "CreditAnalyst": 50000,
    "RelationshipManager": 100000,
    "BranchOfficer": 75000,
    "RiskOfficer": 250000,
    "OperationsSpecialist": 25000,
    "BranchManager": 500000
  },
  "UnlimitedRoles": [ "BankManager", "SuperAdmin" ]
}
```

| Durum | Davranış |
|--------|----------|
| `Enabled: false` | Limit yok (Development varsayılanı) |
| Rol `UnlimitedRoles` içinde | Limit yok (`maxApprovalAmount: null`) |
| Pozisyon limit tablosunda | Tavan = değer; aşım → hata |
| Pozisyon yok / tanımsız | Tavan = 0 → pratikte onay engeli |

Development: `Enabled: false`.

---

## 3. API

### 3.1 `GET /api/CreditDocuments/config`

Yanıt (camelCase):

| Alan | Tip | Anlam |
|------|-----|--------|
| `approveGateMode` | string | `None` \| `Soft` \| `Hard` |
| `softApproveGateEnabled` | bool | `mode == Soft` (geriye dönük) |
| `approvalAuthorityEnabled` | bool | Options.Enabled |
| `maxApprovalAmount` | number \| null | Bu kullanıcı için tavan; null = sınırsız / kapalı |

### 3.2 `POST .../Approve` (mevcut)

Ek kontroller (sırayla, mevcut kurallardan sonra):

1. Hard + eksik zorunlu evrak → `BusinessException` / `MissingRequiredDocuments`
2. DoA açık + tutar > tavan → `AmountExceedsApprovalAuthority`

---

## 4. UI (ApplicationDetailPage)

| Koşul | Davranış |
|--------|----------|
| Soft + eksik evrak | Sarı uyarı; onayda ekstra confirm |
| Hard + eksik evrak | Kırmızı mesaj; submit disabled |
| DoA açık | Limit metni; aşımda kırmızı mesaj + submit disabled |
| API hata mesajı | Mevcut `mapApiError` / toast |

---

## 5. Kabul kriterleri

- [ ] Development: Soft confirm çalışır; Hard blok uygulanmaz; DoA kapalıyken yüksek tutar onaylanır  
- [ ] Base config (veya test override): Hard’da evraksız Approve 400/iş kuralı  
- [ ] CreditAnalyst limiti aşımında Approve reddedilir; BankManager aşabilir  
- [ ] Unit: Hard missing docs + authority exceeded  
- [ ] `GET .../config` staff oturumunda doğru mode/limit döner  

---

## 6. Repoda nerede?

- Options: `CreditDocumentsOptions`, `ApprovalAuthorityOptions`
- Evaluate: `ApprovalAuthorityEvaluator`
- Handler: `ApproveCreditApplicationRequestHandler`
- Controller: `CreditDocumentsController.GetConfig`
- Client: `creditDocumentsConfigApi.ts`, `ApplicationDetailPage.tsx`
- Learning: [30-approve-gate-authority.tr.md](../learning/30-approve-gate-authority.tr.md)
