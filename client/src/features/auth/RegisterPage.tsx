import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { UserPlus } from 'lucide-react'
import { isApiError } from '@/types'
import { Button } from '@/components/Button'
import { useAuthStore } from '@/store/authStore'
import { authApi } from './api'

export function RegisterPage() {
  const [fullName, setFullName] = useState('')
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
      const { accessToken } = await authApi.register({ fullName, email, password })
      setToken(accessToken)
      navigate('/')
    } catch (err) {
      setError(isApiError(err) ? err.message : 'Kayıt yapılamadı.')
    } finally {
      setLoading(false)
    }
  }

  const inputClass =
    'w-full rounded-lg border border-line bg-panel-2 px-4 py-3 text-sm placeholder:text-muted focus:border-neon focus:outline-none'

  return (
    <div className="mx-auto max-w-md px-6 py-20">
      <div className="rounded-2xl border border-line bg-panel p-8">
        <div className="mb-8 text-center">
          <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-neon font-black text-2xl text-black shadow-[0_0_25px_rgb(34_211_238/0.5)]">
            N
          </div>
          <h1 className="mt-4 text-2xl font-bold">Kayıt Ol</h1>
          <p className="mt-1 text-sm text-muted">Nexus Reserve hesabını oluştur</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <input
            type="text"
            required
            placeholder="Ad Soyad"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            className={inputClass}
          />
          <input
            type="email"
            required
            placeholder="E-posta"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className={inputClass}
          />
          <input
            type="password"
            required
            placeholder="Şifre (en az 8 karakter, büyük/küçük harf + rakam)"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className={inputClass}
          />

          {error && (
            <p className="rounded-lg border border-rose-900 bg-rose-950/40 px-4 py-3 text-sm text-rose-400">
              {error}
            </p>
          )}

          <Button type="submit" className="w-full" disabled={loading}>
            <UserPlus size={16} />
            {loading ? 'Oluşturuluyor...' : 'Kayıt Ol'}
          </Button>
        </form>

        <p className="mt-6 text-center text-sm text-muted">
          Zaten hesabın var mı?{' '}
          <Link to="/login" className="text-neon hover:underline">
            Giriş yap
          </Link>
        </p>
      </div>
    </div>
  )
}
