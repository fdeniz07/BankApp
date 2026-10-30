import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/AuthContext'
import { getRememberedEmail } from '../auth/token'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { mapApiError } from '../utils/mapApiError'

/** Unified lab password — matches IdentitySeed / BranchLocaleMap.DemoPassword */
const DEMO_PASSWORD = 'Demo123!'
const DEFAULT_EMAIL = 'staff.tr@bank.local'

const DEMO_ACCOUNTS = [
  { role: 'staff' as const, locale: 'TR', email: 'staff.tr@bank.local' },
  { role: 'staff' as const, locale: 'DE', email: 'staff.de@bank.local' },
  { role: 'staff' as const, locale: 'EN', email: 'staff.en@bank.local' },
  { role: 'manager' as const, locale: 'TR', email: 'manager.tr@bank.local' },
  { role: 'manager' as const, locale: 'DE', email: 'manager.de@bank.local' },
  { role: 'manager' as const, locale: 'EN', email: 'manager.en@bank.local' },
  { role: 'customer' as const, locale: 'TR', email: 'customer.tr@bank.local' },
  { role: 'customer' as const, locale: 'DE', email: 'customer.de@bank.local' },
  { role: 'customer' as const, locale: 'EN', email: 'customer.en@bank.local' },
]

export function LoginPage() {
  const { t } = useTranslation()
  const { login, isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const remembered = getRememberedEmail()
  const [email, setEmail] = useState(remembered || DEFAULT_EMAIL)
  const [password, setPassword] = useState(remembered ? '' : DEMO_PASSWORD)
  const [rememberMe, setRememberMe] = useState(!!remembered)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  if (isAuthenticated) return <Navigate to="/applications" replace />

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
      navigate('/applications', { replace: true })
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

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <h1>{t('appName')}</h1>
          <LanguageSwitcher />
        </div>
        <h2>{t('login.title')}</h2>
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
          <button type="submit" className="btn primary" disabled={loading}>
            {loading ? t('common.loading') : t('login.submit')}
          </button>
        </form>
        <p className="muted register-footer">
          {t('login.noAccount')}{' '}
          <Link to="/register">{t('login.goRegister')}</Link>
        </p>
        <div className="hints">
          <p className="login-demo-password">{t('login.hintPassword')}</p>
          <p className="login-demo-caption">{t('login.demoFill')}</p>
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
          <p>{t('login.hintStaff')}</p>
          <p>{t('login.hintManager')}</p>
          <p>{t('login.hintCustomer')}</p>
          <p>{t('login.hintSuperAdmin')}</p>
        </div>
      </div>
    </div>
  )
}
