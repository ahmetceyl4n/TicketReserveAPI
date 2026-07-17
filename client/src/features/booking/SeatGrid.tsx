import { useMemo, useRef, useState, type KeyboardEvent } from 'react'
import { Clock, X } from 'lucide-react'
import type { Seat } from '@/types'

interface SeatGridProps {
  seats: Seat[]
  selectedSeatIds: ReadonlySet<string>
  onToggleSeat: (seat: Seat) => void
}

/**
 * Koltuk butonunun görsel durumu, domain durumundan (SeatStatus) ayrı bir
 * kavram: "Selected" backend'de yoktur, yalnızca bu kullanıcının ekranındaki
 * geçici bir niyettir. Bu yüzden ayrı bir union olarak modellenir.
 */
type VisualState = 'available' | 'selected' | 'locked' | 'sold'

const SEAT_CLASSES: Record<VisualState, string> = {
  available:
    'border-slate-600/60 bg-slate-800/50 text-slate-300 hover:border-neon hover:text-neon hover:shadow-[0_0_10px_rgb(34_211_238/0.4)] cursor-pointer',
  selected:
    'border-neon bg-neon/20 text-neon font-bold shadow-[0_0_14px_rgb(34_211_238/0.6)] cursor-pointer',
  locked:
    'border-lock/70 bg-lock/10 text-lock shadow-[0_0_10px_rgb(251_191_36/0.35)] cursor-not-allowed',
  sold: 'border-sold/50 bg-sold/15 text-sold/70 cursor-not-allowed',
}

const STATUS_LABELS: Record<VisualState, string> = {
  available: 'boş',
  selected: 'seçili',
  locked: 'işlemde',
  sold: 'satıldı',
}

function getVisualState(seat: Seat, isSelected: boolean): VisualState {
  if (seat.status === 'Sold') return 'sold'
  // Kendi kilitlediğimiz koltuklar da SignalR'dan "Locked" döner; seçimimizde
  // duruyorlarsa başkasının kilidi gibi sarı değil, bizim (cyan) görünmeli.
  if (seat.status === 'Locked') return isSelected ? 'selected' : 'locked'
  return isSelected ? 'selected' : 'available'
}

const ARROW_MOVES: Record<string, [number, number]> = {
  ArrowUp: [-1, 0],
  ArrowDown: [1, 0],
  ArrowLeft: [0, -1],
  ArrowRight: [0, 1],
}

