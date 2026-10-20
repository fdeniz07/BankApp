import { useState, type FormEvent } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/AuthContext'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import type { Gender } from '../types/api'
import { mapApiError } from '../utils/mapApiError'

const GENDERS: Gender[] = ['Female', 'Male', 'PreferNotToSay']

export function RegisterPage() {
  const { t, i18n } = useTranslation()
  const { registerIndividual, isAuthenticated } = useAuth()
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [doneEmail, setDoneEmail] = useState<string | null>(null)
  const [form, setForm] = useState({
    email: '',
    password: '',
    phoneNumber: '',
    address: '',
    firstName: '',
    lastName: '',
    nationalId: '',
    dateOfBirth: '',
    gender: 'PreferNotToSay' as Gender,
    motherName: '',
    fatherName: '',
  })

  if (isAuthenticated) return <Navigate to="/applications" replace />

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const response = await registerIndividual({
        email: form.email,
        password: form.password,
        phoneNumber: form.phoneNumber,
        address: form.address,
        firstName: form.firstName,
        lastName: form.lastName,
        nationalId: form.nationalId,
        dateOfBirth: form.dateOfBirth || null,
        gender: form.gender,
        motherName: form.motherName || null,
        fatherName: form.fatherName || null,
        locale: (i18n.language?.slice(0, 2) as 'tr' | 'en' | 'de') || 'tr',
      })
      setDoneEmail(response.email)
    } catch (err) {
      setError(mapApiError(err, t))
    } finally {
      setLoading(false)
    }
  }

  if (doneEmail) {
    return (
      <div className="login-page">
        <div className="login-card">
          <div className="login-header">
            <h1>{t('appName')}</h1>
            <LanguageSwitcher />
          </div>
          <h2>{t('register.checkEmailTitle')}</h2>
          <p className="success-text">{t('register.checkEmailBody', { email: doneEmail })}</p>
          <p className="muted">{t('register.checkEmailHint')}</p>
          <p className="muted register-footer">
            <Link to="/login">{t('register.goLogin')}</Link>
          </p>
        </div>
      </div>
    )
  }

  return (
    <div className="login-page">
      <div className="login-card register-card">
        <div className="login-header">
          <h1>{t('appName')}</h1>
          <LanguageSwitcher />
        </div>
        <h2>{t('register.title')}</h2>
        <p className="muted">{t('register.subtitle')}</p>
        <form onSubmit={onSubmit} className="form">
          <label>
            {t('profile.firstName')}
            <input
              value={form.firstName}
              onChange={(e) => setForm({ ...form, firstName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.lastName')}
            <input
              value={form.lastName}
              onChange={(e) => setForm({ ...form, lastName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              required
              autoComplete="email"
            />
          </label>
          <label>
            {t('login.password')}
            <input
              type="password"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              required
              minLength={6}
              autoComplete="new-password"
            />
          </label>
          <label>
            {t('profile.phone')}
            <input
              value={form.phoneNumber}
              onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.address')}
            <input
              value={form.address}
              onChange={(e) => setForm({ ...form, address: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.nationalId')}
            <input
              value={form.nationalId}
              onChange={(e) => setForm({ ...form, nationalId: e.target.value })}
              required
              minLength={11}
              maxLength={11}
            />
          </label>
          <label>
            {t('customers.dateOfBirth')}
            <input
              type="date"
              value={form.dateOfBirth}
              onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })}
            />
          </label>
          <label>
            {t('customers.gender')}
            <select
              value={form.gender}
              onChange={(e) => setForm({ ...form, gender: e.target.value as Gender })}
              required
            >
              {GENDERS.map((g) => (
                <option key={g} value={g}>
                  {t(`gender.${g}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('customers.motherName')}
            <input
              value={form.motherName}
              onChange={(e) => setForm({ ...form, motherName: e.target.value })}
            />
          </label>
          <label>
            {t('customers.fatherName')}
            <input
              value={form.fatherName}
              onChange={(e) => setForm({ ...form, fatherName: e.target.value })}
            />
          </label>
          {error && <p className="error-text">{error}</p>}
          <button type="submit" className="btn primary" disabled={loading}>
            {loading ? t('common.loading') : t('register.submit')}
          </button>
        </form>
        <p className="muted register-footer">
          {t('register.hasAccount')}{' '}
          <Link to="/login">{t('register.goLogin')}</Link>
        </p>
      </div>
    </div>
  )
}
