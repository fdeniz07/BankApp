import type { MyProfile, UpdateStaffUserRequest, UploadProfileImageResponse } from '../types/api'
import { apiRequest, apiUpload } from './client'

export function getMyProfile() {
  return apiRequest<MyProfile>('/api/Profile')
}

export function updateMyStaffProfile(body: Omit<UpdateStaffUserRequest, 'id'>) {
  return apiRequest<MyProfile & { isManager?: boolean }>('/api/Profile/staff', {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}

export function uploadProfileAvatar(file: File) {
  const form = new FormData()
  form.append('file', file)
  return apiUpload<UploadProfileImageResponse>('/api/Profile/avatar', form)
}

export function resolveProfileImageUrl(
  path?: string | null,
  options?: { cacheBust?: boolean },
): string | null {
  if (!path) return null
  let url = path
  if (!path.startsWith('http://') && !path.startsWith('https://')) {
    const base = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'
    url = `${base.replace(/\/$/, '')}/${path.replace(/^\//, '')}`
  }
  if (options?.cacheBust) {
    const sep = url.includes('?') ? '&' : '?'
    url = `${url}${sep}v=${Date.now()}`
  }
  return url
}
