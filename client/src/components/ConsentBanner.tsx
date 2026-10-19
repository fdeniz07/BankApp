import { useState } from 'react'
import { useTranslation } from 'react-i18next'

const CONSENT_KEY = 'bankapp-consent'

type ConsentValue = 'accepted' | 'declined'

function readConsent(): ConsentValue | null {
  const v = localStorage.getItem(CONSENT_KEY)
  return v === 'accepted' || v === 'declined' ? v : null
}

export function ConsentBanner() {
  const { t } = useTranslation()
  const [choice, setChoice] = useState<ConsentValue | null>(() => readConsent())

  if (choice) return null

  const persist = (value: ConsentValue) => {
    localStorage.setItem(CONSENT_KEY, value)
    setChoice(value)
  }

  return (
    <div
      className="consent-banner"
      role="dialog"
      aria-labelledby="consent-banner-title"
      aria-describedby="consent-banner-text"
    >
      <div className="consent-banner-copy">
        <p id="consent-banner-title" className="consent-banner-title">
          {t('consent.title')}
        </p>
        <p id="consent-banner-text" className="consent-banner-text">
          {t('consent.body')}
        </p>
      </div>
      <div className="consent-banner-actions">
        <button type="button" className="btn ghost" onClick={() => persist('declined')}>
          {t('consent.decline')}
        </button>
        <button type="button" className="btn primary" onClick={() => persist('accepted')}>
          {t('consent.accept')}
        </button>
      </div>
    </div>
  )
}
