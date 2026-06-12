export interface ProjectSummary {
  id: number
  name: string
}

export interface EditableAssignment {
  id: number
  personName: string
  description: string
  externalIds: Record<string, string>
}

export interface EditableRelation {
  relatedActivityId: string
  type: 'Child' | 'Predecessor' | 'Successor'
}

export interface EditableActivity {
  id: number
  name: string
  assignments: EditableAssignment[]
  relations: EditableRelation[]
  externalIds: Record<string, string>
}

export interface EditableComponent {
  id: number
  name: string
  childComponents: EditableComponent[]
  activities: EditableActivity[]
  externalIds: Record<string, string>
}

export interface EditableProject {
  name: string
  externalIds: Record<string, string>
  components: EditableComponent[]
}

export interface ImportPayload {
  name: string
  externalIds?: Record<string, string>
  components: ImportComponentPayload[]
}

export interface ImportComponentPayload {
  name: string
  externalIds?: Record<string, string>
  childComponents?: ImportComponentPayload[]
  activities?: ImportActivityPayload[]
}

export interface ImportActivityPayload {
  name: string
  externalIds?: Record<string, string>
  assignments?: { personName: string; description?: string; externalIds?: Record<string, string> }[]
  relations?: { relatedActivityId: string; type: string }[]
}
