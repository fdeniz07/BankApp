#!/bin/sh
# Lab: rotate JWT HMAC key in Vault KV secret/bankapp/jwt with a dual-key window.
# - jwt_security_key          → new active key (signs new tokens)
# - jwt_security_key_previous → former active key (still accepted for validation)
# Does not touch database / messaging / observability paths.
#
# Usage (from host, Vault healthy):
#   docker compose --env-file .env exec -e NEW_JWT_KEY='...' vault /bin/sh /vault-rotate-jwt.sh
set -eu

VAULT_ADDR="${VAULT_ADDR:-http://127.0.0.1:8200}"
export VAULT_ADDR
export VAULT_TOKEN="${VAULT_TOKEN:?VAULT_TOKEN is required}"

JWT_PATH="secret/bankapp/jwt"

NEW_JWT_KEY="${NEW_JWT_KEY:-}"
if [ -z "${NEW_JWT_KEY}" ]; then
  # Portable-ish 64+ char key for lab (not cryptographically perfect — lab only).
  NEW_JWT_KEY="BankApp-Rotated-Jwt-Key-$(date +%s)-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"
fi

key_len=$(printf '%s' "${NEW_JWT_KEY}" | wc -c | tr -d ' ')
if [ "${key_len}" -lt 64 ]; then
  echo "NEW_JWT_KEY must be at least 64 characters (got ${key_len})." >&2
  exit 1
fi

if ! vault status >/dev/null 2>&1; then
  echo "Vault is not ready at ${VAULT_ADDR}" >&2
  exit 1
fi

CURRENT="$(vault kv get -field=jwt_security_key "${JWT_PATH}" 2>/dev/null || true)"

if [ -z "${CURRENT}" ]; then
  echo "${JWT_PATH} has no jwt_security_key — run vault-seed first." >&2
  exit 1
fi

vault kv put "${JWT_PATH}" \
  jwt_security_key="${NEW_JWT_KEY}" \
  jwt_security_key_previous="${CURRENT}"

echo "Rotated JWT key at ${JWT_PATH} (previous kept for validation window)."
vault kv get "${JWT_PATH}"
echo "Next: recreate WebApi so it reloads Vault overlay:"
echo "  docker compose --env-file .env up -d --force-recreate --no-deps webapi"
