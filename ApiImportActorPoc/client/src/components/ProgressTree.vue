<script setup lang="ts">
import type { ActivityProgress, ComponentProgress } from '../types/progress'
import ProgressBar from './ProgressBar.vue'

defineProps<{
  components: ComponentProgress[]
  activities?: ActivityProgress[]
  depth?: number
}>()
</script>

<template>
  <ul class="progress-tree" :class="{ 'progress-tree--root': (depth ?? 0) === 0 }">
    <li v-for="component in components" :key="`component-${component.id}`">
      <details open>
        <summary>
          <span class="progress-tree__name">Component: {{ component.name }}</span>
        </summary>
        <ProgressBar :progress="component.progress" />
        <ProgressTree
          :components="component.childComponents"
          :activities="component.activities"
          :depth="(depth ?? 0) + 1"
        />
      </details>
    </li>
    <li v-for="activity in activities ?? []" :key="`activity-${activity.id}`">
      <details open>
        <summary>
          <span class="progress-tree__name">Activity: {{ activity.name }}</span>
        </summary>
        <ProgressBar :progress="activity.progress" />
        <ul class="progress-tree progress-tree--assignments">
          <li v-for="assignment in activity.assignments" :key="assignment.id">
            <div class="progress-tree__assignment">
              <strong>{{ assignment.personName }}</strong>
              <span v-if="assignment.description" class="muted">{{ assignment.description }}</span>
            </div>
            <ProgressBar :progress="assignment.progress" />
          </li>
        </ul>
      </details>
    </li>
  </ul>
</template>
