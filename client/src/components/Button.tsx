import type { ButtonHTMLAttributes, ReactNode } from 'react'

type Variant = 'primary' | 'ghost' | 'danger' | 'outline'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  children: ReactNode
}

const VARIANT_CLASSES: Record<Variant, string> = {
  primary:
    'bg-neon text-black font-semibold hover:bg-cyan-300 shadow-[0_0_20px_rgb(34_211_238/0.35)]',
  outline:
    'border border-neon-soft text-neon bg-neon/5 hover:bg-neon/15 hover:shadow-[0_0_16px_rgb(34_211_238/0.25)]',
  ghost: 'border border-line bg-panel-2 text-gray-200 hover:border-gray-500',
  danger:
    'border border-rose-800 bg-rose-950/40 text-rose-400 hover:bg-rose-900/40 hover:shadow-[0_0_16px_rgb(244_63_94/0.2)]',
}

export function Button({ variant = 'primary', className = '', children, ...rest }: ButtonProps) {
  return (
    <button
      className={`inline-flex items-center justify-center gap-2 rounded-lg px-4 py-2.5 text-sm
        transition-all duration-150 cursor-pointer
        disabled:opacity-40 disabled:pointer-events-none ${VARIANT_CLASSES[variant]} ${className}`}
      {...rest}
    >
      {children}
    </button>
  )
}
