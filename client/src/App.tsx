import type { ReactNode } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { Navbar } from '@/components/Navbar'
import { ToastContainer } from '@/components/ToastContainer'
import { selectIsAdmin, useAuthStore } from '@/store/authStore'
import { LoginPage } from '@/features/auth/LoginPage'
import { HomePage } from '@/features/events/HomePage'
import { SeatSelectionPage } from '@/features/booking/SeatSelectionPage'
import { AdminPage } from '@/features/admin/AdminPage'

/**
 * UI guard'ı bir güvenlik önlemi DEĞİLDİR — sadece kullanıcı deneyimi.
 * Gerçek yetki kontrolü her zaman backend'de ([Authorize(Roles="Admin")]).
 * Token'ı elle düzenleyen biri bu sayfayı görebilir ama API 403 döner.
 */
function RequireAdmin({ children }: { children: ReactNode }) {
  const isAdmin = useAuthStore(selectIsAdmin)
  return isAdmin ? children : <Navigate to="/login" replace />
}

export default function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/events/:eventId/seats" element={<SeatSelectionPage />} />
        <Route
          path="/admin"
          element={
            <RequireAdmin>
              <AdminPage />
            </RequireAdmin>
          }
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      <ToastContainer />
    </BrowserRouter>
  )
}
