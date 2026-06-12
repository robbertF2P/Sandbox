export interface ProjectSummary {
  id: string
  name: string
}

export interface EditableAssignment {
  id: string
  personName: string
  description: string
}

export interface EditableRelation {
  relatedActivityId: string
  type: 'Child' | 'Predecessor' | 'Successor'
}

export interface EditableActivity {
  id: string
  name: string
  assignments: EditableAssignment[]
  relations: EditableRelation[]
}

export interface EditableComponent {
  id: string
  name: string
  childComponents: EditableComponent[]
  activities: EditableActivity[]
}

export interface EditableProject {
  name: string
  components: EditableComponent[]
}

export interface ImportPayload {
  name: string
  components: ImportComponentPayload[]
}

export interface ImportComponentPayload {
  id?: string
  name: string
  childComponents?: ImportComponentPayload[]
  activities?: ImportActivityPayload[]
}

export interface ImportActivityPayload {
  id?: string
  name: string
  assignments?: { id?: string; personName: string; description?: string }[]
  relations?: { relatedActivityId: string; type: string }[]
}
