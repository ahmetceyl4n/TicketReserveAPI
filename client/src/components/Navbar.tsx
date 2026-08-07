import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Activity, LogOut, Terminal, Ticket } from 'lucide-react'
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
          <div className="mt-4 font-semibold">{user?.fullName || user?.email}</div>
          <div className="mt-1 text-sm text-muted">{user?.email}</div>
          <div className="mt-3 inline-block rounded-full border border-neon-soft bg-neon/10 px-3 py-1 font-mono text-xs text-neon">
            {isAdmin ? 'Yönetici' : 'Üye'}
          </div>
        </div>

        <button
          onClick={() => {
            setProfileOpen(false)
            navigate('/reservations')
          }}
          className="mt-4 flex w-full items-center gap-3 rounded-xl border border-line bg-panel-2 px-4 py-3 text-sm hover:border-neon-soft transition-colors cursor-pointer"
        >
          <Ticket size={16} className="text-neon" />
          Biletlerim
        </button>

        <div className="mt-6 space-y-3 rounded-2xl border border-line bg-panel-2 p-5">
          <div className="font-mono text-xs tracking-widest text-muted">HESAP BİLGİLERİ</div>
          <div className="flex justify-between gap-4 border-t border-line pt-3 text-sm">
            <span className="text-muted">E-posta</span>
            <span className="truncate font-medium">{user?.email}</span>
          </div>
          {user?.fullName && user.fullName !== user.email && (
            <div className="flex justify-between gap-4 text-sm">
              <span className="text-muted">Ad Soyad</span>
              <span className="truncate font-medium">{user.fullName}</span>
            </div>
          )}
          <div className="flex justify-between gap-4 text-sm">
            <span className="text-muted">Rol</span>
            <span className="font-medium">{user?.roles.join(', ') || 'Üye'}</span>
          </div>
        </div>
      </SlideOver>
    </header>
  )
}
