import type {
  CreateStaffUserRequest,
  GetListResponse,
  StaffUserListItem,
  UpdateStaffUserRequest,
} from '../types/api'
import { apiRequest } from './client'

export type StaffListParams = {
  pageIndex?: number
  pageSize?: number
  isActive?: boolean | null
  isManager?: boolean | null
  branchId?: string | null
  search?: string | null
}

export function getStaffUsers(params: StaffListParams = {}) {
  const qs = new URLSearchParams({
    PageIndex: String(params.pageIndex ?? 0),
    PageSize: String(params.pageSize ?? 10),
  })
  if (params.isActive === true || params.isActive === false) {
    qs.set('isActive', String(params.isActive))
  }
  if (params.isManager === true || params.isManager === false) {
    qs.set('isManager', String(params.isManager))
  }
  if (params.branchId) qs.set('branchId', params.branchId)
  if (params.search?.trim()) qs.set('search', params.search.trim())

  return apiRequest<GetListResponse<StaffUserListItem>>(`/api/StaffUsers?${qs}`)
}

export function createStaffUser(body: CreateStaffUserRequest) {
  return apiRequest<StaffUserListItem>('/api/StaffUsers', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function updateStaffUser(body: UpdateStaffUserRequest) {
  return apiRequest<StaffUserListItem>('/api/StaffUsers', {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}
