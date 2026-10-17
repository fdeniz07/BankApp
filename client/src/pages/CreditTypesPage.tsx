import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Navigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'react-toastify'
import {
  createCreditType,
  getCreditTypes,
  setCreditTypeActive,
  type CreditTypeSortBy,
} from '../api/creditTypesApi'
import { useAuth } from '../auth/AuthContext'
import type {
  CreditCategory,
  CreditRiskLevel,
  CreditTypeListItem,
  GetListResponse,
  LocaleCode,
} from '../types/api'
import { mapApiError } from '../utils/mapApiError'

const LOCALE_FILTERS: Array<LocaleCode | ''> = ['', 'tr', 'en', 'de']
const CATEGORY_FILTERS: Array<CreditCategory | ''> = ['', 'Individual', 'Corporate']
const LOCALES: LocaleCode[] = ['tr', 'en', 'de']
const CATEGORIES: CreditCategory[] = ['Individual', 'Corporate']
const CURRENCIES = ['TRY', 'EUR', 'USD'] as const
const RISKS: CreditRiskLevel[] = ['Low', 'Medium', 'High']

const emptyCreate = {
  name: '',
  code: '',
  locale: 'tr' as LocaleCode,
  category: 'Individual' as CreditCategory,
  description: '',
  currencyCode: 'TRY' as (typeof CURRENCIES)[number],
  riskLevel: 'Medium' as CreditRiskLevel,
  requiresCollateral: false,
  requiresGuarantor: false,
}

