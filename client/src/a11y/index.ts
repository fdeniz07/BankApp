/**
 * Portable accessibility kit.
 * Copy this folder into another SPA and wire `AccessibilityAssistant` + CSS.
 * Framework-agnostic logic lives in `core.ts`.
 */
export {
  type A11yTheme,
  type A11yFontScale,
  type A11yColorFilter,
  type A11yPrefs,
  type A11yStorageKeys,
  type A11yOptions,
  DEFAULT_A11Y_KEYS,
  BANKAPP_A11Y_KEYS,
  resolveKeys,
  defaultA11yPrefs,
  readA11yPrefs,
  writeA11yPrefs,
  applyA11yPrefs,
  bumpFontScale,
  speakPageText,
  stopSpeaking,
  createA11yController,
} from './core'

export {
  AccessibilityAssistant,
  type AccessibilityAssistantLabels,
  type AccessibilityAssistantProps,
} from './AccessibilityAssistant'
