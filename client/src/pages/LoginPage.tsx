import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/AuthContext'
import { getRememberedEmail } from '../auth/token'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { mapApiError } from '../utils/mapApiError'

const DEFAULT_EMAIL = 'staff@bank.local'

export function LoginPage() {
  const { t } = useTranslation()
  const { login, isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const remembered = getRememberedEmail()
  const [email, setEmail] = useState(remembered || DEFAULT_EMAIL)
  const [password, setPassword] = useState(remembered ? '' : 'Staff123!')
  const [rememberMe, setRememberMe] = useState(!!remembered)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  if (isAuthenticated) return <Navigate to="/applications" replace />

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
          <p>{t('login.hintStaff')}</p>
          <p>{t('login.hintManager')}</p>
          <p>{t('login.hintCustomer')}</p>
        </div>
      </div>
    </div>
  )
}
