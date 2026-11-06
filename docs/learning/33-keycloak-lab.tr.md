# 33 — Keycloak lab (Compose + dual JWT)

[← Önceki](32-admin-tailwind-dashboard.tr.md) · [İçindekiler](00-index.tr.md)

İlgili: [06 JWT / RBAC](06-jwt-rbac.tr.md) · [19 Vault](19-vault-lab.tr.md) · [09 Docker Compose](09-docker-compose.tr.md)

## Bu nedir?

**Keycloak**, açık kaynaklı bir **kimlik ve erişim yönetimi** (IAM) sunucusudur: kullanıcı / rol / istemci (client) tanımlar, OpenID Connect ile **JWT access token** üretir.

## Bu projede ne işe yarar?

**Faz 1 — Compose + realm**

- Keycloak `http://localhost:8180`
- Realm `bankapp` import (roller + demo user + `bankapp-api`)
- Password grant ile token

**Faz 2 — WebApi dual JWT (lab)**

- `Keycloak:Enabled=true` iken WebApi hem **yerel** `TokenOptions` JWT hem **Keycloak** access token kabul eder
- Issuer’da `/realms/bankapp` → Keycloak scheme; aksi halde yerel imza
- `role` → `ClaimTypes.Role`; email ile SQL’den `NameIdentifier` / `branchId` / `customerId` zenginleştirme
- React hâlâ `POST /api/Auth/login` (yerel JWT)

**Sonra:** OAuth / sosyal; login UI’nin Keycloak’a taşınması.

## Ne zaman eklendi?

Roadmap “Keycloak desteği” — Faz 1 Compose; ardından Faz 2 dual bearer.

## Temel kavramlar

| Kavram | Anlam |
|--------|--------|
| Realm | İzole kiracı (`bankapp`) |
| Client | `bankapp-api` (public + password grant) |
| Authority / JWKS | WebApi imza anahtarını Keycloak’tan çeker |
| Policy scheme | Token issuer’a göre hangi JwtBearer |
| Claim enrich | Keycloak `sub` yerine BankApp user Id |
| start-dev | Lab modu (prod değil) |

## Komutlar

### Keycloak’ı ayağa kaldır

```powershell
cd infra
docker compose --env-file .env up -d keycloak
```

**Kontrol:** `docker compose ps keycloak` healthy; `http://localhost:8180` (admin/admin).

### Password grant token

```powershell
curl.exe -s -X POST "http://localhost:8180/realms/bankapp/protocol/openid-connect/token" `
  -H "Content-Type: application/x-www-form-urlencoded" `
  -d "grant_type=password" `
  -d "client_id=bankapp-api" `
  -d "username=staff.tr@bank.local" `
  -d "password=Demo123!"
```

### Keycloak token ile korumalı API (Faz 2)

WebApi Development veya Compose’ta `Keycloak__Enabled=true` olmalı.

```powershell
# $TOKEN = access_token alanını yapıştır
curl.exe -s "http://localhost:8080/api/Profile" `
  -H "Authorization: Bearer $TOKEN"
```

**Kontrol:** 401 değil; profil / personel yanıtı (email seed ile eşleşiyorsa `branchId` dolu).  
**Not:** Yerel login token’ı da aynı endpoint’te çalışmaya devam eder.

### Realm yeniden import

```powershell
docker compose --env-file .env stop keycloak
docker compose --env-file .env rm -f keycloak
docker compose --env-file .env up -d keycloak
```

## Sınıflar, metotlar, paketler

| İsim | Rol |
|------|-----|
| `KeycloakOptions` | Enabled, Authority, ValidIssuers |
| `AddBankAppJwtAuthentication` | Local + Keycloak JwtBearer + `BankApp` policy scheme |
| `KeycloakTokenEnricher` | `role` → Role + SQL enrich |
| `infra/keycloak/bankapp-realm.json` | Realm import |
| `JwtHelper` / Auth login | Yerel token üretimi (değişmedi) |

## Repoda nerede?

- `infra/docker-compose.yml` — `keycloak` + WebApi `Keycloak__*`
- `infra/keycloak/bankapp-realm.json`
- `src/WebApi/Configuration/KeycloakOptions.cs`
- `src/WebApi/Authentication/KeycloakAuthenticationExtensions.cs`
- `src/WebApi/Authentication/KeycloakTokenEnricher.cs`
- `src/WebApi/appsettings*.json`

## Mini akış

1. Keycloak up → realm import  
2. Password grant → access_token  
3. WebApi Bearer Keycloak token → scheme seçimi → JWKS doğrulama → enrich  
4. MediatR RBAC `ClaimRoles()` BankStaff görür  
5. Paralel: browser login → yerel JWT  

## Where useful

- [ ] Admin UI + realm `bankapp`  
- [ ] Token’da `role` içinde `BankStaff`  
- [ ] Keycloak token ile `/api/Profile` (veya staff list) 200  
- [ ] Yerel `Auth/login` token’ı hâlâ çalışıyor  
- [ ] Keycloak kapalıyken (`Enabled=false`) yalnız yerel JWT  

### Felaket / kurtarma

1. `docker compose restart keycloak`  
2. Keycloak recreate (yeniden import)  
3. WebApi log: JWKS / issuer uyumsuzluğu → `ValidIssuers` + `Authority` (Compose’ta `http://keycloak:8080/...`)  
4. Port: `KEYCLOAK_PORT` (8180)
