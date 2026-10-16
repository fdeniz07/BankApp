import { NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/AuthContext'
import { LanguageSwitcher } from './LanguageSwitcher'
import { StatsStrip } from './StatsStrip'

export function AppLayout() {
  const { t } = useTranslation()
  const { user, isStaff, logout } = useAuth()

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand">{t('appName')}</div>
        <nav className="nav">
          <NavLink to="/applications">{t('nav.applications')}</NavLink>
          {isStaff && <NavLink to="/customers">{t('nav.customers')}</NavLink>}
          {isStaff && <NavLink to="/credit-types">{t('nav.creditTypes')}</NavLink>}
          <NavLink to="/profile">{t('nav.profile')}</NavLink>
        </nav>
        <div className="topbar-right">
          <LanguageSwitcher />
          <div className="user-meta">
            <span>{user?.displayName || user?.email}</span>
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
