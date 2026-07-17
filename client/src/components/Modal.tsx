import type { ReactNode } from 'react'
import { X } from 'lucide-react'

interface ModalProps {
  open: boolean
  title: string
  onClose: () => void
  children: ReactNode
}

/** Ortalanmış klasik modal — form'lar için (ör. Yeni Etkinlik) */
export function Modal({ open, title, onClose, children }: ModalProps) {
  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/70 backdrop-blur-sm" onClick={onClose} />
      <div className="relative w-full max-w-lg mx-4 rounded-2xl border border-line bg-panel p-6 shadow-2xl">
        <div className="mb-5 flex items-center justify-between">
          <h2 className="text-lg font-bold">{title}</h2>
          <button
            onClick={onClose}
            className="text-muted hover:text-white transition-colors cursor-pointer"
            aria-label="Kapat"
          >
            <X size={20} />
          </button>
        </div>
        {children}
      </div>
    </div>
  )
}

interface SlideOverProps {
  open: boolean
  title: string
  onClose: () => void
  children: ReactNode
  footer?: ReactNode
}

/** Sağdan kayan panel — ödeme ve profil ekranları için (ekran görüntülerindeki gibi) */
export function SlideOver({ open, title, onClose, children, footer }: SlideOverProps) {
  if (!open) return null

  return (
    <div className="fixed inset-0 z-50">
      <div className="absolute inset-0 bg-black/70 backdrop-blur-sm" onClick={onClose} />
      <div className="absolute right-0 top-0 h-full w-full max-w-md flex flex-col border-l border-line bg-panel animate-slide-in-right">
        <div className="flex items-center justify-between border-b border-line px-6 py-5">
          <h2 className="text-xl font-bold">{title}</h2>
          <button
            onClick={onClose}
            className="text-muted hover:text-white transition-colors cursor-pointer"
            aria-label="Kapat"
          >
            <X size={22} />
          </button>
        </div>
        <div className="flex-1 overflow-y-auto p-6">{children}</div>
        {footer && <div className="border-t border-line p-6">{footer}</div>}
      </div>
    </div>
  )
}
