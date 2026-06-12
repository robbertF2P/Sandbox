import type { GanttMilestone, GanttProjectPlan } from '../types/planning'

const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

export async function fetchProjectPlan(projectId: number): Promise<GanttProjectPlan> {
  const response = await fetch(`${apiBase}/api/projects/${projectId}/plan`)
  if (!response.ok) {
    throw new Error('Failed to load project plan')
  }

  return response.json()
}

export async function setProjectPlanStart(projectId: number, plannedStartDate: string): Promise<GanttProjectPlan> {
  const response = await fetch(`${apiBase}/api/projects/${projectId}/plan/start`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ plannedStartDate }),
  })

  if (!response.ok) {
    throw new Error('Failed to update project start date')
  }

  return response.json()
}

export async function setAssignmentDuration(assignmentId: number, durationDays: number): Promise<GanttProjectPlan> {
  const response = await fetch(`${apiBase}/api/assignments/${assignmentId}/duration`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ durationDays }),
  })

  if (!response.ok) {
    throw new Error('Failed to update assignment duration')
  }

  return response.json()
}

export async function addMilestone(
  projectId: number,
  name: string,
  targetDate: string,
  activityId?: number,
): Promise<GanttMilestone> {
  const response = await fetch(`${apiBase}/api/projects/${projectId}/plan/milestones`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, targetDate, activityId }),
  })

  if (!response.ok) {
    throw new Error('Failed to add milestone')
  }

  return response.json()
}
