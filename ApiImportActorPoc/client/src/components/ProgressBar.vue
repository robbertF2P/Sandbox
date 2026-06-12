<script setup lang="ts">
import type { ProgressSummary } from '../types/progress'

defineProps<{
  progress: ProgressSummary
  label?: string
}>()
</script>

<template>
  <div class="progress-block">
    <div v-if="label" class="progress-block__label">{{ label }}</div>
    <div class="progress-bar">
      <div
        class="progress-bar__fill"
        :style="{ width: `${Math.min(progress.percentComplete, 100)}%` }"
      />
    </div>
    <div class="progress-block__meta">
      <span>{{ progress.hoursWorked }} / {{ progress.budgetedHours }} h</span>
      <span>{{ progress.percentComplete }}%</span>
    </div>
  </div>
</template>
