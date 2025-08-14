import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import Dashboard from './pages/Dashboard'
import { AuthProvider } from './contexts/AuthContext'
import RequireAuth from './components/auth/RequireAuth'
import AuthPage from './pages/AuthPage'

function App() {
  return (
    <BrowserRouter basename={(import.meta.env.BASE_URL || '/').replace(/\/$/, '')}>
      <AuthProvider>
        <Routes>
          <Route path="/auth" element={<AuthPage />} />
          <Route element={<RequireAuth />}>
            <Route path="/" element={<Dashboard />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