export function SeatGrid({ seats, selectedSeatIds, onToggleSeat }: SeatGridProps) {
  // API düz liste döner; her render'da yeniden gruplamamak için memo'lanır
  const rows = useMemo(() => {
    const grouped = new Map<string, Seat[]>()
    for (const seat of seats) {
      const row = grouped.get(seat.row) ?? []
      row.push(seat)
      grouped.set(seat.row, row)
    }
    return [...grouped.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([label, rowSeats]) => ({
        label,
        seats: rowSeats.sort((a, b) => a.number - b.number),
      }))
  }, [seats])

  const gridRef = useRef<HTMLDivElement>(null)

  // Roving tabindex: Tab tuşu grid'e tek noktadan girer/çıkar (112 koltuğu
  // tek tek tab'lamak işkence olurdu), grid içinde gezinme ok tuşlarıyla.
  const [focusSeatId, setFocusSeatId] = useState<string | null>(null)
  const entrySeatId = focusSeatId ?? rows[0]?.seats[0]?.id

  function handleKeyDown(e: KeyboardEvent<HTMLDivElement>) {
    const move = ARROW_MOVES[e.key]
    if (!move) return
    const target = e.target as HTMLElement
    const row = Number(target.dataset.rowIndex)
    const col = Number(target.dataset.colIndex)
    if (Number.isNaN(row) || Number.isNaN(col)) return
    e.preventDefault()
    gridRef.current
      ?.querySelector<HTMLButtonElement>(
        `[data-row-index="${row + move[0]}"][data-col-index="${col + move[1]}"]`,
      )
      ?.focus()
  }

  return (
    <div className="flex flex-col items-center">
      <Stage />

      {/* Dış katman scroll, iç katman perspektif: transform ile overflow
          aynı elemanda olursa kaydırma hesapları bozulur */}
      <div className="max-w-full overflow-x-auto pb-2">
        <div
          ref={gridRef}
          role="group"
          aria-label="Koltuk haritası — ok tuşlarıyla gezinebilirsiniz"
          onKeyDown={handleKeyDown}
          className="flex flex-col gap-2"
          style={{ transform: 'perspective(1100px) rotateX(7deg)', transformOrigin: '50% 0%' }}
        >
          {rows.map(({ label, seats: rowSeats }, rowIndex) => (
            <div key={label} className="flex items-center gap-2">
              <RowLabel label={label} />
              <div className="flex gap-1.5">
                {rowSeats.map((seat, colIndex) => {
                  const state = getVisualState(seat, selectedSeatIds.has(seat.id))
                  const interactive = state === 'available' || state === 'selected'
                  return (
                    <button
                      key={seat.id}
                      data-row-index={rowIndex}
                      data-col-index={colIndex}
                      tabIndex={seat.id === entrySeatId ? 0 : -1}
                      onFocus={() => setFocusSeatId(seat.id)}
                      // disabled yerine aria-disabled: ekran okuyucu ve klavye
                      // kullanıcısı dolu koltuğa da odaklanıp durumunu duyabilmeli
                      aria-disabled={!interactive}
                      aria-pressed={interactive ? state === 'selected' : undefined}
                      aria-label={`${seat.row}${seat.number}, $${seat.price}, ${STATUS_LABELS[state]}`}
                      onClick={() => interactive && onToggleSeat(seat)}
                      title={`${seat.row}${seat.number} — $${seat.price}`}
                      className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-md border
                        text-xs font-medium transition-all duration-150 ${SEAT_CLASSES[state]}`}
                    >
                      {/* Durum salt renkle anlatılmaz: renk körü kullanıcı için ikon */}
                      {state === 'sold' ? (
                        <X size={13} strokeWidth={3} aria-hidden />
                      ) : state === 'locked' ? (
                        <Clock size={13} aria-hidden />
                      ) : (
                        seat.number
                      )}
                    </button>
                  )
                })}
              </div>
              <RowLabel label={label} />
            </div>
          ))}
        </div>
      </div>

      <Legend />
    </div>
  )
}

/** İlk yükleme sırasında gösterilen hayalet grid — köşeden köşeye shimmer dalgası */
export function SeatGridSkeleton() {
  return (
    <div className="flex flex-col items-center" aria-hidden>
      <Stage dimmed />
      <div className="flex flex-col gap-2 pb-2">
        {Array.from({ length: 8 }).map((_, rowIndex) => (
          <div key={rowIndex} className="flex items-center gap-2">
            <span className="w-6" />
            <div className="flex gap-1.5">
              {Array.from({ length: 14 }).map((_, colIndex) => (
                <div
                  key={colIndex}
                  className="h-9 w-9 shrink-0 animate-pulse rounded-md bg-panel-2"
                  style={{ animationDelay: `${(rowIndex + colIndex) * 45}ms` }}
                />
              ))}
            </div>
            <span className="w-6" />
          </div>
        ))}
      </div>
    </div>
  )
}

function Stage({ dimmed = false }: { dimmed?: boolean }) {
  return (
    <div className="mb-10 w-full max-w-md">
      <div
        className={
          dimmed
            ? 'h-2 animate-pulse rounded-full bg-line'
            : 'h-2 rounded-full bg-gradient-to-r from-transparent via-neon to-transparent shadow-[0_0_25px_rgb(34_211_238/0.8)]'
        }
      />
      <div className="mt-3 text-center font-mono text-[11px] tracking-[0.5em] text-neon">
        SAHNE
      </div>
    </div>
  )
}

function Legend() {
  return (
    <div className="mt-10 flex items-center gap-6 rounded-full border border-line bg-panel px-6 py-3">
      <LegendItem boxClass="border-slate-600 bg-slate-800/50" text="BOŞ" textClass="text-slate-400" />
      <LegendItem boxClass="border-lock bg-lock/10 text-lock" text="İŞLEMDE" textClass="text-lock" icon={<Clock size={10} />} />
      <LegendItem boxClass="border-sold bg-sold/15 text-sold" text="SATILDI" textClass="text-sold" icon={<X size={10} strokeWidth={3} />} />
    </div>
  )
}

function LegendItem({
  boxClass,
  text,
  textClass,
  icon,
}: {
  boxClass: string
  text: string
  textClass: string
  icon?: React.ReactNode
}) {
  return (
    <span className="flex items-center gap-2">
      <span className={`flex h-4 w-4 items-center justify-center rounded border ${boxClass}`}>
        {icon}
      </span>
      <span className={`font-mono text-[11px] tracking-wider ${textClass}`}>{text}</span>
    </span>
  )
}

function RowLabel({ label }: { label: string }) {
  return (
    <span className="w-6 text-center font-mono text-xs font-bold text-muted select-none">
      {label}
    </span>
  )
}
