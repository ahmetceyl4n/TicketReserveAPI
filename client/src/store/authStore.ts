import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { User } from '@/types'
import { parseJwt } from '@/features/auth/jwt'

interface AuthState {
  accessToken: string | null
  user: User | null
  setToken: (token: string) => void
  logout: () => void
}

/**
 * Sadece token persist edilir; user her zaman token'dan türetilir.
 * İkisini ayrı saklamak "token süresi dolmuş ama user hâlâ localStorage'da"
 * tutarsızlığına yol açardı — tek doğruluk kaynağı token.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      user: null,
      setToken: (token) => set({ accessToken: token, user: parseJwt(token) }),
      logout: () => set({ accessToken: null, user: null }),
    }),
    {
      name: 'nexus-auth',
      partialize: (state) => ({ accessToken: state.accessToken }),
      onRehydrateStorage: () => (state) => {
        if (!state?.accessToken) return
        const user = parseJwt(state.accessToken)
        // Süresi dolmuş token'ı açılışta temizle, kullanıcıyı login'e düşür
        if (user) {
          useAuthStore.setState({ user })
        } else {
          useAuthStore.setState({ accessToken: null, user: null })
        }
      },
    },
  ),
)

export const selectIsAdmin = (state: AuthState): boolean =>
  state.user?.roles.includes('Admin') ?? false
