import type { User } from '@/types'

/**
 * ASP.NET Core Identity, rol claim'ini kısa "role" adıyla değil bu şema
 * URI'siyle yazar. Mock token kısa adı kullanır; ikisini de okuyoruz ki
 * gerçek backend'e geçişte token parse kodu değişmesin.
 */
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'

interface JwtPayload {
  sub?: string
  email?: string
  fullName?: string
  exp?: number
  role?: string | string[]
  [ROLE_CLAIM]?: string | string[]
  [NAME_CLAIM]?: string
}

export function parseJwt(token: string): User | null {
  try {
    const payloadPart = token.split('.')[1]
    // Base64Url -> Base64: JWT alfabesi farklı, atob doğrudan kabul etmez
    const normalized = payloadPart.replace(/-/g, '+').replace(/_/g, '/')
    const payload = JSON.parse(atob(normalized)) as JwtPayload

    if (payload.exp && payload.exp * 1000 < Date.now()) {
      return null
    }

    const rawRoles = payload[ROLE_CLAIM] ?? payload.role ?? []
    const roles = Array.isArray(rawRoles) ? rawRoles : [rawRoles]

    return {
      id: payload.sub ?? '',
      email: payload.email ?? '',
      fullName: payload.fullName ?? payload[NAME_CLAIM] ?? payload.email ?? '',
      roles,
    }
  } catch {
    return null
  }
}

/** Mock login'in ürettiği, yapısal olarak geçerli ama imzasız JWT */
export function createMockJwt(user: User): string {
  const header = { alg: 'none', typ: 'JWT' }
  const payload = {
    sub: user.id,
    email: user.email,
    fullName: user.fullName,
    role: user.roles,
    exp: Math.floor(Date.now() / 1000) + 60 * 60 * 8,
  }
  const encode = (obj: object) => btoa(JSON.stringify(obj))
  return `${encode(header)}.${encode(payload)}.mock-signature`
}
