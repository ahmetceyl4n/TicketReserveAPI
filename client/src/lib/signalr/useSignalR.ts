import { useEffect, useRef } from 'react'
import * as signalR from '@microsoft/signalr'
import type { SeatStatusChangedEvent } from '@/types'
import { USE_MOCKS } from '@/lib/apiClient'
import { useAuthStore } from '@/store/authStore'
import { mockSeatHub } from './mockSeatHub'

/**
 * Bir etkinliğin koltuk durum değişikliklerine abone olur.
 * Mock modda simülasyon motoruna, gerçek modda backend'deki SeatHub'a
 * bağlanır — çağıran bileşen farkı bilmez, sözleşme aynıdır.
 */
export function useSignalR(
  eventId: string | undefined,
  onSeatStatusChanged: (event: SeatStatusChangedEvent) => void,
): void {
  // Handler ref'te tutulur: her render'da yeniden bağlantı kurulmasın,
  // ama handler her zaman en güncel closure'ı görsün.
  const handlerRef = useRef(onSeatStatusChanged)
  handlerRef.current = onSeatStatusChanged

  useEffect(() => {
    if (!eventId) return

    const stableHandler = (event: SeatStatusChangedEvent) => handlerRef.current(event)

    if (USE_MOCKS) {
      return mockSeatHub.subscribe(eventId, stableHandler)
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/seats', {
        accessTokenFactory: () => useAuthStore.getState().accessToken ?? '',
      })
      .withAutomaticReconnect()
      .build()

    connection.on('SeatStatusChanged', stableHandler)

    let disposed = false
    connection
      .start()
      .then(() => {
        // Bağlantı kurulmadan invoke edilemez; grup katılımı start'ın then'inde.
        // Backend'de karşılığı: SeatHub.JoinEventGroup(eventId)
        if (!disposed) return connection.invoke('JoinEventGroup', eventId)
      })
      .catch((error) => console.error('SignalR bağlantısı kurulamadı', error))

    return () => {
      disposed = true
      connection.stop()
    }
  }, [eventId])
}
