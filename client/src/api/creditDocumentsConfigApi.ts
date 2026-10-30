import { apiRequest } from './client'

export type CreditDocumentsConfig = {
  softApproveGateEnabled: boolean
}

export function getCreditDocumentsConfig() {
  return apiRequest<CreditDocumentsConfig>('/api/CreditDocuments/config')
}
