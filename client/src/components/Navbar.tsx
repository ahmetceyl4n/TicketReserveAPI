import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Activity, CreditCard, LogOut, Settings, Terminal, Ticket } from 'lucide-react'
import { selectIsAdmin, useAuthStore } from '@/store/authStore'
import { SlideOver } from './Modal'
import { Button } from './Button'

export function Navbar() {
  const user = useAuthStore((state) => state.user)
  const isAdmin = useAuthStore(selectIsAdmin)
  const logout = useAuthStore((state) => state.logout)
  const navigate = useNavigate()
  const [profileOpen, setProfileOpen] = useState(false)

  return (
    <header className="sticky top-0 z-40 border-b border-line bg-void/90 backdrop-blur">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-3">
        <Link to="/" className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-neon font-black text-black shadow-[0_0_18px_rgb(34_211_238/0.5)]">
            N
          </div>
          <div>
            <div className="text-sm font-bold tracking-widest">NEXUS RESERVE</div>
            <div className="flex items-center gap-1 font-mono text-[10px] text-neon">
              <Activity size={10} />
              CQRS / SignalR Active
            </div>
          </div>
        </Link>

        <div className="flex items-center gap-3">
          {isAdmin && (
            <Link
              to="/admin"
              className="flex items-center gap-2 rounded-full border border-rose-900 bg-rose-950/40 px-4 py-2 text-xs font-semibold text-rose-400 hover:bg-rose-900/40 transition-colors"
            >
              <Terminal size={14} />
              Komuta Merkezi
            </Link>
          )}

          {user ? (
            <button
              onClick={() => setProfileOpen(true)}
              className="flex items-center gap-2 rounded-full border border-line bg-panel-2 py-1.5 pl-1.5 pr-4 text-sm hover:border-gray-500 transition-colors cursor-pointer"
            >
              <span className="flex h-7 w-7 items-center justify-center rounded-full bg-neon-soft font-bold text-neon">
                {user.email[0]?.toUpperCase()}
              </span>
              {user.email}
            </button>
          ) : (
            <Link
              to="/login"
              className="rounded-full border border-line bg-panel-2 px-4 py-2 text-sm hover:border-gray-500 transition-colors"
            >
              Giriş Yap
            </Link>
          )}
        </div>
      </div>

      <SlideOver
        open={profileOpen}
        title="Profilim"
        onClose={() => setProfileOpen(false)}
        footer={
          <Button
            variant="danger"
            className="w-full"
            onClick={() => {
              logout()
              setProfileOpen(false)
              navigate('/')
            }}
          >
            <LogOut size={16} />
            Çıkış Yap
          </Button>
        }
      >
        <div className="rounded-2xl border border-line bg-panel-2 p-6 text-center">
          <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-neon-soft text-3xl font-bold text-neon shadow-[0_0_25px_rgb(34_211_238/0.3)]">
            {user?.email[0]?.toUpperCase()}
          </div>
          <div className="mt-4 font-semibold">{user?.email}</div>
          <div className="mt-1 font-mono text-xs text-neon">
            {user?.roles.includes('Admin') ? 'Premium Üye' : 'Standart Üye'}
          </div>
        </div>

        <div className="mt-6 space-y-2">
          {[
            { icon: Ticket, title: 'Biletlerim', subtitle: 'Geçmiş ve aktif rezervasyonlar' },
            { icon: CreditCard, title: 'Ödeme Yöntemleri', subtitle: 'Kayıtlı kartlar ve cüzdan' },
            { icon: Settings, title: 'Ayarlar', subtitle: 'Hesap ve bildirim tercihleri' },
          ].map(({ icon: Icon, title, subtitle }) => (
            <button
              key={title}
              className="flex w-full items-center gap-4 rounded-xl px-3 py-3 text-left hover:bg-panel-2 transition-colors cursor-pointer"
            >
              <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-panel-2 border border-line">
                <Icon size={18} />
              </span>
              <span>
                <span className="block font-semibold">{title}</span>
                <span className="block text-xs text-muted">{subtitle}</span>
              </span>
            </button>
          ))}
        </div>
      </SlideOver>
    </header>
  )
}
