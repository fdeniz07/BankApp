import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { fetchFxSnapshot, type FxSnapshot } from '../api/fxApi'

function formatRate(pair: string, rate: number): string {
  const digits = pair.includes('JPY') ? 2 : pair.includes('TRY') ? 4 : 4
  return rate.toLocaleString(undefined, {
    minimumFractionDigits: digits,
    maximumFractionDigits: digits,
  })
}

export function FxTicker() {
  const { t, i18n } = useTranslation()
  const [snapshot, setSnapshot] = useState<FxSnapshot | null>(null)

  useEffect(() => {
    let cancelled = false

    const load = () => {
      fetchFxSnapshot().then((data) => {
        if (!cancelled) setSnapshot(data)
      })
    }

    load()
    const id = window.setInterval(load, 5 * 60 * 1000)
    return () => {
      cancelled = true
      window.clearInterval(id)
    }
  }, [])

  if (!snapshot?.quotes.length) return null

  const items = [...snapshot.quotes, ...snapshot.quotes]
  const updated = new Date(snapshot.updatedAt).toLocaleTimeString(i18n.language, {
    hour: '2-digit',
    minute: '2-digit',
  })

  return (
    <div className="fx-ticker" aria-label={t('fx.label')}>
      <span className="fx-ticker-badge">{t('fx.live')}</span>
      <div className="fx-ticker-track-wrap">
        <div className="fx-ticker-track">
          {items.map((q, i) => (
            <span key={`${q.pair}-${i}`} className="fx-ticker-item">
              <strong>{q.pair}</strong>
              <span>{formatRate(q.pair, q.rate)}</span>
            </span>
          ))}
        </div>
      </div>
      <span className="fx-ticker-updated muted">
        {t('fx.updated', { time: updated })}
      </span>
    </div>
  )
}
