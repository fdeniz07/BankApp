import type {
  CreateStaffUserRequest,
  GetListResponse,
  StaffUserListItem,
  UpdateStaffUserRequest,
} from '../types/api'
import { apiRequest } from './client'

export function getStaffUsers(pageIndex = 0, pageSize = 10, isActive?: boolean | null) {
  const params = new URLSearchParams({
    PageIndex: String(pageIndex),
    PageSize: String(pageSize),
  })
  if (isActive === true || isActive === false) {
    params.set('isActive', String(isActive))
  }

  return apiRequest<GetListResponse<StaffUserListItem>>(`/api/StaffUsers?${params}`)
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
