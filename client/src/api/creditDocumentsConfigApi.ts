import { apiRequest } from './client'

export type ApproveGateMode = 'None' | 'Soft' | 'Hard'

export type CreditDecisionConfig = {
  approveGateMode: ApproveGateMode | string
  softApproveGateEnabled: boolean
  approvalAuthorityEnabled: boolean
  authorityCurrencyCode?: string | null
  maxApprovalAmount: number | null
  positionMaxApprovalAmount?: number | null
  positionCurrencyCode?: string | null
  branchMaxApprovalAmount?: number | null
  branchCurrencyCode?: string | null
}

export function getCreditDocumentsConfig(currencyCode?: string | null) {
  const params = new URLSearchParams()
  if (currencyCode) params.set('currencyCode', currencyCode)
  const q = params.toString()
  return apiRequest<CreditDecisionConfig>(`/api/CreditDocuments/config${q ? `?${q}` : ''}`)
}
