# 19 — HashiCorp Vault (lab)

[← Önceki](18-otel-observability.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](20-pdf-export.tr.md)

## Bu nedir?

**Vault**: Secret (şifre, bağlantı cümlesi, API key) saklama ve dağıtma aracı.  
BankApp’te şu an **Compose dev-mode lab**: seed path `secret/bankapp`. **WebApi henüz Vault’tan okumaz** (hâlâ `.env`).

## Bu projede ne işe yarar?

Eğitim: “secret’lar repoda olmasın” fikri + UI’da secret’lara bakmak.  
İleride: SDK ile WebApi entegrasyonu (roadmap).

## Ne zaman eklendi?

Lab: roadmap “İleride” maddesi olarak kuruldu (Compose).

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Secret engine** | KV store gibi motor |
| **Dev mode** | Lab için gevşek / bellek içi (üretim değil!) |
| **Token** | Vault’a kimlik |

## Komutlar

```powershell
cd infra
docker compose up -d vault
# UI / port: compose ve infra README
```

Vault CLI (container içinde, sürüm/compose’a göre):

```powershell
docker compose exec vault vault status
```

## Sınıflar, metotlar, paketler

Henüz Application’da Vault SDK zorunlu bağımlılık yok.  
Entegrasyon gelince bu bölüme `VaultSharp` veya HTTP client + DI eklenecek.

## Repoda nerede?

```
infra/docker-compose.yml
infra/… vault seed script / README
docs/architecture-notes.tr.md  # Lab — Vault
```

## Mini akış (bugün)

Compose vault ayağa kalkar → seed `secret/bankapp` → UI’da görürsün → WebApi hâlâ env okur.  
Yarınki hedef: startup’ta Vault’tan connection string çekmek.
