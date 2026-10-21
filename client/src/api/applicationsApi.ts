import type {
  ApplicationSortBy,
  ApproveRequest,
  CreateApplicationRequest,
  CreditApplicationDetail,
  CreditApplicationListItem,
  CreditApplicationStatus,
  GetListResponse,
  PaymentPlanResponse,
  RejectRequest,
} from '../types/api'
import { apiRequest } from './client'

export interface GetApplicationsParams {
  pageIndex?: number
  pageSize?: number
  status?: CreditApplicationStatus | ''
  sortBy?: ApplicationSortBy
  sortDesc?: boolean
}

export function getApplications({
  pageIndex = 0,
  pageSize = 10,
  status = '',
  sortBy = 'applicationDate',
  sortDesc = true,
}: GetApplicationsParams = {}) {
  const params = new URLSearchParams({
    PageIndex: String(pageIndex),
    PageSize: String(pageSize),
    SortBy: sortBy,
    SortDesc: String(sortDesc),
  })
  if (status) params.set('status', status)

  return apiRequest<GetListResponse<CreditApplicationListItem>>(
    `/api/CreditApplications?${params}`,
  )
}

export function getApplicationById(id: string) {
  return apiRequest<CreditApplicationDetail>(`/api/CreditApplications/${id}`)
}

export function getPaymentPlan(params: {
  creditSubTypeId?: string
  amount: number
  termMonths: number
  interestRate?: number
}) {
  const search = new URLSearchParams({
    amount: String(params.amount),
    termMonths: String(params.termMonths),
  })
  if (params.creditSubTypeId) search.set('creditSubTypeId', params.creditSubTypeId)
  if (params.interestRate != null) search.set('interestRate', String(params.interestRate))

  return apiRequest<PaymentPlanResponse>(
    `/api/CreditApplications/payment-plan?${search}`,
  )
}

function withIdempotencyKey(idempotencyKey?: string): HeadersInit {
  return { 'Idempotency-Key': idempotencyKey ?? crypto.randomUUID() }
}

export function createApplication(body: CreateApplicationRequest, idempotencyKey?: string) {
  return apiRequest<CreditApplicationDetail>(`/api/CreditApplications`, {
    method: 'POST',
    body: JSON.stringify(body),
    headers: withIdempotencyKey(idempotencyKey),
  })
}

export function cancelApplication(id: string, notes?: string, idempotencyKey?: string) {
  return apiRequest(`/api/CreditApplications/${id}/cancel`, {
    method: 'POST',
    body: JSON.stringify({ notes: notes ?? null }),
    headers: withIdempotencyKey(idempotencyKey),
  })
}

export function approveApplication(id: string, body: ApproveRequest, idempotencyKey?: string) {
  return apiRequest(`/api/CreditApplications/${id}/approve`, {
    method: 'POST',
    body: JSON.stringify(body),
    headers: withIdempotencyKey(idempotencyKey),
  })
}

export function rejectApplication(id: string, body: RejectRequest, idempotencyKey?: string) {
  return apiRequest(`/api/CreditApplications/${id}/reject`, {
    method: 'POST',
    body: JSON.stringify(body),
    headers: withIdempotencyKey(idempotencyKey),
  })
}
