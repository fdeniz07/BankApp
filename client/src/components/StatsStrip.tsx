import { useCallback, useEffect, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { getDashboardSummary } from '../api/dashboardApi'
import { useAuth } from '../auth/AuthContext'
import type { DashboardSummary } from '../types/api'

export const DASHBOARD_REFRESH_EVENT = 'bankapp:dashboard-refresh'

export function StatsStrip() {
  const { t } = useTranslation()
  const { isAuthenticated, isStaff } = useAuth()
  const location = useLocation()
  const [summary, setSummary] = useState<DashboardSummary | null>(null)

  const load = useCallback(() => {
    if (!isAuthenticated) {
      setSummary(null)
      return
    }
    getDashboardSummary()
      .then((data) => setSummary(data))
      .catch(() => setSummary(null))
  }, [isAuthenticated])

  useEffect(() => {
    load()
  }, [load, location.pathname])

  useEffect(() => {
    const onRefresh = () => load()
    window.addEventListener(DASHBOARD_REFRESH_EVENT, onRefresh)
    return () => window.removeEventListener(DASHBOARD_REFRESH_EVENT, onRefresh)
  }, [load])

  if (!summary) return null

  return (
    <div className="stats-strip" aria-label={t('dashboard.statsLabel')}>
      {isStaff && (
        <div className="stats-chip">
          <span>{t('dashboard.customers')}</span>
          <strong>{summary.customerTotal ?? 0}</strong>
        </div>
      )}
      <div className="stats-chip">
        <span>{t('dashboard.applications')}</span>
        <strong>{summary.applicationTotal}</strong>
      </div>
      <div className="stats-chip">
        <span>{t('status.Pending')}</span>
        <strong>{summary.applicationPending}</strong>
      </div>
    </div>
  )
}
