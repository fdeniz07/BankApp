# Infrastructure (local)

This folder holds runtime infrastructure for local development and is the seed for later CI/CD (GitHub Actions → ArgoCD/Kubernetes → Grafana/Prometheus/OpenTelemetry).

## Seq (logging UI)

Shared local Seq instance with a **named Docker volume** so logs survive container restarts.

```bash
# from repository root
docker compose -f infra/docker-compose.yml up -d
```

| Item | Value |
|------|--------|
| UI / ingest | http://localhost:5341 |
| Volume | `bankapp-seq-data` |
| App setting | `Seq:ServerUrl` = `http://localhost:5341` |

Stop without deleting logs:

```bash
docker compose -f infra/docker-compose.yml down
```

Delete Seq data (destructive):

```bash
docker compose -f infra/docker-compose.yml down -v
```

In Seq, filter by property `Application = BankApp` (set via Serilog properties). Other projects using Core can use the same Seq and a different `Application` name.

## Roadmap (not in this phase)

- Redis, SQL Server, WebApi containers in the same compose
- `.github/workflows` build/push
- `infra/k8s` + ArgoCD
- OpenTelemetry collector + Prometheus + Grafana
