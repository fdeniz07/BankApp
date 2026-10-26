# 18 — OpenTelemetry, Prometheus, Grafana

[← Önceki](17-modular-monolith.tr.md) · [İçindekiler](00-index.tr.md) · [Sonraki →](19-vault-lab.tr.md)

## Bu nedir?

**OpenTelemetry (OTel)**: Uygulamadan **trace / metric / (log köprüleri)** toplama standardı.  
**Prometheus**: Metric scrape / saklama.  
**Grafana**: Dashboard UI. Lab’de **Tempo** trace backend olarak kullanılır.  
**Seq** log için kalır (ayrı bölüm).

## Bu projede ne işe yarar?

WebApi OTLP ile collector’a gönderir → Prometheus / Tempo → Grafana `:3001` ile bakarsın.

## Ne zaman eklendi?

**Faz 6** (OTel dilimi).

## Temel kavramlar

| Kavram | Anlamı |
|--------|--------|
| **Trace / Span** | İstek yolculuğu ve adımları |
| **Metric** | Sayaç / histogram (HTTP süresi vb.) |
| **OTLP** | OTel export protokolü |
| **Collector** | Aracı (alır, işler, dağıtır) |

## Komutlar

```powershell
cd infra
docker compose up -d otel-collector prometheus grafana tempo
# Grafana: http://localhost:3001  (şifre README’de)
# Prometheus: http://localhost:9090
```

## Sınıflar, metotlar, paketler

| Paket | Rol |
|-------|-----|
| `OpenTelemetry.Extensions.Hosting` | Host |
| `OpenTelemetry.Instrumentation.AspNetCore` / `Http` / `Runtime` | Otomatik span/metric |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | OTLP export |

Kayıt: WebApi DI / `Program` veya extension metodları.

## Repoda nerede?

```
src/WebApi/… OpenTelemetry setup
infra/docker-compose.yml
infra/otel/ (varsa collector config)
docs/assets/screenshots/en/Metrics on Grafana.png
```

## Mini akış

HTTP istek → AspNetCore instrumentation span → OTLP collector → Tempo/Prometheus → Grafana’da grafik.
