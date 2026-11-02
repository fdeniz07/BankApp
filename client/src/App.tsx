import { Route, Routes } from 'react-router-dom'
import { AccessibilityToolbar } from './components/AccessibilityToolbar'
import { AppLayout } from './components/AppLayout'
import { ConsentBanner } from './components/ConsentBanner'
import { HomeRedirect } from './components/HomeRedirect'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AdminDashboardPage } from './pages/AdminDashboardPage'
import { ApplicationDetailPage } from './pages/ApplicationDetailPage'
import { ApplicationsPage } from './pages/ApplicationsPage'
import { CreditTypesPage } from './pages/CreditTypesPage'
import { CustomerDetailPage } from './pages/CustomerDetailPage'
import { CustomersPage } from './pages/CustomersPage'
import { ConfirmEmailPage } from './pages/ConfirmEmailPage'
import { LoginPage } from './pages/LoginPage'
import { ProfilePage } from './pages/ProfilePage'
import { RegisterPage } from './pages/RegisterPage'
import { BranchesPage } from './pages/BranchesPage'
import { StaffPage } from './pages/StaffPage'

export default function App() {
  return (
    <>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/confirm-email" element={<ConfirmEmailPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route path="/dashboard" element={<AdminDashboardPage />} />
            <Route path="/applications" element={<ApplicationsPage />} />
            <Route path="/applications/:id" element={<ApplicationDetailPage />} />
            <Route path="/credit-types" element={<CreditTypesPage />} />
            <Route path="/customers" element={<CustomersPage />} />
            <Route path="/customers/:kind/:id" element={<CustomerDetailPage />} />
            <Route path="/staff" element={<StaffPage />} />
            <Route path="/branches" element={<BranchesPage />} />
            <Route path="/profile" element={<ProfilePage />} />
          </Route>
        </Route>
        <Route path="*" element={<HomeRedirect />} />
      </Routes>
      <AccessibilityToolbar />
      <ConsentBanner />
    </>
  )
}
