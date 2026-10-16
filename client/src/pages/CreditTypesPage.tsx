import { useEffect, useState } from 'react'
import { Navigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { getCreditTypes, type CreditTypeSortBy } from '../api/creditTypesApi'
import { useAuth } from '../auth/AuthContext'
import type { CreditTypeListItem, GetListResponse, LocaleCode } from '../types/api'
import { ApiError } from '../api/client'

const LOCALE_FILTERS: Array<LocaleCode | ''> = ['', 'tr', 'en', 'de']

export function CreditTypesPage() {
  const { t } = useTranslation()
  const { isStaff } = useAuth()
  const [data, setData] = useState<GetListResponse<CreditTypeListItem> | null>(null)
  const [localeFilter, setLocaleFilter] = useState<LocaleCode | ''>('')
  const [sortBy, setSortBy] = useState<CreditTypeSortBy>('name')
  const [sortDesc, setSortDesc] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!isStaff) return
    let cancelled = false
    setLoading(true)
    getCreditTypes(0, 50, {
      locale: localeFilter || undefined,
      sortBy,
      sortDesc,
    })
      .then((res) => {
        if (!cancelled) setData(res)
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : t('common.error'))
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [isStaff, t, localeFilter, sortBy, sortDesc])

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

  if (!isStaff) return <Navigate to="/applications" replace />

  return (
    <section>
      <div className="list-toolbar">
        <h1>{t('creditTypes.title')}</h1>
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
                  <button type="button" className="th-sort" onClick={() => toggleSort('currencyCode')}>
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
              </tr>
            </thead>
            <tbody>
              {data.items.map((item) => (
                <tr key={item.id}>
                  <td className="col-name">{item.name}</td>
                  <td className="col-code">{item.code}</td>
                  <td>{t(`locale.${item.locale}`, { defaultValue: item.locale })}</td>
                  <td>{t(`category.${item.category}`, { defaultValue: item.category })}</td>
                  <td>{item.currencyCode}</td>
                  <td>{t(`risk.${item.riskLevel}`, { defaultValue: item.riskLevel })}</td>
                  <td>{item.isActive ? t('common.yes') : t('common.no')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
