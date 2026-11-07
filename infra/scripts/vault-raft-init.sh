#!/bin/sh
# Initialize vault-raft-1, unseal, enable KV (lab TLS-off).
#   docker compose -f docker-compose.vault-raft.yml exec vault-raft-1 /bin/sh /vault-raft-init.sh
set -eu

export VAULT_ADDR="${VAULT_ADDR:-http://127.0.0.1:8200}"
INIT_FILE="${INIT_FILE:-/tmp/raft-init.json}"

parse_init() {
  UNSEAL="$(sed -n '/"unseal_keys_b64"/,/]/p' "${INIT_FILE}" | sed -n 's/.*"\([^"]*=\)".*/\1/p' | head -n 1)"
  ROOT="$(sed -n 's/.*"root_token"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "${INIT_FILE}" | head -n 1)"
}

if vault status 2>/dev/null | grep -q 'Initialized.*true'; then
  if [ -f "${INIT_FILE}" ]; then
    parse_init
    if [ -n "${UNSEAL}" ]; then
      vault operator unseal "${UNSEAL}" >/dev/null || true
    fi
    echo "Already initialized. Unseal attempted from ${INIT_FILE}."
    echo "Root token: ${ROOT:-"(see ${INIT_FILE})"}"
    vault status || true
    exit 0
  fi
  echo "Already initialized; no ${INIT_FILE} — unseal peers manually (learning 19)."
  vault status || true
  exit 0
fi

echo "Initializing Raft leader (vault-raft-1)..."
vault operator init -key-shares=1 -key-threshold=1 -format=json > "${INIT_FILE}"
parse_init

if [ -z "${UNSEAL}" ] || [ -z "${ROOT}" ]; then
  echo "Failed to parse init output at ${INIT_FILE}" >&2
  cat "${INIT_FILE}" >&2 || true
  exit 1
fi

vault operator unseal "${UNSEAL}"
export VAULT_TOKEN="${ROOT}"

echo "Root token (lab only — store securely): ${ROOT}"
echo "Unseal key: ${UNSEAL}"

vault secrets enable -path=secret kv-v2 2>/dev/null || true

echo "Join peers from vault-raft-2 / vault-raft-3:"
echo "  vault operator raft join http://vault-raft-1:8200"
echo "  vault operator unseal <UNSEAL>"
echo "Then on leader: vault operator raft list-peers"
