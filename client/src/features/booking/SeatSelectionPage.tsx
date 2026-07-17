import { useCallback, useEffect, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ArrowLeft, ArrowRight, XCircle } from 'lucide-react'
import type { EventSummary, Seat, SeatStatusChangedEvent } from '@/types'
import { isApiError } from '@/types'
import { Button } from '@/components/Button'
import { useSignalR } from '@/lib/signalr/useSignalR'
import { selectTotalPrice, useBookingStore } from '@/store/bookingStore'
import { useToastStore } from '@/store/toastStore'
import { bookingApi } from './api'
import { eventsApi } from '@/features/events/api'
import { InlineLockTimer } from './LockCountdown'
import { PaymentModal } from './PaymentModal'
import { SeatGrid, SeatGridSkeleton } from './SeatGrid'

export function SeatSelectionPage() {
  const { eventId } = useParams<{ eventId: string }>()
  const [event, setEvent] = useState<EventSummary | null>(null)
  const [seats, setSeats] = useState<Seat[]>([])
  const [seatsLoading, setSeatsLoading] = useState(true)
  const [paymentOpen, setPaymentOpen] = useState(false)
  const [locking, setLocking] = useState(false)

  const selectedSeats = useBookingStore((state) => state.selectedSeats)
  const lockExpiresAt = useBookingStore((state) => state.lockExpiresAt)
  const toggleSeat = useBookingStore((state) => state.toggleSeat)
  const removeSeat = useBookingStore((state) => state.removeSeat)
  const clearSelection = useBookingStore((state) => state.clearSelection)
  const setLock = useBookingStore((state) => state.setLock)
  const totalPrice = useBookingStore(selectTotalPrice)
  const pushToast = useToastStore((state) => state.push)

  const selectedIds = new Set(selectedSeats.map((s) => s.id))

  // Kendi tetiklediğimiz lock/purchase da SignalR'dan geri döner;
  // kendi işlemimiz için "Birisi ... aldı" toast'ı basmamak adına ayırt ediyoruz.
  const ownActionSeatIds = useRef<Set<string>>(new Set())

  const loadSeats = useCallback(async () => {
    if (!eventId) return
    setSeats(await bookingApi.getSeats(eventId))
  }, [eventId])

  useEffect(() => {
    if (!eventId) return
    clearSelection()
    setSeatsLoading(true)
    eventsApi
      .getEvent(eventId)
      .then(setEvent)
      .catch(() => setEvent(null))
    loadSeats()
      .catch(() => pushToast('error', 'Koltuklar yüklenemedi.'))
      .finally(() => setSeatsLoading(false))
  }, [eventId, clearSelection, loadSeats, pushToast])

  useSignalR(eventId, (change: SeatStatusChangedEvent) => {
    setSeats((current) =>
      current.map((seat) => (seat.id === change.seatId ? { ...seat, status: change.status } : seat)),
    )

    const seatLabel = `${change.row}${change.number}`
    const isOwnAction = ownActionSeatIds.current.has(change.seatId)

    if (!isOwnAction) {
      if (change.status === 'Locked') {
        pushToast('signalr', `Birisi ${seatLabel} koltuğunu işlemeye aldı.`)
      } else if (change.status === 'Sold') {
        pushToast('signalr', `${seatLabel} koltuğu başka birine satıldı.`)
      } else {
        pushToast('signalr', `${seatLabel} koltuğu tekrar satışta.`)
      }

      // Seçtiğimiz koltuk elimizden kapıldıysa sepetten düşür — kullanıcı
      // var olmayan bir koltuğa ödeme adımına ilerlememeli.
      if (change.status !== 'Available' && selectedIds.has(change.seatId)) {
        removeSeat(change.seatId)
        pushToast('warning', `${seatLabel} koltuğu sepetinizden çıkarıldı.`)
      }
    }
  })

  function handleToggleSeat(seat: Seat) {
    // Aktif kilit varken sepet değiştirilemez: kilit belirli koltuklar için
    // alındı, sepete sonradan eklenen koltuk kilitsiz kalır ve ödeme adımında
    // "kilitli sandığım ama değil" tutarsızlığı doğardı.
    if (lockExpiresAt) {
      pushToast('warning', 'Aktif rezervasyonunuz var. Önce ödemeyi tamamlayın veya vazgeçin.')
      return
    }
    toggleSeat(seat)
  }

  async function handleProceedToPayment() {
    if (!eventId || selectedSeats.length === 0) return

    // Kilit zaten alınmışsa (modal kapatılıp geri dönülüyor) tekrar kilitleme
    if (lockExpiresAt) {
      setPaymentOpen(true)
      return
    }

    setLocking(true)
    const seatIds = selectedSeats.map((s) => s.id)
    try {
      ownActionSeatIds.current = new Set(seatIds)
      const lock = await bookingApi.lockSeats(eventId, seatIds)
      setLock(lock.expiresAtUtc)
      setPaymentOpen(true)
    } catch (error) {
      ownActionSeatIds.current = new Set()
      pushToast('error', isApiError(error) ? error.message : 'Koltuklar kilitlenemedi.')
      // Çakışma yaşandıysa grid'deki güncel durumu göster
      await loadSeats()
      clearSelection()
    } finally {
      setLocking(false)
    }
  }

  async function releaseAndReset(message: string) {
    if (!eventId) return
    await bookingApi.releaseLock(eventId, selectedSeats.map((s) => s.id))
    clearSelection()
    ownActionSeatIds.current = new Set()
    setPaymentOpen(false)
    pushToast('warning', message)
  }

  if (!eventId) return null

  return (
    <div className="mx-auto max-w-6xl px-6 py-10 pb-32">
      <div className="mb-10 flex items-center gap-5">
        <Link
          to="/"
          className="flex h-11 w-11 items-center justify-center rounded-full border border-line bg-panel hover:border-gray-500 transition-colors"
        >
          <ArrowLeft size={18} />
        </Link>
        <div>
          <h1 className="text-3xl font-bold">{event?.name ?? 'Yükleniyor...'}</h1>
          <p className="mt-1 text-neon">{event?.venue}</p>
        </div>
      </div>

      <div className="border-t border-line pt-12">
        {seatsLoading ? (
          <SeatGridSkeleton />
        ) : (
          <SeatGrid seats={seats} selectedSeatIds={selectedIds} onToggleSeat={handleToggleSeat} />
        )}
      </div>

      {/* Seçim özeti çubuğu — kilit alındıysa sayaç ve bilinçli vazgeç butonuyla */}
      {selectedSeats.length > 0 && !paymentOpen && (
        <div className="fixed bottom-6 left-1/2 z-40 -translate-x-1/2">
          <div className="flex items-center gap-6 rounded-2xl border border-line bg-panel/95 backdrop-blur px-6 py-4 shadow-2xl">
            <div>
              <div className="text-xs text-muted">{selectedSeats.length} Koltuk Seçildi</div>
              <div className="text-2xl font-bold">${totalPrice}</div>
            </div>

            {lockExpiresAt ? (
              <>
                <InlineLockTimer
                  expiresAt={lockExpiresAt}
                  onExpired={() =>
                    releaseAndReset('Rezervasyon süreniz doldu, koltuklar tekrar satışa açıldı.')
                  }
                />
                <Button onClick={() => setPaymentOpen(true)}>
                  Ödemeye Dön
                  <ArrowRight size={16} />
                </Button>
                <Button
                  variant="danger"
                  onClick={() => releaseAndReset('Rezervasyondan vazgeçildi, koltuklar serbest bırakıldı.')}
                >
                  <XCircle size={16} />
                  Vazgeç
                </Button>
              </>
            ) : (
              <Button onClick={handleProceedToPayment} disabled={locking}>
                {locking ? 'Kilitleniyor...' : 'Ödeme Adımına Geç'}
                <ArrowRight size={16} />
              </Button>
            )}
          </div>
        </div>
      )}

      <PaymentModal
        open={paymentOpen}
        eventId={eventId}
        // Kapatmak vazgeçmek DEĞİL: kilit TTL'i boyunca yaşar, kullanıcı
        // "Ödemeye Dön" ile kaldığı yerden devam eder. Vazgeç ayrı ve bilinçli.
        onClose={() => setPaymentOpen(false)}
        onPurchased={() => {
          ownActionSeatIds.current = new Set()
          loadSeats()
        }}
      />
    </div>
  )
}
