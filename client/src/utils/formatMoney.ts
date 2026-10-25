const localeMap: Record<string, string> = {
  tr: 'tr-TR',
  en: 'en-US',
  de: 'de-DE',
}

function normalizeCurrency(currencyCode?: string) {
  const code = (currencyCode || 'USD').toUpperCase()
  return code === 'EURO' ? 'EUR' : code
}

/** Display symbol for ISO currency codes (never show TRY/USD/EUR as text in UI). */
export function currencySymbol(currencyCode?: string): string {
  const currency = normalizeCurrency(currencyCode)
  switch (currency) {
    case 'TRY':
      return '₺'
    case 'EUR':
      return '€'
    case 'USD':
      return '$'
    case 'GBP':
      return '£'
    default:
      return currency
  }
}

/**
 * Locale-aware money with currency **symbols** (₺ $ €).
 * TR/DE: `1.212,00 ₺` · EN: `$1,212.00`
 */
export function formatMoney(
  amount: number | null | undefined,
  currencyCode = 'USD',
  language = 'tr',
  fractionDigits = 0,
): string {
  if (amount == null || Number.isNaN(amount)) return '—'

  const locale = localeMap[language.slice(0, 2)] ?? 'tr-TR'
  const currency = normalizeCurrency(currencyCode)
  const symbol = currencySymbol(currency)
  const digits = Math.max(0, fractionDigits)
  const numberPart = amount.toLocaleString(locale, {
    minimumFractionDigits: digits,
    maximumFractionDigits: digits,
  })

  if (language.slice(0, 2) === 'en') {
    return `${symbol}${numberPart}`
  }

  return `${numberPart} ${symbol}`
}
