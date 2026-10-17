import type {
  CreateCreditTypeRequest,
  CreditTypeListItem,
  GetListResponse,
  SetCreditTypeActiveResponse,
} from '../types/api'
import { apiRequest } from './client'

export type CreditTypeSortBy =
  | 'name'
  | 'code'
  | 'locale'
  | 'category'
  | 'currencyCode'
  | 'riskLevel'
  | 'isActive'

export function getCreditTypes(
  pageIndex = 0,
  pageSize = 50,
  options?: {
    locale?: string
    category?: string
    sortBy?: CreditTypeSortBy
    sortDesc?: boolean
  },
) {
  const params = new URLSearchParams({
    PageIndex: String(pageIndex),
    PageSize: String(pageSize),
    SortBy: options?.sortBy ?? 'name',
    SortDesc: String(options?.sortDesc ?? false),
  })
  if (options?.locale) params.set('locale', options.locale)
  if (options?.category) params.set('category', options.category)

  return apiRequest<GetListResponse<CreditTypeListItem>>(
    `/api/CreditTypes?${params}`,
  )
}

export function createCreditType(body: CreateCreditTypeRequest) {
  return apiRequest<CreditTypeListItem>('/api/CreditTypes', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function setCreditTypeActive(id: string, isActive: boolean) {
  return apiRequest<SetCreditTypeActiveResponse>(`/api/CreditTypes/${id}/active`, {
    method: 'PUT',
    body: JSON.stringify({ isActive }),
  })
}
