import type { SeatStatusChangedEvent } from '@/types'
import { mockDb } from '@/lib/mock/db'

type SeatEventHandler = (event: SeatStatusChangedEvent) => void

/**
 * Gerçek KoltukHub'ın yerine geçen simülasyon motoru.
 * Backend'deki SignalR group mantığını taklit eder: her event (etkinlik)
 * kendi "grubu"na sahiptir, abone yalnızca baktığı etkinliğin olaylarını alır.
 * İlk abone geldiğinde simülasyon başlar, son abone ayrılınca durur —
 * timer sızıntısı olmasın diye.
 */
class MockSeatHub {
  private handlers = new Map<string, Set<SeatEventHandler>>()
  private timers = new Map<string, ReturnType<typeof setTimeout>>()

  subscribe(eventId: string, handler: SeatEventHandler): () => void {
    let group = this.handlers.get(eventId)
    if (!group) {
      group = new Set()
      this.handlers.set(eventId, group)
    }
    group.add(handler)

    if (!this.timers.has(eventId)) {
      this.scheduleNextTick(eventId)
    }

    return () => {
      group.delete(handler)
      if (group.size === 0) {
        this.handlers.delete(eventId)
        const timer = this.timers.get(eventId)
        if (timer) clearTimeout(timer)
        this.timers.delete(eventId)
      }
    }
  }

  /** Mock api'ler (ör. satın alma onayı) durum değişikliğini buradan yayınlar */
  emit(event: SeatStatusChangedEvent): void {
    this.handlers.get(event.eventId)?.forEach((handler) => handler(event))
  }

  private scheduleNextTick(eventId: string): void {
    const delay = 4000 + Math.random() * 4000
    const timer = setTimeout(() => {
      this.simulateOtherUsers(eventId)
      if (this.handlers.has(eventId)) {
        this.scheduleNextTick(eventId)
      }
    }, delay)
    this.timers.set(eventId, timer)
  }

  /** Başka kullanıcıların koltuk kilitleme/satın alma davranışını taklit eder */
  private simulateOtherUsers(eventId: string): void {
    const seats = mockDb.getSeats(eventId)
    const locked = seats.filter((s) => s.status === 'Locked')
    const available = seats.filter((s) => s.status === 'Available')

    // Kilitli koltuklar zamanla ya satılır ya serbest kalır; boşlar kilitlenir.
    // Önce kilitlileri çözmek, grid'in tamamen kilitle dolmasını engeller.
    const resolveLock = locked.length > 0 && Math.random() < 0.5
    const seat = resolveLock
      ? locked[Math.floor(Math.random() * locked.length)]
      : available[Math.floor(Math.random() * available.length)]

    if (!seat) return

    const nextStatus = resolveLock ? (Math.random() < 0.6 ? 'Sold' : 'Available') : 'Locked'

    mockDb.updateSeatStatus(eventId, seat.id, nextStatus)
    this.emit({
      eventId,
      seatId: seat.id,
      row: seat.row,
      number: seat.number,
      status: nextStatus,
    })
  }
}

export const mockSeatHub = new MockSeatHub()
