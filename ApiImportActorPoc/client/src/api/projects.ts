import type { ProjectSummary } from '../types/project'

const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

export async function fetchProjects(): Promise<ProjectSummary[]> {
  const response = await fetch(`${apiBase}/api/projects`)
  if (!response.ok) {
    throw new Error('Failed to load projects')
  }

  return response.json()
}

export async function fetchProject(projectId: string): Promise<unknown> {
  const response = await fetch(`${apiBase}/api/projects/${projectId}`)
  if (!response.ok) {
    throw new Error('Project not found')
  }

  return response.json()
}

export async function fetchProjectExport(projectId: string): Promise<unknown> {
  const response = await fetch(`${apiBase}/api/projects/${projectId}/export`)
  if (!response.ok) {
    throw new Error('Export failed')
  }

  return response.json()
}
