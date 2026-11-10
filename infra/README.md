# Infrastructure (local)

Full local stack for later CI/CD (GitHub Actions → ArgoCD/Kubernetes → Grafana/Prometheus/OpenTelemetry).

## Services

| Service | Host URL | Notes |
|---------|----------|--------|
| Seq | http://localhost:5341 | volume `bankapp-seq-data`; Compose **healthcheck**; primary .NET log UX |
| Elasticsearch | http://localhost:9200 | credit search + log indexes; volume `bankapp-elasticsearch-data` |
| Logstash | host `:8081` (HTTP) / `:50000` (TCP) | Serilog → ES pipeline; see learning 31 |
| Kibana | http://localhost:5601 | Discover / data views for `bankapp-logs-*` |
| Redis | `localhost:6379` | volume `bankapp-redis-data`; Compose **healthcheck** |
| Redis Insight | http://localhost:5540 | official UI; add DB **host `redis`**, port **6379** (no password); volume `bankapp-redis-insight-data` |

Lab tooling guide (setup / cautions / best practices): [docs/learning/tr/24-lab-tools.md](../docs/learning/tr/24-lab-tools.md).
| RabbitMQ | `localhost:5672` / UI `:15672` | email-confirmation + domain-events queues; volume `bankapp-rabbitmq-data`; **healthcheck** |
| Vault | http://localhost:8200 | **dev mode** lab; root token from `.env`; `vault-seed` writes `secret/bankapp`; WebApi JWT overlay |
| Vault Raft (optional) | http://localhost:8210 (+8211/8212) | `docker-compose.vault-raft.yml` — see learning 19; daily stack keeps `:8200` |
| Keycloak | http://localhost:8180 | Realm `bankapp` import; SPA PKCE + dual JWT on WebApi |
| SQL Server | `localhost,11433` | Compose service `bankapp-sql`; **external** volume `sqlserver-2025-data`; **healthcheck** |
| WebApi | http://localhost:8080 | `/health` liveness; waits for SQL/Redis/Seq/RabbitMQ **healthy**; OTLP → collector |
| Scalar (Dev) | http://localhost:8080/scalar/v1 | — |
| Client (React) | http://localhost:3000 | nginx static build; Keycloak PKCE button when `VITE_KEYCLOAK_*` set |
| OTel Collector | host `:4317` / `:4318` | receives OTLP from WebApi |
| Tempo | (internal + Grafana) | distributed traces |
| Prometheus | http://localhost:9090 | scrapes collector `:8889` |
| Grafana | http://localhost:3001 | metrics + traces UI (default `admin` / `admin`) |

## Demo accounts

| Role | Email | Password |
|------|-------|----------|
| Staff | `staff.tr@` / `staff.ank@` / `staff.de@` / `staff.nyc@` / `staff.chi@bank.local` | `Demo123!` |
| Manager | `manager.ist@` / `manager.tr@` / `manager.ber@` / `manager.nyc@` / `manager.chi@bank.local` | `Demo123!` |
| Customer | `customer.tr@` / `customer.de@` / `customer.en@bank.local` | `Demo123!` |
| SuperAdmin | `superadmin@bank.local` | `Demo123!` |
| Keycloak (same password) | `staff.tr@` / `manager.tr@` / `customer.tr@` / `superadmin@bank.local` | `Demo123!` |

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
| `RABBITMQ_USER` / `RABBITMQ_PASSWORD` | Broker `DEFAULT_USER`/`PASS` + Vault `secret/bankapp/messaging` (existing `rabbitmq-data` volume ignores credential changes until recreate) |
| `TOKEN_SECURITY_KEY` | JWT signing key (≥ 64 bytes for HMAC-SHA512); seeded into `secret/bankapp/jwt` |
| `VAULT_PORT` | Host port for Vault UI/API (default 8200) |
| `VAULT_DEV_ROOT_TOKEN_ID` | Dev **root** token for `vault-seed` / UI only — **not** injected into WebApi |
| `OTEL_GRPC_PORT` / `OTEL_HTTP_PORT` | Host ports for OTLP (default 4317 / 4318) |
| `PROMETHEUS_PORT` | Host port for Prometheus (default 9090) |
| `GRAFANA_PORT` | Host port for Grafana (default **3001**, avoids client `:3000`) |
| `GRAFANA_ADMIN_USER` / `GRAFANA_ADMIN_PASSWORD` | Grafana login (lab defaults `admin` / `admin`) |

Inside Docker network, the API uses:

