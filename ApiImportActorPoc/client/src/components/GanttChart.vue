<script setup lang="ts">
import { computed } from 'vue'
import type { GanttProjectPlan } from '../types/planning'

const props = defineProps<{
  plan: GanttProjectPlan
}>()

const dayMs = 24 * 60 * 60 * 1000

function parseDate(value: string): number {
  return new Date(`${value}T00:00:00`).getTime()
}

function formatDate(value: string): string {
  return new Date(`${value}T00:00:00`).toLocaleDateString()
}

const range = computed(() => {
  const start = parseDate(props.plan.plannedStartDate)
  const end = parseDate(props.plan.plannedEndDate)
  const milestoneEnds = props.plan.milestones.map((milestone) => parseDate(milestone.targetDate))
  const maxEnd = Math.max(end, ...milestoneEnds, start)
  const totalDays = Math.max(1, Math.round((maxEnd - start) / dayMs) + 1)
  return { start, totalDays }
})

const ticks = computed(() => {
  const labels: string[] = []
  for (let day = 0; day < range.value.totalDays; day += Math.max(1, Math.floor(range.value.totalDays / 8))) {
    const date = new Date(range.value.start + day * dayMs)
    labels.push(date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' }))
  }
  return labels
})

function barStyle(startDate: string, endDate: string, tone: 'activity' | 'assignment' | 'milestone') {
  const startOffset = Math.max(0, Math.round((parseDate(startDate) - range.value.start) / dayMs))
  const duration = Math.max(1, Math.round((parseDate(endDate) - parseDate(startDate)) / dayMs) + 1)
  const left = (startOffset / range.value.totalDays) * 100
  const width = (duration / range.value.totalDays) * 100
  return {
    left: `${left}%`,
    width: `${width}%`,
    '--bar-tone': tone,
  }
}

function milestoneStyle(targetDate: string) {
  const offset = Math.max(0, Math.round((parseDate(targetDate) - range.value.start) / dayMs))
  return { left: `${(offset / range.value.totalDays) * 100}%` }
}

const rows = computed(() =>
  props.plan.activities.flatMap((activity) => [
    {
      key: `activity-${activity.activityId}`,
      label: `${activity.componentName} · ${activity.activityName}`,
      depth: 0,
      startDate: activity.startDate,
      endDate: activity.endDate,
      tone: 'activity' as const,
    },
    ...activity.assignments.map((assignment) => ({
      key: `assignment-${assignment.assignmentId}`,
      label: `${assignment.label} (${assignment.durationDays}d)`,
      depth: 1,
      startDate: assignment.startDate,
      endDate: assignment.endDate,
      tone: 'assignment' as const,
      assignmentId: assignment.assignmentId,
      durationDays: assignment.durationDays,
    })),
  ]),
)
</script>

<template>
  <div class="gantt">
    <div class="gantt__header">
      <div class="gantt__label-col">Work</div>
      <div class="gantt__timeline-col">
        <div class="gantt__ticks">
          <span v-for="(tick, index) in ticks" :key="index">{{ tick }}</span>
        </div>
      </div>
    </div>

    <div v-for="milestone in plan.milestones" :key="`milestone-${milestone.id}`" class="gantt__milestone-row">
      <div class="gantt__label-col gantt__label-col--milestone">◆ {{ milestone.name }}</div>
      <div class="gantt__timeline-col">
        <div class="gantt__milestone-marker" :style="milestoneStyle(milestone.targetDate)" :title="formatDate(milestone.targetDate)" />
      </div>
    </div>

    <div v-for="row in rows" :key="row.key" class="gantt__row">
      <div class="gantt__label-col" :style="{ paddingLeft: `${row.depth * 1.25}rem` }">
        {{ row.label }}
      </div>
      <div class="gantt__timeline-col">
        <div class="gantt__bar" :class="`gantt__bar--${row.tone}`" :style="barStyle(row.startDate, row.endDate, row.tone)">
          <span class="gantt__bar-label">{{ formatDate(row.startDate) }} – {{ formatDate(row.endDate) }}</span>
        </div>
      </div>
    </div>
  </div>
</template>
