import type { Organisation, Project } from '../types/catalog'

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000').replace(/\/$/, '')

async function fetchJson<T>(path: string): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`)
  if (!response.ok) {
    throw new Error(`Request failed (${response.status})`)
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
