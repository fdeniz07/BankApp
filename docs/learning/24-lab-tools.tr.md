# 24 — Lab araçları (izleme ve inceleme)

[← Önceki](23-rate-limiting.tr.md) · [İçindekiler](00-index.tr.md)

İlgili: [07 Seq](07-serilog-seq.tr.md) · [08 Redis](08-redis.tr.md) · [09 Compose](09-docker-compose.tr.md) · [13 RabbitMQ](13-rabbitmq.tr.md) · [18 OTel/Grafana](18-otel-observability.tr.md) · [19 Vault](19-vault-lab.tr.md)

## Bu nedir?

**Lab araçları**: BankApp stack’indeki servisleri (API, cache, kuyruk, log, metrik, secret, DB) **gözlemlemek ve debug etmek** için kullanılan UI / CLI’ler. Redis’in kendisi gibi birçok sunucunun yerleşik “güzel arayüzü” yoktur; Insight, Grafana, Scalar vb. bu boşluğu doldurur.

Dokümantasyon stili: sık kullanılan komutların **üstünde/yanında ne yaptığı**, **sonra ne kontrol edileceği** ve mümkünse **felaket / kurtarma** adımları yazılır (ör. [09 Compose](09-docker-compose.tr.md)).

## Bu projede ne işe yarar?

Compose lab’i ayağa kalkınca çoğu araç zaten URL ile gelir. Amaç: “İstek attım — cache doldu mu? Log’a ne yazıldı? Kuyrukta mesaj var mı? Trace nerede kırıldı?” sorularını aynı makinede cevaplamak.

| Araç | URL / uç | Ne izler? |
|------|----------|-----------|
| **Scalar** | http://localhost:8080/scalar/v1 | HTTP API (JWT, header, body) |
| **Seq** | http://localhost:5341 | Yapılandırılmış uygulama logları |
| **Redis Insight** | http://localhost:5540 | Redis key / TTL / içerik |
| **RabbitMQ Management** | http://localhost:15672 | Kuyruk, mesaj, consumer |
| **Grafana** | http://localhost:3001 | Metrik + trace UI |
| **Prometheus** | http://localhost:9090 | Ham metrik / scrape |
| **Vault UI** | http://localhost:8200 | Lab secret (KV) |
| **SQL istemcisi** *(host)* | `localhost,11433` | Veritabanı (Compose’da UI yok) |
| **Docker Desktop / CLI** | — | Konteyner sağlık / log |

## Ne zaman eklendi?

Servisler Faz 2–6 boyunca geldi; **Redis Insight** ve bu birleşik “lab tools” rehberi eğitim diliminde eklendi.

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Observability** | Log + metric + trace ile sistemin içini görmek |
| **Compose DNS** | Konteynerler birbirine **servis adı** ile ulaşır (`redis`, `webapi`); `localhost` konteyner *içinde* başka anlam taşır |
| **Host port** | Tarayıcının gördüğü `localhost:PORT` (Compose `ports:`) |
| **Lab credential** | Eğitim şifresi; **üretimde kullanma**, rotasyon / Vault / secret manager şart |
| **Liveness vs data UI** | `/health` “yaşıyor mu?”; Insight/Grafana “içeride ne var?” |

---

## Araç araç: kurulum, kullanım, dikkat, best practice

### 1) Scalar (API denemesi)

**Kurulum:** WebApi Dev’de gelir; ekstra kurulum yok.  
**Kullanım:**

1. `POST /api/Auth/login` → token al (demo hesaplar README’de).
2. İstek header: Key = **`Authorization`**, Value = `Bearer <token>` (`Auth` yazma).
3. Kredi **yazma** (create/approve/reject/cancel): ek header **`Idempotency-Key`** (UUID).

**Dikkat:**

- Token süresi dolunca 401; yeniden login.
- Role yetkisi yoksa 403 (ör. manager-only endpoint).
- Rate limit aşımında **429** ([23](23-rate-limiting.tr.md)).

**Best practice:** Aynı senaryoyu tekrarlanabilir tut (sabit Idempotency-Key ile replay / 409 dene). Production OpenAPI’yi public bırakma; lab’de Scalar Dev’e özgüdür.

---

### 2) Seq (log)

Ayrıntılı filtre, checklist, kurtarma: **[07 — Serilog/Seq](07-serilog-seq.tr.md)**.

**Kurulum:** Compose `seq`; lab’de genelde **giriş yok** → http://localhost:5341.

```powershell
cd infra
docker compose --env-file .env up -d seq
# Kontrol: ps → healthy; UI Events; filtre örn. @Level = 'Error'
docker compose --env-file .env logs --tail=50 webapi   # sink bağlanamıyorsa burada
```

**Dikkat:** Volume büyür; lab reset `down -v` ([09](09-docker-compose.tr.md)). Prod auth + retention.

**Best practice:** PII/JWT loglama. Error ile başla; TraceId ile Grafana’ya köprüle.

---

### 3) Redis Insight

