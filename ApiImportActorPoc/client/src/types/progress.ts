export interface ProgressSummary {
  budgetedHours: number
  hoursWorked: number
  percentComplete: number
}

export interface AssignmentProgress {
  id: number
  personName: string
  description?: string | null
  progress: ProgressSummary
}

export interface ActivityProgress {
  id: number
  name: string
  progress: ProgressSummary
  assignments: AssignmentProgress[]
}

export interface ComponentProgress {
  id: number
  name: string
  progress: ProgressSummary
  childComponents: ComponentProgress[]
  activities: ActivityProgress[]
}

export interface ProjectProgress {
  id: number
  name: string
  progress: ProgressSummary
  components: ComponentProgress[]
}

export interface AssignmentListItem {
  id: number
  projectId: number
  projectName: string
  componentPath: string
  activityName: string
  personName: string
  budgetedHours: number
  hoursWorked: number
}

export interface HourBooking {
  id: number
  assignmentId: number
  hours: number
  bookedAt: string
  notes?: string | null
}
