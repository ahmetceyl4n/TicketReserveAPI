import type { LockSeatsResult, Seat } from '@/types'
import { USE_MOCKS, apiClient } from '@/lib/apiClient'
import { mockDb } from '@/lib/mock/db'
import { delay, mockError } from '@/lib/mock/helpers'
import { mockSeatHub } from '@/lib/signalr/mockSeatHub'

const LOCK_DURATION_MS = 10 * 60 * 1000

interface BookingApi {
  getSeats(eventId: string): Promise<Seat[]>
  /** Redis SET NX EX karşılığı: koltuklar 10 dk kullanıcı adına kilitlenir */
  lockSeats(eventId: string, seatIds: string[]): Promise<LockSeatsResult>
  confirmPurchase(eventId: string, seatIds: string[]): Promise<void>
  releaseLock(eventId: string, seatIds: string[]): Promise<void>
}

const httpBookingApi: BookingApi = {
  async getSeats(eventId) {
    const { data } = await apiClient.get<Seat[]>(`/events/${eventId}/seats`)
    return data
  },
  async lockSeats(eventId, seatIds) {
    const { data } = await apiClient.post<LockSeatsResult>(`/events/${eventId}/seats/lock`, { seatIds })
    return data
  },
  async confirmPurchase(eventId, seatIds) {
    await apiClient.post(`/events/${eventId}/seats/purchase`, { seatIds })
  },
  async releaseLock(eventId, seatIds) {
    await apiClient.post(`/events/${eventId}/seats/release`, { seatIds })
  },
}

const mockBookingApi: BookingApi = {
  async getSeats(eventId) {
    await delay()
    if (!mockDb.getEvent(eventId)) {
      throw mockError('Event.NotFound', 'Etkinlik bulunamadı.', 404)
    }
    // Kopya döndürülür: UI kendi state'ini mutasyondan bağımsız yönetsin
    return mockDb.getSeats(eventId).map((seat) => ({ ...seat }))
  },

  async lockSeats(eventId, seatIds) {
    await delay(500)
    const seats = mockDb.getSeats(eventId)

    // Redis'teki NX davranışının taklidi: tek koltuk bile alınmışsa
    // işlem tümüyle reddedilir — kısmi kilit, kullanıcıya yarım sepet bırakır.
    const conflicted = seatIds.filter((id) => {
      const seat = seats.find((s) => s.id === id)
      return !seat || seat.status !== 'Available'
    })
    if (conflicted.length > 0) {
      throw mockError(
        'Seat.AlreadyLocked',
        'Seçtiğiniz koltuklardan bazıları başka bir kullanıcı tarafından alındı.',
        409,
      )
    }

    for (const id of seatIds) {
      const seat = mockDb.updateSeatStatus(eventId, id, 'Locked')
      if (seat) {
        mockSeatHub.emit({ eventId, seatId: id, row: seat.row, number: seat.number, status: 'Locked' })
      }
    }

    return {
      lockedSeatIds: seatIds,
      expiresAtUtc: new Date(Date.now() + LOCK_DURATION_MS).toISOString(),
    }
  },

  async confirmPurchase(eventId, seatIds) {
    await delay(800)
    for (const id of seatIds) {
      const seat = mockDb.updateSeatStatus(eventId, id, 'Sold')
      if (seat) {
        mockSeatHub.emit({ eventId, seatId: id, row: seat.row, number: seat.number, status: 'Sold' })
      }
    }
  },

  async releaseLock(eventId, seatIds) {
    await delay(150)
    for (const id of seatIds) {
      const seat = mockDb.updateSeatStatus(eventId, id, 'Available')
      if (seat) {
        mockSeatHub.emit({ eventId, seatId: id, row: seat.row, number: seat.number, status: 'Available' })
      }
    }
  },
}

export const bookingApi: BookingApi = USE_MOCKS ? mockBookingApi : httpBookingApi
