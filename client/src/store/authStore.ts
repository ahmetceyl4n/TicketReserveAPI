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
      // Rehydrate'i deterministik yapıyoruz: user'ı hydrate anında token'dan
      // türetip state'i doğrudan burada kuruyoruz. onRehydrateStorage'ın
      // sonradan-çalışan callback'i + ayrı setState'i ilk render'da user=null
      // bırakıp "token localStorage'da ama UI çıkış yapmış" tutarsızlığına yol açıyordu.
      merge: (persisted, current) => {
        const token = (persisted as Partial<AuthState> | undefined)?.accessToken ?? null
        const user = token ? parseJwt(token) : null
        // parseJwt null → token süresi dolmuş/bozuk → oturumu tamamen düşür
        return { ...current, accessToken: user ? token : null, user }
      },
    },
  ),
)

export const selectIsAdmin = (state: AuthState): boolean =>
  state.user?.roles.includes('Admin') ?? false
