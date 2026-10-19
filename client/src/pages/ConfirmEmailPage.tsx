import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { confirmEmail } from '../api/authApi'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { mapApiError } from '../utils/mapApiError'

type Status = 'pending' | 'ok' | 'error'

export function ConfirmEmailPage() {
  const { t } = useTranslation()
  const [params] = useSearchParams()
  const token = params.get('token')?.trim() ?? ''
  const [status, setStatus] = useState<Status>('pending')
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!token) {
      setStatus('error')
      setError(t('confirmEmail.missingToken'))
      return
    }

    let cancelled = false
    ;(async () => {
      try {
        await confirmEmail(token)
        if (!cancelled) setStatus('ok')
      } catch (err) {
        if (!cancelled) {
          setStatus('error')
          setError(mapApiError(err, t))
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [token, t])

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <h1>{t('appName')}</h1>
          <LanguageSwitcher />
        </div>
        <h2>{t('confirmEmail.title')}</h2>
        {status === 'pending' && <p className="muted">{t('confirmEmail.pending')}</p>}
        {status === 'ok' && (
          <>
            <p className="success-text">{t('confirmEmail.success')}</p>
            <p className="muted register-footer">
              <Link to="/login">{t('confirmEmail.goLogin')}</Link>
            </p>
          </>
        )}
        {status === 'error' && (
          <>
            <p className="error-text">{error ?? t('common.error')}</p>
            <p className="muted register-footer">
              <Link to="/login">{t('confirmEmail.goLogin')}</Link>
            </p>
          </>
        )}
      </div>
    </div>
  )
}
