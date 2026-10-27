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

Aşağıda her bloktan **önce** ne yaptığı / **sonra** neye bakacağın yazıyor.  
Çalışma dizini her zaman `infra` (veya `-f` / `--env-file` ile kökten).

### Günlük kullanım

```powershell
cd infra

# .env yoksa şablondan oluştur (secret’lar burada; commit etme)
copy .env.example .env

# Tüm lab’i image build ederek arka planda başlat
docker compose --env-file .env up -d --build
# Kontrol: ps’te sql/redis/rabbit/webapi healthy; http://localhost:8080/health → 200
```

```powershell
# Hangi konteyner ayakta, health durumu nedir?
docker compose --env-file .env ps
# Beklenen: bankapp-sql / redis / rabbitmq / webapi → healthy (starting uzun sürebilir)
```

```powershell
# WebApi log’unu canlı izle (migrate/seed/hata burada görünür)
docker compose --env-file .env logs -f webapi
# Ctrl+C sadece takibi keser; konteyner çalışmaya devam eder
```

```powershell
# Son N satır (dosyaya dökmeden hızlı bakış)
docker compose --env-file .env logs --tail=100 webapi seq
```

```powershell
# Sadece bir servisi yeniden ayağa kaldır (ör. Redis Insight ekledikten sonra)
docker compose --env-file .env up -d redis redis-insight
```

```powershell
# Kod değişti: WebApi image’ını cache’siz yeniden derle + container’ı güncelle
docker compose --env-file .env build --no-cache webapi
docker compose --env-file .env up -d webapi
# Kontrol: logs -f webapi + /health
```

```powershell
# Stack’i durdur; named volume’lar KALIR (SQL external volume’a dokunulmaz)
docker compose --env-file .env down
```

| Bayrak / arg | Ne işe yarar? |
|--------------|----------------|
| `--env-file .env` | Port/şifre değişkenlerini oku |
| `-d` | Detached — terminali kilitlemez |
| `--build` | `up` sırasında image’ı yeniden build et |
| `--no-cache` | Layer cache kullanma (temiz build) |
| `logs -f` | Follow — yeni satır geldikçe yaz |
| `logs --tail=N` | Sadece son N satır |
| `exec …` | Çalışan konteyner içinde komut (redis-cli vb.) |

Örnek URL’ler: WebApi `:8080`, client `:3000`, Seq `:5341`, Grafana `:3001`, Prometheus `:9090`, Redis Insight `:5540`, RabbitMQ UI `:15672`, Vault `:8200`.

### Ne kontrol edilir? (sağlık checklist)

| Kontrol | Komut / URL | İyi görünüm |
|---------|-------------|-------------|
| Konteynerler | `compose ps` | `healthy` / `Up` |
| API canlı | `http://localhost:8080/health` | 200, liveness |
| SQL hazır | `ps` içinde sql healthy; ADS `localhost,11433` | Bağlantı açılır |
| Redis | `docker compose exec redis redis-cli ping` | `PONG` |
| Log akışı | Seq UI / `logs webapi` | Seed/login satırları |
| Kuyruk | RabbitMQ UI `:15672` | Consumer bağlı |
| Metrik scrape | Prometheus Targets | `otel-collector` UP |

### Felaket / kurtarma yolları (lab)

Önce **en az yıkıcı** adımı dene; `-v` ve volume silme geri alınamaz (lab verisi gider).

| Durum | Ne yap? | Komut / not |
|-------|---------|-------------|
| Tek servis crash loop | Log’a bak → sadece o servisi restart | `logs -f <servis>` sonra `compose up -d --force-recreate <servis>` |
| WebApi ayağa kalkmıyor | SQL/Redis/Rabbit **healthy** mi? Seed hatası mı? | `ps` → bağımlılıklar; `logs webapi` |
| Port meşgul (3000/8080/…) | `.env` içinde `CLIENT_PORT` / `WEBAPI_PORT` vb. değiştir | `up -d` tekrar |
| Redis / Seq “kirli” lab verisi | Stack’i volume’larla sil (SQL external **kalır**) | `compose down -v` — **Seq + Redis (+ Insight) data silinir** |
| SQL’i de sıfırla (tam lab reset) | External volume’u elle sil | `docker volume rm sqlserver-2025-data` sonra volume’u yeniden oluşturup `up` (infra/README) |
| Image bozuldu / garip cache | no-cache rebuild | `build --no-cache` + `up -d` |
| Hiçbir şey ayağa kalkmıyor | Docker Desktop çalışıyor mu? Disk dolu mu? | Desktop restart; `docker info` |

```powershell
# --- Yıkıcı: Seq/Redis volume’larını da sil (SQL external hariç) ---
# Önce emin ol: lab cache/log kaybolur
docker compose --env-file .env down -v

# --- Daha yıkıcı: SQL verisini de sil ---
# docker volume rm sqlserver-2025-data
# Sonra external volume’u tekrar create et (infra/README Prerequisites), up -d --build
```

```powershell
# Konteyner içinde teşhis (örnekler)
docker compose --env-file .env exec redis redis-cli ping
docker compose --env-file .env exec webapi printenv ASPNETCORE_ENVIRONMENT
```

İzleme araçları ve debug zinciri: [24 — Lab tools](24-lab-tools.tr.md).

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
