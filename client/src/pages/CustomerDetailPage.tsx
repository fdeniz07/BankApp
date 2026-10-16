import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'react-toastify'
import {
  deleteCorporateCustomer,
  deleteIndividualCustomer,
  getCorporateCustomer,
  getIndividualCustomer,
  updateCorporateCustomer,
  updateIndividualCustomer,
} from '../api/customersApi'
import { useAuth } from '../auth/AuthContext'
import type {
  CorporateCustomerDetail,
  Gender,
  IndividualCustomerDetail,
} from '../types/api'
import { mapApiError } from '../utils/mapApiError'

const GENDERS: Gender[] = ['Female', 'Male', 'PreferNotToSay']

export function CustomerDetailPage() {
  const { t } = useTranslation()
  const { isStaff, isManager } = useAuth()
  const navigate = useNavigate()
  const { kind, id } = useParams<{ kind: string; id: string }>()
  const isIndividual = kind === 'individual'
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [individual, setIndividual] = useState<IndividualCustomerDetail | null>(null)
  const [corporate, setCorporate] = useState<CorporateCustomerDetail | null>(null)

  useEffect(() => {
    if (!isStaff || !id) {
      setLoading(false)
      return
    }
    let cancelled = false
    setLoading(true)
    setError(null)
    const loader = isIndividual ? getIndividualCustomer(id) : getCorporateCustomer(id)
    loader
      .then((data) => {
        if (cancelled) return
        if (isIndividual) {
          setIndividual(data as IndividualCustomerDetail)
          setCorporate(null)
        } else {
          setCorporate(data as CorporateCustomerDetail)
          setIndividual(null)
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          const message = mapApiError(err, t)
          setError(message)
          toast.error(message)
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [id, isIndividual, isStaff, t])

  if (!isStaff) {
    return (
      <section>
        <h1>{t('customers.detailTitle')}</h1>
        <p className="muted">{t('customers.staffOnly')}</p>
      </section>
    )
  }

  async function onSave(e: FormEvent) {
    e.preventDefault()
    setSaving(true)
    try {
      if (individual) {
        const updated = await updateIndividualCustomer(individual)
        setIndividual(updated)
      } else if (corporate) {
        const updated = await updateCorporateCustomer(corporate)
        setCorporate(updated)
      }
      toast.success(t('customers.saveSuccess'))
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setSaving(false)
    }
  }

  async function onDelete() {
    const target = individual ?? corporate
    if (!target || !isManager) return

    if (target.isActive) {
      toast.error(t('errors.mustBeInactiveBeforeDelete'))
      return
    }

    if (!window.confirm(t('customers.confirmDelete'))) return

    setDeleting(true)
    try {
      if (individual) await deleteIndividualCustomer(individual.id)
      else if (corporate) await deleteCorporateCustomer(corporate.id)
      toast.success(t('customers.deleteSuccess'))
      navigate('/customers')
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setDeleting(false)
    }
  }

  const canDelete =
    isManager && !!((individual && !individual.isActive) || (corporate && !corporate.isActive))

  return (
    <section>
      <p>
        <Link to="/customers">{t('common.back')}</Link>
      </p>
      <h1>{t('customers.detailTitle')}</h1>
      {loading && <p>{t('common.loading')}</p>}
      {error && <p className="error-text">{error}</p>}

      {!loading && individual && (
        <form className="form create-panel" onSubmit={onSave}>
          <label>
            {t('customers.customerNumber')}
            <input
              value={individual.customerNumber}
              onChange={(e) =>
                setIndividual({ ...individual, customerNumber: e.target.value })
              }
              required
            />
          </label>
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
            {t('customers.nationalId')}
            <input
              value={individual.nationalId}
              onChange={(e) => setIndividual({ ...individual, nationalId: e.target.value })}
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
          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={individual.isActive}
              onChange={(e) => setIndividual({ ...individual, isActive: e.target.checked })}
            />
            {t('customers.active')}
          </label>
          <div className="action-row">
            <button type="submit" className="btn primary" disabled={saving}>
              {saving ? t('common.loading') : t('common.save')}
            </button>
            {isManager && (
              <button
                type="button"
                className="btn danger"
                disabled={deleting || !canDelete}
                title={
                  individual.isActive
                    ? t('errors.mustBeInactiveBeforeDelete')
                    : t('customers.deleteHint')
                }
                onClick={() => void onDelete()}
              >
                {deleting ? t('common.loading') : t('customers.delete')}
              </button>
            )}
          </div>
          {isManager && individual.isActive && (
            <p className="muted">{t('customers.deleteRequiresInactive')}</p>
          )}
          {isManager && !individual.isActive && (
            <p className="muted">{t('customers.deleteHint')}</p>
          )}
        </form>
      )}

      {!loading && corporate && (
        <form className="form create-panel" onSubmit={onSave}>
          <label>
            {t('customers.customerNumber')}
            <input
              value={corporate.customerNumber}
              onChange={(e) =>
                setCorporate({ ...corporate, customerNumber: e.target.value })
              }
              required
            />
          </label>
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
          <label>
            {t('customers.taxNumber')}
            <input
              value={corporate.taxNumber}
              onChange={(e) => setCorporate({ ...corporate, taxNumber: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.taxOffice')}
            <input
              value={corporate.taxOffice}
              onChange={(e) => setCorporate({ ...corporate, taxOffice: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.companyRegistrationNumber')}
            <input
              value={corporate.companyRegistrationNumber}
              onChange={(e) =>
                setCorporate({ ...corporate, companyRegistrationNumber: e.target.value })
              }
              required
            />
          </label>
          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={corporate.isActive}
              onChange={(e) => setCorporate({ ...corporate, isActive: e.target.checked })}
            />
            {t('customers.active')}
          </label>
          <div className="action-row">
            <button type="submit" className="btn primary" disabled={saving}>
              {saving ? t('common.loading') : t('common.save')}
            </button>
            {isManager && (
              <button
                type="button"
                className="btn danger"
                disabled={deleting || !canDelete}
                title={
                  corporate.isActive
                    ? t('errors.mustBeInactiveBeforeDelete')
                    : t('customers.deleteHint')
                }
                onClick={() => void onDelete()}
              >
                {deleting ? t('common.loading') : t('customers.delete')}
              </button>
            )}
          </div>
          {isManager && corporate.isActive && (
            <p className="muted">{t('customers.deleteRequiresInactive')}</p>
          )}
          {isManager && !corporate.isActive && (
            <p className="muted">{t('customers.deleteHint')}</p>
          )}
        </form>
      )}
    </section>
  )
}
