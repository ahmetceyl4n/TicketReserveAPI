import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { KeyRound } from 'lucide-react'
import { isApiError } from '@/types'
import { Button } from '@/components/Button'
import { useAuthStore } from '@/store/authStore'
import { authApi } from './api'

export function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const setToken = useAuthStore((state) => state.setToken)
  const navigate = useNavigate()

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const { accessToken } = await authApi.login({ email, password })
      setToken(accessToken)
      navigate('/')
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Giriş yapılamadı.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="mx-auto max-w-md px-6 py-20">
      <div className="rounded-2xl border border-line bg-panel p-8">
        <div className="mb-8 text-center">
          <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-neon font-black text-2xl text-black shadow-[0_0_25px_rgb(34_211_238/0.5)]">
            N
          </div>
          <h1 className="mt-4 text-2xl font-bold">Giriş Yap</h1>
          <p className="mt-1 text-sm text-muted">Nexus Reserve hesabınla devam et</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <input
            type="email"
            required
            placeholder="E-posta"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="w-full rounded-lg border border-line bg-panel-2 px-4 py-3 text-sm
              placeholder:text-muted focus:border-neon focus:outline-none"
          />
          <input
            type="password"
            required
            placeholder="Şifre"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="w-full rounded-lg border border-line bg-panel-2 px-4 py-3 text-sm
              placeholder:text-muted focus:border-neon focus:outline-none"
          />

          {error && (
            <p className="rounded-lg border border-rose-900 bg-rose-950/40 px-4 py-3 text-sm text-rose-400">
              {error}
            </p>
          )}

          <Button type="submit" className="w-full" disabled={loading}>
            <KeyRound size={16} />
            {loading ? 'Doğrulanıyor...' : 'Giriş Yap'}
          </Button>
        </form>

        <p className="mt-6 rounded-lg border border-line bg-panel-2 px-4 py-3 font-mono text-[11px] leading-relaxed text-muted">
          MOCK MOD — <span className="text-neon">admin@nexus.com</span> ile girersen Admin rolü
          alırsın; başka herhangi bir e-posta User olur. Şifre "wrong" hatalı giriş senaryosunu
          tetikler.
        </p>
      </div>
    </div>
  )
}
