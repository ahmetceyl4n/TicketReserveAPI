import type { ApiError } from '@/types'

/** Gerçek ağ gecikmesini simüle eder; UI loading state'leri mock'ta da test edilebilsin diye */
export function delay(ms = 350): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

/**
 * Deterministik PRNG (mulberry32). Math.random yerine bunun sebebi:
 * aynı event her sayfa yenilemesinde aynı koltuk düzenini üretsin,
 * yoksa "satıldı" görünen koltuk refresh'te "boş" olur ve mock inandırıcılığını kaybeder.
 */
export function seededRandom(seed: number): () => number {
  let state = seed
  return () => {
    state |= 0
    state = (state + 0x6d2b79f5) | 0
    let t = Math.imul(state ^ (state >>> 15), 1 | state)
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296
  }
}

export function hashString(value: string): number {
  let hash = 0
  for (let i = 0; i < value.length; i++) {
    hash = (Math.imul(31, hash) + value.charCodeAt(i)) | 0
  }
  return Math.abs(hash)
}

/** Mock hataları da gerçek API ile aynı ApiError şeklinde fırlatır */
export function mockError(code: string, message: string, status: number): ApiError {
  return { code, message, status }
}
