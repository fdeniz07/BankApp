import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'react-toastify'
import {
  createCorporateCustomer,
  createIndividualCustomer,
  getCorporateCustomers,
  getIndividualCustomers,
  type CustomerListScope,
} from '../api/customersApi'
import { useAuth } from '../auth/AuthContext'
import type {
  CorporateCustomerListItem,
  CreateCorporateCustomerRequest,
  CreateIndividualCustomerRequest,
  Gender,
  GetListResponse,
  IndividualCustomerListItem,
} from '../types/api'
import { mapApiError } from '../utils/mapApiError'

type CategoryTab = 'Individual' | 'Corporate'
const GENDERS: Gender[] = ['Female', 'Male', 'PreferNotToSay']
const SCOPES: CustomerListScope[] = ['active', 'inactive', 'deleted']

const emptyIndividual = (): CreateIndividualCustomerRequest => ({
  customerNumber: '',
  email: '',
  password: '',
  phoneNumber: '',
  address: '',
  firstName: '',
  lastName: '',
  nationalId: '',
  dateOfBirth: '',
  gender: 'PreferNotToSay',
  motherName: '',
  fatherName: '',
})

const emptyCorporate = (): CreateCorporateCustomerRequest => ({
  customerNumber: '',
  email: '',
  password: '',
  phoneNumber: '',
  address: '',
  companyName: '',
  taxNumber: '',
  taxOffice: '',
  companyRegistrationNumber: '',
  authorizedPersonName: '',
})

