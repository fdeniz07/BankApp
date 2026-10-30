import type {
  CorporateCustomerCatalog,
  CorporateCustomerDetail,
  CorporateCustomerListItem,
  CreateCorporateCustomerRequest,
  CreateIndividualCustomerRequest,
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
) {
  const qs = new URLSearchParams({
    PageIndex: String(pageIndex),
    PageSize: String(pageSize),
  })
  if (branchId) qs.set('branchId', branchId)
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
) {
  return apiRequest<GetListResponse<IndividualCustomerListItem>>(
    listPath('/api/IndividualCustomers', scope, pageIndex, pageSize, branchId),
  )
}

export function getCorporateCustomers(
  scope: CustomerListScope,
  pageIndex = 0,
  pageSize = 10,
  branchId?: string | null,
) {
  return apiRequest<GetListResponse<CorporateCustomerListItem>>(
    listPath('/api/CorporateCustomers', scope, pageIndex, pageSize, branchId),
  )
}

export function getIndividualCustomer(id: string) {
  return apiRequest<IndividualCustomerDetail>(`/api/IndividualCustomers/${id}`)
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

export function getCorporateCustomer(id: string) {
  return apiRequest<CorporateCustomerDetail>(`/api/CorporateCustomers/${id}`)
}

export function getCorporateCustomerCatalog() {
  return apiRequest<CorporateCustomerCatalog>('/api/CorporateCustomers/catalog')
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
