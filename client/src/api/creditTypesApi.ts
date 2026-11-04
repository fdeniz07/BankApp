import type {
  CreateCreditTypeRequest,
  CreditTypeFacets,
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
    search?: string
    currencyCode?: string
    riskLevel?: string
    isActive?: boolean | null
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
  if (options?.search?.trim()) params.set('search', options.search.trim())
  if (options?.currencyCode) params.set('currencyCode', options.currencyCode)
  if (options?.riskLevel) params.set('riskLevel', options.riskLevel)
  if (options?.isActive === true || options?.isActive === false) {
    params.set('isActive', String(options.isActive))
  }

  return apiRequest<GetListResponse<CreditTypeListItem>>(`/api/CreditTypes?${params}`)
}

export function getCreditTypeFacets(options?: {
  search?: string
  locale?: string
  category?: string
  currencyCode?: string
  riskLevel?: string
  isActive?: boolean | null
}) {
  const params = new URLSearchParams()
  if (options?.search?.trim()) params.set('search', options.search.trim())
  if (options?.locale) params.set('locale', options.locale)
  if (options?.category) params.set('category', options.category)
  if (options?.currencyCode) params.set('currencyCode', options.currencyCode)
  if (options?.riskLevel) params.set('riskLevel', options.riskLevel)
  if (options?.isActive === true || options?.isActive === false) {
    params.set('isActive', String(options.isActive))
  }
  const qs = params.toString()
  return apiRequest<CreditTypeFacets>(`/api/CreditTypes/facets${qs ? `?${qs}` : ''}`)
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
