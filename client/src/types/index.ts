/**
 * Backend'deki (TicketReserve.Api) DTO'ların frontend karşılıkları.
 * Backend endpoint'leri hazır olduğunda alan adları buradakilerle
 * birebir eşleşmeli — tek doğruluk kaynağı backend sözleşmesidir.
 */

export type SeatStatus = 'Available' | 'Locked' | 'Sold'

export interface Seat {
  id: string
  row: string
  number: number
  price: number
  status: SeatStatus
}

export interface EventSummary {
  id: string
  name: string
  description: string
  venue: string
  /** ISO 8601 — backend DateTime'ı UTC gönderir, formatlama UI'da yapılır */
  date: string
  imageUrl?: string | null
}

export interface CreateEventCommand {
  name: string
  description: string
  venue: string
  date: string
}

export interface CreateSeatsCommand {
  /** Tek harf, A-Z */
  startRow: string
  endRow: string
  seatsPerRow: number
  price: number
}

export interface User {
  id: string
  email: string
  fullName: string
  roles: string[]
}

export interface AuthResponse {
  accessToken: string
}

export interface LoginCommand {
  email: string
  password: string
}

export interface RegisterCommand {
  email: string
  password: string
  fullName: string
}

/** Redis seat lock'ın HTTP'ye yansıyan sonucu */
export interface LockSeatsResult {
  lockedSeatIds: string[]
  /** ISO 8601 — ödeme sayacı bu ana göre geriye sayar */
  expiresAtUtc: string
}

/** SignalR KoltukHub'ın yayınladığı event'in sözleşmesi */
export interface SeatStatusChangedEvent {
  eventId: string
  seatId: string
  row: string
  number: number
  status: SeatStatus
}

/**
 * Backend ProblemDetails (RFC 7807) yanıtının frontend'e indirgenmiş hali.
 * Result Pattern'deki Error.Code buradaki `code`'a denk gelir; UI hata
 * mesajını değil kodu switch'lemek isterse buradan yapar.
 */
export interface ApiError {
  code: string
  message: string
  status: number
}

export function isApiError(value: unknown): value is ApiError {
  return (
    typeof value === 'object' &&
    value !== null &&
    'code' in value &&
    'message' in value &&
    'status' in value
  )
}
