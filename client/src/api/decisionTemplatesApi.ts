import { apiRequest } from './client'

export type DecisionTemplateKind = 'Approve' | 'Reject'

export interface DecisionLetterTemplateItem {
  id: string
  kind: DecisionTemplateKind | string
  locale: string
  name: string
  body: string
}

export interface UpsertDecisionLetterTemplateRequest {
  id?: string
  kind: DecisionTemplateKind
  locale: string
  name: string
  body: string
}

export function getDecisionLetterTemplates(kind?: DecisionTemplateKind) {
  const q = kind ? `?kind=${encodeURIComponent(kind)}` : ''
  return apiRequest<DecisionLetterTemplateItem[]>(`/api/DecisionLetterTemplates${q}`)
}

export function createDecisionLetterTemplate(body: UpsertDecisionLetterTemplateRequest) {
  return apiRequest<DecisionLetterTemplateItem>('/api/DecisionLetterTemplates', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function updateDecisionLetterTemplate(body: UpsertDecisionLetterTemplateRequest & { id: string }) {
  return apiRequest<DecisionLetterTemplateItem>('/api/DecisionLetterTemplates', {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}

export function deleteDecisionLetterTemplate(id: string) {
  return apiRequest<{ id: string }>(`/api/DecisionLetterTemplates/${id}`, {
    method: 'DELETE',
  })
}
