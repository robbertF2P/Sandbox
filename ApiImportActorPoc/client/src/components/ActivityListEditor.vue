<script setup lang="ts">
import AssignmentListEditor from './AssignmentListEditor.vue'
import type { EditableActivity } from '../types/project'
import { createEmptyActivity, createEmptyRelation } from '../utils/projectEditor'

defineProps<{
  activities: EditableActivity[]
}>()

const emit = defineEmits<{
  update: [activities: EditableActivity[]]
}>()

function addActivity(current: EditableActivity[]) {
  emit('update', [...current, createEmptyActivity()])
}

function removeActivity(current: EditableActivity[], index: number) {
  emit('update', current.filter((_, i) => i !== index))
}

function patchActivity(current: EditableActivity[], index: number, patch: Partial<EditableActivity>) {
  emit(
    'update',
    current.map((activity, i) => (i === index ? { ...activity, ...patch } : activity)),
  )
}

function addRelation(current: EditableActivity[], index: number) {
  const activity = current[index]
  patchActivity(current, index, {
    relations: [...activity.relations, createEmptyRelation()],
  })
}

function updateRelation(
  current: EditableActivity[],
  activityIndex: number,
  relationIndex: number,
  field: 'relatedActivityId' | 'type',
  value: string,
) {
  const activity = current[activityIndex]
  const relations = activity.relations.map((relation, i) =>
    i === relationIndex ? { ...relation, [field]: value } : relation,
  )
  patchActivity(current, activityIndex, { relations })
}

function removeRelation(current: EditableActivity[], activityIndex: number, relationIndex: number) {
  const activity = current[activityIndex]
  patchActivity(current, activityIndex, {
    relations: activity.relations.filter((_, i) => i !== relationIndex),
  })
}
</script>

<template>
  <div class="sub-list">
    <div class="sub-list__header">
      <h4>Activities</h4>
      <button type="button" class="btn-secondary" @click="addActivity(activities)">Add</button>
    </div>
    <p v-if="activities.length === 0" class="muted">No activities yet.</p>
    <article v-for="(activity, index) in activities" :key="activity.id" class="nested-card">
      <div class="list-row">
        <input
          :value="activity.name"
          placeholder="Activity name"
          @input="patchActivity(activities, index, { name: ($event.target as HTMLInputElement).value })"
        />
        <span class="id-chip">{{ activity.id.slice(0, 8) }}</span>
        <button type="button" class="btn-danger" @click="removeActivity(activities, index)">Remove</button>
      </div>

      <AssignmentListEditor
        :assignments="activity.assignments"
        @update="patchActivity(activities, index, { assignments: $event })"
      />

      <div class="sub-list">
        <div class="sub-list__header">
          <h4>Relations</h4>
          <button type="button" class="btn-secondary" @click="addRelation(activities, index)">Add</button>
        </div>
        <div
          v-for="(relation, relationIndex) in activity.relations"
          :key="`${activity.id}-${relationIndex}`"
          class="list-row"
        >
          <input
            :value="relation.relatedActivityId"
            placeholder="Related activity id"
            @input="updateRelation(activities, index, relationIndex, 'relatedActivityId', ($event.target as HTMLInputElement).value)"
          />
          <select
            :value="relation.type"
            @change="updateRelation(activities, index, relationIndex, 'type', ($event.target as HTMLSelectElement).value)"
          >
            <option value="Child">Child</option>
            <option value="Predecessor">Predecessor</option>
            <option value="Successor">Successor</option>
          </select>
          <button type="button" class="btn-danger" @click="removeRelation(activities, index, relationIndex)">
            Remove
          </button>
        </div>
      </div>
    </article>
  </div>
</template>
