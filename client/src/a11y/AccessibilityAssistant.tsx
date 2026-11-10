import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import {
  type A11yColorFilter,
  type A11yPrefs,
  type A11yStorageKeys,
  BANKAPP_A11Y_KEYS,
  bumpFontScale,
  createA11yController,
  speakPageText,
  stopSpeaking,
} from './core'
import './a11y-assistant.css'

export interface AccessibilityAssistantLabels {
  toggle: string
  title: string
  close: string
  light: string
  dark: string
  fontUp: string
  fontDown: string
  contrast: string
  blueFilter: string
  hideImages: string
  underlineLinks: string
  reduceMotion: string
  colorFilter: string
  colorNone: string
  colorProtanopia: string
  colorDeuteranopia: string
  colorTritanopia: string
  speak: string
  stopSpeak: string
  hideAssistant: string
  reset: string
}

export interface AccessibilityAssistantProps {
  labels: AccessibilityAssistantLabels
  /** SpeechSynthesis language, e.g. tr-TR / en-US / de-DE */
  speechLang?: string
  /** Override storage keys; BankApp default = legacy keys */
  keys?: Partial<A11yStorageKeys>
  /** data-attribute prefix; default a11y */
  attrPrefix?: string
  className?: string
}

function SvgFilters() {
  return (
    <svg className="a11y-svg-filters" aria-hidden="true" focusable="false">
      <defs>
        <filter id="a11y-protanopia">
          <feColorMatrix
            type="matrix"
            values="0.567 0.433 0 0 0  0.558 0.442 0 0 0  0 0.242 0.758 0 0  0 0 0 1 0"
          />
        </filter>
        <filter id="a11y-deuteranopia">
          <feColorMatrix
            type="matrix"
            values="0.625 0.375 0 0 0  0.7 0.3 0 0 0  0 0.3 0.7 0 0  0 0 0 1 0"
          />
        </filter>
        <filter id="a11y-tritanopia">
          <feColorMatrix
            type="matrix"
            values="0.95 0.05 0 0 0  0 0.433 0.567 0 0  0 0.475 0.525 0 0  0 0 0 1 0"
          />
        </filter>
      </defs>
    </svg>
  )
}

function Glyph({ children, size = 18 }: { children: ReactNode; size?: number }) {
  return (
    <svg viewBox="0 0 24 24" width={size} height={size} aria-hidden="true" focusable="false">
      {children}
    </svg>
  )
}

