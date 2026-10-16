import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { login as loginApi, registerIndividual as registerIndividualApi } from '../api/authApi'
import type { RegisterIndividualRequest } from '../types/api'
import {
  clearSession,
  getAuthUser,
  hasRole,
  saveProfileOverride,
  saveSession,
  type AuthUser,
  type CustomerCategory,
} from './token'

interface AuthContextValue {
  user: AuthUser | null
  isAuthenticated: boolean
  isStaff: boolean
  isManager: boolean
  login: (email: string, password: string) => Promise<void>
  registerIndividual: (body: RegisterIndividualRequest) => Promise<void>
  logout: () => void
  updateProfileLocal: (patch: { displayName?: string; customerCategory?: CustomerCategory }) => void
}

function applySession(response: {
  accessToken: { token: string; expiration: string }
  displayName?: string | null
  customerCategory?: string | null
}) {
  saveSession(response.accessToken.token, response.accessToken.expiration)

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

  const login = useCallback(async (email: string, password: string) => {
    const response = await loginApi(email, password)
    applySession(response)
    setUser(getAuthUser())
  }, [])

  const registerIndividual = useCallback(async (body: RegisterIndividualRequest) => {
    const response = await registerIndividualApi(body)
    applySession(response)
    setUser(getAuthUser())
  }, [])

  const logout = useCallback(() => {
    clearSession()
    setUser(null)
  }, [])

  const updateProfileLocal = useCallback(
    (patch: { displayName?: string; customerCategory?: CustomerCategory }) => {
      setUser((current) => {
        if (!current) return current
        const next: AuthUser = {
          ...current,
          displayName: patch.displayName ?? current.displayName,
          customerCategory: patch.customerCategory ?? current.customerCategory,
        }
        saveProfileOverride({
          displayName: next.displayName,
          customerCategory: next.customerCategory,
        })
        return next
      })
    },
    [],
  )

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
