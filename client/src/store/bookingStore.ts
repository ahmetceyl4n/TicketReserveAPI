import { create } from 'zustand'
import type { Seat } from '@/types'

interface BookingState {
  /** Kullanıcının bu oturumda seçtiği (henüz kilitlenmemiş veya kilitli) koltuklar */
  selectedSeats: Seat[]
  /** Redis lock'ın dolacağı an (ms epoch); null = aktif kilit yok */
  lockExpiresAt: number | null
  toggleSeat: (seat: Seat) => void
  removeSeat: (seatId: string) => void
  setLock: (expiresAtUtc: string) => void
  clearSelection: () => void
}

export const useBookingStore = create<BookingState>()((set) => ({
  selectedSeats: [],
  lockExpiresAt: null,

  toggleSeat: (seat) =>
    set((state) => {
      const exists = state.selectedSeats.some((s) => s.id === seat.id)
      return {
        selectedSeats: exists
          ? state.selectedSeats.filter((s) => s.id !== seat.id)
          : [...state.selectedSeats, seat],
      }
    }),

  // SignalR "koltuk başkası tarafından alındı" event'i geldiğinde sepetten düşürmek için
  removeSeat: (seatId) =>
    set((state) => ({
      selectedSeats: state.selectedSeats.filter((s) => s.id !== seatId),
    })),

  setLock: (expiresAtUtc) => set({ lockExpiresAt: new Date(expiresAtUtc).getTime() }),

  clearSelection: () => set({ selectedSeats: [], lockExpiresAt: null }),
}))

export const selectTotalPrice = (state: BookingState): number =>
  state.selectedSeats.reduce((sum, seat) => sum + seat.price, 0)
