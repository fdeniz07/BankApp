import type { LoggedResponse, RegisterIndividualRequest } from '../types/api'
import { apiRequest } from './client'

export function login(email: string, password: string) {
  return apiRequest<LoggedResponse>('/api/Auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  })
}

export function registerIndividual(body: RegisterIndividualRequest) {
  return apiRequest<LoggedResponse>('/api/Auth/register-individual', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}
