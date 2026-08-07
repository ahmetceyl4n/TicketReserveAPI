import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { ArrowLeft, Calendar, MapPin, Ticket, Trash2 } from 'lucide-react'
import type { ReservationSummary } from '@/types'
import { isApiError } from '@/types'
import { Button } from '@/components/Button'
import { useToastStore } from '@/store/toastStore'
import { reservationsApi } from './api'

const dateFormatter = new Intl.DateTimeFormat('tr-TR', { dateStyle: 'medium', timeStyle: 'short' })
const priceFormatter = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' })

export function MyReservationsPage() {
  const [reservations, setReservations] = useState<ReservationSummary[] | null>(null)
  const [cancelingId, setCancelingId] = useState<string | null>(null)
  const pushToast = useToastStore((state) => state.push)

  useEffect(() => {
    reservationsApi
      .getMyReservations()
      .then(setReservations)
      .catch(() => {
        setReservations([])
        pushToast('error', 'Biletler yüklenemedi.')
      })
  }, [pushToast])

  async function handleCancel(reservation: ReservationSummary) {
    setCancelingId(reservation.reservationId)
    try {
      await reservationsApi.cancelReservation(reservation.reservationId)
      setReservations((current) =>
        (current ?? []).filter((r) => r.reservationId !== reservation.reservationId),
      )
      pushToast('success', `${reservation.seatRow}${reservation.seatNumber} bileti iptal edildi.`)
    } catch (error) {
      pushToast('error', isApiError(error) ? error.message : 'İptal başarısız.')
    } finally {
      setCancelingId(null)
    }
  }

  return (
    <div className="mx-auto max-w-4xl px-6 py-12">
      <Link
        to="/"
        className="mb-6 inline-flex items-center gap-2 text-sm text-muted hover:text-white transition-colors"
      >
        <ArrowLeft size={14} />
        Ana Sayfa
      </Link>
      <h1 className="text-3xl font-bold">Biletlerim</h1>
      <p className="mt-2 text-muted">Satın aldığın biletler ve iptal işlemleri.</p>

      <div className="mt-8 space-y-4">
        {reservations === null ? (
          Array.from({ length: 2 }).map((_, i) => (
            <div key={i} className="h-24 animate-pulse rounded-2xl border border-line bg-panel" />
          ))
        ) : reservations.length === 0 ? (
          <div className="rounded-2xl border border-line bg-panel p-12 text-center">
            <Ticket className="mx-auto text-muted" size={40} />
            <p className="mt-4 text-muted">Henüz biletin yok.</p>
            <Link to="/" className="mt-4 inline-block text-neon hover:underline">
              Etkinliklere göz at →
            </Link>
          </div>
        ) : (
          reservations.map((r) => (
            <div
              key={r.reservationId}
              className="flex items-center justify-between gap-4 rounded-2xl border border-line bg-panel p-5"
            >
              <div className="min-w-0">
                <div className="flex items-center gap-2">
                  <span className="rounded-lg bg-neon-soft px-2.5 py-1 font-mono text-sm font-bold text-neon">
                    {r.seatRow}
                    {r.seatNumber}
                  </span>
                  <span className="truncate font-bold">{r.eventName}</span>
                </div>
                <div className="mt-2 flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted">
                  <span className="inline-flex items-center gap-1">
                    <MapPin size={13} />
                    {r.venue}
                  </span>
                  <span className="inline-flex items-center gap-1">
                    <Calendar size={13} />
                    {dateFormatter.format(new Date(r.eventDate))}
                  </span>
                  <span className="font-medium text-gray-300">{priceFormatter.format(r.price)}</span>
                </div>
              </div>
              <Button
                variant="danger"
                onClick={() => handleCancel(r)}
                disabled={cancelingId === r.reservationId}
              >
                <Trash2 size={15} />
                {cancelingId === r.reservationId ? 'İptal ediliyor...' : 'İptal Et'}
              </Button>
            </div>
          ))
        )}
      </div>
    </div>
  )
}
