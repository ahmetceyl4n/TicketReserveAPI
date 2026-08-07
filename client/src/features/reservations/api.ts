import type { ReservationSummary } from '@/types'
import { USE_MOCKS, apiClient } from '@/lib/apiClient'
import { delay } from '@/lib/mock/helpers'

interface ReservationsApi {
  getMyReservations(): Promise<ReservationSummary[]>
  cancelReservation(reservationId: string): Promise<void>
}

const httpReservationsApi: ReservationsApi = {
  async getMyReservations() {
    const { data } = await apiClient.get<ReservationSummary[]>('/reservations/me')
    return data
  },
  async cancelReservation(reservationId) {
    await apiClient.delete(`/reservations/${reservationId}`)
  },
}

// Mock modda rezervasyon akışı simüle edilmiyor (satın alma da mock'ta kalıcı değil) —
// boş liste dönüyoruz ki sayfa mock modda da çökmeden "henüz bilet yok" göstersin.
const mockReservationsApi: ReservationsApi = {
  async getMyReservations() {
    await delay()
    return []
  },
  async cancelReservation() {
    await delay()
  },
}

export const reservationsApi: ReservationsApi = USE_MOCKS ? mockReservationsApi : httpReservationsApi
