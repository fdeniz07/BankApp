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

function IconPerson() {
  return (
    <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
      <circle cx="12" cy="5" r="2.2" fill="currentColor" />
      <path
        d="M7 9.5h10l-1.2 3.2H8.2L7 9.5zm2.2 4.2h5.6l1.4 6.1h-2.1l-.7-3H10.6l-.7 3H7.8l1.4-6.1z"
        fill="currentColor"
      />
    </svg>
  )
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
          className={`a11y-fab${className ? ` ${className}` : ''}`}
          style={{ position: 'fixed', right: '1rem', bottom: '1rem', zIndex: 45 }}
          onClick={() => patch({ assistantHidden: false, panelOpen: true })}
          aria-label={labels.toggle}
          title={labels.toggle}
        >
          <IconPerson />
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
              icon="Aa"
              active={prefs.theme === 'light'}
              pressed={prefs.theme === 'light'}
              onClick={() => patch({ theme: 'light' })}
            />
            <Row
              label={labels.dark}
              icon="Aa"
              active={prefs.theme === 'dark'}
              pressed={prefs.theme === 'dark'}
              onClick={() => patch({ theme: 'dark' })}
            />

            <div className="a11y-font-row">
              <button
                type="button"
                className="a11y-item"
                onClick={() => patch({ fontScale: bumpFontScale(prefs.fontScale, -1) })}
                disabled={prefs.fontScale === 100}
                aria-label={labels.fontDown}
              >
                A−
              </button>
              <span className="a11y-item-meta" aria-live="polite">
                {prefs.fontScale}%
              </span>
              <button
                type="button"
                className="a11y-item"
                onClick={() => patch({ fontScale: bumpFontScale(prefs.fontScale, 1) })}
                disabled={prefs.fontScale === 160}
                aria-label={labels.fontUp}
              >
                A+
              </button>
            </div>

            <Row
              label={labels.contrast}
              icon="C"
              active={prefs.contrast}
              pressed={prefs.contrast}
              onClick={() => patch({ contrast: !prefs.contrast })}
            />
            <Row
              label={labels.blueFilter}
              icon="BF"
              active={prefs.blueFilter}
              pressed={prefs.blueFilter}
              onClick={() => patch({ blueFilter: !prefs.blueFilter })}
            />
            <Row
              label={labels.hideImages}
              icon="IMG"
              active={prefs.hideImages}
              pressed={prefs.hideImages}
              onClick={() => patch({ hideImages: !prefs.hideImages })}
            />
            <Row
              label={labels.underlineLinks}
              icon="U"
              active={prefs.underlineLinks}
              pressed={prefs.underlineLinks}
              onClick={() => patch({ underlineLinks: !prefs.underlineLinks })}
            />
            <Row
              label={labels.reduceMotion}
              icon="RM"
              active={prefs.reduceMotion}
              pressed={prefs.reduceMotion}
              onClick={() => patch({ reduceMotion: !prefs.reduceMotion })}
            />

            <label className="a11y-item-label" style={{ padding: '0.25rem 0.5rem', opacity: 0.85 }}>
              {labels.colorFilter}
            </label>
            <select
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

            <Row label={labels.speak} icon="TTS" onClick={() => speakPageText(speechLang)} />
            <Row label={labels.stopSpeak} icon="STOP" onClick={() => stopSpeaking()} />
            <Row
              label={labels.reset}
              icon="RST"
              onClick={() => {
                stopSpeaking()
                controller.reset()
                setPrefsState(controller.getPrefs())
              }}
            />
            <Row
              label={labels.hideAssistant}
              icon="HID"
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
          <IconPerson />
        </button>
      </div>
    </>
  )
}
