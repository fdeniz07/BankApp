import { useCallback, useEffect, useState } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { getDashboardSummary } from '../api/dashboardApi'
import { useAuth } from '../auth/AuthContext'
import { DASHBOARD_REFRESH_EVENT } from '../components/StatsStrip'
import { CREDIT_APPLICATION_CHANGED_EVENT } from '../realtime/creditApplicationEvents'
import type { DashboardSummary } from '../types/api'

export function AdminDashboardPage() {
  const { t } = useTranslation()
  const { isManager, isSuperAdmin, user } = useAuth()
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const welcomeName = user?.displayName || user?.email || ''

  const load = useCallback(() => {
    setLoading(true)
    return getDashboardSummary()
      .then((data) => setSummary(data))
      .catch(() => setSummary(null))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  useEffect(() => {
    const onRefresh = () => void load()
    window.addEventListener(DASHBOARD_REFRESH_EVENT, onRefresh)
    window.addEventListener(CREDIT_APPLICATION_CHANGED_EVENT, onRefresh)
    return () => {
      window.removeEventListener(DASHBOARD_REFRESH_EVENT, onRefresh)
      window.removeEventListener(CREDIT_APPLICATION_CHANGED_EVENT, onRefresh)
    }
  }, [load])

  if (!isManager) return <Navigate to="/applications" replace />

  const kpis = summary
    ? [
        { label: t('dashboard.customers'), value: summary.customerTotal ?? 0 },
        { label: t('dashboard.individualActive'), value: summary.individualActive ?? 0 },
        { label: t('dashboard.corporateActive'), value: summary.corporateActive ?? 0 },
        { label: t('dashboard.creditTypes'), value: summary.creditTypeTotal ?? 0 },
        { label: t('dashboard.applications'), value: summary.applicationTotal },
        { label: t('status.Pending'), value: summary.applicationPending, highlight: true },
        { label: t('status.Approved'), value: summary.applicationApproved },
        { label: t('status.Rejected'), value: summary.applicationRejected },
        { label: t('status.Cancelled'), value: summary.applicationCancelled },
      ]
    : []

  return (
    <section className="mx-auto max-w-6xl space-y-8">
      <header className="relative overflow-hidden rounded-2xl border border-[var(--line)] bg-gradient-to-br from-slate-900 via-slate-800 to-teal-900 px-8 py-10 text-white shadow-lg">
        <p className="text-sm font-semibold tracking-[0.2em] text-teal-200 uppercase">
          {t('appName')}
        </p>
        <h1 className="mt-3 text-3xl font-semibold tracking-tight sm:text-4xl">
          {t('adminDashboard.welcome', { name: welcomeName })}
        </h1>
        <p className="mt-3 max-w-2xl text-base text-slate-200">
          {t('adminDashboard.subtitle')}
        </p>
      </header>

      {loading && <p className="text-[var(--ink)]">{t('common.loading')}</p>}

      {!loading && summary && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {kpis.map((kpi) => (
            <article
              key={kpi.label}
              className={`rounded-xl border border-[var(--line)] bg-[var(--surface)] px-5 py-4 shadow-sm ${
                kpi.highlight ? 'ring-2 ring-amber-400/70' : ''
              }`}
            >
              <span className="text-sm text-[var(--muted,#666)]">{kpi.label}</span>
              <strong className="mt-2 block text-3xl font-semibold text-[var(--ink)]">
                {kpi.value}
              </strong>
            </article>
          ))}
        </div>
      )}

      <div>
        <h2 className="mb-3 text-lg font-semibold text-[var(--ink)]">
          {t('adminDashboard.quickActions')}
        </h2>
        <div className="flex flex-wrap gap-3">
          <Link
            to="/applications?status=Pending"
            className="rounded-lg bg-amber-500 px-4 py-2.5 text-sm font-semibold text-slate-950 no-underline hover:bg-amber-400"
          >
            {t('adminDashboard.openPending', {
              count: summary?.applicationPending ?? 0,
            })}
          </Link>
          <Link
            to="/applications"
            className="rounded-lg border border-[var(--line)] bg-[var(--surface)] px-4 py-2.5 text-sm font-semibold text-[var(--ink)] no-underline hover:border-teal-600"
          >
            {t('nav.applications')}
          </Link>
          <Link
            to="/customers"
            className="rounded-lg border border-[var(--line)] bg-[var(--surface)] px-4 py-2.5 text-sm font-semibold text-[var(--ink)] no-underline hover:border-teal-600"
          >
            {t('nav.customers')}
          </Link>
          <Link
            to="/staff"
            className="rounded-lg border border-[var(--line)] bg-[var(--surface)] px-4 py-2.5 text-sm font-semibold text-[var(--ink)] no-underline hover:border-teal-600"
          >
            {t('nav.staff')}
          </Link>
          {isSuperAdmin && (
            <Link
              to="/branches"
              className="rounded-lg border border-[var(--line)] bg-[var(--surface)] px-4 py-2.5 text-sm font-semibold text-[var(--ink)] no-underline hover:border-teal-600"
            >
              {t('nav.branches')}
            </Link>
          )}
        </div>
      </div>
    </section>
  )
}
