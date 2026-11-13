#!/bin/sh
# Seeds split KV v2 paths + AppRole for the local Vault lab.
# Paths:
#   secret/bankapp/jwt           — JWT HMAC keys (WebApi)
#   secret/bankapp/database      — SQL SA password (WebApi)
#   secret/bankapp/messaging     — RabbitMQ credentials (WebApi)
#   secret/bankapp/cache         — Redis password (WebApi)
#   secret/bankapp/observability — Grafana/Keycloak admin (operators only; WebApi policy denies)
#
# Writes AppRole credentials to APPROLE_DIR (Compose volume shared with WebApi).
# Root token is bootstrap-only — WebApi must use AppRole.
set -eu

VAULT_ADDR="${VAULT_ADDR:-http://127.0.0.1:8200}"
export VAULT_ADDR
export VAULT_TOKEN="${VAULT_TOKEN:?VAULT_TOKEN is required}"

APPROLE_DIR="${APPROLE_DIR:-/vault-approle}"
ROLE_NAME="${VAULT_APPROLE_NAME:-bankapp-webapi}"
POLICY_NAME="${VAULT_POLICY_NAME:-bankapp-webapi}"

JWT_KEY="${TOKEN_SECURITY_KEY:-change-me-jwt-key}"
SQL_PASSWORD="${MSSQL_SA_PASSWORD:-change-me-sql}"
RMQ_USER="${RABBITMQ_USER:-guest}"
RMQ_PASSWORD="${RABBITMQ_PASSWORD:-guest}"
REDIS_PASSWORD="${REDIS_PASSWORD:-change-me-redis}"
ELASTICSEARCH_PASSWORD="${ELASTICSEARCH_PASSWORD:-}"
GRAFANA_USER="${GRAFANA_ADMIN_USER:-admin}"
GRAFANA_PASSWORD="${GRAFANA_ADMIN_PASSWORD:-admin}"
KEYCLOAK_USER="${KEYCLOAK_ADMIN_USER:-admin}"
KEYCLOAK_PASSWORD="${KEYCLOAK_ADMIN_PASSWORD:-admin}"
GOOGLE_CLIENT_SECRET="${KEYCLOAK_GOOGLE_CLIENT_SECRET:-}"
GITHUB_CLIENT_SECRET="${KEYCLOAK_GITHUB_CLIENT_SECRET:-}"

echo "Waiting for Vault at ${VAULT_ADDR}..."
i=0
while [ "$i" -lt 30 ]; do
  if vault status >/dev/null 2>&1; then
    break
  fi
  i=$((i + 1))
  sleep 1
done

if ! vault status >/dev/null 2>&1; then
  echo "Vault is not ready at ${VAULT_ADDR}" >&2
  exit 1
fi

# --- Policy (least privilege for WebApi) ---
vault policy write "${POLICY_NAME}" - <<EOF
# KV v2 data + metadata for app secrets only (no observability, no write)
path "secret/data/bankapp/jwt" {
  capabilities = ["read"]
}
path "secret/metadata/bankapp/jwt" {
  capabilities = ["read"]
}
path "secret/data/bankapp/database" {
  capabilities = ["read"]
}
path "secret/metadata/bankapp/database" {
  capabilities = ["read"]
}
path "secret/data/bankapp/messaging" {
  capabilities = ["read"]
}
path "secret/metadata/bankapp/messaging" {
  capabilities = ["read"]
}
path "secret/data/bankapp/cache" {
  capabilities = ["read"]
}
path "secret/metadata/bankapp/cache" {
  capabilities = ["read"]
}
EOF
echo "Policy ${POLICY_NAME} written."

# --- AppRole auth ---
vault auth enable approle 2>/dev/null || true
vault write "auth/approle/role/${ROLE_NAME}" \
  token_policies="${POLICY_NAME}" \
  token_ttl=1h \
  token_max_ttl=4h \
  secret_id_ttl=0 \
  token_num_uses=0
echo "AppRole ${ROLE_NAME} configured."

mkdir -p "${APPROLE_DIR}"
ROLE_ID="$(vault read -field=role_id "auth/approle/role/${ROLE_NAME}/role-id")"
# Always mint a fresh secret_id for the Compose volume (lab recreate-friendly).
SECRET_ID="$(vault write -field=secret_id -f "auth/approle/role/${ROLE_NAME}/secret-id")"
printf '%s' "${ROLE_ID}" > "${APPROLE_DIR}/role_id"
printf '%s' "${SECRET_ID}" > "${APPROLE_DIR}/secret_id"
chmod 644 "${APPROLE_DIR}/role_id" "${APPROLE_DIR}/secret_id"
echo "AppRole credentials written to ${APPROLE_DIR}/role_id and secret_id."