**Kurulum:** Compose `redis-insight` (`REDIS_INSIGHT_PORT`, varsayılan `5540`).  
**İlk bağlantı (kritik):**

```text
redis://redis:6379
```

Insight **konteyner içinde** çalıştığı için host **`redis`** (Compose servis adı). `127.0.0.1` / `localhost` Insight sürecinin kendisidir → bağlantı fail.

**Bu projede göreceğin key örnekleri:**

- `BankApp:CreditTypeList(...)` / `CreditSubTypeList(...)` — query cache  
- `BankApp:CacheGroup:CreditTypes` / `CreditSubTypes` — grup indeksi  
- Idempotency kayıtları — yazma endpoint’lerinden sonra  

**Dikkat:** Insight’tan `FLUSHDB` / key silmek lab’i bozabilir (cache miss normal; idempotency silersen tekrar işlem riski). Prod Redis’e Insight’ı açık internete koyma.

**Best practice:** Önce Browser’da oku; silmeden TTL ve type’a bak. Cache invalidation’ı uygulama komutuyla (CreditType update) gözlemlemek öğreticidir.

---

### 4) RabbitMQ Management

Ayrıntılı kuyruk tablosu + felaket: **[13 — RabbitMQ](13-rabbitmq.tr.md)**.

**Kurulum:** UI http://localhost:15672 → **`guest` / `guest`**.

```powershell
cd infra
docker compose --env-file .env up -d rabbitmq
docker compose --env-file .env exec rabbitmq rabbitmq-diagnostics -q ping
# UI: Queues → Consumers ≥ 1, Ready birikmiyor olmalı
```

**Dikkat:** `guest` = lab. UI’dan Get message consumer’ı bozabilir.

**Best practice:** Önce Consumers + Ready; birikince Seq + `logs webapi`.

---

### 5) Grafana + Prometheus (+ Tempo)

**Kurulum:** Compose OTel yığını; Grafana `:3001` → **`admin` / `admin`** (`GRAFANA_ADMIN_*`). Prometheus `:9090` lab’de genelde login’siz.  
**Kullanım:** Grafana’da provision edilmiş datasource’lar; Explore ile metric/trace. İlk girişte şifre değiştir uyarısını lab’de not et, prod’da mutlaka değiştir.

**Temel komutlar / adımlar**

```powershell
cd infra
# OTel + UI servislerini ayağa kaldır (stack zaten full up ise gerekmez)
docker compose --env-file .env up -d otel-collector tempo prometheus grafana

# Durum
docker compose --env-file .env ps otel-collector tempo prometheus grafana
docker compose --env-file .env logs -f grafana

# Host’tan hızlı sağlık kontrolü
curl -s -o NUL -w "%{http_code}" http://localhost:3001/api/health   # Grafana
curl -s http://localhost:9090/-/ready                                 # Prometheus
```

