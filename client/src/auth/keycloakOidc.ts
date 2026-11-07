import { UserManager, WebStorageStateStore, type User } from 'oidc-client-ts'

const KEYCLOAK_SESSION_KEY = 'bankapp.keycloakSession'

export function isKeycloakConfigured(): boolean {
  return Boolean(
    import.meta.env.VITE_KEYCLOAK_URL &&
      import.meta.env.VITE_KEYCLOAK_REALM &&
      import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
  )
}

function authority(): string {
  const base = String(import.meta.env.VITE_KEYCLOAK_URL ?? '').replace(/\/$/, '')
  const realm = String(import.meta.env.VITE_KEYCLOAK_REALM ?? '')
  return `${base}/realms/${realm}`
}

let manager: UserManager | null = null

function getManager(): UserManager {
  if (!isKeycloakConfigured()) {
    throw new Error('Keycloak is not configured (VITE_KEYCLOAK_*).')
  }

  if (!manager) {
    manager = new UserManager({
      authority: authority(),
      client_id: String(import.meta.env.VITE_KEYCLOAK_CLIENT_ID),
      redirect_uri: `${window.location.origin}/login/callback`,
      post_logout_redirect_uri: `${window.location.origin}/login`,
      response_type: 'code',
      scope: 'openid profile email',
      automaticSilentRenew: false,
      userStore: new WebStorageStateStore({ store: window.sessionStorage }),
    })
  }

  return manager
}

export function markKeycloakSession(active: boolean) {
  if (active) sessionStorage.setItem(KEYCLOAK_SESSION_KEY, '1')
  else sessionStorage.removeItem(KEYCLOAK_SESSION_KEY)
}

export function isKeycloakSession(): boolean {
  return sessionStorage.getItem(KEYCLOAK_SESSION_KEY) === '1'
}

export async function beginKeycloakLogin(): Promise<void> {
  await getManager().signinRedirect()
}

export async function completeKeycloakLogin(): Promise<{
  accessToken: string
  expiration: string
}> {
  const user: User = await getManager().signinRedirectCallback()
  if (!user.access_token) {
    throw new Error('Keycloak callback did not return an access_token.')
  }

  const expiresAtMs = user.expires_at ? user.expires_at * 1000 : Date.now() + 3600_000
  markKeycloakSession(true)

  return {
    accessToken: user.access_token,
    expiration: new Date(expiresAtMs).toISOString(),
  }
}

/** Local clear always; federated logout when session came from Keycloak. */
export async function endKeycloakSessionIfNeeded(): Promise<void> {
  if (!isKeycloakConfigured() || !isKeycloakSession()) {
    markKeycloakSession(false)
    return
  }

  markKeycloakSession(false)
  try {
    await getManager().signoutRedirect()
  } catch {
    // Fall through — local session already cleared by caller.
  }
}
