import type { CreateEventCommand, EventSummary } from '@/types'
import { USE_MOCKS, apiClient } from '@/lib/apiClient'
import { mockDb } from '@/lib/mock/db'
import { delay, mockError } from '@/lib/mock/helpers'

interface EventsApi {
  getEvents(): Promise<EventSummary[]>
  getEvent(id: string): Promise<EventSummary>
  createEvent(command: CreateEventCommand): Promise<EventSummary>
  deleteEvent(id: string): Promise<void>
}

const httpEventsApi: EventsApi = {
  async getEvents() {
    const { data } = await apiClient.get<EventSummary[]>('/events')
    return data
  },
  async getEvent(id) {
    const { data } = await apiClient.get<EventSummary>(`/events/${id}`)
    return data
  },
  async createEvent(command) {
    const { data } = await apiClient.post<EventSummary>('/events', command)
    return data
  },
  async deleteEvent(id) {
    await apiClient.delete(`/events/${id}`)
  },
}

const mockEventsApi: EventsApi = {
  async getEvents() {
    await delay()
    return mockDb.getEvents()
  },

  async getEvent(id) {
    await delay(200)
    const event = mockDb.getEvent(id)
    if (!event) {
      throw mockError('Event.NotFound', 'Etkinlik bulunamadı.', 404)
    }
    return event
  },

  async createEvent(command) {
    await delay()
    const event: EventSummary = {
      id: `evt-${Date.now()}`,
      ...command,
      imageUrl: null,
    }
    mockDb.addEvent(event)
    return event
  },

  async deleteEvent(id) {
    await delay()
    if (!mockDb.removeEvent(id)) {
      throw mockError('Event.NotFound', 'Etkinlik bulunamadı.', 404)
    }
  },
}

export const eventsApi: EventsApi = USE_MOCKS ? mockEventsApi : httpEventsApi
