export type FxQuote = {
  pair: string
  rate: number
}

export type FxSnapshot = {
  base: string
  updatedAt: string
  quotes: FxQuote[]
  /** True when open.er-api failed and static sample rates are shown. */
  isDemo: boolean
}

const PAIRS: { pair: string; from: string; to: string }[] = [
  { pair: 'USD/TRY', from: 'USD', to: 'TRY' },
  { pair: 'EUR/TRY', from: 'EUR', to: 'TRY' },
  { pair: 'GBP/TRY', from: 'GBP', to: 'TRY' },
  { pair: 'EUR/USD', from: 'EUR', to: 'USD' },
  { pair: 'GBP/USD', from: 'GBP', to: 'USD' },
  { pair: 'USD/JPY', from: 'USD', to: 'JPY' },
]

const FALLBACK: FxSnapshot = {
  base: 'USD',
  updatedAt: new Date().toISOString(),
  isDemo: true,
  quotes: [
    { pair: 'USD/TRY', rate: 34.2 },
    { pair: 'EUR/TRY', rate: 37.1 },
    { pair: 'GBP/TRY', rate: 44.5 },
    { pair: 'EUR/USD', rate: 1.08 },
    { pair: 'GBP/USD', rate: 1.3 },
    { pair: 'USD/JPY', rate: 151.2 },
  ],
}

type ErApiResponse = {
  result?: string
  base_code?: string
  time_last_update_utc?: string
  rates?: Record<string, number>
}

async function fetchUsdRates(): Promise<ErApiResponse> {
  const res = await fetch('https://open.er-api.com/v6/latest/USD')
  if (!res.ok) throw new Error(`FX HTTP ${res.status}`)
  return (await res.json()) as ErApiResponse
}

function rateFromUsd(rates: Record<string, number>, from: string, to: string): number | null {
  if (from === to) return 1
  if (from === 'USD') {
    const v = rates[to]
    return typeof v === 'number' && v > 0 ? v : null
  }
  if (to === 'USD') {
    const v = rates[from]
    return typeof v === 'number' && v > 0 ? 1 / v : null
  }
  const fromUsd = rates[from]
  const toUsd = rates[to]
  if (typeof fromUsd !== 'number' || typeof toUsd !== 'number' || fromUsd <= 0) return null
  return toUsd / fromUsd
}

export async function fetchFxSnapshot(): Promise<FxSnapshot> {
  try {
    const data = await fetchUsdRates()
    if (data.result !== 'success' || !data.rates) return { ...FALLBACK, updatedAt: new Date().toISOString() }

    const quotes = PAIRS.map(({ pair, from, to }) => {
      const rate = rateFromUsd(data.rates!, from, to)
      return rate == null ? null : { pair, rate }
    }).filter((q): q is FxQuote => q != null)

    if (quotes.length === 0) return { ...FALLBACK, updatedAt: new Date().toISOString() }

    return {
      base: data.base_code ?? 'USD',
      updatedAt: data.time_last_update_utc
        ? new Date(data.time_last_update_utc).toISOString()
        : new Date().toISOString(),
      quotes,
      isDemo: false,
    }
  } catch {
    return { ...FALLBACK, updatedAt: new Date().toISOString() }
  }
}
