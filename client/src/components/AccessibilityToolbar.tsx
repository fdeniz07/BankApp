import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'

const THEME_KEY = 'bankapp-theme'
const CONTRAST_KEY = 'bankapp-contrast'
const FONT_KEY = 'bankapp-font-scale'
const OPEN_KEY = 'bankapp-a11y-open'

type Theme = 'light' | 'dark'
type FontScale = 100 | 112 | 125 | 140

function readTheme(): Theme {
  const v = localStorage.getItem(THEME_KEY)
  return v === 'dark' ? 'dark' : 'light'
}

function readContrast(): boolean {
  return localStorage.getItem(CONTRAST_KEY) === '1'
}

function readFontScale(): FontScale {
  const n = Number(localStorage.getItem(FONT_KEY) ?? '100')
  return n === 112 || n === 125 || n === 140 ? n : 100
}

function applyPrefs(theme: Theme, contrast: boolean, fontScale: FontScale) {
  const root = document.documentElement
  root.setAttribute('data-theme', theme)
  root.setAttribute('data-contrast', contrast ? 'high' : 'normal')
  root.style.setProperty('--font-scale', String(fontScale / 100))
}

export function AccessibilityToolbar() {
  const { t } = useTranslation()
  const [open, setOpen] = useState(() => localStorage.getItem(OPEN_KEY) === '1')
  const [theme, setTheme] = useState<Theme>(readTheme)
  const [contrast, setContrast] = useState(readContrast)
  const [fontScale, setFontScale] = useState<FontScale>(readFontScale)

  useEffect(() => {
    applyPrefs(theme, contrast, fontScale)
  }, [theme, contrast, fontScale])

  const toggleOpen = () => {
    setOpen((prev) => {
      const next = !prev
      localStorage.setItem(OPEN_KEY, next ? '1' : '0')
      return next
    })
  }

  const setThemePersist = (next: Theme) => {
    setTheme(next)
    localStorage.setItem(THEME_KEY, next)
  }

  const toggleContrast = () => {
    setContrast((prev) => {
      const next = !prev
      localStorage.setItem(CONTRAST_KEY, next ? '1' : '0')
      return next
    })
  }

  const bumpFont = (delta: -1 | 1) => {
    const steps: FontScale[] = [100, 112, 125, 140]
    const idx = steps.indexOf(fontScale)
    const next = steps[Math.min(steps.length - 1, Math.max(0, idx + delta))]!
    setFontScale(next)
    localStorage.setItem(FONT_KEY, String(next))
  }

  const resetAll = () => {
    setThemePersist('light')
    setContrast(false)
    localStorage.setItem(CONTRAST_KEY, '0')
    setFontScale(100)
    localStorage.setItem(FONT_KEY, '100')
  }

  return (
    <div className={`a11y-dock${open ? ' is-open' : ''}`}>
      {open && (
        <div
          className="a11y-panel"
          role="dialog"
          aria-label={t('a11y.title')}
        >
          <p className="a11y-panel-title">{t('a11y.title')}</p>
          <div className="a11y-row">
            <button
              type="button"
              className={`btn a11y-btn${theme === 'light' ? ' is-active' : ''}`}
              onClick={() => setThemePersist('light')}
              aria-pressed={theme === 'light'}
            >
              {t('a11y.light')}
            </button>
            <button
              type="button"
              className={`btn a11y-btn${theme === 'dark' ? ' is-active' : ''}`}
              onClick={() => setThemePersist('dark')}
              aria-pressed={theme === 'dark'}
            >
              {t('a11y.dark')}
            </button>
          </div>
          <div className="a11y-row">
            <button
              type="button"
              className="btn a11y-btn"
              onClick={() => bumpFont(-1)}
              disabled={fontScale === 100}
              aria-label={t('a11y.fontDown')}
            >
              A−
            </button>
            <span className="a11y-scale">{fontScale}%</span>
            <button
              type="button"
              className="btn a11y-btn"
              onClick={() => bumpFont(1)}
              disabled={fontScale === 140}
              aria-label={t('a11y.fontUp')}
            >
              A+
            </button>
          </div>
          <button
            type="button"
            className={`btn a11y-btn a11y-btn-wide${contrast ? ' is-active' : ''}`}
            onClick={toggleContrast}
            aria-pressed={contrast}
          >
            {t('a11y.contrast')}
          </button>
          <button type="button" className="btn ghost a11y-btn-wide" onClick={resetAll}>
            {t('a11y.reset')}
          </button>
        </div>
      )}
      <button
        type="button"
        className="a11y-fab"
        onClick={toggleOpen}
        aria-expanded={open}
        aria-label={t('a11y.toggle')}
        title={t('a11y.toggle')}
      >
        <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true">
          <circle cx="12" cy="5" r="2.2" fill="currentColor" />
          <path
            d="M7 9.5h10l-1.2 3.2H8.2L7 9.5zm2.2 4.2h5.6l1.4 6.1h-2.1l-.7-3H10.6l-.7 3H7.8l1.4-6.1z"
            fill="currentColor"
          />
        </svg>
      </button>
    </div>
  )
}
