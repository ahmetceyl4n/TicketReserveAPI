import { useEffect, useState } from 'react'
import type { EventSummary } from '@/types'
import { useToastStore } from '@/store/toastStore'
import { eventsApi } from './api'
import { EventCard } from './EventCard'

export function HomePage() {
  const [events, setEvents] = useState<EventSummary[] | null>(null)
  const pushToast = useToastStore((state) => state.push)

  useEffect(() => {
    eventsApi
      .getEvents()
      .then(setEvents)
      .catch(() => pushToast('error', 'Etkinlikler yüklenemedi.'))
  }, [pushToast])

  return (
    <div className="mx-auto max-w-7xl px-6 py-12">
      <h1 className="text-4xl font-bold">Yaklaşan Etkinlikler</h1>
      <p className="mt-2 text-muted">Yüksek performanslı altyapı ile gerçek zamanlı koltuk seçimi.</p>

      <div className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {events === null
          ? Array.from({ length: 3 }).map((_, i) => <EventCardSkeleton key={i} />)
          : events.map((event) => <EventCard key={event.id} event={event} />)}
      </div>
    </div>
  )
}

function EventCardSkeleton() {
  return (
    <div className="overflow-hidden rounded-2xl border border-line bg-panel" aria-hidden>
      <div className="h-40 animate-pulse bg-panel-2" />
      <div className="space-y-3 p-5">
        <div className="h-5 w-2/3 animate-pulse rounded bg-panel-2" />
        <div className="h-4 w-full animate-pulse rounded bg-panel-2" />
        <div className="h-4 w-1/2 animate-pulse rounded bg-panel-2" />
        <div className="mt-2 h-10 animate-pulse rounded-lg bg-panel-2" />
      </div>
    </div>
  )
}
