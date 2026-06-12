import type {
  EditableActivity,
  EditableAssignment,
  EditableComponent,
  EditableProject,
  EditableRelation,
  ImportActivityPayload,
  ImportComponentPayload,
  ImportPayload,
} from '../types/project'

export function newId(): string {
  return crypto.randomUUID()
}

export function createEmptyProject(): EditableProject {
  return {
    name: 'New vessel project',
    components: [createEmptyComponent('Hull Block')],
  }
}

export function createEmptyComponent(name = 'Component'): EditableComponent {
  return {
    id: newId(),
    name,
    childComponents: [],
    activities: [],
  }
}

export function createEmptyActivity(): EditableActivity {
  return {
    id: newId(),
    name: 'New activity',
    assignments: [],
    relations: [],
  }
}

export function createEmptyAssignment(): EditableAssignment {
  return {
    id: newId(),
    personName: '',
    description: '',
  }
}

export function createEmptyRelation(): EditableRelation {
  return {
    relatedActivityId: '',
    type: 'Successor',
  }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function fromApiModel(model: any): EditableProject {
  return {
    name: model.name,
    components: (model.components ?? []).map(mapComponent),
  }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mapComponent(component: any): EditableComponent {
  return {
    id: component.id,
    name: component.name,
    childComponents: (component.childComponents ?? []).map(mapComponent),
    activities: (component.activities ?? []).map(mapActivity),
  }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mapActivity(activity: any): EditableActivity {
  return {
    id: activity.id,
    name: activity.name,
    assignments: (activity.assignments ?? []).map((assignment: any) => ({
      id: assignment.id,
      personName: assignment.personName,
      description: assignment.description ?? '',
    })),
    relations: (activity.relations ?? []).map((relation: any) => ({
      relatedActivityId: relation.relatedActivityId,
      type: relation.type,
    })),
  }
}

export function toImportPayload(project: EditableProject): ImportPayload {
  return {
    name: project.name,
    components: project.components.map(toComponentPayload),
  }
}

function toComponentPayload(component: EditableComponent): ImportComponentPayload {
  return {
    id: component.id,
    name: component.name,
    childComponents: component.childComponents.length
      ? component.childComponents.map(toComponentPayload)
      : undefined,
    activities: component.activities.length
      ? component.activities.map(toActivityPayload)
      : undefined,
  }
}

function toActivityPayload(activity: EditableActivity): ImportActivityPayload {
  return {
    id: activity.id,
    name: activity.name,
    assignments: activity.assignments.length
      ? activity.assignments.map((assignment) => ({
          id: assignment.id,
          personName: assignment.personName,
          description: assignment.description || undefined,
        }))
      : undefined,
    relations: activity.relations.length
      ? activity.relations.map((relation) => ({
          relatedActivityId: relation.relatedActivityId,
          type: relation.type,
        }))
      : undefined,
  }
}

export function downloadJson(filename: string, data: unknown) {
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = filename
  anchor.click()
  URL.revokeObjectURL(url)
}
