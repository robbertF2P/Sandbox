import type { Organisation, Project } from '../types/catalog'

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000').replace(/\/$/, '')

async function fetchJson<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, init)
  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { error?: string } | null
    throw new Error(body?.error ?? `Request failed (${response.status})`)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}

export function getApiBaseUrl(): string {
  return apiBaseUrl
}

export function listOrganisations(): Promise<Organisation[]> {
  return fetchJson<Organisation[]>('/api/organisations')
}

export function listProjects(): Promise<Project[]> {
  return fetchJson<Project[]>('/api/projects')
}

export interface CreateProjectPayload {
  organisationId: string
  name: string
  description?: string | null
}

export interface UpdateProjectPayload {
  name: string
  description?: string | null
}

export function createProject(payload: CreateProjectPayload): Promise<Project> {
  return fetchJson<Project>('/api/projects', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
}

export function updateProject(id: string, payload: UpdateProjectPayload): Promise<Project> {
  return fetchJson<Project>(`/api/projects/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
}

export function deleteProject(id: string): Promise<void> {
  return fetchJson<void>(`/api/projects/${id}`, { method: 'DELETE' })
}
