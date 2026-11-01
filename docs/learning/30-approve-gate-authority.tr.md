# 30 — Onay evrak kapısı + yetki tavanı (DoA)

## Bu nedir?

İki bağımsız config:

1. **ApproveGateMode** — eksik kimlik/gelir için `None` / `Soft` / `Hard`
2. **ApprovalAuthority** — personel pozisyonuna göre max onay tutarı

## Bu projede ne işe yarar?

Lab’de Development Soft + DoA kapalı; prod-benzeri base Hard + DoA açık. Kod her zaman her iki yolu da içerir; ortam `appsettings` ile seçer.

## Ne zaman eklendi?

Soft boolean kapıdan hemen sonra (Faz 6 UX / güvenlik).

## Temel kavramlar

| Kavram | Anlam |
|--------|--------|
| Soft | Uyarı + confirm; API onaylar |
| Hard | API + UI blok |
| DoA | Delegation of authority — pozisyon limiti |
| UnlimitedRoles | BankManager / SuperAdmin muafiyeti |

## Komutlar

Config’i görmek (JWT personel):

```powershell
# Login sonrası token ile
curl.exe -s http://localhost:8080/api/CreditDocuments/config `
  -H "Authorization: Bearer <token>"
```

Doğrulama: `approveGateMode`, `approvalAuthorityEnabled`, `maxApprovalAmount`.

Development’ta Soft’u Hard yapmak için `appsettings.Development.json` içinde `ApproveGateMode` değerini değiştirip API’yi yeniden başlatın.

## Sınıflar, metotlar, paketler

- `ApproveDocumentGateMode`, `CreditDocumentsOptions`
- `ApprovalAuthorityOptions`, `ApprovalAuthorityEvaluator`
- `CreditApplicationBusinessRules.RequiredDocumentsShouldExistWhenHardGate`
- `CreditApplicationBusinessRules.ApprovedAmountShouldBeWithinApprovalAuthority`
- `ApproveCreditApplicationRequestHandler`
- `CreditDocumentsController.GetConfig`

## Repoda nerede?

- `src/Application/.../CreditApplicationDocuments/`
- `src/Application/.../ApprovalAuthority/`
- `src/WebApi/appsettings.json` / `appsettings.Development.json`
- `client/src/api/creditDocumentsConfigApi.ts`
- `client/src/pages/ApplicationDetailPage.tsx`
- BRD/FRD: `docs/requirements/`

## Mini akış

Personel detayı açar → `GET config` → Soft ise eksik evrakta confirm; Hard ise buton kilitli → DoA açıksa tutar tavana bakılır → Approve handler aynı kuralları sunucuda uygular.

## Where useful

- [ ] Development Soft: evraksız onay confirm ile geçer
- [ ] Hard override: evraksız Approve iş kuralı hatası
- [ ] DoA Enabled + CreditAnalyst düşük limit: yüksek tutar reddedilir
- [ ] Manager rolü: `maxApprovalAmount` null

Lab: limitler paket para birimiyle aynı kabul edilir (çok PB DoA yok).
