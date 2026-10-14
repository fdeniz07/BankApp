# Infrastructure (local)

Local runtime dependencies for development. Seed for later CI/CD (GitHub Actions → ArgoCD/Kubernetes → Grafana/Prometheus/OpenTelemetry).

## Start

```bash
# from repository root
docker compose -f infra/docker-compose.yml up -d
```

| Service | URL / port | Volume |
|---------|------------|--------|
| Seq | http://localhost:5341 | `bankapp-seq-data` |
| Redis | `localhost:6379` | `bankapp-redis-data` |

App settings:

- `Seq:ServerUrl` = `http://localhost:5341`
- `Redis:ConnectionString` = `localhost:6379`
- `CacheSettings:SlidingExpirationMinutes` = `30`

Stop without deleting data:

```bash
docker compose -f infra/docker-compose.yml down
```

Delete volumes (destructive):

```bash
docker compose -f infra/docker-compose.yml down -v
```

## Caching (when Redis is used)

Only MediatR requests that implement `ICacheableRequest` read/write Redis (CreditTypes / CreditSubTypes queries).

Mutating commands that implement `ICacheRemoverRequest` clear the related cache group after success.

In Seq, look for:

- `Returning cached response for {CacheKey}` → Redis hit
- `Cache group invalidated: CreditTypes` → after create/update/delete

## Roadmap (later phases)

- SQL Server + WebApi containers in this compose
- `.github/workflows` build/push
- `infra/k8s` + ArgoCD
- OpenTelemetry + Prometheus + Grafana
