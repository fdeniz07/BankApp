import type {
  CorporateCustomerCatalog,
  CorporateCustomerDetail,
  CorporateCustomerListItem,
  CreateCorporateCustomerRequest,
  CreateIndividualCustomerRequest,
  CustomerFacets,
  GetListResponse,
  IndividualCustomerDetail,
  IndividualCustomerListItem,
} from '../types/api'
import { apiRequest } from './client'

export type CustomerListScope = 'active' | 'inactive' | 'deleted'

function listPath(
  base: string,
  scope: CustomerListScope,
  pageIndex: number,
  pageSize: number,
  branchId?: string | null,
  search?: string | null,
) {
  const qs = new URLSearchParams({
    PageIndex: String(pageIndex),
    PageSize: String(pageSize),
  })
  if (branchId) qs.set('branchId', branchId)
  if (search?.trim()) qs.set('search', search.trim())
  if (scope === 'active') {
    qs.set('isActive', 'true')
    return `${base}?${qs}`
  }
  if (scope === 'inactive') return `${base}/inactive?${qs}`
  return `${base}/deleted?${qs}`
}

export function getIndividualCustomers(
  scope: CustomerListScope,
  pageIndex = 0,
  pageSize = 10,
  branchId?: string | null,
  search?: string | null,
) {
  return apiRequest<GetListResponse<IndividualCustomerListItem>>(
    listPath('/api/IndividualCustomers', scope, pageIndex, pageSize, branchId, search),
  )
}

export function getCorporateCustomers(
  scope: CustomerListScope,
  pageIndex = 0,
  pageSize = 10,
  branchId?: string | null,
  search?: string | null,
) {
  return apiRequest<GetListResponse<CorporateCustomerListItem>>(
    listPath('/api/CorporateCustomers', scope, pageIndex, pageSize, branchId, search),
  )
}

export function getCustomerFacets(params: {
  kind: 'Individual' | 'Corporate'
  scope: CustomerListScope
  branchId?: string | null
  search?: string | null
}) {
  const qs = new URLSearchParams({ kind: params.kind })
  if (params.branchId) qs.set('branchId', params.branchId)
  if (params.search?.trim()) qs.set('search', params.search.trim())
  if (params.scope === 'active') qs.set('isActive', 'true')
  else if (params.scope === 'inactive') qs.set('isActive', 'false')
  else qs.set('onlyDeleted', 'true')
  return apiRequest<CustomerFacets>(`/api/Customers/facets?${qs}`)
}

export function getIndividualCustomer(id: string) {
  return apiRequest<IndividualCustomerDetail>(`/api/IndividualCustomers/${id}`)
}

export function getCorporateCustomer(id: string) {
  return apiRequest<CorporateCustomerDetail>(`/api/CorporateCustomers/${id}`)
}

export function getCorporateCustomerCatalog() {
  return apiRequest<CorporateCustomerCatalog>('/api/CorporateCustomers/catalog')
}

export function createIndividualCustomer(body: CreateIndividualCustomerRequest) {
  return apiRequest<IndividualCustomerDetail>('/api/IndividualCustomers', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function updateIndividualCustomer(body: IndividualCustomerDetail) {
  return apiRequest<IndividualCustomerDetail>('/api/IndividualCustomers', {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}

export function deleteIndividualCustomer(id: string) {
  return apiRequest<IndividualCustomerDetail>(`/api/IndividualCustomers/${id}`, {
    method: 'DELETE',
  })
}

export function createCorporateCustomer(body: CreateCorporateCustomerRequest) {
  return apiRequest<CorporateCustomerDetail>('/api/CorporateCustomers', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function updateCorporateCustomer(body: CorporateCustomerDetail) {
  return apiRequest<CorporateCustomerDetail>('/api/CorporateCustomers', {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}

export function deleteCorporateCustomer(id: string) {
  return apiRequest<CorporateCustomerDetail>(`/api/CorporateCustomers/${id}`, {
    method: 'DELETE',
  })
}
