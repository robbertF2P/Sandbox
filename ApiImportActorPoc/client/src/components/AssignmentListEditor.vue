<script setup lang="ts">
import type { EditableAssignment } from '../types/project'
import { createEmptyAssignment } from '../utils/projectEditor'

defineProps<{
  assignments: EditableAssignment[]
}>()

const emit = defineEmits<{
  update: [assignments: EditableAssignment[]]
}>()

function addAssignment(current: EditableAssignment[]) {
  emit('update', [...current, createEmptyAssignment()])
}

function removeAssignment(current: EditableAssignment[], index: number) {
  emit('update', current.filter((_, i) => i !== index))
}

function patchAssignment(
  current: EditableAssignment[],
  index: number,
  patch: Partial<EditableAssignment>,
) {
  emit(
    'update',
    current.map((assignment, i) => (i === index ? { ...assignment, ...patch } : assignment)),
  )
}
</script>

<template>
  <div class="sub-list">
    <div class="sub-list__header">
      <h4>Assignments</h4>
      <button type="button" class="btn-secondary" @click="addAssignment(assignments)">Add</button>
    </div>
    <p v-if="assignments.length === 0" class="muted">No assignments yet.</p>
    <div v-for="(assignment, index) in assignments" :key="assignment.id" class="list-row">
      <input
        :value="assignment.personName"
        placeholder="Person / trade"
        @input="patchAssignment(assignments, index, { personName: ($event.target as HTMLInputElement).value })"
      />
      <input
        :value="assignment.description"
        placeholder="Description"
        @input="patchAssignment(assignments, index, { description: ($event.target as HTMLInputElement).value })"
      />
      <button type="button" class="btn-danger" @click="removeAssignment(assignments, index)">Remove</button>
    </div>
  </div>
</template>
