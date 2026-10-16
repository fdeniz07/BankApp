const localeMap: Record<string, string> = {
  tr: 'tr-TR',
  en: 'en-US',
  de: 'de-DE',
}

function normalizeCurrency(currencyCode?: string) {
  const code = (currencyCode || 'USD').toUpperCase()
  return code === 'EURO' ? 'EUR' : code
}

/** TR/DE: amount then currency; EN: standard locale currency format (symbol first). */
export function formatMoney(
  amount: number | null | undefined,
  currencyCode = 'USD',
  language = 'tr',
  fractionDigits = 0,
): string {
  if (amount == null || Number.isNaN(amount)) return '—'

  const locale = localeMap[language] ?? 'tr-TR'
  const currency = normalizeCurrency(currencyCode)
  const digits = Math.max(0, fractionDigits)
  const numberPart = amount.toLocaleString(locale, {
    minimumFractionDigits: digits,
    maximumFractionDigits: digits,
  })

  if (language === 'en') {
    try {
      return new Intl.NumberFormat(locale, {
        style: 'currency',
        currency,
        minimumFractionDigits: digits,
        maximumFractionDigits: digits,
      }).format(amount)
    } catch {
      return `${currency} ${numberPart}`
    }
  }

  const suffix =
    currency === 'TRY' ? '₺' : currency === 'EUR' ? '€' : currency === 'USD' ? '$' : currency

  return `${numberPart} ${suffix}`
}
