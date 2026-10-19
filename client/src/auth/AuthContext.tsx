import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { login as loginApi, registerIndividual as registerIndividualApi } from '../api/authApi'
import { getMyProfile, resolveProfileImageUrl } from '../api/profileApi'
import type { RegisterIndividualRequest, RegisteredIndividualResponse } from '../types/api'
import {
  clearSession,
  getAuthUser,
  hasRole,
  readProfileOverride,
  saveProfileOverride,
  saveSession,
  setRememberedEmail,
  type AuthUser,
  type CustomerCategory,
} from './token'

type ProfilePatch = {
  displayName?: string
  customerCategory?: CustomerCategory
  profileImageUrl?: string | null
}

interface AuthContextValue {
  user: AuthUser | null
  isAuthenticated: boolean
  isStaff: boolean
  isManager: boolean
  login: (email: string, password: string, rememberMe?: boolean) => Promise<void>
  registerIndividual: (body: RegisterIndividualRequest) => Promise<RegisteredIndividualResponse>
  logout: () => void
  updateProfileLocal: (patch: ProfilePatch) => void
}

function applySession(
  response: {
    accessToken: { token: string; expiration: string }
    displayName?: string | null
    customerCategory?: string | null
  },
  persist: boolean,
) {
  saveSession(response.accessToken.token, response.accessToken.expiration, persist)

  const category =
    response.customerCategory === 'Individual' || response.customerCategory === 'Corporate'
      ? response.customerCategory
      : undefined
  if (response.displayName || category) {
    saveProfileOverride({
      displayName: response.displayName ?? undefined,
      customerCategory: category,
    })
  }
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => getAuthUser())

  const refreshAvatar = useCallback(async () => {
    try {
      const mine = await getMyProfile()
      const url = resolveProfileImageUrl(mine.profileImageUrl)
      const displayName =
        `${mine.firstName ?? ''} ${mine.lastName ?? ''}`.trim() || mine.email
      setUser((current) => {
        if (!current) return current
        const next: AuthUser = {
          ...current,
          displayName: displayName || current.displayName,
          profileImageUrl: url ?? undefined,
        }
        const prev = readProfileOverride()
        saveProfileOverride({
          displayName: next.displayName,
          customerCategory: next.customerCategory ?? prev?.customerCategory,
          profileImageUrl: next.profileImageUrl,
        })
        return next
      })
    } catch {
      // ignore — header still works without avatar
    }
  }, [])

  useEffect(() => {
    if (!user?.userId) return
    void refreshAvatar()
    // only on userId change / login
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user?.userId])

  const login = useCallback(async (email: string, password: string, rememberMe = false) => {
    const response = await loginApi(email, password, rememberMe)
    applySession(response, rememberMe)
    setRememberedEmail(rememberMe ? email.trim() : null)
    setUser(getAuthUser())
  }, [])

  const registerIndividual = useCallback(async (body: RegisterIndividualRequest) => {
    const response = await registerIndividualApi(body)
    // Registration no longer issues a JWT; caller shows confirm-email UX.
    return response
  }, [])

  const logout = useCallback(() => {
    clearSession()
    setUser(null)
  }, [])

  const updateProfileLocal = useCallback((patch: ProfilePatch) => {
    setUser((current) => {
      if (!current) return current
      const next: AuthUser = {
        ...current,
        displayName: patch.displayName ?? current.displayName,
        customerCategory: patch.customerCategory ?? current.customerCategory,
        profileImageUrl:
          patch.profileImageUrl === null
            ? undefined
            : (patch.profileImageUrl ?? current.profileImageUrl),
      }
      saveProfileOverride({
        displayName: next.displayName,
        customerCategory: next.customerCategory,
        profileImageUrl: next.profileImageUrl,
      })
      return next
    })
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: !!user,
      isStaff: hasRole(user, 'BankStaff'),
      isManager: hasRole(user, 'BankManager'),
      login,
      registerIndividual,
      logout,
      updateProfileLocal,
    }),
    [user, login, registerIndividual, logout, updateProfileLocal],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
