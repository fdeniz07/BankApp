import { useTranslation } from 'react-i18next'

const languages = [
  { code: 'tr', label: 'TR' },
  { code: 'en', label: 'EN' },
  { code: 'de', label: 'DE' },
] as const

export function LanguageSwitcher() {
  const { i18n, t } = useTranslation()

  return (
    <label className="lang-switch">
      <span className="sr-only">{t('language')}</span>
      <select
        value={i18n.language}
        onChange={(e) => void i18n.changeLanguage(e.target.value)}
        aria-label={t('language')}
      >
        {languages.map((lang) => (
          <option key={lang.code} value={lang.code}>
            {lang.label}
          </option>
        ))}
      </select>
    </label>
  )
}
