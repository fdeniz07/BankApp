import { useTranslation } from 'react-i18next'
import { AccessibilityAssistant, BANKAPP_A11Y_KEYS } from '../a11y'

const SPEECH_LANG: Record<string, string> = {
  tr: 'tr-TR',
  en: 'en-US',
  de: 'de-DE',
}

/** BankApp shell — thin i18n wrapper around the portable a11y kit. */
export function AccessibilityToolbar() {
  const { t, i18n } = useTranslation()
  const lang = (i18n.language || 'tr').slice(0, 2)

  return (
    <AccessibilityAssistant
      keys={BANKAPP_A11Y_KEYS}
      speechLang={SPEECH_LANG[lang] ?? 'tr-TR'}
      labels={{
        toggle: t('a11y.toggle'),
        title: t('a11y.title'),
        close: t('a11y.close'),
        light: t('a11y.light'),
        dark: t('a11y.dark'),
        fontUp: t('a11y.fontUp'),
        fontDown: t('a11y.fontDown'),
        contrast: t('a11y.contrast'),
        blueFilter: t('a11y.blueFilter'),
        hideImages: t('a11y.hideImages'),
        underlineLinks: t('a11y.underlineLinks'),
        reduceMotion: t('a11y.reduceMotion'),
        colorFilter: t('a11y.colorFilter'),
        colorNone: t('a11y.colorNone'),
        colorProtanopia: t('a11y.colorProtanopia'),
        colorDeuteranopia: t('a11y.colorDeuteranopia'),
        colorTritanopia: t('a11y.colorTritanopia'),
        speak: t('a11y.speak'),
        stopSpeak: t('a11y.stopSpeak'),
        hideAssistant: t('a11y.hideAssistant'),
        reset: t('a11y.reset'),
      }}
    />
  )
}
