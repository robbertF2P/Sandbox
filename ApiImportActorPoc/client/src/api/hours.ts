import type { AssignmentListItem, HourBooking } from '../types/progress'

const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

export async function fetchAssignments(): Promise<AssignmentListItem[]> {
  const response = await fetch(`${apiBase}/api/assignments`)
  if (!response.ok) {
    throw new Error('Failed to load assignments')
  }

  return response.json()
}

export async function bookHours(
  assignmentId: number,
  hours: number,
  notes?: string,
): Promise<HourBooking> {
  const response = await fetch(`${apiBase}/api/assignments/${assignmentId}/hours`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ hours, notes: notes || null }),
  })

  if (!response.ok) {
    const body = await response.json().catch(() => null)
    throw new Error(body?.error ?? 'Failed to book hours')
  }

  return response.json()
}