# --- Migrate legacy flat secret/bankapp if present ---
LEGACY_JWT=""
LEGACY_PREV=""
LEGACY_SQL=""
LEGACY_RMQ=""
if vault kv get secret/bankapp >/dev/null 2>&1; then
  echo "Migrating legacy secret/bankapp → split paths..."
  LEGACY_JWT="$(vault kv get -field=jwt_security_key secret/bankapp 2>/dev/null || true)"
  LEGACY_PREV="$(vault kv get -field=jwt_security_key_previous secret/bankapp 2>/dev/null || true)"
  LEGACY_SQL="$(vault kv get -field=mssql_sa_password secret/bankapp 2>/dev/null || true)"
  LEGACY_RMQ="$(vault kv get -field=rabbitmq_password secret/bankapp 2>/dev/null || true)"
  vault kv metadata delete -force secret/bankapp 2>/dev/null || vault kv delete secret/bankapp 2>/dev/null || true
  echo "Legacy secret/bankapp removed after migrate."
fi

ensure_jwt() {
  if vault kv get secret/bankapp/jwt >/dev/null 2>&1; then
    echo "secret/bankapp/jwt already present — keep (preserves dual-key rotate)."
    vault kv get secret/bankapp/jwt
    return
  fi
  JWT_VALUE="${LEGACY_JWT:-$JWT_KEY}"
  if [ -n "${LEGACY_PREV}" ]; then
    vault kv put secret/bankapp/jwt \
      jwt_security_key="${JWT_VALUE}" \
      jwt_security_key_previous="${LEGACY_PREV}"
  else
    vault kv put secret/bankapp/jwt \
      jwt_security_key="${JWT_VALUE}"
  fi
  echo "Seeded secret/bankapp/jwt."
}

ensure_database() {
  if vault kv get secret/bankapp/database >/dev/null 2>&1; then
    echo "secret/bankapp/database already present — keep."
    return
  fi
  SQL_VALUE="${LEGACY_SQL:-$SQL_PASSWORD}"
  vault kv put secret/bankapp/database \
    mssql_sa_password="${SQL_VALUE}"
  echo "Seeded secret/bankapp/database."
}

ensure_messaging() {
  if vault kv get secret/bankapp/messaging >/dev/null 2>&1; then
    echo "secret/bankapp/messaging already present — keep."
    return
  fi
  RMQ_PASS_VALUE="${LEGACY_RMQ:-$RMQ_PASSWORD}"
  vault kv put secret/bankapp/messaging \
    rabbitmq_username="${RMQ_USER}" \
    rabbitmq_password="${RMQ_PASS_VALUE}"
  echo "Seeded secret/bankapp/messaging."
}

ensure_cache() {
  if vault kv get secret/bankapp/cache >/dev/null 2>&1; then
    echo "secret/bankapp/cache already present — keep."
    return
  fi
  vault kv put secret/bankapp/cache \
    redis_password="${REDIS_PASSWORD}"
  echo "Seeded secret/bankapp/cache."
}

ensure_observability() {
  # Observability may be re-seeded from env each run (admin lab passwords).
  OBS_ARGS="grafana_admin_user=${GRAFANA_USER} grafana_admin_password=${GRAFANA_PASSWORD} keycloak_admin_user=${KEYCLOAK_USER} keycloak_admin_password=${KEYCLOAK_PASSWORD} elasticsearch_lab_xpack_enabled=false"
  if [ -n "${ELASTICSEARCH_PASSWORD}" ]; then
    OBS_ARGS="${OBS_ARGS} elasticsearch_password=${ELASTICSEARCH_PASSWORD}"
  fi
  if [ -n "${GOOGLE_CLIENT_SECRET}" ]; then
    OBS_ARGS="${OBS_ARGS} keycloak_google_client_secret=${GOOGLE_CLIENT_SECRET}"
  fi
  if [ -n "${GITHUB_CLIENT_SECRET}" ]; then
    OBS_ARGS="${OBS_ARGS} keycloak_github_client_secret=${GITHUB_CLIENT_SECRET}"
  fi
  # shellcheck disable=SC2086
  vault kv put secret/bankapp/observability ${OBS_ARGS}
  echo "Seeded secret/bankapp/observability."
}

ensure_jwt
ensure_database
ensure_messaging
ensure_cache
ensure_observability

echo "Vault seed complete (split paths + AppRole ${ROLE_NAME})."
echo "WebApi should login with AppRole files under ${APPROLE_DIR} (not root token)."
