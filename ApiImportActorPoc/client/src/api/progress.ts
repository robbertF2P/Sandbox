import type { ProjectProgress } from '../types/progress'

const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

export async function fetchProjectProgress(projectId: number): Promise<ProjectProgress> {
  const response = await fetch(`${apiBase}/api/projects/${projectId}/progress`)
  if (!response.ok) {
    throw new Error('Failed to load project progress')
  }

  return response.json()
}
