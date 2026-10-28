# 23 — Rate limiting (ASP.NET Core)

[← Önceki](22-decision-letter-email.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](24-lab-tools.tr.md)

İlgili: [19 — Vault lab](19-vault-lab.tr.md)

## Bu nedir?

**Rate limiting**: Aynı istemcinin / IP’nin belirli sürede kaç istek atabileceğini sınırlamak. Aşılınca HTTP **429 Too Many Requests**.

## Bu projede ne işe yarar?

Brute-force login ve kredi yazma spam’ini sınırlamak. Üç politika; **ortama göre farklı limitler**:

| Policy | Development (lab) | Production-benzeri (`appsettings.json`) | Nerede |
|--------|-------------------|------------------------------------------|--------|
| `auth` | 20 / 60s | 10 / 60s | `AuthController` |
| `write` | 60 / 60s | 30 / 60s | CreditApplications create/approve/reject/cancel/update/delete |
| `fixed` | 100 / 60s | 60 / 60s | Diğer controller’lar |

Compose varsayılanı `ASPNETCORE_ENVIRONMENT=Development` → lab gevşek limitleri kullanır.  
`/health` ve SignalR hub rate limit’ten muaf.

## Ne zaman eklendi?

Temel `fixed` daha önce vardı; **auth / write** politikaları güvenlik diliminde eklendi.  
**Ortam ayrımı** (sıkı base + gevşek Development) sertleştirme lab diliminde (2026-10-28).

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Fixed window** | Sabit zaman penceresinde N istek |
| **PermitLimit** | Penceredeki izin sayısı |
| **QueueLimit** | Limit dolunca kuyruk (0 = hemen 429) |
| **EnableRateLimiting** | Controller/action attribute |
| **Ortam katmanı** | `appsettings.json` + `appsettings.{Environment}.json` birleşimi |

## Komutlar

Üretim benzeri base (`appsettings.json`) — sıkı varsayılanlar:

```json
"RateLimiting": {
  "Fixed": { "PermitLimit": 60, "WindowSeconds": 60 },
  "Auth": { "PermitLimit": 10, "WindowSeconds": 60 },
  "Write": { "PermitLimit": 30, "WindowSeconds": 60 }
}
```

Lab overlay (`appsettings.Development.json`) — yerel / Compose Development:

```json
"RateLimiting": {
  "Fixed": { "PermitLimit": 100, "WindowSeconds": 60 },
  "Auth": { "PermitLimit": 20, "WindowSeconds": 60 },
  "Write": { "PermitLimit": 60, "WindowSeconds": 60 }
}
```

Hangi ortamın yüklendiğini kontrol et (beklenen lab: `Development`):

```powershell
docker compose -f infra/docker-compose.yml --env-file infra/.env exec webapi printenv ASPNETCORE_ENVIRONMENT
# Beklenen: Development → gevşek Auth 20/60s
```

Hızlı deneme (login spam → 429). Development’ta ~21. istekten sonra 429; Production config’te ~11:

```powershell
1..30 | ForEach-Object {
  curl -s -o NUL -w "%{http_code}`n" -X POST http://localhost:8080/api/Auth/login `
    -H "Content-Type: application/json" `
    -d '{"email":"x","password":"y"}'
}
# İlk ~20 satır 401/400; sonra 429. Pencere dolunca (60s) tekrar izin açılır.
```

Sıkı limitleri Compose’ta denemek (lab; kalıcı değil):

```powershell
# Geçici: Production + aynı compose (sadece rate-limit deneyi için)
# infra/.env içinde ASPNETCORE_ENVIRONMENT=Production yapıp webapi recreate
# İş bitince Development’a geri al — aksi halde UI/test spam 429 üretebilir
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --force-recreate webapi
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
src/WebApi/appsettings.json                 # sıkı (prod-benzeri) base
src/WebApi/appsettings.Development.json    # lab gevşek overlay
```

## Mini akış

İstek → RateLimiter middleware → policy (auth/write/fixed) sayacı (ortam config’inden) → izin varsa action; yoksa 429.

## Where useful

### Sağlık checklist

| Kontrol | Nasıl | İyi görünüm |
|---------|-------|-------------|
| Ortam | `printenv ASPNETCORE_ENVIRONMENT` | Lab: `Development` |
| Auth 429 | Yukarıdaki 30× login döngüsü | Dev: ~21. istekte 429 |
| Health muaf | `curl http://localhost:8080/health` tekrarları | Hep 200 (limit yok) |
| Write | Aynı IP ile çok create/approve | Limit sonrası 429 |

### Felaket / kurtarma (lab)

| Durum | Yol |
|-------|-----|
| UI sürekli 429 | 60s bekle **veya** WebApi recreate (sayacı sıfırlar) |
| Testler / demo spam 429 | `ASPNETCORE_ENVIRONMENT=Development` olduğunu doğrula; Production denemesi yaptıysan geri al |
| Limit hiç tetiklenmiyor | Yanlış host/port; başka IP partition; policy attribute eksik endpoint |
| Health 429 | Olmamalı — middleware muafiyetini kontrol et |
