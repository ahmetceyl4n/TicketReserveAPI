import { Clock } from 'lucide-react'
import { useCountdown } from './useCountdown'

interface LockTimerProps {
  expiresAt: number
  onExpired: () => void
}

/** Redis seat lock'ın TTL'ini görselleştiren büyük geri sayım (ödeme panelindeki 09:58) */
export function LockCountdown({ expiresAt, onExpired }: LockTimerProps) {
  const { minutes, seconds, critical } = useCountdown(expiresAt, onExpired)

  return (
    <div
      className={`rounded-2xl border p-5 text-center ${
        critical ? 'border-rose-800 bg-rose-950/30' : 'border-lock/40 bg-lock/5'
      }`}
    >
      <Clock size={20} className={`mx-auto ${critical ? 'text-rose-400' : 'text-lock'}`} />
      <div
        className={`mt-2 font-mono text-4xl font-bold tracking-[0.2em] ${
          critical ? 'text-rose-400' : 'text-lock'
        }`}
      >
        {minutes}:{seconds}
      </div>
      <p className="mt-3 text-xs text-muted leading-relaxed">
        Koltuklar sizin için geçici olarak ayrıldı.
        <br />
        Lütfen süreniz dolmadan işlemi tamamlayın.
      </p>
    </div>
  )
}

/** Seçim çubuğunda görünen kompakt sayaç — modal kapansa da kilit yaşıyor */
export function InlineLockTimer({ expiresAt, onExpired }: LockTimerProps) {
  const { minutes, seconds, critical } = useCountdown(expiresAt, onExpired)

  return (
    <span
      className={`flex items-center gap-2 font-mono text-lg font-bold ${
        critical ? 'text-rose-400' : 'text-lock'
      }`}
    >
      <Clock size={16} />
      {minutes}:{seconds}
    </span>
  )
}
