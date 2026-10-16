const TOKEN_KEY = 'bankapp.token'
const EXP_KEY = 'bankapp.tokenExp'
const PROFILE_OVERRIDE_KEY = 'bankapp.profileOverride'

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const NAME_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/nameidentifier'
const EMAIL_CLAIM = 'email'
const CUSTOMER_CLAIM = 'customerId'
const DISPLAY_NAME_CLAIM = 'displayName'
const CUSTOMER_CATEGORY_CLAIM = 'customerCategory'

export type CustomerCategory = 'Individual' | 'Corporate'

export interface AuthUser {
  userId: string
  email: string
  displayName?: string
  roles: string[]
  customerId?: string
  customerCategory?: CustomerCategory
}

interface ProfileOverride {
  displayName?: string
  customerCategory?: CustomerCategory
}

function decodeBase64UrlUtf8(value: string): string {
  const base64 = value.replace(/-/g, '+').replace(/_/g, '/')
  const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4)
  const binary = atob(padded)
  const bytes = Uint8Array.from(binary, (ch) => ch.charCodeAt(0))
  return new TextDecoder('utf-8').decode(bytes)
}

function decodePayload(token: string): Record<string, unknown> | null {
  try {
    const part = token.split('.')[1]
    if (!part) return null
    const json = decodeBase64UrlUtf8(part)
    return JSON.parse(json) as Record<string, unknown>
  } catch {
    return null
  }
}

function claimValues(payload: Record<string, unknown>, key: string): string[] {
  const value = payload[key]
  if (typeof value === 'string') return [value]
  if (Array.isArray(value)) return value.filter((v): v is string => typeof v === 'string')
  return []
}

function readProfileOverride(): ProfileOverride | null {
  try {
    const raw = localStorage.getItem(PROFILE_OVERRIDE_KEY)
    if (!raw) return null
    return JSON.parse(raw) as ProfileOverride
  } catch {
    return null
  }
}

export function saveProfileOverride(override: ProfileOverride) {
  localStorage.setItem(PROFILE_OVERRIDE_KEY, JSON.stringify(override))
}

export function clearProfileOverride() {
  localStorage.removeItem(PROFILE_OVERRIDE_KEY)
}

export function saveSession(token: string, expiration: string) {
  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(EXP_KEY, expiration)
  clearProfileOverride()
}

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(EXP_KEY)
  clearProfileOverride()
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function getAuthUser(): AuthUser | null {
  const token = getToken()
  if (!token) return null

  const payload = decodePayload(token)
  if (!payload) return null

  const exp = typeof payload.exp === 'number' ? payload.exp : 0
  if (exp * 1000 < Date.now()) {
    clearSession()
    return null
  }

  const email = claimValues(payload, EMAIL_CLAIM)[0] ?? ''
  const userId = claimValues(payload, NAME_CLAIM)[0] ?? ''
  const roles = claimValues(payload, ROLE_CLAIM)
  const customerId = claimValues(payload, CUSTOMER_CLAIM)[0]
  const displayName = claimValues(payload, DISPLAY_NAME_CLAIM)[0]
  const categoryRaw = claimValues(payload, CUSTOMER_CATEGORY_CLAIM)[0]
  const customerCategory =
    categoryRaw === 'Individual' || categoryRaw === 'Corporate' ? categoryRaw : undefined

  const override = readProfileOverride()

  return {
    userId,
    email,
    displayName: override?.displayName || displayName || undefined,
    roles,
    customerId,
    customerCategory: override?.customerCategory || customerCategory,
  }
}

export function hasRole(user: AuthUser | null, role: string): boolean {
  return !!user?.roles.includes(role)
}
