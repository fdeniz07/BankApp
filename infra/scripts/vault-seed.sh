#!/bin/sh
# Seeds KV v2 path secret/bankapp for the local Vault lab.
# Used by Compose service vault-seed (and can be run manually against a running vault).
set -eu

VAULT_ADDR="${VAULT_ADDR:-http://127.0.0.1:8200}"
export VAULT_ADDR
export VAULT_TOKEN="${VAULT_TOKEN:?VAULT_TOKEN is required}"

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

JWT_KEY="${TOKEN_SECURITY_KEY:-change-me-jwt-key}"
SQL_PASSWORD="${MSSQL_SA_PASSWORD:-change-me-sql}"
RMQ_PASSWORD="${RABBITMQ_PASSWORD:-guest}"

# Dev server already mounts KV v2 at "secret/".
vault kv put secret/bankapp \
  jwt_security_key="${JWT_KEY}" \
  mssql_sa_password="${SQL_PASSWORD}" \
  rabbitmq_password="${RMQ_PASSWORD}"

echo "Seeded secret/bankapp (jwt_security_key, mssql_sa_password, rabbitmq_password)."
vault kv get secret/bankapp
