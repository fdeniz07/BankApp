import { useEffect, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import {
  getCorporateCustomer,
  getIndividualCustomer,
  updateCorporateCustomer,
  updateIndividualCustomer,
} from '../api/customersApi'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import type { CorporateCustomerDetail, Gender, IndividualCustomerDetail } from '../types/api'

const GENDERS: Gender[] = ['Female', 'Male', 'PreferNotToSay']

export function ProfilePage() {
  const { t } = useTranslation()
  const { user, isStaff, updateProfileLocal } = useAuth()
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)
  const [individual, setIndividual] = useState<IndividualCustomerDetail | null>(null)
  const [corporate, setCorporate] = useState<CorporateCustomerDetail | null>(null)

  useEffect(() => {
    if (!user?.customerId || isStaff) {
      setLoading(false)
      return
    }

    let cancelled = false
    setLoading(true)
    setError(null)

    const load = async () => {
      try {
        if (user.customerCategory === 'Corporate') {
          const data = await getCorporateCustomer(user.customerId!)
          if (!cancelled) {
            setCorporate(data)
            setIndividual(null)
          }
          return
        }

        try {
          const data = await getIndividualCustomer(user.customerId!)
          if (!cancelled) {
            setIndividual(data)
            setCorporate(null)
            if (!user.customerCategory) {
              updateProfileLocal({ customerCategory: 'Individual' })
            }
          }
        } catch {
          const data = await getCorporateCustomer(user.customerId!)
          if (!cancelled) {
            setCorporate(data)
            setIndividual(null)
            updateProfileLocal({ customerCategory: 'Corporate' })
          }
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : t('common.error'))
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [user?.customerId, user?.customerCategory, isStaff, t, updateProfileLocal])

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    if (!user?.customerId) return
    setSaving(true)
    setError(null)
    setMessage(null)
    try {
      if (individual) {
        const updated = await updateIndividualCustomer(individual)
        setIndividual(updated)
        updateProfileLocal({
          displayName: `${updated.firstName} ${updated.lastName}`.trim(),
          customerCategory: 'Individual',
        })
      } else if (corporate) {
        const updated = await updateCorporateCustomer(corporate)
        setCorporate(updated)
        updateProfileLocal({
          displayName: updated.companyName,
          customerCategory: 'Corporate',
        })
      }
      setMessage(t('profile.saveSuccess'))
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'))
    } finally {
      setSaving(false)
    }
  }

  if (isStaff || !user?.customerId) {
    return (
      <section>
        <h1>{t('profile.title')}</h1>
        <dl className="info-grid">
          <div>
            <dt>{t('profile.displayName')}</dt>
            <dd>{user?.displayName || user?.email}</dd>
          </div>
          <div>
            <dt>{t('login.email')}</dt>
            <dd>{user?.email}</dd>
          </div>
          <div>
            <dt>{t('profile.role')}</dt>
            <dd>{user?.roles.map((r) => t(`roles.${r}`, { defaultValue: r })).join(', ')}</dd>
          </div>
        </dl>
        <p className="muted">{t('profile.staffHint')}</p>
      </section>
    )
  }

  return (
    <section>
      <h1>{t('profile.title')}</h1>
      {user.customerCategory && (
        <p className="muted">
          {t('profile.category')}:{' '}
          {t(`category.${user.customerCategory}`, { defaultValue: user.customerCategory })}
        </p>
      )}
      {loading && <p>{t('common.loading')}</p>}
      {error && <p className="error-text">{error}</p>}
      {message && <p className="success-text">{message}</p>}

      {!loading && individual && (
        <form className="form create-panel" onSubmit={onSubmit}>
          <label>
            {t('profile.firstName')}
            <input
              value={individual.firstName}
              onChange={(e) => setIndividual({ ...individual, firstName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.lastName')}
            <input
              value={individual.lastName}
              onChange={(e) => setIndividual({ ...individual, lastName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={individual.email}
              onChange={(e) => setIndividual({ ...individual, email: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.phone')}
            <input
              value={individual.phoneNumber}
              onChange={(e) => setIndividual({ ...individual, phoneNumber: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.address')}
            <input
              value={individual.address}
              onChange={(e) => setIndividual({ ...individual, address: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.gender')}
            <select
              value={individual.gender || ''}
              onChange={(e) =>
                setIndividual({
                  ...individual,
                  gender: e.target.value || null,
                })
              }
            >
              <option value="">{t('customers.genderUnset')}</option>
              {GENDERS.map((g) => (
                <option key={g} value={g}>
                  {t(`gender.${g}`)}
                </option>
              ))}
            </select>
          </label>
          <button type="submit" className="btn" disabled={saving}>
            {saving ? t('common.loading') : t('common.save')}
          </button>
        </form>
      )}

      {!loading && corporate && (
        <form className="form create-panel" onSubmit={onSubmit}>
          <label>
            {t('profile.companyName')}
            <input
              value={corporate.companyName}
              onChange={(e) => setCorporate({ ...corporate, companyName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.authorizedPerson')}
            <input
              value={corporate.authorizedPersonName}
              onChange={(e) =>
                setCorporate({ ...corporate, authorizedPersonName: e.target.value })
              }
              required
            />
          </label>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={corporate.email}
              onChange={(e) => setCorporate({ ...corporate, email: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.phone')}
            <input
              value={corporate.phoneNumber}
              onChange={(e) => setCorporate({ ...corporate, phoneNumber: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.address')}
            <input
              value={corporate.address}
              onChange={(e) => setCorporate({ ...corporate, address: e.target.value })}
              required
            />
          </label>
          <button type="submit" className="btn" disabled={saving}>
            {saving ? t('common.loading') : t('common.save')}
          </button>
        </form>
      )}
    </section>
  )
}
