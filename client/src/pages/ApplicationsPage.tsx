import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'react-toastify'
import {
  approveApplication,
  cancelApplication,
  createApplication,
  getApplications,
  getPaymentPlan,
  rejectApplication,
} from '../api/applicationsApi'
import { getCreditSubTypes } from '../api/creditSubTypesApi'
import { getDashboardSummary } from '../api/dashboardApi'
import { useAuth } from '../auth/AuthContext'
import { DASHBOARD_REFRESH_EVENT } from '../components/StatsStrip'
import type {
  ApplicationSortBy,
  CreditApplicationListItem,
  CreditApplicationStatus,
  CreditSubTypeListItem,
  DashboardSummary,
  GetListResponse,
  PaymentPlanResponse,
} from '../types/api'
import { formatMoney } from '../utils/formatMoney'
import { mapApiError } from '../utils/mapApiError'

const STATUS_OPTIONS: Array<CreditApplicationStatus | ''> = [
  '',
  'Pending',
  'Approved',
  'Rejected',
  'Cancelled',
]

export function ApplicationsPage() {
  const { t, i18n } = useTranslation()
  const { isStaff, user } = useAuth()
  const locale = (i18n.language || 'tr').slice(0, 2)
  const [data, setData] = useState<GetListResponse<CreditApplicationListItem> | null>(null)
  const [pageIndex, setPageIndex] = useState(0)
  const [statusFilter, setStatusFilter] = useState<CreditApplicationStatus | ''>('')
  const [sortBy, setSortBy] = useState<ApplicationSortBy>('applicationDate')
  const [sortDesc, setSortDesc] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [subTypes, setSubTypes] = useState<CreditSubTypeListItem[]>([])
  const [creating, setCreating] = useState(false)
  const [cancellingId, setCancellingId] = useState<string | null>(null)
  const [decidingId, setDecidingId] = useState<string | null>(null)
  const [decidingAction, setDecidingAction] = useState<'approve' | 'reject' | null>(null)
  const [paymentPlan, setPaymentPlan] = useState<PaymentPlanResponse | null>(null)
  const [dashboard, setDashboard] = useState<DashboardSummary | null>(null)
  const [form, setForm] = useState({
    creditSubTypeId: '',
    requestedAmount: '',
    requestedTermMonths: '',
    purpose: '',
  })

  const loadDashboard = useCallback(() => {
    return getDashboardSummary()
      .then((res) => {
        setDashboard(res)
        window.dispatchEvent(new Event(DASHBOARD_REFRESH_EVENT))
      })
      .catch(() => setDashboard(null))
  }, [])

  const loadList = useCallback(() => {
    setLoading(true)
    setError(null)
    return getApplications({
      pageIndex,
      pageSize: isStaff ? 10 : 50,
      status: statusFilter,
      sortBy,
      sortDesc,
    })
      .then((res) => setData(res))
      .catch((err: unknown) => {
        const message = mapApiError(err, t)
        setError(message)
        toast.error(message)
      })
      .finally(() => setLoading(false))
  }, [pageIndex, isStaff, statusFilter, sortBy, sortDesc, t])

  useEffect(() => {
    let cancelled = false
    loadList().then(() => {
      if (cancelled) return
    })
    return () => {
      cancelled = true
    }
  }, [loadList])

  useEffect(() => {
    void loadDashboard()
  }, [loadDashboard])

  useEffect(() => {
    if (isStaff) return
    let cancelled = false
    getCreditSubTypes(0, 100, {
      locale,
      category: user?.customerCategory,
    })
      .then((res) => {
        if (cancelled) return
        const active = res.items.filter((s) => {
          if (!s.isActive || s.isDeleted) return false
          if (!user?.customerCategory) return true
          return s.category === user.customerCategory
        })
        setSubTypes(active)
        setForm((prev) => {
          const selected = active.find((s) => s.id === prev.creditSubTypeId) ?? active[0]
          if (!selected) {
            return { ...prev, creditSubTypeId: '', requestedAmount: '', requestedTermMonths: '' }
          }
          const amount =
            prev.creditSubTypeId === selected.id && prev.requestedAmount
              ? prev.requestedAmount
              : String(selected.amountOptions[0] ?? selected.minAmount)
          const term =
            prev.creditSubTypeId === selected.id && prev.requestedTermMonths
              ? prev.requestedTermMonths
              : String(selected.termOptions[0] ?? selected.minTermMonths)
          return {
            ...prev,
            creditSubTypeId: selected.id,
            requestedAmount: amount,
            requestedTermMonths: term,
          }
        })
      })
      .catch(() => {
        if (!cancelled) setSubTypes([])
      })
    return () => {
      cancelled = true
    }
  }, [isStaff, locale, user?.customerCategory])

  const selectedSubType = useMemo(
    () => subTypes.find((s) => s.id === form.creditSubTypeId),
    [subTypes, form.creditSubTypeId],
  )

  useEffect(() => {
    if (isStaff || !selectedSubType || !form.requestedAmount || !form.requestedTermMonths) {
      setPaymentPlan(null)
      return
    }
    let cancelled = false
    getPaymentPlan({
      creditSubTypeId: selectedSubType.id,
      amount: Number(form.requestedAmount),
      termMonths: Number(form.requestedTermMonths),
    })
      .then((plan) => {
        if (!cancelled) setPaymentPlan(plan)
      })
      .catch(() => {
        if (!cancelled) setPaymentPlan(null)
      })
    return () => {
      cancelled = true
    }
  }, [isStaff, selectedSubType, form.requestedAmount, form.requestedTermMonths])

  const welcomeName = user?.displayName || user?.email

  function toggleSort(column: ApplicationSortBy) {
    setPageIndex(0)
    if (sortBy === column) {
      setSortDesc((d) => !d)
      return
    }
    setSortBy(column)
    setSortDesc(column === 'applicationDate' || column === 'requestedAmount')
  }

  function sortIndicator(column: ApplicationSortBy) {
    if (sortBy !== column) return ''
    return sortDesc ? ' ↓' : ' ↑'
  }

  function onSubTypeChange(id: string) {
    const selected = subTypes.find((s) => s.id === id)
    setForm({
      creditSubTypeId: id,
      requestedAmount: String(selected?.amountOptions[0] ?? selected?.minAmount ?? ''),
      requestedTermMonths: String(selected?.termOptions[0] ?? selected?.minTermMonths ?? ''),
      purpose: form.purpose,
    })
  }

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    if (!form.creditSubTypeId) return
    setCreating(true)
    setError(null)
    setSuccess(null)
    const idempotencyKey = crypto.randomUUID()
    try {
      await createApplication(
        {
          creditSubTypeId: form.creditSubTypeId,
          requestedAmount: Number(form.requestedAmount),
          requestedTermMonths: Number(form.requestedTermMonths),
          purpose: form.purpose || null,
        },
        idempotencyKey,
      )
      const ok = t('applications.createSuccess')
      setSuccess(ok)
      toast.success(ok)
      setForm((prev) => ({ ...prev, purpose: '' }))
      setPageIndex(0)
      await loadList()
      await loadDashboard()
    } catch (err: unknown) {
      const message = mapApiError(err, t)
      setError(message)
      toast.error(message)
    } finally {
      setCreating(false)
    }
  }

  async function onCancel(id: string) {
    if (!window.confirm(t('applications.confirmCancel'))) return
    setCancellingId(id)
    setError(null)
    setSuccess(null)
    const idempotencyKey = crypto.randomUUID()
    try {
      await cancelApplication(id, undefined, idempotencyKey)
      const ok = t('applications.cancelSuccess')
      setSuccess(ok)
      toast.success(ok)
      await loadList()
      await loadDashboard()
    } catch (err: unknown) {
      const message = mapApiError(err, t)
      setError(message)
      toast.error(message)
    } finally {
      setCancellingId(null)
    }
  }

  async function onQuickApprove(item: CreditApplicationListItem) {
    if (!window.confirm(t('applications.confirmQuickApprove'))) return
    setDecidingId(item.id)
    setDecidingAction('approve')
    setError(null)
    setSuccess(null)
    const idempotencyKey = crypto.randomUUID()
    try {
      await approveApplication(
        item.id,
        {
          approvedAmount: item.requestedAmount,
          approvedTermMonths: item.requestedTermMonths,
          approvedInterestRate: item.interestRate,
          decisionBy: user?.email,
        },
        idempotencyKey,
      )
      const ok = t('applications.approveSuccess')
      setSuccess(ok)
      toast.success(ok)
      await loadList()
      await loadDashboard()
    } catch (err: unknown) {
      const message = mapApiError(err, t)
      setError(message)
      toast.error(message)
    } finally {
      setDecidingId(null)
      setDecidingAction(null)
    }
  }

  async function onQuickReject(item: CreditApplicationListItem) {
    const reason = window.prompt(t('applications.promptRejectReason'))
    if (reason == null) return
    const trimmed = reason.trim()
    if (!trimmed) {
      toast.error(t('applications.rejectReasonRequired'))
      return
    }
    if (!window.confirm(t('applications.confirmReject'))) return
    setDecidingId(item.id)
    setDecidingAction('reject')
    setError(null)
    setSuccess(null)
    const idempotencyKey = crypto.randomUUID()
    try {
      await rejectApplication(
        item.id,
        {
          rejectionReason: trimmed,
          decisionBy: user?.email,
        },
        idempotencyKey,
      )
      const ok = t('applications.rejectSuccess')
      setSuccess(ok)
      toast.success(ok)
      await loadList()
      await loadDashboard()
    } catch (err: unknown) {
      const message = mapApiError(err, t)
      setError(message)
      toast.error(message)
    } finally {
      setDecidingId(null)
      setDecidingAction(null)
    }
  }

  return (
    <section>
      <div className="welcome-panel">
        <h1>{t('dashboard.welcome', { name: welcomeName })}</h1>
        <p className="muted">
          {isStaff ? t('dashboard.subtitleStaff') : t('dashboard.subtitle')}
        </p>
        {dashboard && (
          <div className="summary-grid">
            {isStaff && (
              <>
                <article className="summary-card">
                  <span>{t('dashboard.customers')}</span>
                  <strong>{dashboard.customerTotal ?? 0}</strong>
                </article>
                <article className="summary-card">
                  <span>{t('dashboard.individualActive')}</span>
                  <strong>{dashboard.individualActive ?? 0}</strong>
                </article>
                <article className="summary-card">
                  <span>{t('dashboard.corporateActive')}</span>
                  <strong>{dashboard.corporateActive ?? 0}</strong>
                </article>
                <article className="summary-card">
                  <span>{t('dashboard.customerInactive')}</span>
                  <strong>{dashboard.customerInactive ?? 0}</strong>
                </article>
                <article className="summary-card">
                  <span>{t('dashboard.creditTypes')}</span>
                  <strong>{dashboard.creditTypeTotal ?? 0}</strong>
                </article>
                <article className="summary-card">
                  <span>{t('dashboard.creditTypeIndividual')}</span>
                  <strong>{dashboard.creditTypeIndividual ?? 0}</strong>
                </article>
                <article className="summary-card">
                  <span>{t('dashboard.creditTypeCorporate')}</span>
                  <strong>{dashboard.creditTypeCorporate ?? 0}</strong>
                </article>
              </>
            )}
            <article className="summary-card">
              <span>{t('dashboard.applications')}</span>
              <strong>{dashboard.applicationTotal}</strong>
            </article>
            <article className="summary-card">
              <span>{t('status.Pending')}</span>
              <strong>{dashboard.applicationPending}</strong>
            </article>
            <article className="summary-card">
              <span>{t('status.Approved')}</span>
              <strong>{dashboard.applicationApproved}</strong>
            </article>
            <article className="summary-card">
              <span>{t('status.Rejected')}</span>
              <strong>{dashboard.applicationRejected}</strong>
            </article>
            <article className="summary-card">
              <span>{t('status.Cancelled')}</span>
              <strong>{dashboard.applicationCancelled}</strong>
            </article>
          </div>
        )}
      </div>

      {!isStaff && (
        <div className="panel create-panel">
          <h2>{t('applications.newTitle')}</h2>
          <p className="muted">{t('applications.newHint')}</p>
          <form className="form" onSubmit={onCreate}>
            <label>
              {t('applications.creditType')}
              <select
                value={form.creditSubTypeId}
                onChange={(e) => onSubTypeChange(e.target.value)}
                required
              >
                {subTypes.length === 0 && (
                  <option value="">{t('common.empty')}</option>
                )}
                {subTypes.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name} · {formatMoney(s.minAmount, s.currencyCode, i18n.language)} –{' '}
                    {formatMoney(s.maxAmount, s.currencyCode, i18n.language)} · {s.minTermMonths}–
                    {s.maxTermMonths} {t('applications.termUnit')} · %{s.interestRate}
                  </option>
                ))}
              </select>
            </label>
            {selectedSubType && (
              <p className="muted form-rate-hint">
                {t('applications.interestRate')}: %{selectedSubType.interestRate}
              </p>
            )}
            <label>
              {t('applications.amount')}
              <select
                value={form.requestedAmount}
                onChange={(e) =>
                  setForm((prev) => ({ ...prev, requestedAmount: e.target.value }))
                }
                required
              >
                {(selectedSubType?.amountOptions ?? []).map((amount) => (
                  <option key={amount} value={amount}>
                    {formatMoney(amount, selectedSubType?.currencyCode, i18n.language)}
                  </option>
                ))}
              </select>
            </label>
            <label>
              {t('applications.term')}
              <select
                value={form.requestedTermMonths}
                onChange={(e) =>
                  setForm((prev) => ({ ...prev, requestedTermMonths: e.target.value }))
                }
                required
              >
                {(selectedSubType?.termOptions ?? []).map((term) => (
                  <option key={term} value={term}>
                    {term} {t('applications.termUnit')}
                  </option>
                ))}
              </select>
            </label>
            <label>
              {t('applications.purpose')}
              <input
                type="text"
                value={form.purpose}
                onChange={(e) =>
                  setForm((prev) => ({ ...prev, purpose: e.target.value }))
                }
              />
            </label>
            <button type="submit" className="btn primary" disabled={creating || !form.creditSubTypeId}>
              {creating ? t('common.loading') : t('applications.create')}
            </button>
          </form>

          {paymentPlan && paymentPlan.installments.length > 0 && (
            <div className="payment-plan">
              <h3>{t('paymentPlan.title')}</h3>
              <p className="muted">
                {t('paymentPlan.rateLine', { rate: paymentPlan.interestRate })}
              </p>
              <p className="muted">
                {t('paymentPlan.summary', {
                  monthly: formatMoney(
                    paymentPlan.monthlyPayment,
                    paymentPlan.currencyCode,
                    i18n.language,
                  ),
                  total: formatMoney(
                    paymentPlan.totalPayment,
                    paymentPlan.currencyCode,
                    i18n.language,
                  ),
                  interest: formatMoney(
                    paymentPlan.totalInterest,
                    paymentPlan.currencyCode,
                    i18n.language,
                  ),
                })}
              </p>
              <div className="table-wrap">
                <table>
                  <thead>
                    <tr>
                      <th>{t('paymentPlan.month')}</th>
                      <th>{t('paymentPlan.payment')}</th>
                      <th>{t('paymentPlan.principal')}</th>
                      <th>{t('paymentPlan.interest')}</th>
                      <th>{t('paymentPlan.balance')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {paymentPlan.installments.map((row) => (
                      <tr key={row.month}>
                        <td>{row.month}</td>
                        <td>{formatMoney(row.payment, paymentPlan.currencyCode, i18n.language)}</td>
                        <td>{formatMoney(row.principal, paymentPlan.currencyCode, i18n.language)}</td>
                        <td>{formatMoney(row.interest, paymentPlan.currencyCode, i18n.language)}</td>
                        <td>
                          {formatMoney(row.remainingBalance, paymentPlan.currencyCode, i18n.language)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      )}

      <div className="list-toolbar">
        <h1 className={isStaff ? undefined : 'section-title'}>{t('applications.title')}</h1>
        <label className="filter-label">
          {t('applications.status')}
          <select
            value={statusFilter}
            onChange={(e) => {
              setPageIndex(0)
              setStatusFilter(e.target.value as CreditApplicationStatus | '')
            }}
          >
            {STATUS_OPTIONS.map((s) => (
              <option key={s || 'all'} value={s}>
                {s ? t(`status.${s}`) : t('applications.allStatuses')}
              </option>
            ))}
          </select>
        </label>
      </div>

      {loading && <p>{t('common.loading')}</p>}
      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}
      {!loading && !error && data && data.items.length === 0 && (
        <div className="empty-state">
          <p>{t('dashboard.emptyTitle')}</p>
          <p className="muted">{t('dashboard.emptyHint')}</p>
        </div>
      )}
      {!loading && data && data.items.length > 0 && (
        <>
          <div className="table-wrap">
            <table className="apps-table">
              <thead>
                <tr>
                  <th className="col-number">
                    <button type="button" className="th-sort" onClick={() => toggleSort('applicationNumber')}>
                      {t('applications.number')}
                      {sortIndicator('applicationNumber')}
                    </button>
                  </th>
                  <th className="col-type">
                    <button type="button" className="th-sort" onClick={() => toggleSort('creditSubTypeName')}>
                      {t('applications.creditType')}
                      {sortIndicator('creditSubTypeName')}
                    </button>
                  </th>
                  <th className="col-amount">
                    <button type="button" className="th-sort" onClick={() => toggleSort('requestedAmount')}>
                      {t('applications.amount')}
                      {sortIndicator('requestedAmount')}
                    </button>
                  </th>
                  <th className="col-term">
                    <button type="button" className="th-sort" onClick={() => toggleSort('requestedTermMonths')}>
                      {t('applications.term')}
                      {sortIndicator('requestedTermMonths')}
                    </button>
                  </th>
                  <th className="col-rate">{t('applications.interestRate')}</th>
                  <th className="col-status">
                    <button type="button" className="th-sort" onClick={() => toggleSort('status')}>
                      {t('applications.status')}
                      {sortIndicator('status')}
                    </button>
                  </th>
                  <th className="col-date">
                    <button type="button" className="th-sort" onClick={() => toggleSort('applicationDate')}>
                      {t('applications.date')}
                      {sortIndicator('applicationDate')}
                    </button>
                  </th>
                  <th className="col-actions">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="col-number">{item.applicationNumber}</td>
                    <td className="col-type">{item.creditSubTypeName || '—'}</td>
                    <td className="col-amount">
                      {formatMoney(item.requestedAmount, item.currencyCode, i18n.language)}
                    </td>
                    <td className="col-term">{item.requestedTermMonths}</td>
                    <td className="col-rate">
                      %
                      {item.approvedInterestRate != null
                        ? item.approvedInterestRate
                        : item.interestRate}
                    </td>
                    <td className="col-status">
                      <span className={`badge status-${item.status.toLowerCase()}`}>
                        {t(`status.${item.status}`, { defaultValue: item.status })}
                      </span>
                    </td>
                    <td className="col-date">
                      {new Date(item.applicationDate).toLocaleString(i18n.language, {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </td>
                    <td className="actions-cell col-actions">
                      <Link to={`/applications/${item.id}`} className="btn info">
                        {t('common.details')}
                      </Link>
                      {isStaff && item.status === 'Pending' && (
                        <>
                          <button
                            type="button"
                            className="btn success"
                            disabled={decidingId === item.id}
                            title={t('applications.quickApproveHint')}
                            onClick={() => onQuickApprove(item)}
                          >
                            {decidingId === item.id && decidingAction === 'approve'
                              ? t('common.loading')
                              : t('applications.approve')}
                          </button>
                          <button
                            type="button"
                            className="btn reject"
                            disabled={decidingId === item.id}
                            onClick={() => onQuickReject(item)}
                          >
                            {decidingId === item.id && decidingAction === 'reject'
                              ? t('common.loading')
                              : t('applications.reject')}
                          </button>
                        </>
                      )}
                      {!isStaff && item.status === 'Pending' && (
                        <button
                          type="button"
                          className="btn danger"
                          disabled={cancellingId === item.id}
                          onClick={() => onCancel(item.id)}
                        >
                          {cancellingId === item.id
                            ? t('common.loading')
                            : t('applications.cancelApplication')}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {isStaff && (
            <div className="pager">
              <button
                type="button"
                className="btn ghost"
                disabled={!data.hasPrevious}
                onClick={() => setPageIndex((p) => Math.max(0, p - 1))}
              >
                {t('common.prev')}
              </button>
              <span>
                {t('common.page')} {data.index + 1} {t('common.of')} {Math.max(data.pages, 1)}
              </span>
              <button
                type="button"
                className="btn ghost"
                disabled={!data.hasNext}
                onClick={() => setPageIndex((p) => p + 1)}
              >
                {t('common.next')}
              </button>
            </div>
          )}
        </>
      )}
    </section>
  )
}
