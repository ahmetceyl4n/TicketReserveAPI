import type { EventSummary, Seat, SeatStatus } from '@/types'
import { hashString, seededRandom } from './helpers'

/**
 * In-memory mock veritabanı. Modül seviyesinde tek instance —
 * sayfalar arası gezinirken state korunur (SPA yaşadığı sürece),
 * tam yenilemede seed'li üretim sayesinde aynı düzen geri gelir.
 */

export const SEAT_ROWS = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'] as const
export const SEATS_PER_ROW = 14

const events: EventSummary[] = [
  {
    id: 'evt-cyberpunk-symphony',
    name: 'Cyberpunk Symphony',
    description: 'An immersive orchestral experience featuring futuristic soundscapes.',
    venue: 'Neo-Tokyo Arena',
    date: '2026-11-12T23:00:00Z',
    imageUrl: null,
  },
  {
    id: 'evt-galactic-esports',
    name: 'Galactic eSports Finals',
    description: 'The ultimate championship match for the planetary league.',
    venue: 'The Quantum Dome',
    date: '2026-12-05T21:00:00Z',
    imageUrl: null,
  },
  {
    id: 'evt-holographic-theater',
    name: 'Holographic Theater: The Void',
    description: 'A mind-bending theatrical performance using cutting-edge holograms.',
    venue: 'Starlight Pavilion',
    date: '2026-11-01T00:00:00Z',
    imageUrl: null,
  },
]

const seatsByEvent = new Map<string, Seat[]>()

function generateSeats(eventId: string): Seat[] {
  const random = seededRandom(hashString(eventId))
  const seats: Seat[] = []

  for (const row of SEAT_ROWS) {
    for (let number = 1; number <= SEATS_PER_ROW; number++) {
      const roll = random()
      // Dağılım ekran görüntülerine göre ayarlı: ~%15 satılmış, ~%10 işlemde
      const status: SeatStatus = roll < 0.15 ? 'Sold' : roll < 0.25 ? 'Locked' : 'Available'
      seats.push({
        id: `${eventId}:${row}${number}`,
        row,
        number,
        price: 120 + Math.floor(random() * 60),
        status,
      })
    }
  }
  return seats
}

export const mockDb = {
  getEvents(): EventSummary[] {
    return [...events]
  },

  getEvent(id: string): EventSummary | undefined {
    return events.find((e) => e.id === id)
  },

  addEvent(event: EventSummary): void {
    events.unshift(event)
  },

  removeEvent(id: string): boolean {
    const index = events.findIndex((e) => e.id === id)
    if (index === -1) return false
    events.splice(index, 1)
    seatsByEvent.delete(id)
    return true
  },

  getSeats(eventId: string): Seat[] {
    let seats = seatsByEvent.get(eventId)
    if (!seats) {
      seats = generateSeats(eventId)
      seatsByEvent.set(eventId, seats)
    }
    return seats
  },

  updateSeatStatus(eventId: string, seatId: string, status: SeatStatus): Seat | undefined {
    const seat = this.getSeats(eventId).find((s) => s.id === seatId)
    if (seat) seat.status = status
    return seat
  },
}
