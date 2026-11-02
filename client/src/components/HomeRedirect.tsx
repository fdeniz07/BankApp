import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { homePath } from '../utils/homePath'

/** Role-aware default landing after login / unknown routes. */
export function HomeRedirect() {
  const { isManager } = useAuth()
  return <Navigate to={homePath(isManager)} replace />
}