const Icons = {
  person: (
    <Glyph size={28}>
      <circle cx="12" cy="5" r="2.4" fill="currentColor" />
      <path
        d="M6.5 9.3h11l-1.3 3.4H7.8L6.5 9.3zm2.3 4.4h6.2l1.5 6.4h-2.3l-.75-3.2H10.05l-.75 3.2H7.3l1.5-6.4z"
        fill="currentColor"
      />
    </Glyph>
  ),
  sun: (
    <Glyph>
      <circle cx="12" cy="12" r="4" fill="none" stroke="currentColor" strokeWidth="1.8" />
      <path
        d="M12 2.5v2.2M12 19.3v2.2M2.5 12h2.2M19.3 12h2.2M5.1 5.1l1.6 1.6M17.3 17.3l1.6 1.6M18.9 5.1l-1.6 1.6M6.7 17.3l-1.6 1.6"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
      />
    </Glyph>
  ),
  moon: (
    <Glyph>
      <path
        d="M16.5 3.5a8.5 8.5 0 1 0 4 12.4A7 7 0 1 1 16.5 3.5z"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
    </Glyph>
  ),
  contrast: (
    <Glyph>
      <circle cx="12" cy="12" r="8" fill="none" stroke="currentColor" strokeWidth="1.8" />
      <path d="M12 4a8 8 0 0 1 0 16V4z" fill="currentColor" />
    </Glyph>
  ),
  blueFilter: (
    <Glyph>
      <circle cx="12" cy="12" r="7" fill="none" stroke="currentColor" strokeWidth="1.8" />
      <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
      <circle cx="12" cy="12" r="3" fill="currentColor" opacity="0.35" />
    </Glyph>
  ),
  hideImages: (
    <Glyph>
      <rect x="3.5" y="5.5" width="17" height="13" rx="2" fill="none" stroke="currentColor" strokeWidth="1.8" />
      <circle cx="9" cy="10.5" r="1.5" fill="currentColor" />
      <path d="M4.5 16.5l4.5-4 3.2 2.8 3-3.3 4.3 4.5" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinejoin="round" />
      <path d="M4 4l16 16" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </Glyph>
  ),
  underline: (
    <Glyph>
      <path d="M7 6v6.5a5 5 0 0 0 10 0V6" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
      <path d="M5.5 19.5h13" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </Glyph>
  ),
  reduceMotion: (
    <Glyph>
      <rect x="7" y="5" width="3.2" height="14" rx="1" fill="currentColor" />
      <rect x="13.8" y="5" width="3.2" height="14" rx="1" fill="currentColor" />
    </Glyph>
  ),
  colorFilter: (
    <Glyph>
      <circle cx="9.5" cy="10" r="4.2" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <circle cx="14.5" cy="10" r="4.2" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <circle cx="12" cy="14.5" r="4.2" fill="none" stroke="currentColor" strokeWidth="1.6" />
    </Glyph>
  ),
  speak: (
    <Glyph>
      <path d="M4 10v4h3.2L12 18V6L7.2 10H4z" fill="currentColor" />
      <path
        d="M15.2 9.2a3.2 3.2 0 0 1 0 5.6M17.5 7a5.5 5.5 0 0 1 0 10"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.7"
        strokeLinecap="round"
      />
    </Glyph>
  ),
  stop: (
    <Glyph>
      <rect x="6.5" y="6.5" width="11" height="11" rx="1.5" fill="currentColor" />
    </Glyph>
  ),
  reset: (
    <Glyph>
      <path
        d="M6.2 9.2A7 7 0 1 1 5 12"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
      />
      <path d="M6.2 5.2v4.2H10" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </Glyph>
  ),
  hide: (
    <Glyph>
      <path
        d="M2.8 12s3.4-6 9.2-6 9.2 6 9.2 6-3.4 6-9.2 6-9.2-6-9.2-6z"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.7"
      />
      <circle cx="12" cy="12" r="2.4" fill="none" stroke="currentColor" strokeWidth="1.7" />
      <path d="M4 4l16 16" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </Glyph>
  ),
  fontDown: (
    <Glyph>
      <path d="M5 17L10 5h2l5 12h-2.2l-1-2.6H8.2L7.2 17H5zM9 12.2h4.2L11.1 7.2 9 12.2z" fill="currentColor" />
      <path d="M18 15h4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </Glyph>
  ),
  fontUp: (
    <Glyph>
      <path d="M4 17L9.5 4h2L17 17h-2.3l-1.1-2.8H7.4L6.3 17H4zM8.2 12h5.1L10.75 6.4 8.2 12z" fill="currentColor" />
      <path d="M19 12v6M16 15h6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </Glyph>
  ),
}

function Row({
  active,
  onClick,
  label,
  icon,
  pressed,
  disabled,
}: {
  active?: boolean
  onClick: () => void
  label: string
  icon: ReactNode
  pressed?: boolean
  disabled?: boolean
}) {
  return (
    <button
      type="button"
      className={`a11y-item${active ? ' is-active' : ''}`}
      onClick={onClick}
      aria-pressed={pressed}
      disabled={disabled}
    >
      <span className="a11y-item-icon">{icon}</span>
      <span className="a11y-item-label">{label}</span>
    </button>
  )
}

