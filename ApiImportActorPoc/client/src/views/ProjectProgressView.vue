<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { useRoute } from 'vue-router'
import { fetchProjectProgress } from '../api/progress'
import ProgressBar from '../components/ProgressBar.vue'
import ProgressTree from '../components/ProgressTree.vue'
import type { ProjectProgress } from '../types/progress'

const route = useRoute()
const progress = ref<ProjectProgress | null>(null)
const error = ref('')
const loading = ref(true)

onMounted(async () => {
  try {
    progress.value = await fetchProjectProgress(Number(route.params.id))
  } catch {
    error.value = 'Could not load progress for this project.'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <section class="panel">
    <div class="page-header">
      <div>
        <h1>Project progress</h1>
        <p v-if="progress" class="muted">{{ progress.name }}</p>
      </div>
      <RouterLink
        v-if="progress"
        class="btn-link"
        :to="`/projects/${progress.id}/book-hours`"
      >
        Book hours
      </RouterLink>
    </div>

    <p v-if="loading" class="muted">Loading…</p>
    <p v-else-if="error" class="error">{{ error }}</p>
    <template v-else-if="progress">
      <ProgressBar :progress="progress.progress" label="Project total" />
      <ProgressTree :components="progress.components" />
    </template>
  </section>
</template>
