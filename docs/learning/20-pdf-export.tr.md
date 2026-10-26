# 20 — QuestPDF ve ödeme planı export

[← Önceki](19-vault-lab.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](21-ci-github-actions.tr.md)

## Bu nedir?

**QuestPDF**: C# ile PDF oluşturma kütüphanesi.  
BankApp’te amortisman **ödeme planını** PDF indir / yazdır.

## Bu projede ne işe yarar?

- API: `payment-plan/export?format=pdf&locale=…`
- Pluggable exporter (`IPaymentPlanDocumentExporter`) — ileride CSV/XLSX.
- Locale’e göre etiket + para sembolleri (₺ $ €).

## Ne zaman eklendi?

PDF/print UX: Faz 6 sonrası özellik dilimi (README Features + screenshots).

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Exporter** | Document → byte[] / dosya |
| **Locale** | PDF yazı dili |
| **Amortization** | Taksit / faiz / kalan anapara tablosu |

## Komutlar

```powershell
dotnet add src/Application package QuestPDF
```

HTTP (token ile):

```http
GET /api/CreditApplications/payment-plan/export?format=pdf&amount=50000&termMonths=24&locale=tr
```

## Sınıflar, metotlar, paketler

| İsim | Rol |
|------|-----|
| `QuestPDF` | PDF motoru |
| `PaymentPlanPdfExporter` | `IPaymentPlanDocumentExporter` |
| `PaymentPlanPdfLabels.For(locale)` | TR/EN/DE etiket |
| `PaymentPlanMoneyFormatter` | Sembollü tutar |
| `exportPaymentPlan` (client) | Blob indir + print |

## Repoda nerede?

```
src/Application/Services/Amortization/Export/
client/src/api/applicationsApi.ts
client/src/pages/ApplicationDetailPage.tsx
docs/screenshots/bank-manager.en.md  # Print&Pdf
```

## Mini akış

UI “PDF” → `locale` + plan parametreleri → handler plan hesaplar → QuestPDF document → dosya indir / print dialog.
