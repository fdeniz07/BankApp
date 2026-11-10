/**
 * Framework-agnostic accessibility preferences.
 * Copy this folder to another project and call `applyA11yPrefs` / `createA11yController`.
 */

export type A11yTheme = 'light' | 'dark'
export type A11yFontScale = 100 | 112 | 125 | 140 | 160
export type A11yColorFilter = 'none' | 'protanopia' | 'deuteranopia' | 'tritanopia'

export interface A11yPrefs {
  theme: A11yTheme
  contrast: boolean
  fontScale: A11yFontScale
  blueFilter: boolean
  hideImages: boolean
  underlineLinks: boolean
  reduceMotion: boolean
  colorFilter: A11yColorFilter
  panelOpen: boolean
  assistantHidden: boolean
}

export interface A11yStorageKeys {
  theme: string
  contrast: string
  fontScale: string
  blueFilter: string
  hideImages: string
  underlineLinks: string
  reduceMotion: string
  colorFilter: string
  panelOpen: string
  assistantHidden: string
}

export interface A11yOptions {
  /** Prefix for data attributes / CSS hooks, default `a11y` → data-a11y-* */
  attrPrefix?: string
  /** localStorage key map (BankApp keeps legacy keys) */
  keys?: Partial<A11yStorageKeys>
}

const FONT_STEPS: A11yFontScale[] = [100, 112, 125, 140, 160]

export const DEFAULT_A11Y_KEYS: A11yStorageKeys = {
  theme: 'a11y-theme',
  contrast: 'a11y-contrast',
  fontScale: 'a11y-font-scale',
  blueFilter: 'a11y-blue-filter',
  hideImages: 'a11y-hide-images',
  underlineLinks: 'a11y-underline-links',
  reduceMotion: 'a11y-reduce-motion',
  colorFilter: 'a11y-color-filter',
  panelOpen: 'a11y-panel-open',
  assistantHidden: 'a11y-assistant-hidden',
}

/** BankApp legacy-compatible keys */
export const BANKAPP_A11Y_KEYS: A11yStorageKeys = {
  theme: 'bankapp-theme',
  contrast: 'bankapp-contrast',
  fontScale: 'bankapp-font-scale',
  blueFilter: 'bankapp-a11y-blue-filter',
  hideImages: 'bankapp-a11y-hide-images',
  underlineLinks: 'bankapp-a11y-underline-links',
  reduceMotion: 'bankapp-a11y-reduce-motion',
  colorFilter: 'bankapp-a11y-color-filter',
  panelOpen: 'bankapp-a11y-open',
  assistantHidden: 'bankapp-a11y-hidden',
}

export function resolveKeys(partial?: Partial<A11yStorageKeys>): A11yStorageKeys {
  return { ...DEFAULT_A11Y_KEYS, ...partial }
}

export function defaultA11yPrefs(): A11yPrefs {
  return {
    theme: 'light',
    contrast: false,
    fontScale: 100,
    blueFilter: false,
    hideImages: false,
    underlineLinks: false,
    reduceMotion: false,
    colorFilter: 'none',
    panelOpen: false,
    assistantHidden: false,
  }
}

function parseFont(raw: string | null): A11yFontScale {
  const n = Number(raw ?? '100')
  return (FONT_STEPS.includes(n as A11yFontScale) ? n : 100) as A11yFontScale
}

function parseColorFilter(raw: string | null): A11yColorFilter {
  if (raw === 'protanopia' || raw === 'deuteranopia' || raw === 'tritanopia') return raw
  return 'none'
}

export function readA11yPrefs(keys: A11yStorageKeys = DEFAULT_A11Y_KEYS): A11yPrefs {
  if (typeof localStorage === 'undefined') return defaultA11yPrefs()
  return {
    theme: localStorage.getItem(keys.theme) === 'dark' ? 'dark' : 'light',
    contrast: localStorage.getItem(keys.contrast) === '1',
    fontScale: parseFont(localStorage.getItem(keys.fontScale)),
    blueFilter: localStorage.getItem(keys.blueFilter) === '1',
    hideImages: localStorage.getItem(keys.hideImages) === '1',
    underlineLinks: localStorage.getItem(keys.underlineLinks) === '1',
    reduceMotion: localStorage.getItem(keys.reduceMotion) === '1',
    colorFilter: parseColorFilter(localStorage.getItem(keys.colorFilter)),
    panelOpen: localStorage.getItem(keys.panelOpen) === '1',
    assistantHidden: localStorage.getItem(keys.assistantHidden) === '1',
  }
}

