# 08 — Redis (cache and Idempotency)

[← Previous](07-serilog-seq.md) · [Contents](00-index.md) · [Next →](09-docker-compose.md)

## What is this?

**Redis**: Fast in-memory key-value store.  
In BankApp two main jobs: **cache** (lists) and storing **Idempotency-Key** values.

**Analogy:** Redis is the bank’s **quick notepad** — hot lists are cached; idempotency is the “this receipt number was already processed” stamp.

```mermaid
flowchart LR
  API[WebApi] --> R[Redis]
  R --> CACHE[List cache]
  R --> IDEM[Idempotency-Key TTL]
  INS[Redis Insight] --> R
```

## Role in BankApp

1. Speed up reads such as credit type / sub-type lists (`ICacheRemoverRequest` for invalidation).
2. Safely handle repeated create/approve/reject/cancel with the same `Idempotency-Key` (24-hour TTL).
3. In the lab, browse keys in the browser via **Redis Insight** UI (`http://localhost:5540`).

## When added?

Cache: **Phase 2–3**. Idempotency: **Phase 6**. Redis Insight UI: lab (Compose).

## Core concepts

| Concept | Meaning |
|--------|--------|
| **Key / Value** | Key and value |
| **TTL** | Time until the key is removed automatically |
| **Cache stampede** | Sudden DB load when cache is empty (worth knowing) |
| **Idempotency** | Sending the same request twice yields the same effect / replay |
| **Redis Insight** | Official web UI; Redis has no built-in UI |

```mermaid
sequenceDiagram
  participant C as Client
  participant API as WebApi
  participant R as Redis
  C->>API: POST + Idempotency-Key
  API->>R: Key exists?
  alt First request
    API->>API: Process
    API->>R: Store response with TTL
  else Retry
    R-->>API: Stored response
    API-->>C: Replay
  end
```

## Commands

```powershell
cd infra
docker compose --env-file .env up -d redis redis-insight

# Optional: enter container and inspect keys
docker compose exec redis redis-cli KEYS "*"
```

Browser: `http://localhost:5540` → **Add Redis database** → Host **`redis`**, Port **`6379`**, no password.  
(`localhost` from the Insight container does not reach Redis; use the Compose service name.)

Screen (Redis Insight):

<img src="../../assets/screenshots/en/Cache%20on%20Redis%20Insight.png" alt="Redis Insight cache" width="720" />

HTTP side:

```http
POST /api/CreditApplications
Idempotency-Key: 3f2c9a...   # client generates (UUID)
```

## Classes, methods, packages

| Name | Role |
|------|-----|
| `Microsoft.Extensions.Caching.StackExchangeRedis` | IDistributedCache |
| Cache pipeline / `ICacheRemoverRequest` | Clear group on write |
| Idempotency middleware / filter (WebApi Credit) | Key + body hash check |

## Where in the repo?

```
src/WebApi/…          # Redis DI, idempotency
src/Core/…            # cache pipeline pieces
infra/docker-compose.yml   # redis + redis-insight
infra/.env.example         # REDIS_INSIGHT_PORT
tests/.../IdempotencyKeyTests.cs
```

## Mini flow (Idempotency)

First POST + Key → operation runs, response written to Redis.  
Same Key + same body → stored response replayed (no second outbox).  
Same Key + different body → **409 Conflict**.

## Beginner lab

1. `docker compose up -d redis redis-insight` from `infra`.
2. In Insight connect with host **`redis`**, port **6379**.
3. Open credit type list in UI → inspect cache keys in Redis.
4. Send two POSTs with the same `Idempotency-Key` (integration test or Postman).
5. `redis-cli PING` → `PONG`.

## Common mistakes

- Using `localhost` as Insight host inside Compose → service name `redis`.  
- Changing the idempotency key on every retry → protection is lost.  
- Writes without cache invalidation → stale lists (`ICacheRemoverRequest`).  

## Where it helps

- [ ] `PING` returns PONG  
- [ ] Insight connection is green  
- [ ] Duplicate POST does not double-process  
