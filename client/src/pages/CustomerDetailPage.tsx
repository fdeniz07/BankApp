import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'react-toastify'
import {
  deleteCorporateCustomer,
  deleteIndividualCustomer,
  getCorporateCustomer,
  getCorporateCustomerCatalog,
  getIndividualCustomer,
  updateCorporateCustomer,
  updateIndividualCustomer,
} from '../api/customersApi'
import { useAuth } from '../auth/AuthContext'
import { SuggestInput } from '../components/SuggestInput'
import type {
  CorporateCustomerCatalog,
  CorporateCustomerDetail,
  Gender,
  IndividualCustomerDetail,
} from '../types/api'
import { mapApiError } from '../utils/mapApiError'

const GENDERS: Gender[] = ['Female', 'Male', 'PreferNotToSay']

const emptyCatalog = (): CorporateCustomerCatalog => ({
  companyTypes: [],
  companySectors: [],
  companyIndustries: [],
  companySubIndustries: [],
})

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
  const [catalog, setCatalog] = useState<CorporateCustomerCatalog>(emptyCatalog)

  useEffect(() => {
    if (!isStaff || !id) {
      setLoading(false)
      return
    }
    let cancelled = false
    setLoading(true)
    setError(null)
    const loader = isIndividual
      ? getIndividualCustomer(id).then((data) => {
          if (cancelled) return
          setIndividual(data)
          setCorporate(null)
        })
      : Promise.all([getCorporateCustomer(id), getCorporateCustomerCatalog()]).then(
          ([data, cat]) => {
            if (cancelled) return
            setCorporate(data)
            setIndividual(null)
            setCatalog(cat)
          },
        )
    loader
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
        try {
          setCatalog(await getCorporateCustomerCatalog())
        } catch {
          // keep previous suggestions
        }
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
            <input value={individual.customerNumber} readOnly disabled />
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
              minLength={11}
              maxLength={11}
            />
          </label>
          <label>
            {t('customers.dateOfBirth')}
            <input
              type="date"
              value={individual.dateOfBirth?.slice(0, 10) || ''}
              onChange={(e) =>
                setIndividual({ ...individual, dateOfBirth: e.target.value || null })
              }
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
          <label>
            {t('customers.motherName')}
            <input
              value={individual.motherName || ''}
              onChange={(e) =>
                setIndividual({ ...individual, motherName: e.target.value || null })
              }
            />
          </label>
          <label>
            {t('customers.fatherName')}
            <input
              value={individual.fatherName || ''}
              onChange={(e) =>
                setIndividual({ ...individual, fatherName: e.target.value || null })
              }
            />
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
            <input value={corporate.customerNumber} readOnly disabled />
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
          <label>
            {t('customers.companyEmail')}
            <input
              type="email"
              value={corporate.companyEmail || ''}
              onChange={(e) =>
                setCorporate({ ...corporate, companyEmail: e.target.value || null })
              }
            />
          </label>
          <label>
            {t('customers.companyAddress')}
            <input
              value={corporate.companyAddress || ''}
              onChange={(e) =>
                setCorporate({ ...corporate, companyAddress: e.target.value || null })
              }
            />
          </label>
          <p className="muted">{t('customers.suggestHint')}</p>
          <label>
            {t('customers.companyType')}
            <SuggestInput
              id="detail-companyType"
              value={corporate.companyType || ''}
              options={catalog.companyTypes}
              onChange={(v) => setCorporate({ ...corporate, companyType: v || null })}
            />
          </label>
          <label>
            {t('customers.companySector')}
            <SuggestInput
              id="detail-companySector"
              value={corporate.companySector || ''}
              options={catalog.companySectors}
              onChange={(v) => setCorporate({ ...corporate, companySector: v || null })}
            />
          </label>
          <label>
            {t('customers.companyIndustry')}
            <SuggestInput
              id="detail-companyIndustry"
              value={corporate.companyIndustry || ''}
              options={catalog.companyIndustries}
              onChange={(v) => setCorporate({ ...corporate, companyIndustry: v || null })}
            />
          </label>
          <label>
            {t('customers.companySubIndustry')}
            <SuggestInput
              id="detail-companySubIndustry"
              value={corporate.companySubIndustry || ''}
              options={catalog.companySubIndustries}
              onChange={(v) => setCorporate({ ...corporate, companySubIndustry: v || null })}
            />
          </label>
          <label>
            {t('customers.companyFoundationDate')}
            <input
              type="date"
              value={corporate.companyFoundationDate?.slice(0, 10) || ''}
              onChange={(e) =>
                setCorporate({
                  ...corporate,
                  companyFoundationDate: e.target.value
                    ? `${e.target.value}T00:00:00.000Z`
                    : null,
                })
              }
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
