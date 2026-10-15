# Infrastructure (local)

Local backend stack and seed for later CI/CD (GitHub Actions → ArgoCD/Kubernetes → Grafana/Prometheus/OpenTelemetry).

## Services

| Service | Host URL | Notes |
|---------|----------|--------|
| Seq | http://localhost:5341 | volume `bankapp-seq-data` |
| Redis | `localhost:6379` | volume `bankapp-redis-data` |
| SQL Server | `localhost,11433` | Compose service `bankapp-sql`; **external** volume `sqlserver-2025-data` (same data as former `SQLServer2025`) |
| WebApi | http://localhost:8080 | — |
| Scalar (Dev) | http://localhost:8080/scalar/v1 | — |

## Prerequisites

- Docker Desktop running
- External volume `sqlserver-2025-data` must already exist (created by the previous SQL container)
- Port `8080` free (or set `WEBAPI_PORT` in `infra/.env`)

## Quick start

```bash
# from repository root
copy infra\.env.example infra\.env    # Windows
# cp infra/.env.example infra/.env    # Linux/macOS

docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --build
```

Stop (keep volumes; SQL external volume is never removed by Compose):

```bash
docker compose -f infra/docker-compose.yml --env-file infra/.env down
```

Reset Seq/Redis data only (destructive for those volumes):

```bash
docker compose -f infra/docker-compose.yml --env-file infra/.env down -v
```

To wipe SQL data as well (destructive):

```bash
docker volume rm sqlserver-2025-data
```

## Environment

Secrets and ports live in `infra/.env` (gitignored). Template: `infra/.env.example`.

| Variable | Purpose |
|----------|---------|
| `MSSQL_SA_PASSWORD` | SQL SA password (must match existing DB if volume already has data) |
| `SQL_PORT` | Host port for SQL (default 11433) |
| `TOKEN_SECURITY_KEY` | JWT signing key (≥ 64 bytes for HMAC-SHA512) |
| `WEBAPI_PORT` | Host port for API (default 8080) |

Inside Docker network, the API uses:

- SQL: `sqlserver,1433`
- Redis: `redis:6379`
- Seq: `http://seq`

## Database bootstrap

On container start, WebApi applies EF migrations and runs Bogus/identity seed when:

- `Database__RunMigrations=true`
- `Database__RunSeed=true`

(Local `dotnet run` with Development also migrates/seeds by default.)

Seed logins:

- `staff@bank.local` / `Staff123!`
- `customer@bank.local` / `Customer123!`

## Local VS debug (without containerized API)

Run WebApi from Visual Studio against:

- SQL on `localhost,11433` (`appsettings.Development.json`)
- Seq/Redis from Compose (`localhost:5341`, `localhost:6379`)

## Caching / logging

- Cache: CreditTypes / CreditSubTypes queries (`ICacheableRequest`)
- Logs: Seq UI → filter `Application = 'BankApp'`

## Roadmap

- Unit tests (next)
- React frontend
- GitHub Actions / ArgoCD / K8s
- OpenTelemetry + Prometheus + Grafana
