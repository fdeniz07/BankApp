import { Link, NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/AuthContext'
import { useCreditApplicationRealtime } from '../realtime/useCreditApplicationRealtime'
import { FxTicker } from './FxTicker'
import { LanguageSwitcher } from './LanguageSwitcher'
import { StatsStrip } from './StatsStrip'

export function AppLayout() {
  const { t } = useTranslation()
  const { user, isStaff, isManager, logout } = useAuth()
  useCreditApplicationRealtime()
  const initial = (user?.displayName || user?.email || '?').slice(0, 1).toUpperCase()

  return (
    <div className="app-shell">
      <FxTicker />
      <header className="topbar">
        <div className="brand">{t('appName')}</div>
        <nav className="nav">
          <NavLink to="/applications">{t('nav.applications')}</NavLink>
          {isStaff && <NavLink to="/customers">{t('nav.customers')}</NavLink>}
          {isStaff && <NavLink to="/credit-types">{t('nav.creditTypes')}</NavLink>}
          {isManager && <NavLink to="/staff">{t('nav.staff')}</NavLink>}
          <NavLink to="/profile">{t('nav.profile')}</NavLink>
        </nav>
        <div className="topbar-right">
          <LanguageSwitcher />
          <Link to="/profile" className="user-profile-link" title={t('nav.profile')}>
            <div className="avatar-preview avatar-sm topbar-avatar">
              {user?.profileImageUrl ? (
                <img src={user.profileImageUrl} alt="" />
              ) : (
                <span className="avatar-fallback">{initial}</span>
              )}
            </div>
            <div className="user-meta">
              <span className="user-display-name">{user?.displayName || user?.email}</span>
              <span className="role-chip">
                {user?.roles.map((r) => t(`roles.${r}`, { defaultValue: r })).join(', ')}
              </span>
              {user?.customerCategory && (
                <span className="role-chip">
                  {t(`category.${user.customerCategory}`, {
                    defaultValue: user.customerCategory,
                  })}
                </span>
              )}
            </div>
          </Link>
          <button type="button" className="btn ghost" onClick={logout}>
            {t('nav.logout')}
          </button>
        </div>
      </header>
      <StatsStrip />
      <main className="page">
        <Outlet />
      </main>
    </div>
  )
}
