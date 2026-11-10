import { useEffect, useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/AuthContext'
import { getAuthUser, getRememberedEmail } from '../auth/token'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { homePath } from '../utils/homePath'
import { mapApiError } from '../utils/mapApiError'

function isManagerUser(): boolean {
  const u = getAuthUser()
  return !!u?.roles.some((r) => r === 'BankManager' || r === 'SuperAdmin')
}

/** Unified lab password — matches IdentitySeed / BranchLocaleMap.DemoPassword */
const DEMO_PASSWORD = 'Demo123!'
const DEFAULT_EMAIL = 'staff.tr@bank.local'
const DESKTOP_MQ = '(min-width: 768px)'

const DEMO_ACCOUNTS = [
  { role: 'staff' as const, locale: 'IST', email: 'staff.tr@bank.local' },
  { role: 'staff' as const, locale: 'ANK', email: 'staff.ank@bank.local' },
  { role: 'staff' as const, locale: 'BER', email: 'staff.de@bank.local' },
  { role: 'staff' as const, locale: 'NYC', email: 'staff.nyc@bank.local' },
  { role: 'staff' as const, locale: 'CHI', email: 'staff.chi@bank.local' },
  { role: 'manager' as const, locale: 'IST', email: 'manager.ist@bank.local' },
  { role: 'manager' as const, locale: 'ANK', email: 'manager.tr@bank.local' },
  { role: 'manager' as const, locale: 'BER', email: 'manager.ber@bank.local' },
  { role: 'manager' as const, locale: 'NYC', email: 'manager.nyc@bank.local' },
  { role: 'manager' as const, locale: 'CHI', email: 'manager.chi@bank.local' },
  { role: 'customer' as const, locale: 'TR', email: 'customer.tr@bank.local' },
  { role: 'customer' as const, locale: 'DE', email: 'customer.de@bank.local' },
  { role: 'customer' as const, locale: 'EN', email: 'customer.en@bank.local' },
]

export function LoginPage() {
  const { t } = useTranslation()
  const { login, loginWithKeycloak, keycloakEnabled, isAuthenticated, isManager } = useAuth()
  const navigate = useNavigate()
  const remembered = getRememberedEmail()
  const [email, setEmail] = useState(remembered || DEFAULT_EMAIL)
  const [password, setPassword] = useState(remembered ? '' : DEMO_PASSWORD)
  const [rememberMe, setRememberMe] = useState(!!remembered)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [keycloakLoading, setKeycloakLoading] = useState(false)
  const [labOpen, setLabOpen] = useState(() =>
    typeof window !== 'undefined' ? window.matchMedia(DESKTOP_MQ).matches : true,
  )

  useEffect(() => {
    const mq = window.matchMedia(DESKTOP_MQ)
    const sync = () => setLabOpen(mq.matches)
    sync()
    mq.addEventListener('change', sync)
    return () => mq.removeEventListener('change', sync)
  }, [])

  if (isAuthenticated) return <Navigate to={homePath(isManager)} replace />

  function fillDemo(demoEmail: string) {
    setEmail(demoEmail)
    setPassword(DEMO_PASSWORD)
    setError(null)
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await login(email, password, rememberMe)
      navigate(homePath(isManagerUser()), { replace: true })
    } catch (err) {
      const mapped = mapApiError(err, t)
      setError(mapped === t('common.error') ? t('login.error') : mapped)
    } finally {
      setLoading(false)
    }
  }

  const roleLabel = (role: (typeof DEMO_ACCOUNTS)[number]['role']) => {
    if (role === 'staff') return t('login.demoStaff')
    if (role === 'manager') return t('login.demoManager')
    return t('login.demoCustomer')
  }

  const labPanel = (
    <>
      <p className="login-demo-password">{t('login.hintPassword')}</p>
      <p className="login-demo-caption muted">{t('login.demoFill')}</p>
      <div className="login-demo-grid" role="group" aria-label={t('login.demoFill')}>
        {DEMO_ACCOUNTS.map((account) => (
          <button
            key={account.email}
            type="button"
            className="btn ghost login-demo-chip"
            onClick={() => fillDemo(account.email)}
            title={account.email}
          >
            {roleLabel(account.role)} {account.locale}
          </button>
        ))}
        <button
          type="button"
          className="btn ghost login-demo-chip"
          onClick={() => fillDemo('superadmin@bank.local')}
          title="superadmin@bank.local"
        >
          SuperAdmin
        </button>
      </div>
      <details className="login-hint-details">
        <summary>{t('login.labEmails')}</summary>
        <div className="login-hint-list">
          <p>{t('login.hintStaff')}</p>
          <p>{t('login.hintManager')}</p>
          <p>{t('login.hintCustomer')}</p>
          <p>{t('login.hintSuperAdmin')}</p>
          {keycloakEnabled && <p>{t('login.hintKeycloak')}</p>}
        </div>
      </details>
    </>
  )

  return (
    <div className="login-page">
      <div className="login-shell">
        <div className="login-header">
          <h1>{t('appName')}</h1>
          <LanguageSwitcher />
        </div>

        <div className="login-body">
          <section className="login-main" aria-labelledby="login-heading">
            <h2 id="login-heading">{t('login.title')}</h2>
            <p className="muted">{t('login.subtitle')}</p>
            <form onSubmit={onSubmit} className="form">
              <label>
                {t('login.email')}
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  autoComplete="username"
                />
              </label>
              <label>
                {t('login.password')}
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  autoComplete="current-password"
                />
              </label>
              <label className="checkbox-row">
                <input
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                />
                <span>{t('login.rememberMe')}</span>
              </label>
              {error && <p className="error-text">{error}</p>}
              <button type="submit" className="btn primary" disabled={loading || keycloakLoading}>
                {loading ? t('common.loading') : t('login.submit')}
              </button>
            </form>
            {keycloakEnabled && (
              <div className="login-keycloak">
                <p className="muted login-or">{t('login.or')}</p>
                <button
                  type="button"
                  className="btn ghost login-keycloak-btn"
                  disabled={loading || keycloakLoading}
                  onClick={async () => {
                    setError(null)
                    setKeycloakLoading(true)
                    try {
                      await loginWithKeycloak()
                    } catch {
                      setError(t('login.keycloakError'))
                      setKeycloakLoading(false)
                    }
                  }}
                >
                  {keycloakLoading ? t('common.loading') : t('login.keycloak')}
                </button>
              </div>
            )}
            <p className="muted register-footer">
              {t('login.noAccount')}{' '}
              <Link to="/register">{t('login.goRegister')}</Link>
            </p>
          </section>

          <aside className="login-aside" aria-label={t('login.labTitle')}>
            <details
              className="login-lab-panel"
              open={labOpen}
              onToggle={(e) => setLabOpen((e.target as HTMLDetailsElement).open)}
            >
              <summary className="login-lab-summary">{t('login.labTitle')}</summary>
              <div className="login-lab-body">{labPanel}</div>
            </details>
          </aside>
        </div>
      </div>
    </div>
  )
}
