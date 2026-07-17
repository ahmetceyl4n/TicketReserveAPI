import { Link } from 'react-router-dom'
import { ArrowRight, Calendar, Gamepad2, MapPin, Music, Sparkles } from 'lucide-react'
import type { EventSummary } from '@/types'
import { hashString } from '@/lib/mock/helpers'

const PLACEHOLDER_VISUALS = [
  { icon: Music, gradient: 'from-fuchsia-900/60 via-panel-2 to-panel' },
  { icon: Gamepad2, gradient: 'from-cyan-900/60 via-panel-2 to-panel' },
  { icon: Sparkles, gradient: 'from-violet-900/60 via-panel-2 to-panel' },
]

const dateFormatter = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric',
  month: 'short',
  weekday: 'short',
  hour: '2-digit',
  minute: '2-digit',
})

export function EventCard({ event }: { event: EventSummary }) {
  // imageUrl backend'den gelene kadar: isimden türetilen deterministik placeholder
  const visual = PLACEHOLDER_VISUALS[hashString(event.id) % PLACEHOLDER_VISUALS.length]
  const Icon = visual.icon

  return (
    <article className="group overflow-hidden rounded-2xl border border-line bg-panel transition-all hover:border-neon-soft hover:shadow-[0_0_30px_rgb(34_211_238/0.1)]">
      <div className={`flex h-40 items-center justify-center bg-gradient-to-br ${visual.gradient}`}>
        {event.imageUrl ? (
          <img src={event.imageUrl} alt={event.name} className="h-full w-full object-cover" />
        ) : (
          <Icon size={56} className="text-white/25 transition-transform group-hover:scale-110" />
        )}
      </div>

      <div className="p-5">
        <h3 className="text-lg font-bold">{event.name}</h3>
        <p className="mt-2 line-clamp-2 text-sm text-muted">{event.description}</p>

        <div className="mt-4 flex items-center gap-4 text-xs text-muted">
          <span className="flex items-center gap-1.5">
            <Calendar size={13} className="text-neon" />
            {dateFormatter.format(new Date(event.date))}
          </span>
          <span className="flex items-center gap-1.5">
            <MapPin size={13} className="text-neon" />
            {event.venue}
          </span>
        </div>

        <Link
          to={`/events/${event.id}/seats`}
          className="mt-5 flex items-center justify-center gap-2 rounded-lg border border-neon-soft bg-neon/5
            py-2.5 text-sm font-semibold text-neon transition-all
            hover:bg-neon/15 hover:shadow-[0_0_16px_rgb(34_211_238/0.25)]"
        >
          Hemen Koltuk Seç
          <ArrowRight size={15} />
        </Link>
      </div>
    </article>
  )
}
