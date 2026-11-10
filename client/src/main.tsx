import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { ToastContainer } from 'react-toastify'
import { AuthProvider } from './auth/AuthContext'
import './i18n'
import './styles/tailwind.css'
import './index.css'
import 'react-toastify/dist/ReactToastify.css'
import App from './App.tsx'
import { applyA11yPrefs, BANKAPP_A11Y_KEYS, readA11yPrefs } from './a11y'

;(() => {
  applyA11yPrefs(readA11yPrefs(BANKAPP_A11Y_KEYS))
})()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <App />
        <ToastContainer
          position="top-center"
          autoClose={4500}
          newestOnTop
          closeOnClick
          pauseOnHover
          theme="colored"
        />
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
)
