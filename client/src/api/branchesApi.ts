import type { BranchListItem, CreateBranchRequest, UpdateBranchRequest } from '../types/api'
import { apiRequest } from './client'

export function getBranches(isActive: boolean | null = true) {
  const params = new URLSearchParams()
  if (isActive !== null) params.set('isActive', String(isActive))
  const q = params.toString()
  return apiRequest<BranchListItem[]>(`/api/Branches${q ? `?${q}` : ''}`)
}

export function createBranch(body: CreateBranchRequest) {
  return apiRequest<BranchListItem>('/api/Branches', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function updateBranch(body: UpdateBranchRequest) {
  return apiRequest<BranchListItem>('/api/Branches', {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}
