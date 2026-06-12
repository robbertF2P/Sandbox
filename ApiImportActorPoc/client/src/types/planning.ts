export interface GanttAssignmentRow {
  assignmentId: number
  label: string
  durationDays: number
  startDate: string
  endDate: string
}

export interface GanttActivityRow {
  activityId: number
  activityName: string
  componentName: string
  startDate: string
  endDate: string
  assignments: GanttAssignmentRow[]
}

export interface GanttMilestone {
  id: number
  name: string
  targetDate: string
  activityId?: number | null
}

export interface GanttProjectPlan {
  projectId: number
  projectName: string
  plannedStartDate: string
  plannedEndDate: string
  calculatedAt: string
  activities: GanttActivityRow[]
  milestones: GanttMilestone[]
}
