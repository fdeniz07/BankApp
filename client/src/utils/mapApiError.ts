import type { TFunction } from 'i18next'
import { ApiError } from '../api/client'

const MESSAGE_KEYS: Record<string, string> = {
  'Customer type does not match credit category.': 'errors.categoryMismatch',
  'Requested amount is out of allowed range.': 'errors.amountOutOfRange',
  'Requested amount must match the package amount steps.': 'errors.amountNotOnPackageGrid',
  'Requested term is out of allowed range.': 'errors.termOutOfRange',
  'Requested term must be one of the allowed package terms.': 'errors.termNotAllowed',
  'Credit sub type does not exist.': 'errors.creditSubTypeNotExists',
  'Credit sub type is not active.': 'errors.creditSubTypeNotActive',
  'Customer does not exist.': 'errors.customerNotExists',
  'Customer is not active.': 'errors.customerNotActive',
  'Customer must be inactive before delete.': 'errors.mustBeInactiveBeforeDelete',
  'Customer has open credit applications (Pending or Approved) and cannot be deleted.':
    'errors.hasOpenCreditApplications',
  'Gender must be one of: Female, Male, PreferNotToSay.': 'errors.invalidGender',
  'One or more validation errors occurred.': 'errors.validation',
  'Validation error(s)': 'errors.validation',
  Unauthorized: 'errors.unauthorized',
}

export function mapApiError(err: unknown, t: TFunction): string {
  if (!(err instanceof ApiError)) return t('common.error')

  const raw = err.rawMessage || err.message
  const key = MESSAGE_KEYS[raw]
  if (key) return t(key)

  for (const [needle, i18nKey] of Object.entries(MESSAGE_KEYS)) {
    if (raw.includes(needle)) return t(i18nKey)
  }

  if (/validation/i.test(raw)) return t('errors.validation')

  return raw || t('common.error')
}