export function CustomersPage() {
  const { t } = useTranslation()
  const { isStaff } = useAuth()
  const [category, setCategory] = useState<CategoryTab>('Individual')
  const [scope, setScope] = useState<CustomerListScope>('active')
  const [pageIndex, setPageIndex] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const [creating, setCreating] = useState(false)
  const [individualData, setIndividualData] =
    useState<GetListResponse<IndividualCustomerListItem> | null>(null)
  const [corporateData, setCorporateData] =
    useState<GetListResponse<CorporateCustomerListItem> | null>(null)
  const [individualForm, setIndividualForm] = useState(emptyIndividual)
  const [corporateForm, setCorporateForm] = useState(emptyCorporate)

  const loadList = useCallback(() => {
    if (!isStaff) return Promise.resolve()
    setLoading(true)
    setError(null)
    const loader =
      category === 'Individual'
        ? getIndividualCustomers(scope, pageIndex, 10).then((res) => {
            setIndividualData(res)
            setCorporateData(null)
          })
        : getCorporateCustomers(scope, pageIndex, 10).then((res) => {
            setCorporateData(res)
            setIndividualData(null)
          })

    return loader
      .catch((err: unknown) => {
        const message = mapApiError(err, t)
        setError(message)
        toast.error(message)
      })
      .finally(() => setLoading(false))
  }, [category, scope, pageIndex, isStaff, t])

  useEffect(() => {
    void loadList()
  }, [loadList])

  if (!isStaff) {
    return (
      <section>
        <h1>{t('customers.title')}</h1>
        <p className="muted">{t('customers.staffOnly')}</p>
      </section>
    )
  }

  async function onCreateIndividual(e: FormEvent) {
    e.preventDefault()
    setCreating(true)
    try {
      await createIndividualCustomer({
        ...individualForm,
        dateOfBirth: individualForm.dateOfBirth || null,
        motherName: individualForm.motherName || null,
        fatherName: individualForm.fatherName || null,
      })
      toast.success(t('customers.createSuccess'))
      setIndividualForm(emptyIndividual())
      setShowCreate(false)
      setPageIndex(0)
      await loadList()
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setCreating(false)
    }
  }

  async function onCreateCorporate(e: FormEvent) {
    e.preventDefault()
    setCreating(true)
    try {
      await createCorporateCustomer(corporateForm)
      toast.success(t('customers.createSuccess'))
      setCorporateForm(emptyCorporate())
      setShowCreate(false)
      setPageIndex(0)
      await loadList()
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setCreating(false)
    }
  }

  const pages =
    category === 'Individual' ? (individualData?.pages ?? 0) : (corporateData?.pages ?? 0)
  const count =
    category === 'Individual' ? (individualData?.count ?? 0) : (corporateData?.count ?? 0)

  return (
    <section>
      <div className="page-header-row">
        <h1>{t('customers.title')}</h1>
        <button type="button" className="btn primary" onClick={() => setShowCreate((v) => !v)}>
          {showCreate ? t('common.cancel') : t('customers.newCustomer')}
        </button>
      </div>

      <p className="filter-count" aria-live="polite">
        {t('customers.filteredCount', {
          category: t(`category.${category}`),
          scope: t(`customers.scope_${scope}`),
          count,
        })}
      </p>

      <div className="chip-row" role="tablist" aria-label={t('customers.category')}>
        {(['Individual', 'Corporate'] as const).map((c) => (
          <button
            key={c}
            type="button"
            role="tab"
            className={`chip-btn${category === c ? ' active' : ''}`}
            onClick={() => {
              setCategory(c)
              setPageIndex(0)
              setShowCreate(false)
            }}
          >
            {t(`category.${c}`)}
          </button>
        ))}
      </div>

      <div className="chip-row" role="tablist" aria-label={t('customers.scope')}>
        {SCOPES.map((s) => (
          <button
            key={s}
            type="button"
            role="tab"
            className={`chip-btn${scope === s ? ' active' : ''}`}
            onClick={() => {
              setScope(s)
              setPageIndex(0)
            }}
          >
            {t(`customers.scope_${s}`)}
          </button>
        ))}
      </div>

      {showCreate && category === 'Individual' && (
        <form className="form create-panel" onSubmit={onCreateIndividual}>
          <h2>{t('customers.newIndividual')}</h2>
          <label>
            {t('customers.customerNumber')}
            <input
              value={individualForm.customerNumber}
              onChange={(e) =>
                setIndividualForm({ ...individualForm, customerNumber: e.target.value })
              }
              required
            />
          </label>
          <label>
            {t('profile.firstName')}
            <input
              value={individualForm.firstName}
              onChange={(e) => setIndividualForm({ ...individualForm, firstName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.lastName')}
            <input
              value={individualForm.lastName}
              onChange={(e) => setIndividualForm({ ...individualForm, lastName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={individualForm.email}
              onChange={(e) => setIndividualForm({ ...individualForm, email: e.target.value })}
              required
            />
          </label>
          <label>
            {t('login.password')}
            <input
              type="password"
              value={individualForm.password}
              onChange={(e) => setIndividualForm({ ...individualForm, password: e.target.value })}
              required
              minLength={6}
            />
          </label>
          <label>
            {t('profile.phone')}
            <input
              value={individualForm.phoneNumber}
              onChange={(e) =>
                setIndividualForm({ ...individualForm, phoneNumber: e.target.value })
              }
              required
            />
          </label>
          <label>
            {t('profile.address')}
            <input
              value={individualForm.address}
              onChange={(e) => setIndividualForm({ ...individualForm, address: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.nationalId')}
            <input
              value={individualForm.nationalId}
              onChange={(e) =>
                setIndividualForm({ ...individualForm, nationalId: e.target.value })
              }
              required
              minLength={11}
              maxLength={11}
            />
          </label>
          <label>
            {t('customers.gender')}
            <select
              value={individualForm.gender}
              onChange={(e) =>
                setIndividualForm({ ...individualForm, gender: e.target.value as Gender })
              }
              required
            >
              {GENDERS.map((g) => (
                <option key={g} value={g}>
                  {t(`gender.${g}`)}
                </option>
              ))}
            </select>
          </label>
          <button type="submit" className="btn primary" disabled={creating}>
            {creating ? t('common.loading') : t('customers.create')}
          </button>
        </form>
      )}

      {showCreate && category === 'Corporate' && (
        <form className="form create-panel" onSubmit={onCreateCorporate}>
          <h2>{t('customers.newCorporate')}</h2>
          <label>
            {t('customers.customerNumber')}
            <input
              value={corporateForm.customerNumber}
              onChange={(e) =>
                setCorporateForm({ ...corporateForm, customerNumber: e.target.value })
              }
              required
            />
          </label>
          <label>
            {t('profile.companyName')}
            <input
              value={corporateForm.companyName}
              onChange={(e) => setCorporateForm({ ...corporateForm, companyName: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.authorizedPerson')}
            <input
              value={corporateForm.authorizedPersonName}
              onChange={(e) =>
                setCorporateForm({ ...corporateForm, authorizedPersonName: e.target.value })
              }
              required
            />
          </label>
          <label>
            {t('login.email')}
            <input
              type="email"
              value={corporateForm.email}
              onChange={(e) => setCorporateForm({ ...corporateForm, email: e.target.value })}
              required
            />
          </label>
          <label>
            {t('login.password')}
            <input
              type="password"
              value={corporateForm.password}
              onChange={(e) => setCorporateForm({ ...corporateForm, password: e.target.value })}
              required
              minLength={6}
            />
          </label>
          <label>
            {t('profile.phone')}
            <input
              value={corporateForm.phoneNumber}
              onChange={(e) => setCorporateForm({ ...corporateForm, phoneNumber: e.target.value })}
              required
            />
          </label>
          <label>
            {t('profile.address')}
            <input
              value={corporateForm.address}
              onChange={(e) => setCorporateForm({ ...corporateForm, address: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.taxNumber')}
            <input
              value={corporateForm.taxNumber}
              onChange={(e) => setCorporateForm({ ...corporateForm, taxNumber: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.taxOffice')}
            <input
              value={corporateForm.taxOffice}
              onChange={(e) => setCorporateForm({ ...corporateForm, taxOffice: e.target.value })}
              required
            />
          </label>
          <label>
            {t('customers.companyRegistrationNumber')}
            <input
              value={corporateForm.companyRegistrationNumber}
              onChange={(e) =>
                setCorporateForm({
                  ...corporateForm,
                  companyRegistrationNumber: e.target.value,
                })
              }
              required
            />
          </label>
          <button type="submit" className="btn primary" disabled={creating}>
            {creating ? t('common.loading') : t('customers.create')}
          </button>
        </form>
      )}

      {loading && <p>{t('common.loading')}</p>}
      {error && <p className="error-text">{error}</p>}

      {!loading && !error && (
        <>
          <div className="table-wrap">
            <table className="apps-table">
              <thead>
                <tr>
                  <th>{t('customers.customerNumber')}</th>
                  <th>{t('customers.name')}</th>
                  <th>{t('login.email')}</th>
                  <th>{t('profile.phone')}</th>
                  <th>{t('customers.active')}</th>
                  <th>{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {category === 'Individual' &&
                  (individualData?.items ?? []).map((row) => (
                    <tr key={row.id}>
                      <td>{row.customerNumber}</td>
                      <td>
                        {row.firstName} {row.lastName}
                      </td>
                      <td>{row.email}</td>
                      <td>{row.phoneNumber}</td>
                      <td>{row.isActive ? t('common.yes') : t('common.no')}</td>
                      <td>
                        <Link
                          className="btn info"
                          to={`/customers/individual/${row.id}`}
                        >
                          {t('common.details')}
                        </Link>
                      </td>
                    </tr>
                  ))}
                {category === 'Corporate' &&
                  (corporateData?.items ?? []).map((row) => (
                    <tr key={row.id}>
                      <td>{row.customerNumber}</td>
                      <td>{row.companyName}</td>
                      <td>{row.email}</td>
                      <td>{row.phoneNumber}</td>
                      <td>{row.isActive ? t('common.yes') : t('common.no')}</td>
                      <td>
                        <Link
                          className="btn info"
                          to={`/customers/corporate/${row.id}`}
                        >
                          {t('common.details')}
                        </Link>
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
          {count === 0 && <p className="muted">{t('common.empty')}</p>}
          <div className="pager">
            <button
              type="button"
              className="btn ghost"
              disabled={pageIndex <= 0}
              onClick={() => setPageIndex((p) => Math.max(0, p - 1))}
            >
              {t('common.prev')}
            </button>
            <span>
              {t('common.page')} {pageIndex + 1} {t('common.of')} {Math.max(pages, 1)}
            </span>
            <button
              type="button"
              className="btn ghost"
              disabled={pageIndex + 1 >= pages}
              onClick={() => setPageIndex((p) => p + 1)}
            >
              {t('common.next')}
            </button>
          </div>
        </>
      )}
    </section>
  )
}
