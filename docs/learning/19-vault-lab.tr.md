# 19 — HashiCorp Vault (lab) + WebApi overlay

[← Önceki](18-otel-observability.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](20-pdf-export.tr.md)

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
| **Dev mode** | Üretim değil; veri ephemeral |

## Komutlar

```powershell
cd infra
docker compose up -d vault vault-seed
# UI: http://localhost:8200  token = VAULT_DEV_ROOT_TOKEN_ID

# Seed içeriği
docker compose exec vault vault kv get secret/bankapp
```

WebApi log’da: `Vault overlay applied ... (TokenOptions:SecurityKey)` veya fallback uyarısı.

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
