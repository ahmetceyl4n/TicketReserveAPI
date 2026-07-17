import { useEffect, useRef, useState } from 'react'

interface Countdown {
  minutes: string
  seconds: string
  /** Son 60 saniye — UI kırmızıya döner */
  critical: boolean
}

/**
 * Verilen ana kadar geriye sayar; süre dolunca onExpired'ı BİR KEZ çağırır.
 * Hem büyük ödeme sayacı hem seçim çubuğundaki kompakt sayaç bunu kullanır —
 * "süre doldu" kararının iki ayrı yerde farklı verilmesi tutarsızlık doğururdu.
 */
export function useCountdown(expiresAt: number, onExpired: () => void): Countdown {
  const [remainingMs, setRemainingMs] = useState(() => Math.max(0, expiresAt - Date.now()))

  // onExpired ref'te: parent her render olduğunda sayaç sıfırlanmamalı
  const onExpiredRef = useRef(onExpired)
  onExpiredRef.current = onExpired

  useEffect(() => {
    const initial = Math.max(0, expiresAt - Date.now())
    setRemainingMs(initial)
    if (initial === 0) {
      // Bileşen süresi geçmiş bir kilide mount olduysa 1 sn bekletmeden bildir
      onExpiredRef.current()
      return
    }

    const timer = setInterval(() => {
      const remaining = Math.max(0, expiresAt - Date.now())
      setRemainingMs(remaining)
      if (remaining === 0) {
        clearInterval(timer)
        onExpiredRef.current()
      }
    }, 1000)
    return () => clearInterval(timer)
  }, [expiresAt])

  const totalSeconds = Math.floor(remainingMs / 1000)
  return {
    minutes: String(Math.floor(totalSeconds / 60)).padStart(2, '0'),
    seconds: String(totalSeconds % 60).padStart(2, '0'),
    critical: totalSeconds < 60,
  }
}
