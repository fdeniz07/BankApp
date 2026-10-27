# 23 — Rate limiting (ASP.NET Core)

[← Önceki](22-decision-letter-email.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](24-lab-tools.tr.md)

İlgili: [19 — Vault lab](19-vault-lab.tr.md)

## Bu nedir?

**Rate limiting**: Aynı istemcinin / IP’nin belirli sürede kaç istek atabileceğini sınırlamak. Aşılınca HTTP **429 Too Many Requests**.

## Bu projede ne işe yarar?

Brute-force login ve kredi yazma spam’ini lab’de sınırlamak. Üç politika:

| Policy | Varsayılan | Nerede |
|--------|------------|--------|
| `auth` | 20 / 60s | `AuthController` |
| `write` | 60 / 60s | CreditApplications create/approve/reject/cancel/update/delete |
| `fixed` | 100 / 60s | Diğer controller’lar |

`/health` ve SignalR hub rate limit’ten muaf.

## Ne zaman eklendi?

Temel `fixed` daha önce vardı; **auth / write** politikaları güvenlik diliminde eklendi.

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Fixed window** | Sabit zaman penceresinde N istek |
| **PermitLimit** | Penceredeki izin sayısı |
| **QueueLimit** | Limit dolunca kuyruk (0 = hemen 429) |
| **EnableRateLimiting** | Controller/action attribute |

## Komutlar

Config (`appsettings.json`):

```json
"RateLimiting": {
  "Fixed": { "PermitLimit": 100, "WindowSeconds": 60 },
  "Auth": { "PermitLimit": 20, "WindowSeconds": 60 },
  "Write": { "PermitLimit": 60, "WindowSeconds": 60 }
}
```

Hızlı deneme (login spam → 429):

```powershell
1..30 | ForEach-Object {
  curl -s -o NUL -w "%{http_code}`n" -X POST http://localhost:8080/api/Auth/login `
    -H "Content-Type: application/json" `
    -d '{"email":"x","password":"y"}'
}
```

## Sınıflar, metotlar, paketler

| İsim | Rol |
|------|-----|
| `RateLimitPolicies` | `Fixed` / `Auth` / `Write` sabitleri |
| `AddRateLimitingConfiguration` | `AddFixedWindowLimiter` kayıtları |
| `UseRateLimiter()` | Middleware |
| `Microsoft.AspNetCore.RateLimiting` | Framework API |

## Repoda nerede?

```
src/WebApi/Constants/RateLimitPolicies.cs
src/WebApi/Extensions/ServiceCollectionExtensions.cs
src/WebApi/Extensions/ApplicationBuilderExtensions.cs
src/WebApi/Modules/Identity/Controllers/AuthController.cs
src/WebApi/Modules/Credit/Controllers/CreditApplicationsController.cs
src/WebApi/appsettings.json
```

## Mini akış

İstek → RateLimiter middleware → policy (auth/write/fixed) sayacı → izin varsa action; yoksa 429.
