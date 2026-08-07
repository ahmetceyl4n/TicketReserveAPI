import type { ReactNode } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { Navbar } from '@/components/Navbar'
import { ToastContainer } from '@/components/ToastContainer'
import { selectIsAdmin, useAuthStore } from '@/store/authStore'
import { LoginPage } from '@/features/auth/LoginPage'
import { RegisterPage } from '@/features/auth/RegisterPage'
import { HomePage } from '@/features/events/HomePage'
import { SeatSelectionPage } from '@/features/booking/SeatSelectionPage'
import { AdminPage } from '@/features/admin/AdminPage'
import { MyReservationsPage } from '@/features/reservations/MyReservationsPage'

/**
 * UI guard'ı bir güvenlik önlemi DEĞİLDİR — sadece kullanıcı deneyimi.
 * Gerçek yetki kontrolü her zaman backend'de ([Authorize(Roles="Admin")]).
 * Token'ı elle düzenleyen biri bu sayfayı görebilir ama API 403 döner.
 */
function RequireAdmin({ children }: { children: ReactNode }) {
  const isAdmin = useAuthStore(selectIsAdmin)
  return isAdmin ? children : <Navigate to="/login" replace />
}

// Sadece giriş yapmış kullanıcı; RequireAdmin'in rol-kontrolsüz versiyonu.
// UI guard'ı = UX; asıl koruma backend'de (/reservations/me [Authorize]).
function RequireAuth({ children }: { children: ReactNode }) {
  const user = useAuthStore((state) => state.user)
  return user ? children : <Navigate to="/login" replace />
}

export default function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/events/:eventId/seats" element={<SeatSelectionPage />} />
        <Route
          path="/reservations"
          element={
            <RequireAuth>
              <MyReservationsPage />
            </RequireAuth>
          }
        />
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
