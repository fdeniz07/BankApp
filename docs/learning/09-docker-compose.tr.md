# 09 — Docker Compose

[← Önceki](08-redis.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](10-testing.tr.md)

## Bu nedir?

**Docker**: Uygulamayı container’da paketler.  
**Compose**: Birden fazla container’ı (SQL, Redis, Seq, API, client, …) tek YAML ile ayağa kaldırır.

## Bu projede ne işe yarar?

Yerel “lab prod” yığını: DB + cache + log + kuyruk + Vault + OTel + WebApi + React nginx.

## Ne zaman eklendi?

**Faz 3**; servisler zamanla çoğaldı (RabbitMQ, Vault, OTel, Grafana…).

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Image** | Çalıştırılabilir şablon |
| **Container** | Image’ın çalışan örneği |
| **Volume** | Kalıcı disk (SQL data) |
| **Healthcheck** | Servis hazır mı? |
| **depends_on** | Başlama sırası (health ile güçlendirilir) |
| **.env** | Compose değişkenleri (secret’ları commit etme) |

## Komutlar

```powershell
cd infra

# Tüm stack’i build edip kaldır
docker compose up -d --build

# Durum
docker compose ps

# Log
docker compose logs -f webapi

# Tek servis yeniden build (cache’siz)
docker compose build --no-cache webapi
docker compose up -d webapi

# Indir
docker compose down
```

| Bayrak | Anlamı |
|--------|--------|
| `-d` | Arka planda |
| `--build` | Image’ı yeniden derle |
| `--no-cache` | Docker layer cache kullanma |
| `-f` | Log’u takip et |

Örnek URL’ler (README ile doğrula): WebApi `:8080`, client `:3000`, Seq, Grafana `:3001`, Prometheus `:9090`.

## Sınıflar, metotlar, paketler

Burada “sınıf” yok; önemli dosyalar:

- `infra/docker-compose.yml`
- `infra/.env.example` → kopyala `.env`
- `src/WebApi/Dockerfile`, `client/Dockerfile`
- `DISABLE_HTTPS_REDIRECTION=true` (HTTP lab + Scalar)

## Repoda nerede?

```
infra/docker-compose.yml
infra/.env.example
infra/README.md
```

## Mini akış

`compose up` → SQL healthy → WebApi migrate/seed → client nginx → tarayıcıda login.
