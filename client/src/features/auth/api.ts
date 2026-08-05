import type { AuthResponse, LoginCommand, RegisterCommand } from '@/types'
import { USE_MOCKS, apiClient } from '@/lib/apiClient'
import { delay, mockError } from '@/lib/mock/helpers'
import { createMockJwt } from './jwt'

interface AuthApi {
  login(command: LoginCommand): Promise<AuthResponse>
  register(command: RegisterCommand): Promise<AuthResponse>
}

const httpAuthApi: AuthApi = {
  async login(command) {
    const { data } = await apiClient.post<AuthResponse>('/auth/login', command)
    return data
  },
  async register(command) {
    // Backend register token DÖNMÜYOR (sadece {id,email,fullName}). Kaydın ardından
    // aynı bilgilerle login yapıp token'ı oradan alıyoruz (auto-login deseni).
    await apiClient.post('/auth/register', command)
    const { data } = await apiClient.post<AuthResponse>('/auth/login', {
      email: command.email,
      password: command.password,
    })
    return data
  },
}

/**
 * Mock kurallar: admin@nexus.com → Admin rolü, diğer her email → User.
 * Şifre "wrong" ise hata — backend'in InvalidCredentials akışını
 * (bkz. AuthErrors, 401 + aynı mesaj) UI'da test edebilmek için.
 */
const mockAuthApi: AuthApi = {
  async login(command) {
    await delay()
    if (command.password === 'wrong') {
      throw mockError('Auth.InvalidCredentials', 'E-posta veya şifre hatalı.', 401)
    }
    const isAdmin = command.email.toLowerCase() === 'admin@nexus.com'
    return {
      accessToken: createMockJwt({
        id: isAdmin ? 'usr-admin' : `usr-${command.email}`,
        email: command.email,
        fullName: isAdmin ? 'Nexus Admin' : command.email.split('@')[0],
        roles: isAdmin ? ['Admin', 'User'] : ['User'],
      }),
    }
  },

  async register(command) {
    await delay()
    if (command.email.toLowerCase() === 'admin@nexus.com') {
      throw mockError('Auth.EmailAlreadyExists', 'Bu e-posta adresi zaten kayıtlı.', 409)
    }
    return {
      accessToken: createMockJwt({
        id: `usr-${command.email}`,
        email: command.email,
        fullName: command.fullName,
        roles: ['User'],
      }),
    }
  },
}

export const authApi: AuthApi = USE_MOCKS ? mockAuthApi : httpAuthApi
