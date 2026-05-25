export interface Organisation {
  id: string
  name: string
  createdAt: string
}

export interface Project {
  id: string
  organisationId: string
  name: string
  description: string | null
  createdAt: string
}

export type DataEventType = 'ProjectCreated' | 'ProjectUpdated' | 'ProjectDeleted'

export interface DataEventNotification {
  eventType: DataEventType
  project: Project
  occurredAt: string
}

export interface ProjectFormPayload {
  organisationId: string
  name: string
  description: string
}
