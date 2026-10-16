import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import tr from './locales/tr/common.json'
import en from './locales/en/common.json'
import de from './locales/de/common.json'

const STORAGE_KEY = 'bankapp.lang'

const saved = localStorage.getItem(STORAGE_KEY)
const initial = saved === 'en' || saved === 'de' || saved === 'tr' ? saved : 'tr'

void i18n.use(initReactI18next).init({
  resources: {
    tr: { common: tr },
    en: { common: en },
    de: { common: de },
  },
  lng: initial,
  fallbackLng: 'tr',
  defaultNS: 'common',
  interpolation: { escapeValue: false },
})

i18n.on('languageChanged', (lng) => {
  localStorage.setItem(STORAGE_KEY, lng)
  document.documentElement.lang = lng
})

document.documentElement.lang = initial

export default i18n
