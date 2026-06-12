import type {
  EditableActivity,
  EditableAssignment,
  EditableComponent,
  EditableProject,
  EditableRelation,
  ImportPayload,
} from '../types/project'

let tempId = -1

export function newTempId(): number {
  return tempId--
}

export function resetTempIds(): void {
  tempId = -1
}

export function createEmptyProject(): EditableProject {
  resetTempIds()
  return {
    name: 'New vessel project',
    externalIds: {},
    components: [createEmptyComponent('Hull Block')],
  }
}

export function createEmptyComponent(name = 'Component'): EditableComponent {
  return {
    id: newTempId(),
    name,
    externalIds: {},
    childComponents: [],
    activities: [],
  }
}

export function createEmptyActivity(): EditableActivity {
  return {
    id: newTempId(),
    name: 'New activity',
    externalIds: {},
    assignments: [],
    relations: [],
  }
}

export function createEmptyAssignment(): EditableAssignment {
  return {
    id: newTempId(),
    personName: '',
    description: '',
    externalIds: {},
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
    externalIds: model.externalIds ?? {},
    components: (model.components ?? []).map(mapComponent),
  }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mapComponent(component: any): EditableComponent {
  return {
    id: component.id,
    name: component.name,
    externalIds: component.externalIds ?? {},
    childComponents: (component.childComponents ?? []).map(mapComponent),
    activities: (component.activities ?? []).map(mapActivity),
  }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mapActivity(activity: any): EditableActivity {
  return {
    id: activity.id,
    name: activity.name,
    externalIds: activity.externalIds ?? {},
    assignments: (activity.assignments ?? []).map((assignment: any) => ({
      id: assignment.id,
      personName: assignment.personName,
      description: assignment.description ?? '',
      externalIds: assignment.externalIds ?? {},
    })),
    relations: (activity.relations ?? []).map((relation: any) => ({
      relatedActivityId: String(relation.relatedActivityId),
      type: relation.type,
    })),
  }
}

export function toImportPayload(project: EditableProject): ImportPayload {
  return {
    name: project.name,
    externalIds: Object.keys(project.externalIds).length ? project.externalIds : undefined,
    components: project.components.map(toComponentPayload),
  }
}

function toComponentPayload(component: EditableComponent) {
  return {
    name: component.name,
    externalIds: Object.keys(component.externalIds).length ? component.externalIds : undefined,
    childComponents: component.childComponents.length
      ? component.childComponents.map(toComponentPayload)
      : undefined,
    activities: component.activities.length
      ? component.activities.map(toActivityPayload)
      : undefined,
  }
}

function toActivityPayload(activity: EditableActivity) {
  return {
    name: activity.name,
    externalIds: Object.keys(activity.externalIds).length ? activity.externalIds : undefined,
    assignments: activity.assignments.length
      ? activity.assignments.map((assignment) => ({
          personName: assignment.personName,
          description: assignment.description || undefined,
          externalIds: Object.keys(assignment.externalIds).length ? assignment.externalIds : undefined,
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
