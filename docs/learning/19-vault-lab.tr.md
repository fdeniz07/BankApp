# 19 — HashiCorp Vault (lab) + WebApi overlay

[← Önceki](18-otel-observability.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](20-pdf-export.tr.md)

İlgili: [06 JWT](06-jwt-rbac.tr.md) · [24 Lab tools](24-lab-tools.tr.md) · [09 Compose](09-docker-compose.tr.md)

## Bu nedir?

**Vault**: Secret saklama. BankApp lab’inde Compose **dev-mode** Vault + `vault-seed` → KV path `secret/bankapp`.  
WebApi startup’ta opsiyonel olarak **JWT güvenlik anahtarını** Vault’tan okuyup `TokenOptions:SecurityKey` üzerine yazar.

## Bu projede ne işe yarar?

- Secret’ların UI’da görülmesi (eğitim).
- Compose’ta `Vault__Enabled=true` iken JWT key Vault’tan gelir; Vault down ise `.env` / `TokenOptions__SecurityKey` fallback.
- SQL connection string hâlâ Compose env (basit tutmak için); sonraki dilimde genişletilebilir.

## Ne zaman eklendi?

Compose lab daha önce; **WebApi JWT overlay** güvenlik diliminde.

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **KV v2** | `secret/data/{path}` HTTP API |
| **X-Vault-Token** | İstek header’ındaki token |
| **Overlay** | `AddInMemoryCollection` ile config üstüne yazma |
| **Dev mode** | Üretim değil; unseal/raft yok; veri ephemeral |
| **Root token** | Lab’de her şeye yetkili — prod anti-pattern |

## Komutlar

```powershell
cd infra

# Vault + seed job (seed bir kez secret yazar)
docker compose --env-file .env up -d vault
docker compose --env-file .env up vault-seed
# Kontrol: vault healthy; UI http://localhost:8200
# Token: .env içindeki VAULT_DEV_ROOT_TOKEN_ID (ör. bankapp-vault-root-token)
```

**Vault UI**

1. Method: **Token** → yapıştır `VAULT_DEV_ROOT_TOKEN_ID`.
2. Secrets → KV → `secret` → `bankapp` (veya seed path’i).
3. `jwt_security_key` alanını oku (değeri ekran görüntüsüne koyma).

```powershell
# CLI ile aynı secret (konteyner içi vault binary)
docker compose --env-file .env exec vault vault kv get secret/bankapp
# Beklenen: jwt_security_key satırı; hata → seed çalışmadı veya path yanlış
```

```powershell
# Vault durumu (dev mode’da seal bilgisi)
docker compose --env-file .env exec vault vault status

# WebApi overlay uygulandı mı?
docker compose --env-file .env logs webapi | Select-String -Pattern "Vault"
# Beklenen: "Vault overlay applied" veya fallback uyarısı
```

```powershell
# Host’tan UI / API kaba kontrol
curl -s -o NUL -w "%{http_code}`n" http://localhost:8200/ui/
curl -s -H "X-Vault-Token: bankapp-vault-root-token" http://localhost:8200/v1/secret/data/bankapp
# Token’ı kendi .env değerinle değiştir; 200 + JSON data
```

### Ne kontrol edilir?

| Kontrol | Nasıl | İyi görünüm |
|---------|-------|-------------|
| Vault up | `compose ps vault` | healthy / Up |
| Seed | `vault kv get secret/bankapp` | key dolu |
| Overlay | `logs webapi` + Vault | overlay applied |
| Fallback | Vault’u durdurup WebApi restart | `.env` key ile ayakta (lab) |

### Felaket / kurtarma (lab)

| Durum | Yol |
|-------|-----|
| UI “sealed” / boş (dev reset) | Dev Vault restart → **seed’i tekrar çalıştır** (`up vault-seed`) |
| kv get not found | Seed exit code / `logs vault-seed`; path `secret/bankapp` |
| WebApi 500 JWT / imza | Overlay yok + yanlış key uzunluğu; `.env` `TOKEN_SECURITY_KEY` ≥ 64 byte |
| Token unuttun | `infra/.env` → `VAULT_DEV_ROOT_TOKEN_ID` (örnek dosyada da var) |

```powershell
# Seed’i yeniden koştur (Vault ayaktayken)
docker compose --env-file .env up vault-seed

