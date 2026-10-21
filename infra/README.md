# Infrastructure (local)

Full local stack for later CI/CD (GitHub Actions → ArgoCD/Kubernetes → Grafana/Prometheus/OpenTelemetry).

## Services

| Service | Host URL | Notes |
|---------|----------|--------|
| Seq | http://localhost:5341 | volume `bankapp-seq-data`; Compose **healthcheck** |
| Redis | `localhost:6379` | volume `bankapp-redis-data`; Compose **healthcheck** |
| RabbitMQ | `localhost:5672` / UI `:15672` | email-confirmation + domain-events queues; volume `bankapp-rabbitmq-data`; **healthcheck** |
| Vault | http://localhost:8200 | **dev mode** lab; root token from `.env`; `vault-seed` writes `secret/bankapp` |
| SQL Server | `localhost,11433` | Compose service `bankapp-sql`; **external** volume `sqlserver-2025-data`; **healthcheck** |
| WebApi | http://localhost:8080 | `/health` liveness; waits for SQL/Redis/Seq/RabbitMQ **healthy** (not Vault yet) |
| Scalar (Dev) | http://localhost:8080/scalar/v1 | — |
| Client (React) | http://localhost:3000 | nginx static build; waits for WebApi **healthy**; own **healthcheck** |

## Demo accounts

| Role | Email | Password |
|------|-------|----------|
| Staff | `staff@bank.local` (also `.tr` / `.en` / `.de`) | `Staff123!` |
| Manager | `manager@bank.local` | `Manager123!` |
| Customer | `customer@bank.local` (also `.tr` / `.en` / `.de`) | `Customer123!` |

## Prerequisites

- Docker Desktop running
- External volume `sqlserver-2025-data` must already exist
- Ports `3000` / `8080` free (or set `CLIENT_PORT` / `WEBAPI_PORT`)

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
| `WEBAPI_PORT` | Host port for API (default 8080) |
| `CLIENT_PORT` | Host port for React UI (default 3000) |
| `VITE_API_BASE_URL` | Browser API URL baked into the client image at build time |
| `PUBLIC_CLIENT_BASE_URL` | Client origin used in email-confirmation links |
| `RABBITMQ_PORT` / `RABBITMQ_MANAGEMENT_PORT` | Host ports for AMQP / management UI |
| `RABBITMQ_PASSWORD` | Mirrored into Vault seed only (Compose broker still uses `guest`) |
| `TOKEN_SECURITY_KEY` | JWT signing key (≥ 64 bytes for HMAC-SHA512) |
| `VAULT_PORT` | Host port for Vault UI/API (default 8200) |
| `VAULT_DEV_ROOT_TOKEN_ID` | Dev root token (lab only; not for production) |

Inside Docker network, the API uses:

- SQL: `sqlserver,1433`
- Redis: `redis:6379`
- Seq: `http://seq`
- RabbitMQ: `rabbitmq:5672`
- Vault: `http://vault:8200` (available; **WebApi does not read it yet**)

The React app runs in the browser, so it calls the host-mapped API (`VITE_API_BASE_URL`, default `http://localhost:8080`), not the Compose service name.

## Database bootstrap

On container start, WebApi applies EF migrations and runs Bogus/identity seed when:

- `Database__RunMigrations=true`
- `Database__RunSeed=true`

(Local `dotnet run` with Development also migrates/seeds by default.)

Seed logins:

- `staff@bank.local` / `Staff123!`
- `customer@bank.local` / `Customer123!`

## Local VS / Vite debug

- WebApi from Visual Studio against SQL `localhost,11433`
- Client: `cd client && npm run dev` (port 3000)
- Seq/Redis from Compose

## Health checks

All long-running Compose services expose a Docker `healthcheck`. Startup order:

1. SQL / Redis / Seq / RabbitMQ become **healthy**
2. WebApi starts (migrate + seed), then becomes **healthy** via `GET /health`
3. Client starts and becomes **healthy** (nginx serves `/`)

Check status:

```bash
docker compose -f infra/docker-compose.yml --env-file infra/.env ps
```

WebApi liveness (host): `http://localhost:8080/health` — no auth; does not probe SQL/Redis (avoids false-unhealthy during seed).

## HashiCorp Vault (lab)

Dev-mode Vault for learning secret management. **WebApi still loads secrets from `.env` / Compose env** — Vault is side-by-side until a later integration slice.

```bash
# start only Vault (+ one-shot seed)
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d vault vault-seed

# or full stack (includes vault + vault-seed)
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --build
```

1. Open http://localhost:8200
2. Sign in with token = `VAULT_DEV_ROOT_TOKEN_ID` (default `bankapp-vault-root-token`)
3. Browse KV path **`secret/bankapp`** — keys: `jwt_security_key`, `mssql_sa_password`, `rabbitmq_password`
4. Seed script: [`scripts/vault-seed.sh`](scripts/vault-seed.sh) (also run by Compose `vault-seed`)

Re-seed manually after Vault restart (dev data is ephemeral):

```bash
docker compose -f infra/docker-compose.yml --env-file infra/.env up vault-seed
```

## Caching / logging

- Cache: CreditTypes / CreditSubTypes queries (`ICacheableRequest`); Idempotency-Key responses in Redis
- Logs: Seq UI → filter `Application = 'BankApp'`

## Roadmap

- GitHub Actions / ArgoCD / K8s
- OpenTelemetry + Prometheus + Grafana
- WebApi reading secrets from Vault (next Vault slice)
- File/raft Vault (non-dev) for production-like labs
