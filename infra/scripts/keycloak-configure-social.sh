#!/bin/sh
# Lab: register Google / GitHub Keycloak identity providers when secrets are set.
# Empty client id/secret → skip that IdP.
#
# From host (Keycloak healthy):
#   docker compose --env-file .env exec -e KEYCLOAK_GOOGLE_CLIENT_ID=... -e KEYCLOAK_GOOGLE_CLIENT_SECRET=... \
#     -e KEYCLOAK_GITHUB_CLIENT_ID=... -e KEYCLOAK_GITHUB_CLIENT_SECRET=... \
#     keycloak /bin/sh /keycloak-configure-social.sh
set -eu

KC_HOME="${KC_HOME:-/opt/keycloak}"
KCADM="${KC_HOME}/bin/kcadm.sh"
SERVER="${KEYCLOAK_INTERNAL_URL:-http://127.0.0.1:8080}"
ADMIN_USER="${KEYCLOAK_ADMIN_USER:-admin}"
ADMIN_PASSWORD="${KEYCLOAK_ADMIN_PASSWORD:-admin}"
REALM="${KEYCLOAK_REALM:-bankapp}"

GOOGLE_ID="${KEYCLOAK_GOOGLE_CLIENT_ID:-}"
GOOGLE_SECRET="${KEYCLOAK_GOOGLE_CLIENT_SECRET:-}"
GITHUB_ID="${KEYCLOAK_GITHUB_CLIENT_ID:-}"
GITHUB_SECRET="${KEYCLOAK_GITHUB_CLIENT_SECRET:-}"

"${KCADM}" config credentials \
  --server "${SERVER}" \
  --realm master \
  --user "${ADMIN_USER}" \
  --password "${ADMIN_PASSWORD}"

upsert_idp() {
  alias="$1"
  provider_id="$2"
  client_id="$3"
  client_secret="$4"

  if [ -z "${client_id}" ] || [ -z "${client_secret}" ]; then
    echo "Skip ${alias}: client id/secret not set."
    return 0
  fi

  if "${KCADM}" get "identity-provider/instances/${alias}" -r "${REALM}" >/dev/null 2>&1; then
    "${KCADM}" update "identity-provider/instances/${alias}" -r "${REALM}" \
      -s enabled=true \
      -s "config.clientId=${client_id}" \
      -s "config.clientSecret=${client_secret}" \
      -s "config.syncMode=IMPORT" \
      -s trustEmail=true
    echo "Updated IdP ${alias}."
  else
    "${KCADM}" create identity-provider/instances -r "${REALM}" \
      -s "alias=${alias}" \
      -s "providerId=${provider_id}" \
      -s enabled=true \
      -s trustEmail=true \
      -s "config.clientId=${client_id}" \
      -s "config.clientSecret=${client_secret}" \
      -s "config.syncMode=IMPORT"
    echo "Created IdP ${alias}."
  fi
}

upsert_idp "google" "google" "${GOOGLE_ID}" "${GOOGLE_SECRET}"
upsert_idp "github" "github" "${GITHUB_ID}" "${GITHUB_SECRET}"

echo "Done. Social buttons appear on Keycloak hosted login when IdPs are enabled."
echo "SPA still uses the single Keycloak button (broker choice is on Keycloak UI)."
