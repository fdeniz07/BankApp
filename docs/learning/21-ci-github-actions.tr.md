# 21 — GitHub Actions CI

[← Önceki](20-pdf-export.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](22-decision-letter-email.tr.md)

## Bu nedir?

**CI** (Continuous Integration): Her push/PR’da otomatik build + test.  
**GitHub Actions**: Repo içi YAML workflow’ları runner’da çalıştırır.

## Bu projede ne işe yarar?

`.github/workflows/ci.yml`:

1. Backend: restore → build → unit → integration  
2. Client: Node setup → `npm ci` / build  

Yeşil CI = main’e güven.

## Ne zaman eklendi?

**Faz 5** civarı; sonra genişletildi.

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Workflow** | YAML dosyası |
| **Job** | Paralel çalışabilen iş (backend / client) |
| **Step** | Job içi adım |
| **Runner** | `ubuntu-latest` sanal makine |
| **Badge / check** | PR’da geçti/kaldı işareti |

## Komutlar

Yerelde CI’yı taklit:

```powershell
dotnet restore BankApp.slnx
dotnet build BankApp.slnx --configuration Release --no-restore
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj -c Release --no-build
dotnet test tests/WebApi.IntegrationTests/WebApi.IntegrationTests.csproj -c Release --no-build

cd client
npm ci
npm run build
```

GitHub CLI:

```powershell
gh run list --limit 5
gh run view <id> --log-failed
```

## Sınıflar, metotlar, paketler

Actions (Node 24 runtime — v4 Node 20 deprecation uyarısını temizler):

- `actions/checkout@v5`
- `actions/setup-dotnet@v5`
- `actions/setup-node@v5`

## Repoda nerede?

```
.github/workflows/ci.yml
```

## Mini akış

`git push` → Actions tetiklenir → NU1605 / test fail varsa kırmızı → düzelt → tekrar yeşil.