Grafana UI (http://localhost:3001):

1. Login: `admin` / `admin`.
2. **Dashboards → BankApp → BankApp HTTP (OTel)** — hazır lab paneli.
3. Sol menü **Explore**:
   - Datasource **Prometheus** — örnek: `http_server_request_duration_seconds_count` veya `up` (scrape ayakta mı).
   - Datasource **Tempo** — Search / service adı: `bankapp-webapi` (birkaç API isteği attıktan sonra).
4. Trace aç → span süreleri; aynı anda Seq’te `TraceId` ile filtrele.

Prometheus UI (http://localhost:9090): **Status → Targets** → `otel-collector:8889` **UP** olmalı. **Graph**’ta `up{job="otel-collector"}` dene.

**Dikkat:** Port çakışması: client `:3000`, Grafana `:3001`. Trace yoksa: WebApi’de OTel açık mı (`OpenTelemetry__Enabled`), collector sağlıklı mı, birkaç istek attın mı (`/health` trace dışı olabilir) — [18](18-otel-observability.tr.md).

**Best practice:** “Yavaş endpoint” → Grafana Tempo trace → ilgili span → Seq’te aynı `TraceId`. Metric’i dashboard’a dökmeden önce Explore / Prometheus’ta doğrula. Lab şifresini prod’da asla bırakma.

---

### 6) Vault UI

Ayrıntılı CLI, overlay log, kurtarma: **[19 — Vault lab](19-vault-lab.tr.md)**.

**Kurulum:** http://localhost:8200 → Method **Token** = `VAULT_DEV_ROOT_TOKEN_ID` → KV `secret/bankapp`.

```powershell
cd infra
docker compose --env-file .env up -d vault
docker compose --env-file .env up vault-seed
docker compose --env-file .env exec vault vault kv get secret/bankapp
# WebApi: logs | Select-String Vault  → overlay applied / fallback
```

**Dikkat:** Dev mode ≠ prod. Root token’ı ekran görüntüsüne koyma.

**Best practice:** Secret’ı UI/CLI ile oku; seed + WebApi recreate ile yenile.

---

### 7) SQL Server istemcisi (host aracı)

**Kurulum (Compose dışı):** [Azure Data Studio](https://learn.microsoft.com/sql/azure-data-studio/) veya SSMS / VS Code MSSQL eklentisi.  
**Bağlantı:** Server `localhost,11433` (virgül!), Login `sa`, şifre `infra/.env` → `MSSQL_SA_PASSWORD`.

**Dikkat:** Port **11433** (1433 host’ta çakışmasın diye). `Encrypt` / trust server certificate: lab’de ADS “Trust server certificate” gerekir. Prod şifreyi `.env.example` ile aynı bırakma.

**Best practice:** Önce salt-okuma sorguları. Seed / migrate’i uygulama akışına bırak; UI’dan şema oynatma eğitim dışında riskli.

---

### 8) Docker Desktop / Compose CLI

Ayrıntılı bayraklar, checklist ve felaket tablosu: **[09 — Docker Compose](09-docker-compose.tr.md)**. Özet:

```powershell
cd infra

# Ne ayakta / healthy mi? (ilk bakılacak yer)
docker compose --env-file .env ps

# WebApi neden kalkmıyor / seed ne dedi?
docker compose --env-file .env logs -f webapi

# Sadece cache UI’yi güncelle
docker compose --env-file .env up -d redis redis-insight

# Stack durdur (volume’lar kalır — “yumuşak kapanış”)
docker compose --env-file .env down

# Lab Seq/Redis verisini sıfırla (yıkıcı; SQL external volume silinmez)
# docker compose --env-file .env down -v
```

**Dikkat:** `down -v` → Seq/Redis/(Insight) data gider. SQL için ayrı `docker volume rm sqlserver-2025-data`. Port çakışmasında `.env` portlarını değiştir.

**Best practice / kurtarma sırası:** `ps` → ilgili `logs` → bağımlılık healthy mi → `--force-recreate` tek servis → gerekirse `down -v` → en son SQL volume.

---

### 9) İstemci tarafı (bonus)

| Araç | Ne için |
|------|---------|
| Tarayıcı DevTools → Network | React → API çağrıları, 401/429 |
| React DevTools | Component state (opsiyonel eklenti) |
| Postman / Bruno | Scalar alternatifi; koleksiyon paylaşımı |

**Best practice:** CORS / cookie sorununda Network + Seq birlikte. Token’ı Git’e / public issue’ya yapıştırma.

---

## Genel lab best practice’leri

1. **Önce stack’i ayağa kaldır**, sonra aracı aç (`compose ps` yeşil/healthy).  
2. **URL = host port**; konteyner→konteyner = **servis adı** (Insight→Redis örneği).  
3. Kimlik bilgilerini README / `.env.example` ile hizala; kendi `.env` dosyanı commit etme.  
4. Bir olayı üç açıdan doğrula: **HTTP (Scalar)** → **log (Seq)** → **yan etki (Redis / Rabbit / SQL / Grafana)**.  
5. Lab şifreleri eğitim içindir; prod checklist: auth açık UI, TLS, zayıf default yok, retention, network policy.  
6. Insight / SQL UI’dan destructive işlem öncesi ne sildiğini bil.

## Komutlar (hızlı hatırlatma)

```powershell
# Stack
cd infra
copy .env.example .env   # bir kez
docker compose --env-file .env up -d --build

# Redis CLI (Insight alternatifi)
docker compose exec redis redis-cli KEYS "BankApp:*"
```

| Servis | Lab kimlik |
|--------|------------|
| Grafana | `admin` / `admin` |
| RabbitMQ | `guest` / `guest` |
| Vault | Token `bankapp-vault-root-token` (veya `.env`) |
| SQL | `sa` / `MSSQL_SA_PASSWORD` |
| Redis / Seq / Prometheus | Lab’de genelde auth yok |

## Sınıflar, metotlar, paketler

Bu bölüm çoğunlukla **ops / DX**. Kod tarafı bağlantılar:

| İsim | Rol |
|------|-----|
| `AddStackExchangeRedisCache` | Redis cache + idempotency altyapısı |
| Serilog → Seq sink | Log |
| OTLP exporter | Trace/metric → collector |
| `IdempotencyKeyFilter` | Redis’te yazma cevabı |

## Repoda nerede?

```
infra/docker-compose.yml
infra/.env.example
infra/README.md
README*.md                 # API özeti / URL tablosu
docs/learning/07…19, 23    # Servis başına derinlik
docs/learning/08-redis.tr.md  # Insight bağlantı notu
```

## Mini akış (önerilen debug zinciri)

1. Scalar: login → `GET /api/CreditTypes` (`Authorization: Bearer …`).  
2. Redis Insight: `BankApp:CreditTypeList…` key’i göründü mü?  
3. Seq: ilgili request log satırları.  
4. CreditType update → CacheGroup temizlendi mi?  
5. (İsteğe bağlı) Grafana’da HTTP süresi / trace.

Aynı zinciri approve + `Idempotency-Key` ile tekrarla → Redis’te idempotency kaydı + RabbitMQ’da decision-letter / domain event kuyruğu.
