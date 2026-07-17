import { AlertTriangle, CheckCircle2, Info, XCircle } from 'lucide-react'
import { useToastStore, type ToastKind } from '@/store/toastStore'

const KIND_STYLES: Record<ToastKind, { icon: typeof Info; accent: string }> = {
  signalr: { icon: Info, accent: 'text-neon' },
  success: { icon: CheckCircle2, accent: 'text-emerald-400' },
  error: { icon: XCircle, accent: 'text-rose-400' },
  warning: { icon: AlertTriangle, accent: 'text-lock' },
}

/** Sağ alt köşedeki bildirim yığını — SignalR event'leri buraya düşer */
export function ToastContainer() {
  const toasts = useToastStore((state) => state.toasts)
  const dismiss = useToastStore((state) => state.dismiss)

  return (
    <div className="fixed bottom-4 right-4 z-[60] flex flex-col gap-2 w-80">
      {toasts.map((toast) => {
        const { icon: Icon, accent } = KIND_STYLES[toast.kind]
        return (
          <button
            key={toast.id}
            onClick={() => dismiss(toast.id)}
            className="animate-toast-in flex items-start gap-3 rounded-xl border border-line
              bg-panel/95 backdrop-blur px-4 py-3 text-left shadow-xl cursor-pointer
              hover:border-gray-600 transition-colors"
          >
            <Icon size={18} className={`${accent} mt-0.5 shrink-0`} />
            <div className="min-w-0">
              <div className={`flex items-center gap-1.5 font-mono text-[10px] tracking-widest ${accent}`}>
                {toast.title}
                <span className="inline-block h-1.5 w-1.5 rounded-full bg-current" />
              </div>
              <p className="mt-0.5 text-sm text-gray-200">{toast.message}</p>
            </div>
          </button>
        )
      })}
    </div>
  )
}
