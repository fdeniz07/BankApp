import { Navigate, Route, Routes } from 'react-router-dom'
import { AccessibilityToolbar } from './components/AccessibilityToolbar'
import { AppLayout } from './components/AppLayout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { ApplicationDetailPage } from './pages/ApplicationDetailPage'
import { ApplicationsPage } from './pages/ApplicationsPage'
import { CreditTypesPage } from './pages/CreditTypesPage'
import { CustomerDetailPage } from './pages/CustomerDetailPage'
import { CustomersPage } from './pages/CustomersPage'
import { LoginPage } from './pages/LoginPage'
import { ProfilePage } from './pages/ProfilePage'
import { RegisterPage } from './pages/RegisterPage'
import { StaffPage } from './pages/StaffPage'

export default function App() {
  return (
    <>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route path="/applications" element={<ApplicationsPage />} />
            <Route path="/applications/:id" element={<ApplicationDetailPage />} />
            <Route path="/credit-types" element={<CreditTypesPage />} />
            <Route path="/customers" element={<CustomersPage />} />
            <Route path="/customers/:kind/:id" element={<CustomerDetailPage />} />
            <Route path="/staff" element={<StaffPage />} />
            <Route path="/profile" element={<ProfilePage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/applications" replace />} />
      </Routes>
      <AccessibilityToolbar />
    </>
  )
}
