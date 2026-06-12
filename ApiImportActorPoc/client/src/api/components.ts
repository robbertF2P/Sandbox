const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

export interface ComponentTemplateSummary {
  id: number
  name: string
  activityCount: number
  assignmentCount: number
}

export interface InstantiateComponentResult {
  componentId: number
  activityCount: number
  assignmentCount: number
}

export async function fetchComponentTemplates(projectId: string): Promise<ComponentTemplateSummary[]> {
  const response = await fetch(`${apiBase}/api/projects/${projectId}/component-templates`)
  if (!response.ok) {
    throw new Error('Failed to load component templates')
  }

  return response.json()
}

export async function setComponentTemplate(componentId: number, isTemplate: boolean): Promise<unknown> {
  const response = await fetch(`${apiBase}/api/components/${componentId}/template`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ isTemplate }),
  })

  if (!response.ok) {
    throw new Error('Failed to update template flag')
  }

  return response.json()
}

export async function instantiateFromTemplate(
  templateComponentId: number,
  name: string,
  parentComponentId?: number,
): Promise<InstantiateComponentResult> {
  const response = await fetch(`${apiBase}/api/components/${templateComponentId}/instantiate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, parentComponentId }),
  })

  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error(body.error ?? 'Failed to create component from template')
  }

  return response.json()
}
