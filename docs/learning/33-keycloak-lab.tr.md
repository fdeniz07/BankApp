# 33 — Keycloak lab (Faz 1: Compose + realm)

[← Önceki](32-admin-tailwind-dashboard.tr.md) · [İçindekiler](00-index.tr.md)

İlgili: [06 JWT / RBAC](06-jwt-rbac.tr.md) · [19 Vault](19-vault-lab.tr.md) · [09 Docker Compose](09-docker-compose.tr.md)

## Bu nedir?

**Keycloak**, açık kaynaklı bir **kimlik ve erişim yönetimi** (IAM) sunucusudur: kullanıcı / rol / istemci (client) tanımlar, OpenID Connect ile **JWT access token** üretir.

## Bu projede ne işe yarar?

Lab **Faz 1** (bu bölüm):

- Compose’ta Keycloak ayağa kalkar (`http://localhost:8180`)
- `bankapp` realm import edilir (roller + demo kullanıcılar + `bankapp-api` client)
- Token alınır; roller JWT’de `role` claim’i olarak gelir (Faz 2’de WebApi `RoleClaimType = "role"`)

**Henüz yok (Faz 2+):** WebApi’nin Keycloak token’ını doğrulaması; React login’in Keycloak’a yönlenmesi. Bugün hâlâ `POST /api/Auth/login` + yerel JWT.

## Ne zaman eklendi?

Roadmap “Keycloak desteği” — ilk lab iskeleti (Compose + realm), JWT geçişinden önce.

## Temel kavramlar

| Kavram | Anlam |
|--------|--------|
| Realm | Keycloak’ta izole kiracı (bizde `bankapp`) |
| Client | Token isteyen uygulama (`bankapp-api`) |
| Realm role | `Customer` / `BankStaff` / `BankManager` / `SuperAdmin` |
| Direct access grant | Lab için password grant (kullanıcı/şifre → token) |
| Protocol mapper | Rolü JWT claim’e yazar |
| start-dev | Geliştirme modu (HTTPS zorunlu değil; production değil) |

## Komutlar

### Keycloak’ı ayağa kaldır

`infra` klasöründen (diğer stack ayaktaysa yalnız keycloak da yeter):

```powershell
cd infra
docker compose --env-file .env up -d keycloak
```

**Ne yapar?** `bankapp-realm.json` import edilir; UI `:8180`’de dinler.  
**Kontrol:** `docker compose ps keycloak` → healthy / running; tarayıcıda admin konsol.

### Admin konsol

```text
http://localhost:8180
```

Kullanıcı / şifre: `.env` → `KEYCLOAK_ADMIN_USER` / `KEYCLOAK_ADMIN_PASSWORD` (örnek: `admin` / `admin`). Realm seç: **bankapp**.

### Password grant ile access token al

```powershell
curl.exe -s -X POST "http://localhost:8180/realms/bankapp/protocol/openid-connect/token" `
  -H "Content-Type: application/x-www-form-urlencoded" `
  -d "grant_type=password" `
  -d "client_id=bankapp-api" `
  -d "username=staff.tr@bank.local" `
  -d "password=Demo123!"
```

**Ne yapar?** Keycloak JWT döner (`access_token`).  
**Kontrol:** JSON’da `access_token` var; [jwt.io](https://jwt.io) veya decode ile rol claim’inde `BankStaff` görünür.  
**Önemli bayraklar:** `client_id=bankapp-api` (public + direct access); WebApi bu token’ı **henüz kabul etmez**.

### Realm’i yeniden import etmek

Import yalnızca **ilk** boş data ile güvenilir. Realm bozulduysa:

```powershell
docker compose --env-file .env stop keycloak
docker compose --env-file .env rm -f keycloak
docker compose --env-file .env up -d keycloak
```

(`start-dev` kalıcı volume kullanmıyoruz; container silinince realm yeniden import olur.)

## Sınıflar, metotlar, paketler

| İsim | Rol |
|------|-----|
| `infra/keycloak/bankapp-realm.json` | Realm / client / roller / demo user |
| Compose `keycloak` | `quay.io/keycloak/keycloak:26.0` + `--import-realm` |
| Protocol mapper `bankapp-realm-roles` | Claim adı: `role` (noktalı URI Keycloak’ta iç içe JSON olur; lab’de kısa ad) |
| Mevcut `JwtHelper` / `Auth/login` | Hâlâ yerel token (Faz 1 dışı) |

## Repoda nerede?

- `infra/docker-compose.yml` — `keycloak` servisi
- `infra/keycloak/bankapp-realm.json`
- `infra/.env.example` — `KEYCLOAK_*`
- Bu bölüm: `docs/learning/33-keycloak-lab.tr.md`

## Mini akış

1. `docker compose up -d keycloak`  
2. Admin UI → realm `bankapp` → Users / Roles  
3. curl password grant → `access_token`  
4. Token’da BankApp rol claim’i  
5. (Sonra) WebApi `Authority` / JWKS ile doğrulama — Faz 2  

## Where useful

- [ ] `http://localhost:8180` admin login çalışıyor  
- [ ] Realm listesinde `bankapp` var  
- [ ] `staff.tr@bank.local` / `Demo123!` ile token alınıyor  
- [ ] Access token’da `role` dizisinde `BankStaff`  

- [ ] `manager.tr@bank.local` → `BankManager`  
- [ ] WebApi `POST /api/Auth/login` hâlâ eski JWT üretiyor (bilinçli)

### Felaket / kurtarma (az yıkıcı → çok)

1. `docker compose restart keycloak`  
2. Container recreate (yukarıdaki rm + up) — realm yeniden import  
3. Port çakışması: `.env` içinde `KEYCLOAK_PORT` değiştir (varsayılan 8180)

Lab notu: Keycloak ilk açılış 30–90 sn sürebilir (`start_period`).
