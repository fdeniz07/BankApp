import type { CreditTypeListItem, GetListResponse } from '../types/api'
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
  options?: { locale?: string; sortBy?: CreditTypeSortBy; sortDesc?: boolean },
) {
  const params = new URLSearchParams({
    PageIndex: String(pageIndex),
    PageSize: String(pageSize),
    SortBy: options?.sortBy ?? 'name',
    SortDesc: String(options?.sortDesc ?? false),
  })
  if (options?.locale) params.set('locale', options.locale)

  return apiRequest<GetListResponse<CreditTypeListItem>>(
    `/api/CreditTypes?${params}`,
  )
}