export function CreditTypesPage() {
  const { t } = useTranslation()
  const { isStaff, isManager } = useAuth()
  const [data, setData] = useState<GetListResponse<CreditTypeListItem> | null>(null)
  const [localeFilter, setLocaleFilter] = useState<LocaleCode | ''>('')
  const [categoryFilter, setCategoryFilter] = useState<CreditCategory | ''>('')
  const [sortBy, setSortBy] = useState<CreditTypeSortBy>('name')
  const [sortDesc, setSortDesc] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [showCreate, setShowCreate] = useState(false)
  const [creating, setCreating] = useState(false)
  const [updatingId, setUpdatingId] = useState<string | null>(null)
  const [form, setForm] = useState(emptyCreate)

  const loadList = useCallback(() => {
    if (!isStaff) return Promise.resolve()
    setLoading(true)
    setError(null)
    return getCreditTypes(0, 50, {
      locale: localeFilter || undefined,
      category: categoryFilter || undefined,
      sortBy,
      sortDesc,
    })
      .then((res) => setData(res))
      .catch((err: unknown) => {
        setError(mapApiError(err, t))
        toast.error(mapApiError(err, t))
      })
      .finally(() => setLoading(false))
  }, [isStaff, t, localeFilter, categoryFilter, sortBy, sortDesc])

  useEffect(() => {
    void loadList()
  }, [loadList])

  function toggleSort(column: CreditTypeSortBy) {
    if (sortBy === column) {
      setSortDesc((d) => !d)
      return
    }
    setSortBy(column)
    setSortDesc(false)
  }

  function sortIndicator(column: CreditTypeSortBy) {
    if (sortBy !== column) return ''
    return sortDesc ? ' ↓' : ' ↑'
  }

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setCreating(true)
    try {
      await createCreditType({
        name: form.name.trim(),
        code: form.code.trim(),
        locale: form.locale,
        category: form.category,
        description: form.description.trim() || null,
        currencyCode: form.currencyCode,
        riskLevel: form.riskLevel,
        requiresCollateral: form.requiresCollateral,
        requiresGuarantor: form.requiresGuarantor,
      })
      toast.success(t('creditTypes.createSuccess'))
      setForm(emptyCreate)
      setShowCreate(false)
      await loadList()
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setCreating(false)
    }
  }

  async function onToggleActive(item: CreditTypeListItem) {
    const nextActive = !item.isActive
    if (
      !window.confirm(
        nextActive ? t('creditTypes.confirmActivate') : t('creditTypes.confirmDeactivate'),
      )
    ) {
      return
    }
    setUpdatingId(item.id)
    try {
      await setCreditTypeActive(item.id, nextActive)
      toast.success(t('creditTypes.updateSuccess'))
      await loadList()
    } catch (err) {
      toast.error(mapApiError(err, t))
    } finally {
      setUpdatingId(null)
    }
  }

  if (!isStaff) return <Navigate to="/applications" replace />

  const count = data?.count ?? 0
  const categoryLabel = categoryFilter
    ? t(`category.${categoryFilter}`)
    : t('creditTypes.allCategories')

  return (
    <section>
      <div className="list-toolbar">
        <h1>{t('creditTypes.title')}</h1>
        <div className="list-toolbar-actions">
          {isManager && (
            <button
              type="button"
              className="btn primary"
              onClick={() => setShowCreate((v) => !v)}
            >
              {showCreate ? t('common.cancel') : t('creditTypes.newType')}
            </button>
          )}
          <label className="filter-label">
            {t('creditTypes.locale')}
            <select
              value={localeFilter}
              onChange={(e) => setLocaleFilter(e.target.value as LocaleCode | '')}
            >
              {LOCALE_FILTERS.map((locale) => (
                <option key={locale || 'all'} value={locale}>
                  {locale ? t(`locale.${locale}`) : t('creditTypes.allLocales')}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      <p className="filter-count" aria-live="polite">
        {t('creditTypes.filteredCount', {
          category: categoryLabel,
          count,
        })}
      </p>

      {isManager && showCreate && (
        <form className="panel form" onSubmit={onCreate}>
          <h2>{t('creditTypes.newType')}</h2>
          <label>
            {t('creditTypes.name')}
            <input
              value={form.name}
              onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
              required
              maxLength={100}
            />
          </label>
          <label>
            {t('creditTypes.code')}
            <input
              value={form.code}
              onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))}
              required
              maxLength={50}
            />
          </label>
          <label>
            {t('creditTypes.locale')}
            <select
              value={form.locale}
              onChange={(e) => setForm((f) => ({ ...f, locale: e.target.value as LocaleCode }))}
            >
              {LOCALES.map((locale) => (
                <option key={locale} value={locale}>
                  {t(`locale.${locale}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('creditTypes.category')}
            <select
              value={form.category}
              onChange={(e) =>
                setForm((f) => ({ ...f, category: e.target.value as CreditCategory }))
              }
            >
              {CATEGORIES.map((c) => (
                <option key={c} value={c}>
                  {t(`category.${c}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('creditTypes.currency')}
            <select
              value={form.currencyCode}
              onChange={(e) =>
                setForm((f) => ({
                  ...f,
                  currencyCode: e.target.value as (typeof CURRENCIES)[number],
                }))
              }
            >
              {CURRENCIES.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('creditTypes.risk')}
            <select
              value={form.riskLevel}
              onChange={(e) =>
                setForm((f) => ({ ...f, riskLevel: e.target.value as CreditRiskLevel }))
              }
            >
              {RISKS.map((r) => (
                <option key={r} value={r}>
                  {t(`risk.${r}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('creditTypes.description')}
            <input
              value={form.description}
              onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
              maxLength={500}
            />
          </label>
          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={form.requiresCollateral}
              onChange={(e) =>
                setForm((f) => ({ ...f, requiresCollateral: e.target.checked }))
              }
            />
            {t('creditTypes.requiresCollateral')}
          </label>
          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={form.requiresGuarantor}
              onChange={(e) => setForm((f) => ({ ...f, requiresGuarantor: e.target.checked }))}
            />
            {t('creditTypes.requiresGuarantor')}
          </label>
          <button type="submit" className="btn success" disabled={creating}>
            {creating ? t('common.loading') : t('creditTypes.create')}
          </button>
        </form>
      )}

      <div className="chip-row" role="tablist" aria-label={t('creditTypes.category')}>
        {CATEGORY_FILTERS.map((c) => (
          <button
            key={c || 'all'}
            type="button"
            role="tab"
            className={`chip-btn${categoryFilter === c ? ' active' : ''}`}
            onClick={() => setCategoryFilter(c)}
          >
            {c ? t(`category.${c}`) : t('creditTypes.allCategories')}
          </button>
        ))}
      </div>

      {loading && <p>{t('common.loading')}</p>}
      {error && <p className="error-text">{error}</p>}
      {!loading && !error && data && data.items.length === 0 && (
        <p className="muted">{t('common.empty')}</p>
      )}
      {!loading && data && data.items.length > 0 && (
        <div className="table-wrap">
          <table className="credit-types-table">
            <thead>
              <tr>
                <th className="col-name">
                  <button type="button" className="th-sort" onClick={() => toggleSort('name')}>
                    {t('creditTypes.name')}
                    {sortIndicator('name')}
                  </button>
                </th>
                <th className="col-code">
                  <button type="button" className="th-sort" onClick={() => toggleSort('code')}>
                    {t('creditTypes.code')}
                    {sortIndicator('code')}
                  </button>
                </th>
                <th>
                  <button type="button" className="th-sort" onClick={() => toggleSort('locale')}>
                    {t('creditTypes.locale')}
                    {sortIndicator('locale')}
                  </button>
                </th>
                <th>
                  <button type="button" className="th-sort" onClick={() => toggleSort('category')}>
                    {t('creditTypes.category')}
                    {sortIndicator('category')}
                  </button>
                </th>
                <th>
                  <button
                    type="button"
                    className="th-sort"
                    onClick={() => toggleSort('currencyCode')}
                  >
                    {t('creditTypes.currency')}
                    {sortIndicator('currencyCode')}
                  </button>
                </th>
                <th>
                  <button type="button" className="th-sort" onClick={() => toggleSort('riskLevel')}>
                    {t('creditTypes.risk')}
                    {sortIndicator('riskLevel')}
                  </button>
                </th>
                <th>
                  <button type="button" className="th-sort" onClick={() => toggleSort('isActive')}>
                    {t('creditTypes.active')}
                    {sortIndicator('isActive')}
                  </button>
                </th>
                {isManager && <th className="col-actions">{t('common.actions')}</th>}
              </tr>
            </thead>
            <tbody>
              {data.items.map((item) => {
                const busy = updatingId === item.id
                const actionLabel = item.isActive
                  ? t('creditTypes.deactivateShort')
                  : t('creditTypes.activateShort')
                const actionTitle = item.isActive
                  ? t('creditTypes.deactivate')
                  : t('creditTypes.activate')
                return (
                  <tr key={item.id}>
                    <td className="col-name">{item.name}</td>
                    <td className="col-code">{item.code}</td>
                    <td>{t(`locale.${item.locale}`, { defaultValue: item.locale })}</td>
                    <td>{t(`category.${item.category}`, { defaultValue: item.category })}</td>
                    <td>{item.currencyCode}</td>
                    <td>{t(`risk.${item.riskLevel}`, { defaultValue: item.riskLevel })}</td>
                    <td>
                      <span
                        className={`badge ${item.isActive ? 'status-approved' : 'status-rejected'}`}
                      >
                        {item.isActive ? t('common.yes') : t('common.no')}
                      </span>
                    </td>
                    {isManager && (
                      <td className="actions-cell col-actions">
                        <button
                          type="button"
                          className={item.isActive ? 'btn reject' : 'btn success'}
                          disabled={busy}
                          title={actionTitle}
                          aria-label={actionTitle}
                          onClick={() => onToggleActive(item)}
                        >
                          {busy ? t('common.loading') : actionLabel}
                        </button>
                      </td>
                    )}
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
