<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { bookHours, fetchAssignments } from '../api/hours'
import ProgressBar from '../components/ProgressBar.vue'
import type { AssignmentListItem } from '../types/progress'

const route = useRoute()
const assignments = ref<AssignmentListItem[]>([])
const selectedId = ref<number | null>(null)
const hours = ref('8')
const notes = ref('')
const statusMessage = ref('')
const isError = ref(false)
const loading = ref(true)
const busy = ref(false)

const projectFilter = computed(() => {
  const raw = route.params.id
  return raw ? Number(raw) : null
})

const visibleAssignments = computed(() =>
  projectFilter.value
    ? assignments.value.filter((assignment) => assignment.projectId === projectFilter.value)
    : assignments.value,
)

const selectedAssignment = computed(() =>
  visibleAssignments.value.find((assignment) => assignment.id === selectedId.value) ?? null,
)

onMounted(async () => {
  try {
    assignments.value = await fetchAssignments()
    if (visibleAssignments.value.length > 0) {
      selectedId.value = visibleAssignments.value[0].id
    }
  } catch {
    statusMessage.value = 'Could not load assignments.'
    isError.value = true
  } finally {
    loading.value = false
  }
})

async function submit() {
  if (!selectedId.value) {
    return
  }

  const parsedHours = Number(hours.value)
  if (!Number.isFinite(parsedHours) || parsedHours <= 0) {
    statusMessage.value = 'Enter a positive number of hours.'
    isError.value = true
    return
  }

  busy.value = true
  statusMessage.value = ''
  isError.value = false

  try {
    await bookHours(selectedId.value, parsedHours, notes.value || undefined)
    assignments.value = await fetchAssignments()
    statusMessage.value = `Booked ${parsedHours} h on ${selectedAssignment.value?.personName}.`
    notes.value = ''
  } catch (error) {
    statusMessage.value = error instanceof Error ? error.message : 'Booking failed.'
    isError.value = true
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <section class="panel">
    <div class="page-header">
      <div>
        <h1>Book hours</h1>
        <p class="muted">Record worked hours against an assignment.</p>
      </div>
      <RouterLink
        v-if="projectFilter"
        class="btn-link"
        :to="`/projects/${projectFilter}/progress`"
      >
        View progress
      </RouterLink>
    </div>

    <p v-if="loading" class="muted">Loading…</p>
    <p v-else-if="visibleAssignments.length === 0" class="muted">
      No assignments yet. Import a project with budgeted hours first.
    </p>
    <form v-else class="book-hours-form" @submit.prevent="submit">
      <label>
        Assignment
        <select v-model.number="selectedId">
          <option
            v-for="assignment in visibleAssignments"
            :key="assignment.id"
            :value="assignment.id"
          >
            {{ assignment.projectName }} — {{ assignment.componentPath }} — {{ assignment.activityName }} — {{ assignment.personName }}
          </option>
        </select>
      </label>

      <ProgressBar
        v-if="selectedAssignment"
        :progress="{
          budgetedHours: selectedAssignment.budgetedHours,
          hoursWorked: selectedAssignment.hoursWorked,
          percentComplete: selectedAssignment.budgetedHours <= 0
            ? (selectedAssignment.hoursWorked > 0 ? 100 : 0)
            : Math.round(selectedAssignment.hoursWorked / selectedAssignment.budgetedHours * 1000) / 10,
        }"
        label="Current assignment progress"
      />

      <label>
        Hours worked
        <input v-model="hours" type="number" min="0.25" step="0.25" required />
      </label>

      <label>
        Notes
        <input v-model="notes" type="text" placeholder="Optional shift note" />
      </label>

      <button type="submit" :disabled="busy">{{ busy ? 'Saving…' : 'Book hours' }}</button>
      <p v-if="statusMessage" :class="isError ? 'error' : 'success'">{{ statusMessage }}</p>
    </form>
  </section>
</template>
