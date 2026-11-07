import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/AuthContext'
import { getAuthUser } from '../auth/token'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { homePath } from '../utils/homePath'

export function LoginCallbackPage() {
  const { t } = useTranslation()
  const { completeKeycloakLogin } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      try {
        await completeKeycloakLogin()
        if (cancelled) return
        const user = getAuthUser()
        const manager =
          !!user?.roles.some((r) => r === 'BankManager' || r === 'SuperAdmin')
        navigate(homePath(manager), { replace: true })
      } catch {
        if (!cancelled) setError(t('login.keycloakError'))
      }
    })()
    return () => {
      cancelled = true
    }
  }, [completeKeycloakLogin, navigate, t])

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <h1>{t('appName')}</h1>
          <LanguageSwitcher />
        </div>
        <h2>{t('login.keycloakCallbackTitle')}</h2>
        {error ? (
          <>
            <p className="error-text">{error}</p>
            <button type="button" className="btn primary" onClick={() => navigate('/login')}>
              {t('login.goBack')}
            </button>
          </>
        ) : (
          <p className="muted">{t('common.loading')}</p>
        )}
      </div>
    </div>
  )
}
