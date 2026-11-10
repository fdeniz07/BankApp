# 18 — OpenTelemetry, Prometheus, Grafana

[← Previous](17-modular-monolith.md) · [Contents](00-index.md) · [Next →](19-vault-lab.md)

## What is this?

**OpenTelemetry (OTel)**: Standard for collecting **traces / metrics / (log bridges)** from the app.  
**Prometheus**: Metric scrape and storage.  
**Grafana**: Dashboard UI. In the lab **Tempo** is the trace backend.  
**Seq** remains for logs (separate chapter — chapter 07).

**Analogy:** OTel is security **camera footage**; a span is time at each door; Prometheus is the counter panel; Grafana is the **control room** on one screen.

```mermaid
flowchart LR
  API["WebApi OTLP"] --> COL["otel-collector"]
  COL --> PROM["Prometheus"]
  COL --> TEMPO["Tempo"]
  PROM --> GRAF["Grafana port 3001"]
  TEMPO --> GRAF
```

## Role in BankApp

WebApi sends OTLP to the collector → Prometheus / Tempo → inspect in Grafana at `:3001`.

## When added?

**Phase 6** (OTel slice).

## Core concepts

| Concept | Meaning |
|--------|--------|
| **Trace / Span** | Request journey and steps |
| **Metric** | Counter / histogram (HTTP duration, etc.) |
| **OTLP** | OTel export protocol |
| **Collector** | Middle tier (receive, process, fan-out) |
| **service.name** | Filter in Grafana/Tempo: `bankapp-webapi` |

```mermaid
sequenceDiagram
  participant Browser as Client
  participant API as WebApi
  participant COL as collector
  participant TEMPO as Tempo
  participant GRAF as Grafana
  Browser->>API: GET api call
  API->>COL: OTLP span
  COL->>TEMPO: trace record
  Note over GRAF: Explore Tempo service bankapp-webapi
  GRAF->>TEMPO: trace query
```

## Commands

```powershell
cd infra
docker compose --env-file .env up -d otel-collector prometheus grafana tempo

# Grafana http://localhost:3001  → admin / admin
# Prometheus http://localhost:9090
# OTLP (app → collector) localhost:4317

docker compose --env-file .env ps otel-collector tempo prometheus grafana
curl -s http://localhost:9090/-/ready
curl -s -o NUL -w "%{http_code}`n" http://localhost:3001/api/health
```

Quick tour in Grafana:

1. Send a few API requests (Scalar / client; `/health` is usually outside tracing).
2. **Dashboards → BankApp → BankApp HTTP (OTel)**.
3. **Explore → Tempo** → service `bankapp-webapi`.
4. **Explore → Prometheus** → e.g. `up` or HTTP duration metrics.
5. In Seq, check `TraceId` for the same request.

Detailed lab tools notes: [24 — Lab tools](24-lab-tools.md).

## Classes, methods, packages

| Package | Role |
|-------|-----|
| `OpenTelemetry.Extensions.Hosting` | Host integration |
| `OpenTelemetry.Instrumentation.AspNetCore` / `Http` / `Runtime` | Automatic span/metric |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | OTLP export |

Registration: WebApi DI / `Program` or extension methods.

## Where in the repo?

```
src/WebApi/… OpenTelemetry setup
infra/docker-compose.yml
infra/otel/ (collector config if present)
docs/assets/screenshots/en/Metrics on Grafana.png
```

## Mini flow

HTTP request → AspNetCore instrumentation span → OTLP collector → Tempo/Prometheus → charts in Grafana.

## Beginner lab

1. `docker compose --env-file .env up -d otel-collector prometheus grafana tempo webapi`.
2. Send real API traffic (not only `/health`).
3. Grafana → **Dashboards → BankApp → BankApp HTTP (OTel)**.
4. **Explore → Tempo** → service `bankapp-webapi`.
5. Match `TraceId` in Seq for the same request.

## Common mistakes

- Opening Grafana before generating traffic  
- Calling only `/health` and expecting rich traces  
- Ignoring OTLP errors when collector is down  
- Skipping Prometheus `up` before reading dashboards  

## Where it helps

- [ ] `curl http://localhost:9090/-/ready` → 200  
- [ ] Grafana health 200  
- [ ] Tempo shows `bankapp-webapi` spans  
- [ ] Seq TraceId correlates with traces (lab)
