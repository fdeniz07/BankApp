import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { ToastContainer } from 'react-toastify'
import { AuthProvider } from './auth/AuthContext'
import './i18n'
import './index.css'
import 'react-toastify/dist/ReactToastify.css'
import App from './App.tsx'

;(() => {
  const root = document.documentElement
  const theme = localStorage.getItem('bankapp-theme') === 'dark' ? 'dark' : 'light'
  const contrast = localStorage.getItem('bankapp-contrast') === '1' ? 'high' : 'normal'
  const font = Number(localStorage.getItem('bankapp-font-scale') ?? '100')
  const scale = font === 112 || font === 125 || font === 140 ? font / 100 : 1
  root.setAttribute('data-theme', theme)
  root.setAttribute('data-contrast', contrast)
  root.style.setProperty('--font-scale', String(scale))
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
