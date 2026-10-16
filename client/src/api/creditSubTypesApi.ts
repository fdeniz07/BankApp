import type { CreditCategory, CreditSubTypeListItem, GetListResponse } from '../types/api'
import { apiRequest } from './client'

export function getCreditSubTypes(
  pageIndex = 0,
  pageSize = 100,
  options?: { creditTypeId?: string; locale?: string; category?: CreditCategory },
) {
  const params = new URLSearchParams({
    PageIndex: String(pageIndex),
    PageSize: String(pageSize),
  })
  if (options?.creditTypeId) params.set('creditTypeId', options.creditTypeId)
  if (options?.locale) params.set('locale', options.locale)
  if (options?.category) params.set('category', options.category)

  return apiRequest<GetListResponse<CreditSubTypeListItem>>(
    `/api/CreditSubTypes?${params}`,
  )
}