export function writeA11yPrefs(prefs: A11yPrefs, keys: A11yStorageKeys = DEFAULT_A11Y_KEYS) {
  localStorage.setItem(keys.theme, prefs.theme)
  localStorage.setItem(keys.contrast, prefs.contrast ? '1' : '0')
  localStorage.setItem(keys.fontScale, String(prefs.fontScale))
  localStorage.setItem(keys.blueFilter, prefs.blueFilter ? '1' : '0')
  localStorage.setItem(keys.hideImages, prefs.hideImages ? '1' : '0')
  localStorage.setItem(keys.underlineLinks, prefs.underlineLinks ? '1' : '0')
  localStorage.setItem(keys.reduceMotion, prefs.reduceMotion ? '1' : '0')
  localStorage.setItem(keys.colorFilter, prefs.colorFilter)
  localStorage.setItem(keys.panelOpen, prefs.panelOpen ? '1' : '0')
  localStorage.setItem(keys.assistantHidden, prefs.assistantHidden ? '1' : '0')
}

/** Apply prefs to <html> via data attributes + CSS variables (no framework). */
export function applyA11yPrefs(prefs: A11yPrefs, options: A11yOptions = {}) {
  const root = document.documentElement
  const p = options.attrPrefix ?? 'a11y'

  root.setAttribute('data-theme', prefs.theme)
  root.setAttribute('data-contrast', prefs.contrast ? 'high' : 'normal')
  root.setAttribute(`data-${p}-blue-filter`, prefs.blueFilter ? '1' : '0')
  root.setAttribute(`data-${p}-hide-images`, prefs.hideImages ? '1' : '0')
  root.setAttribute(`data-${p}-underline-links`, prefs.underlineLinks ? '1' : '0')
  root.setAttribute(`data-${p}-reduce-motion`, prefs.reduceMotion ? '1' : '0')
  root.setAttribute(`data-${p}-color-filter`, prefs.colorFilter)
  root.style.setProperty('--font-scale', String(prefs.fontScale / 100))
}

export function bumpFontScale(current: A11yFontScale, delta: -1 | 1): A11yFontScale {
  const idx = FONT_STEPS.indexOf(current)
  const next = Math.min(FONT_STEPS.length - 1, Math.max(0, idx + delta))
  return FONT_STEPS[next]!
}

export function speakPageText(lang?: string) {
  if (typeof window === 'undefined' || !window.speechSynthesis) return
  window.speechSynthesis.cancel()
  const text = (document.body?.innerText ?? '').replace(/\s+/g, ' ').trim().slice(0, 4000)
  if (!text) return
  const utter = new SpeechSynthesisUtterance(text)
  if (lang) utter.lang = lang
  window.speechSynthesis.speak(utter)
}

export function stopSpeaking() {
  if (typeof window === 'undefined' || !window.speechSynthesis) return
  window.speechSynthesis.cancel()
}

export function createA11yController(options: A11yOptions & { keys?: Partial<A11yStorageKeys> } = {}) {
  const keys = resolveKeys(options.keys)
  let prefs = readA11yPrefs(keys)

  const persist = () => {
    writeA11yPrefs(prefs, keys)
    applyA11yPrefs(prefs, options)
  }

  return {
    getPrefs: () => ({ ...prefs }),
    setPrefs: (patch: Partial<A11yPrefs>) => {
      prefs = { ...prefs, ...patch }
      persist()
    },
    reset: () => {
      prefs = defaultA11yPrefs()
      persist()
    },
    boot: () => {
      prefs = readA11yPrefs(keys)
      applyA11yPrefs(prefs, options)
    },
  }
}
