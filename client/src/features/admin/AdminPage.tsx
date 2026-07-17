import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { ArrowLeft, LayoutGrid, Plus, SquarePen, Terminal, Trash2 } from 'lucide-react'
import type { EventSummary } from '@/types'
import { isApiError } from '@/types'
import { Button } from '@/components/Button'
import { Modal } from '@/components/Modal'
import { useToastStore } from '@/store/toastStore'
import { eventsApi } from '@/features/events/api'

const dateFormatter = new Intl.DateTimeFormat('tr-TR', { dateStyle: 'short' })

export function AdminPage() {
  const [events, setEvents] = useState<EventSummary[]>([])
  const [createOpen, setCreateOpen] = useState(false)
  const pushToast = useToastStore((state) => state.push)

  useEffect(() => {
    eventsApi
      .getEvents()
      .then(setEvents)
      .catch(() => pushToast('error', 'Etkinlikler yüklenemedi.'))
  }, [pushToast])

  async function handleDelete(event: EventSummary) {
    try {
      await eventsApi.deleteEvent(event.id)
      setEvents((current) => current.filter((e) => e.id !== event.id))
      pushToast('success', `"${event.name}" silindi.`)
    } catch (error) {
      pushToast('error', isApiError(error) ? error.message : 'Silme işlemi başarısız.')
    }
  }

  return (
    <div className="mx-auto flex max-w-7xl gap-8 px-6 py-10">
      {/* Sol menü */}
      <aside className="w-64 shrink-0">
        <div className="rounded-2xl border border-line bg-panel p-4">
          <div className="px-3 pb-3 font-mono text-xs tracking-widest text-muted">
            ADMIN CONSOLE
          </div>
          <nav className="space-y-1">
            <button className="flex w-full items-center gap-3 rounded-xl border border-neon-soft bg-neon/10 px-4 py-3 text-sm font-semibold text-neon">
              <LayoutGrid size={16} />
              Etkinlik Yönetimi
            </button>
            <button className="flex w-full items-center gap-3 rounded-xl px-4 py-3 text-sm text-gray-300 hover:bg-panel-2 transition-colors cursor-pointer">
              <Terminal size={16} />
              Sistem Logları
            </button>
          </nav>
        </div>
        <Link
          to="/"
          className="mt-4 flex items-center gap-2 px-4 text-sm text-muted hover:text-white transition-colors"
        >
          <ArrowLeft size={14} />
          Ana Sayfa
        </Link>
      </aside>

      {/* İçerik */}
      <main className="flex-1 rounded-2xl border border-line bg-panel p-8">
        <div className="mb-8 flex items-start justify-between">
          <div>
            <h1 className="text-2xl font-bold">Etkinlik Yönetimi</h1>
            <p className="mt-1 text-sm text-muted">Sistemdeki tüm etkinlikleri yönetin.</p>
          </div>
          <Button variant="outline" onClick={() => setCreateOpen(true)}>
            <Plus size={16} />
            Yeni Etkinlik
          </Button>
        </div>

        <div className="space-y-3 border-t border-line pt-6">
          {events.map((event) => (
            <div
              key={event.id}
              className="flex items-center justify-between rounded-xl border border-line bg-panel-2 px-5 py-4"
            >
              <div>
                <div className="font-bold">{event.name}</div>
                <div className="mt-0.5 text-sm text-muted">
                  {event.venue} • {dateFormatter.format(new Date(event.date))}
                </div>
              </div>
              <div className="flex items-center gap-2">
                <button
                  className="rounded-lg p-2 text-neon hover:bg-neon/10 transition-colors cursor-pointer"
                  title="Düzenle"
                >
                  <SquarePen size={17} />
                </button>
                <button
                  onClick={() => handleDelete(event)}
                  className="rounded-lg p-2 text-rose-400 hover:bg-rose-950/40 transition-colors cursor-pointer"
                  title="Sil"
                >
                  <Trash2 size={17} />
                </button>
              </div>
            </div>
          ))}
          {events.length === 0 && (
            <p className="py-10 text-center text-sm text-muted">Henüz etkinlik yok.</p>
          )}
        </div>
      </main>

      <CreateEventModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={(event) => {
          setEvents((current) => [event, ...current])
          setCreateOpen(false)
          pushToast('success', `"${event.name}" oluşturuldu.`)
        }}
      />
    </div>
  )
}

function CreateEventModal({
  open,
  onClose,
  onCreated,
}: {
  open: boolean
  onClose: () => void
  onCreated: (event: EventSummary) => void
}) {
  const [saving, setSaving] = useState(false)
  const pushToast = useToastStore((state) => state.push)

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    const form = new FormData(e.currentTarget)
    setSaving(true)
    try {
      const event = await eventsApi.createEvent({
        name: String(form.get('name')),
        description: String(form.get('description')),
        venue: String(form.get('venue')),
        date: new Date(String(form.get('date'))).toISOString(),
      })
      onCreated(event)
    } catch (error) {
      pushToast('error', isApiError(error) ? error.message : 'Etkinlik oluşturulamadı.')
    } finally {
      setSaving(false)
    }
  }

  const inputClass =
    'w-full rounded-lg border border-line bg-panel-2 px-4 py-3 text-sm placeholder:text-muted focus:border-neon focus:outline-none'

  return (
    <Modal open={open} title="Yeni Etkinlik" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <input name="name" required placeholder="Etkinlik adı" className={inputClass} />
        <textarea
          name="description"
          required
          placeholder="Açıklama"
          rows={3}
          className={inputClass}
        />
        <input name="venue" required placeholder="Mekan" className={inputClass} />
        <input name="date" required type="datetime-local" className={inputClass} />
        <Button type="submit" className="w-full" disabled={saving}>
          {saving ? 'Kaydediliyor...' : 'Oluştur'}
        </Button>
      </form>
    </Modal>
  )
}
