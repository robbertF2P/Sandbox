<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { useRoute } from 'vue-router'
import { addMilestone, fetchProjectPlan, setAssignmentDuration, setProjectPlanStart } from '../api/planning'
import GanttChart from '../components/GanttChart.vue'
import type { GanttProjectPlan } from '../types/planning'

const route = useRoute()
const projectId = Number(route.params.id)
const plan = ref<GanttProjectPlan | null>(null)
const loading = ref(true)
const error = ref('')
const message = ref('')
const startDateInput = ref('')
const milestoneName = ref('')
const milestoneDate = ref('')
const durationEdits = ref<Record<number, string>>({})

async function loadPlan() {
  loading.value = true
  error.value = ''
  try {
    plan.value = await fetchProjectPlan(projectId)
    startDateInput.value = plan.value.plannedStartDate
    durationEdits.value = Object.fromEntries(
      plan.value.activities.flatMap((activity) =>
        activity.assignments.map((assignment) => [assignment.assignmentId, String(assignment.durationDays)]),
      ),
    )
  } catch {
    error.value = 'Could not load project plan.'
  } finally {
    loading.value = false
  }
}

onMounted(loadPlan)

async function updateStartDate() {
  if (!startDateInput.value) {
    return
  }

  try {
    plan.value = await setProjectPlanStart(projectId, startDateInput.value)
    message.value = 'Timeline recalculated from new start date.'
  } catch {
    error.value = 'Failed to update start date.'
  }
}

async function updateDuration(assignmentId: number) {
  const value = Number(durationEdits.value[assignmentId])
  if (!value || value <= 0) {
    return
  }

  try {
    plan.value = await setAssignmentDuration(assignmentId, value)
    message.value = 'Timeline recalculated from assignment duration.'
    await loadPlan()
  } catch {
    error.value = 'Failed to update assignment duration.'
  }
}

async function createMilestone() {
  if (!milestoneName.value || !milestoneDate.value) {
    return
  }

  try {
    await addMilestone(projectId, milestoneName.value, milestoneDate.value)
    milestoneName.value = ''
    milestoneDate.value = ''
    message.value = 'Milestone added.'
    await loadPlan()
  } catch {
    error.value = 'Failed to add milestone.'
  }
}
</script>

<template>
  <section class="panel editor">
    <div class="page-header">
      <div>
        <h1>Project planning</h1>
        <p v-if="plan" class="muted">
          {{ plan.projectName }} · {{ plan.plannedStartDate }} → {{ plan.plannedEndDate }}
        </p>
      </div>
      <RouterLink class="btn-link" :to="`/projects/${projectId}`">Back to project</RouterLink>
    </div>

    <p v-if="loading" class="muted">Loading…</p>
    <p v-else-if="error" class="error">{{ error }}</p>
    <p v-if="message" class="success">{{ message }}</p>

    <template v-if="plan">
      <div class="plan-controls">
        <label class="field">
          <span>Project start</span>
          <div class="list-row">
            <input v-model="startDateInput" type="date" />
            <button type="button" class="btn btn--primary" @click="updateStartDate">Recalculate</button>
          </div>
        </label>

        <div class="plan-controls__milestones">
          <h3>Milestones</h3>
          <div class="list-row">
            <input v-model="milestoneName" placeholder="Milestone name" />
            <input v-model="milestoneDate" type="date" />
            <button type="button" class="btn btn--ghost" @click="createMilestone">Add</button>
          </div>
        </div>
      </div>

      <GanttChart :plan="plan" />

      <div class="plan-durations">
        <h3>Assignment durations (days)</h3>
        <p class="muted">Planning durations are separate from budgeted hours. Changing a duration recalculates the timeline.</p>
        <div
          v-for="activity in plan.activities"
          :key="activity.activityId"
          class="nested-card"
        >
          <h4>{{ activity.componentName }} · {{ activity.activityName }}</h4>
          <div
            v-for="assignment in activity.assignments"
            :key="assignment.assignmentId"
            class="list-row"
          >
            <span>{{ assignment.label }}</span>
            <input
              v-model="durationEdits[assignment.assignmentId]"
              type="number"
              min="0.5"
              step="0.5"
            />
            <button type="button" class="btn btn--ghost" @click="updateDuration(assignment.assignmentId)">
              Update
            </button>
          </div>
        </div>
      </div>
    </template>
  </section>
</template>
