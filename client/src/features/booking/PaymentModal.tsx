import { useState } from 'react'
import { ShieldCheck } from 'lucide-react'
import { isApiError } from '@/types'
import { Button } from '@/components/Button'
import { SlideOver } from '@/components/Modal'
import { selectTotalPrice, useBookingStore } from '@/store/bookingStore'
import { useToastStore } from '@/store/toastStore'
import { bookingApi } from './api'
import { LockCountdown } from './LockCountdown'

interface PaymentModalProps {
  open: boolean
  eventId: string
  onClose: () => void
  onPurchased: () => void
}

export function PaymentModal({ open, eventId, onClose, onPurchased }: PaymentModalProps) {
  const selectedSeats = useBookingStore((state) => state.selectedSeats)
  const lockExpiresAt = useBookingStore((state) => state.lockExpiresAt)
  const clearSelection = useBookingStore((state) => state.clearSelection)
  const totalPrice = useBookingStore(selectTotalPrice)
  const pushToast = useToastStore((state) => state.push)
  const [paying, setPaying] = useState(false)

  const seatIds = selectedSeats.map((s) => s.id)

  async function handleLockExpired() {
    // TTL doldu: gerçek sistemde Redis anahtarı kendiliğinden silinir;
    // mock'ta aynı sonucu release çağrısıyla üretiyoruz.
    await bookingApi.releaseLock(eventId, seatIds)
    clearSelection()
    onClose()
    pushToast('warning', 'Rezervasyon süreniz doldu, koltuklar tekrar satışa açıldı.')
  }

  async function handlePay() {
    setPaying(true)
    try {
      await bookingApi.confirmPurchase(eventId, seatIds)
      clearSelection()
      onClose()
      onPurchased()
      pushToast('success', 'Ödeme tamamlandı, biletleriniz oluşturuldu!')
    } catch (error) {
      pushToast('error', isApiError(error) ? error.message : 'Ödeme sırasında bir hata oluştu.')
    } finally {
      setPaying(false)
    }
  }

  return (
    <SlideOver
      open={open}
      title="Ödeme Yap"
      onClose={onClose}
      footer={
        <div>
          <div className="mb-4 flex items-center justify-between">
            <span className="text-muted">Toplam Ödenecek</span>
            <span className="text-2xl font-bold">${totalPrice}</span>
          </div>
          <Button className="w-full" onClick={handlePay} disabled={paying || selectedSeats.length === 0}>
            {paying ? 'İşleniyor...' : 'Ödemeyi Tamamla'}
          </Button>
        </div>
      }
    >
      {lockExpiresAt && <LockCountdown expiresAt={lockExpiresAt} onExpired={handleLockExpired} />}

      <div className="mt-6 rounded-2xl border border-line bg-panel-2 p-5">
        <h3 className="font-mono text-xs tracking-widest text-muted">SİPARİŞ ÖZETİ</h3>
        <div className="mt-4 space-y-3">
          {selectedSeats.map((seat) => (
            <div
              key={seat.id}
              className="flex items-center justify-between rounded-xl border border-line bg-panel px-4 py-3"
            >
              <div className="flex items-center gap-3">
                <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-neon-soft/40 font-mono text-sm font-bold text-neon">
                  {seat.row}
                  {seat.number}
                </span>
                <span className="text-sm">Standart Bilet</span>
              </div>
              <span className="font-semibold">${seat.price}</span>
            </div>
          ))}
        </div>

        <div className="mt-4 flex items-center gap-2 rounded-xl border border-emerald-900 bg-emerald-950/30 px-4 py-3 text-sm text-emerald-400">
          <ShieldCheck size={16} className="shrink-0" />
          Redis lock oluşturuldu. CQRS event bekleniyor.
        </div>
      </div>
    </SlideOver>
  )
}