# Vault’u recreate + seed (lab secret’ları yenilenir)
docker compose --env-file .env up -d --force-recreate vault
docker compose --env-file .env up vault-seed
docker compose --env-file .env up -d --force-recreate webapi
```

**Dikkat / best practice:** Dev mode ≠ prod (Raft, policy, rotation). Root token’ı public issue’ya yapıştırma. SQL/Rabbit hâlâ Compose env — bilinçli sadeleştirme.

## Secret rotasyonu ve Raft (bilinçli olarak sonra)

Bu lab **dev-mode** Vault kullanır: tek süreç, bellek/ephemeral, root token. Amaç JWT overlay’i görmek — HA production değil.

| Konu | Lab’de durum | Üretimde (sonraki dilim) |
|------|--------------|---------------------------|
| **Raft / HA** | Yok (dev server) | En az 3 node Raft, unseal, audit |
| **Policy** | Root token her şeye yetkili | App role / policy: sadece `secret/data/bankapp` okuma |
| **Secret rotasyonu** | Seed tekrar + WebApi recreate (manuel) | Lease/TTL, JWT key rotate → rolling restart; SQL/Rabbit ayrı |
| **JWT key değiştirme (lab)** | UI veya `vault kv put` → `webapi` force-recreate | Key id / dual-key penceresi (kesinti az) |

Lab’de “rotasyon” deneyi (en az yıkıcı → daha yıkıcı):

```powershell
# 1) Sadece KV değerini güncelle (UI veya CLI), sonra WebApi’yi yeniden başlat
docker compose -f infra/docker-compose.yml --env-file infra/.env exec vault `
  vault kv put secret/bankapp jwt_security_key="YENI_EN_AZ_64_BYTE_KEY_BURAYA________________________"
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --force-recreate webapi
# Seq/log: Vault overlay applied; eski JWT’ler geçersiz olabilir → yeniden login

# 2) Vault recreate (dev secret’lar silinir) → seed → webapi
# (-v volume silme: bu Compose’ta Vault genelde ephemeral; yine de dikkat)
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --force-recreate vault
docker compose -f infra/docker-compose.yml --env-file infra/.env up vault-seed
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --force-recreate webapi
```

**Raft neden şimdi yok?** Compose’a 3 node + unseal + storage eklemek eğitim maliyetini şişirir; JWT overlay öğrenildikten sonra ayrı platform dilimi olarak anlamlı. Roadmap: *Raft Vault / secret rotasyonu (üretim)*.

## Sınıflar, metotlar, paketler

| İsim | Rol |
|------|-----|
| `VaultOptions` | Enabled, Address, Token, MountPath, SecretPath |
| `ApplyVaultSecretsAsync` | HTTP GET KV → `jwt_security_key` → config overlay |
| `Program.cs` | DI’dan önce `await builder.Configuration.ApplyVaultSecretsAsync()` |

Paket: ek Vault SDK yok — `HttpClient` + JSON.

## Repoda nerede?

```
src/WebApi/Configuration/VaultOptions.cs
src/WebApi/Configuration/VaultConfigurationExtensions.cs
src/WebApi/Program.cs
src/WebApi/appsettings.json          # Vault:Enabled=false (local)
infra/docker-compose.yml             # Vault__Enabled=true
infra/scripts/vault-seed.sh
infra/.env.example
```

## Mini akış

vault-seed `jwt_security_key` yazar → WebApi start → GET `/v1/secret/data/bankapp` → SecurityKey overlay → JWT imza bu key ile.
