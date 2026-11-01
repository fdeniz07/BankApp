import { apiRequest } from './client'

export type ApproveGateMode = 'None' | 'Soft' | 'Hard'

export type CreditDecisionConfig = {
  approveGateMode: ApproveGateMode | string
  softApproveGateEnabled: boolean
  approvalAuthorityEnabled: boolean
  maxApprovalAmount: number | null
}

export function getCreditDocumentsConfig() {
  return apiRequest<CreditDecisionConfig>('/api/CreditDocuments/config')
}
