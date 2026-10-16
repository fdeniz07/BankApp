import type { DashboardSummary } from '../types/api'
import { apiRequest } from './client'

export function getDashboardSummary() {
  return apiRequest<DashboardSummary>('/api/Dashboard/summary')
}
