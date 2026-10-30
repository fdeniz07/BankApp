import type { CreditApplicationDocumentItem, CreditDocumentType } from '../types/api'

export const REQUIRED_DOCUMENT_TYPES: CreditDocumentType[] = ['IdentityDocument', 'IncomeProof']

export function missingRequiredDocumentTypes(
  documents: Array<Pick<CreditApplicationDocumentItem, 'documentType'>>,
): CreditDocumentType[] {
  const present = new Set(
    documents.map((d) => String(d.documentType)).filter(Boolean),
  )
  return REQUIRED_DOCUMENT_TYPES.filter((type) => !present.has(type))
}

export function hasMissingRequiredDocuments(
  documents: Array<Pick<CreditApplicationDocumentItem, 'documentType'>>,
): boolean {
  return missingRequiredDocumentTypes(documents).length > 0
}
