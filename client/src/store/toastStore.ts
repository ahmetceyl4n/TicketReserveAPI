import { create } from 'zustand'

export type ToastKind = 'signalr' | 'success' | 'error' | 'warning'

export interface Toast {
  id: number
  kind: ToastKind
  title: string
  message: string
  /** Birleştirilmiş SignalR toast'ında kaç event'in katlandığı */
  count: number
  createdAt: number
}

interface ToastState {
  toasts: Toast[]
  push: (kind: ToastKind, message: string, title?: string) => void
  dismiss: (id: number) => void
}

let nextId = 1
const TOAST_LIFETIME_MS = 4500
/** Bu pencere içinde art arda gelen SignalR event'leri tek karta katlanır */
const COALESCE_WINDOW_MS = 2000

const DEFAULT_TITLES: Record<ToastKind, string> = {
  signalr: 'SIGNALR EVENT',
  success: 'BAŞARILI',
  error: 'HATA',
  warning: 'UYARI',
}

// Zamanlayıcılar store state'inin dışında tutulur: state'e koymak
// her set çağrısında gereksiz render tetiklerdi, UI'ın timer'ı görmesi gerekmiyor.
const dismissTimers = new Map<number, ReturnType<typeof setTimeout>>()

export const useToastStore = create<ToastState>()((set, get) => {
  const scheduleDismiss = (id: number) => {
    clearTimeout(dismissTimers.get(id))
    dismissTimers.set(
      id,
      setTimeout(() => {
        dismissTimers.delete(id)
        set((state) => ({ toasts: state.toasts.filter((t) => t.id !== id) }))
      }, TOAST_LIFETIME_MS),
    )
  }

  return {
    toasts: [],

    push: (kind, message, title) => {
      // SignalR event yağmuru: pencere içindeki yeni event, yeni kart açmak
      // yerine son kartın sayacını artırır ve ömrünü tazeler. createdAt her
      // birleşmede yenilendiği için kesintisiz akış tek kartta toplanır.
      if (kind === 'signalr') {
        const recent = get().toasts.findLast((t) => t.kind === 'signalr')
        if (recent && Date.now() - recent.createdAt < COALESCE_WINDOW_MS) {
          const count = recent.count + 1
          set((state) => ({
            toasts: state.toasts.map((t) =>
              t.id === recent.id
                ? { ...t, count, createdAt: Date.now(), message: `${count} koltuğun durumu değişti.` }
                : t,
            ),
          }))
          scheduleDismiss(recent.id)
          return
        }
      }

      const id = nextId++
      set((state) => ({
        // En fazla 4 toast: birleştirmeye rağmen farklı türler birikirse ekran dolmasın
        toasts: [
          ...state.toasts,
          { id, kind, message, title: title ?? DEFAULT_TITLES[kind], count: 1, createdAt: Date.now() },
        ].slice(-4),
      }))
      scheduleDismiss(id)
    },

    dismiss: (id) => {
      clearTimeout(dismissTimers.get(id))
      dismissTimers.delete(id)
      set((state) => ({ toasts: state.toasts.filter((t) => t.id !== id) }))
    },
  }
})
