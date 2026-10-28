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
  'Credit type is not active.': 'errors.creditTypeNotActive',
  'Credit type does not exist.': 'errors.creditTypeNotExists',
  'Credit type code already exists.': 'errors.creditTypeCodeExists',
  'Customer does not exist.': 'errors.customerNotExists',
  'Customer is not active.': 'errors.customerNotActive',
  'Customer must be inactive before delete.': 'errors.mustBeInactiveBeforeDelete',
  'Customer has open credit applications (Pending or Approved) and cannot be deleted.':
    'errors.hasOpenCreditApplications',
  'Gender must be one of: Female, Male, PreferNotToSay.': 'errors.invalidGender',
  'Staff user does not exist.': 'errors.staffNotExists',
  'User is not a staff account.': 'errors.staffNotStaffAccount',
  'You cannot change your own staff account status this way.': 'errors.staffCannotModifySelf',
  'You cannot deactivate your own staff account.': 'errors.staffCannotModifySelf',
  'Cannot deactivate the last active bank manager.': 'errors.staffCannotDeactivateLastManager',
  'Cannot remove manager role from the last active bank manager.':
    'errors.staffCannotDemoteLastManager',
  'Email already exists.': 'errors.emailAlreadyExists',
  'Email must not contain Turkish characters': 'errors.emailTurkishChars',
  'Email address is not confirmed. Please check your inbox.': 'errors.emailNotConfirmed',
  'Confirmation link is invalid or expired.': 'errors.confirmTokenInvalid',
  'Email address is already confirmed.': 'errors.emailAlreadyConfirmed',
  'Profile image file is required.': 'errors.avatarRequired',
  'Profile image must be at most 2 MB.': 'errors.avatarTooLarge',
  'Profile image must be JPEG, PNG, or WebP.': 'errors.avatarInvalidType',
  'Profile image content is invalid.': 'errors.avatarInvalidContent',
  'Document file is required.': 'errors.documentRequired',
  'Document must be at most 5 MB.': 'errors.documentTooLarge',
  'Document must be PDF, JPEG, or PNG.': 'errors.documentInvalidType',
  'Document content does not match the declared type.': 'errors.documentInvalidContent',
  'Document type is invalid.': 'errors.documentTypeInvalid',
  'Documents can only be uploaded while the application is Pending.':
    'errors.documentOnlyPending',
  'At most 10 documents are allowed per application.': 'errors.documentMaxExceeded',
  'Credit application document not found.': 'errors.documentNotFound',
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
