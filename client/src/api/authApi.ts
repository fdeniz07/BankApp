import type {
  ConfirmEmailResponse,
  LoggedResponse,
  RegisterIndividualRequest,
  RegisteredIndividualResponse,
} from '../types/api'
import { apiRequest } from './client'

export function login(email: string, password: string) {
  return apiRequest<LoggedResponse>('/api/Auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  })
}

export function registerIndividual(body: RegisterIndividualRequest) {
  return apiRequest<RegisteredIndividualResponse>('/api/Auth/register-individual', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function confirmEmail(token: string) {
  return apiRequest<ConfirmEmailResponse>('/api/Auth/confirm-email', {
    method: 'POST',
    body: JSON.stringify({ token }),
  })
}