- SQL: `sqlserver,1433`
- Redis: `redis:6379`
- Seq: `http://seq`
- RabbitMQ: `rabbitmq:5672`
- Vault: `http://vault:8200` (Compose: `Vault__Enabled=true`, AppRole files on `vault-approle`, `FailClosed=true`; overlays JWT + SQL password + RabbitMQ)

The React app runs in the browser, so it calls the host-mapped API (`VITE_API_BASE_URL`, default `http://localhost:8080`), not the Compose service name.

## Database bootstrap

On container start, WebApi applies EF migrations and runs Bogus/identity seed when:

- `Database__RunMigrations=true`
- `Database__RunSeed=true`

(Local `dotnet run` with Development also migrates/seeds by default.)

Seed logins:

- `staff.tr@bank.local` / `Demo123!`
- `customer.tr@bank.local` / `Demo123!`

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

Dev-mode Vault for learning secret management with a **prod-shaped** auth model:

| Path | Contents | Who reads |
|------|----------|-----------|
| `secret/bankapp/jwt` | `jwt_security_key` (+ optional `jwt_security_key_previous`) | WebApi (AppRole) |
| `secret/bankapp/database` | `mssql_sa_password` | WebApi (AppRole) |
| `secret/bankapp/messaging` | `rabbitmq_username`, `rabbitmq_password` | WebApi (AppRole) |
| `secret/bankapp/observability` | Grafana/Keycloak admin (+ optional social client secrets) | Operators / UI only — **denied** to WebApi policy |

- **Root token** (`VAULT_DEV_ROOT_TOKEN_ID`): `vault-seed` + Vault UI bootstrap only — **not** given to WebApi.
- **AppRole** `bankapp-webapi`: seed writes `role_id` / `secret_id` to Compose volume `bankapp-vault-approle`; WebApi logs in and overlays JWT + SQL connection `Password` + RabbitMQ credentials.
- **FailClosed=true** on Compose WebApi: Vault/AppRole failure aborts startup (local `dotnet run` keeps `Vault:Enabled=false`).
- SQL / RabbitMQ / Grafana / Keycloak **containers** still bootstrap from Compose `.env` (Vault Agent / CSI is out of scope for this lab).

```bash
# start only Vault (+ one-shot seed)
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d vault vault-seed

# or full stack (includes vault + vault-seed)
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --build
```

1. Open http://localhost:8200
2. Sign in with token = `VAULT_DEV_ROOT_TOKEN_ID` (default `bankapp-vault-root-token`)
3. Browse KV paths under **`secret/bankapp/`** (`jwt`, `database`, `messaging`, `observability`)
4. Seed script: [`scripts/vault-seed.sh`](scripts/vault-seed.sh) (also run by Compose `vault-seed`)

Re-seed manually after Vault restart (dev data is ephemeral; re-mints AppRole `secret_id`):

```bash
docker compose -f infra/docker-compose.yml --env-file infra/.env up vault-seed
docker compose -f infra/docker-compose.yml --env-file infra/.env up -d --force-recreate --no-deps webapi
```

## Caching / logging / observability

- Cache: CreditTypes / CreditSubTypes queries (`ICacheableRequest`); Idempotency-Key responses in Redis
- Logs: Seq UI → filter `Application = 'BankApp'` (also `TraceId` / `SpanId` when OTel is on)
- Traces/metrics: WebApi OTLP → collector → Tempo + Prometheus → Grafana

### Lab — OpenTelemetry

```text
WebApi → otel-collector → Tempo (traces)
                       → Prometheus scrape (metrics)
                       → Grafana
Seq stays for structured logs (complement, not replacement)
```

1. Full stack up (`docker compose ... up -d --build`)
2. Hit the API a few times (login, list applications, `/health` is excluded from traces)
3. Open http://localhost:3001 → login `admin` / `admin`
4. **Dashboards → BankApp → BankApp HTTP (OTel)** (or Explore → Prometheus / Tempo)
5. Tempo: search service `bankapp-webapi`
6. Seq: same request should show `TraceId` when OTel enabled (Compose sets `OpenTelemetry__Enabled=true`)

Local `dotnet run` keeps OTel **off** by default (`appsettings`); point `OpenTelemetry:OtlpEndpoint` at `http://localhost:4317` and set `Enabled: true` if the collector is running.

## Roadmap

- GitHub Actions / ArgoCD / K8s; chatbot (see docs roadmap)
- WebApi JWT overlay + dual-key rotate — **done**
- AppRole + split KV paths (jwt/database/messaging/observability) + FailClosed — **done**
- Optional Raft HA lab — **done** (`docker-compose.vault-raft.yml`; still TLS for prod)
- Keycloak dual JWT + SPA PKCE + social IdP script — **done**
- Elasticsearch live search — **done** (learning 31)