export function AccessibilityAssistant({
  labels,
  speechLang,
  keys = BANKAPP_A11Y_KEYS,
  attrPrefix = 'a11y',
  className,
}: AccessibilityAssistantProps) {
  const controller = useMemo(
    () => createA11yController({ keys, attrPrefix }),
    [attrPrefix, keys],
  )

  const [prefs, setPrefsState] = useState<A11yPrefs>(() => controller.getPrefs())

  useEffect(() => {
    controller.boot()
    setPrefsState(controller.getPrefs())
  }, [controller])

  const patch = useCallback(
    (partial: Partial<A11yPrefs>) => {
      controller.setPrefs(partial)
      setPrefsState(controller.getPrefs())
    },
    [controller],
  )

  if (prefs.assistantHidden) {
    return (
      <>
        <SvgFilters />
        <button
          type="button"
          className={`a11y-fab a11y-fab-solo${className ? ` ${className}` : ''}`}
          onClick={() => patch({ assistantHidden: false, panelOpen: true })}
          aria-label={labels.toggle}
          title={labels.toggle}
        >
          {Icons.person}
        </button>
      </>
    )
  }

  return (
    <>
      <SvgFilters />
      <div className={`a11y-dock${className ? ` ${className}` : ''}`}>
        {prefs.panelOpen && (
          <div className="a11y-panel" role="dialog" aria-label={labels.title}>
            <div className="a11y-panel-header">
              <p className="a11y-panel-title">{labels.title}</p>
              <button
                type="button"
                className="a11y-panel-close"
                onClick={() => patch({ panelOpen: false })}
                aria-label={labels.close}
              >
                ×
              </button>
            </div>

            <Row
              label={labels.light}
              icon={Icons.sun}
              active={prefs.theme === 'light'}
              pressed={prefs.theme === 'light'}
              onClick={() => patch({ theme: 'light' })}
            />
            <Row
              label={labels.dark}
              icon={Icons.moon}
              active={prefs.theme === 'dark'}
              pressed={prefs.theme === 'dark'}
              onClick={() => patch({ theme: 'dark' })}
            />

            <div className="a11y-font-row">
              <button
                type="button"
                className="a11y-item a11y-font-btn"
                onClick={() => patch({ fontScale: bumpFontScale(prefs.fontScale, -1) })}
                disabled={prefs.fontScale === 100}
                aria-label={labels.fontDown}
              >
                <span className="a11y-item-icon">{Icons.fontDown}</span>
              </button>
              <span className="a11y-item-meta" aria-live="polite">
                {prefs.fontScale}%
              </span>
              <button
                type="button"
                className="a11y-item a11y-font-btn"
                onClick={() => patch({ fontScale: bumpFontScale(prefs.fontScale, 1) })}
                disabled={prefs.fontScale === 160}
                aria-label={labels.fontUp}
              >
                <span className="a11y-item-icon">{Icons.fontUp}</span>
              </button>
            </div>

            <Row
              label={labels.contrast}
              icon={Icons.contrast}
              active={prefs.contrast}
              pressed={prefs.contrast}
              onClick={() => patch({ contrast: !prefs.contrast })}
            />
            <Row
              label={labels.blueFilter}
              icon={Icons.blueFilter}
              active={prefs.blueFilter}
              pressed={prefs.blueFilter}
              onClick={() => patch({ blueFilter: !prefs.blueFilter })}
            />
            <Row
              label={labels.hideImages}
              icon={Icons.hideImages}
              active={prefs.hideImages}
              pressed={prefs.hideImages}
              onClick={() => patch({ hideImages: !prefs.hideImages })}
            />
            <Row
              label={labels.underlineLinks}
              icon={Icons.underline}
              active={prefs.underlineLinks}
              pressed={prefs.underlineLinks}
              onClick={() => patch({ underlineLinks: !prefs.underlineLinks })}
            />
            <Row
              label={labels.reduceMotion}
              icon={Icons.reduceMotion}
              active={prefs.reduceMotion}
              pressed={prefs.reduceMotion}
              onClick={() => patch({ reduceMotion: !prefs.reduceMotion })}
            />

            <div className="a11y-color-row">
              <span className="a11y-item-icon" aria-hidden="true">
                {Icons.colorFilter}
              </span>
              <label className="a11y-color-label" htmlFor="a11y-color-filter">
                {labels.colorFilter}
              </label>
              <select
                id="a11y-color-filter"
                className="a11y-select"
                value={prefs.colorFilter}
                onChange={(e) => patch({ colorFilter: e.target.value as A11yColorFilter })}
                aria-label={labels.colorFilter}
              >
                <option value="none">{labels.colorNone}</option>
                <option value="protanopia">{labels.colorProtanopia}</option>
                <option value="deuteranopia">{labels.colorDeuteranopia}</option>
                <option value="tritanopia">{labels.colorTritanopia}</option>
              </select>
            </div>

            <Row label={labels.speak} icon={Icons.speak} onClick={() => speakPageText(speechLang)} />
            <Row label={labels.stopSpeak} icon={Icons.stop} onClick={() => stopSpeaking()} />
            <Row
              label={labels.reset}
              icon={Icons.reset}
              onClick={() => {
                stopSpeaking()
                controller.reset()
                setPrefsState(controller.getPrefs())
              }}
            />
            <Row
              label={labels.hideAssistant}
              icon={Icons.hide}
              onClick={() => patch({ assistantHidden: true, panelOpen: false })}
            />
          </div>
        )}

        <button
          type="button"
          className="a11y-fab"
          onClick={() => patch({ panelOpen: !prefs.panelOpen })}
          aria-expanded={prefs.panelOpen}
          aria-label={labels.toggle}
          title={labels.toggle}
        >
          {Icons.person}
        </button>
      </div>
    </>
  )
}
