import axios, { AxiosError } from 'axios'
import type { ApiError } from '@/types'
import { useAuthStore } from '@/store/authStore'

/**
 * Tek anahtar: .env'de VITE_USE_MOCKS=false yapıldığında tüm feature
 * api modülleri mock yerine gerçek HTTP implementasyonuna geçer.
 * Varsayılan mock — backend hazır olmadan frontend geliştirilebilsin diye.
 */
export const USE_MOCKS = import.meta.env.VITE_USE_MOCKS !== 'false'

export const apiClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use((config) => {
  // Store'dan senkron okuma (hook değil): interceptor React dışında çalışır
  const token = useAuthStore.getState().accessToken
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

apiClient.interceptors.response.use(undefined, (error: AxiosError) => {
  // Backend ProblemDetails (RFC 7807) döner; UI katmanı axios detayı görmesin,
  // her hata tek tip ApiError olarak yükselsin diye burada indirgenir.
  const data = error.response?.data as
    | { title?: string; detail?: string; type?: string; errorCode?: string }
    | undefined

  const apiError: ApiError = {
    code: data?.errorCode ?? data?.type ?? 'Unknown',
    message: data?.detail ?? data?.title ?? 'Beklenmedik bir hata oluştu.',
    status: error.response?.status ?? 0,
  }
  return Promise.reject(apiError)
})
