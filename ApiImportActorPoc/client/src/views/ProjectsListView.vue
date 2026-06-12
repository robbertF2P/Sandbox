<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { fetchProjects } from '../api/projects'
import type { ProjectSummary } from '../types/project'

const projects = ref<ProjectSummary[]>([])
const error = ref('')
const loading = ref(true)

onMounted(async () => {
  try {
    projects.value = await fetchProjects()
  } catch {
    error.value = 'Could not load projects. Import and persist a vessel first.'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <section class="panel">
    <div class="page-header">
      <div>
        <h1>Vessel projects</h1>
        <p class="muted">Persisted shipbuilding projects from the database.</p>
      </div>
      <RouterLink class="btn-link" to="/projects/new">New project</RouterLink>
    </div>

    <p v-if="loading" class="muted">Loading…</p>
    <p v-else-if="error" class="error">{{ error }}</p>
    <ul v-else class="entity-list">
      <li v-for="project in projects" :key="project.id">
        <RouterLink :to="`/projects/${project.id}`">{{ project.name }}</RouterLink>
        <span class="id-chip">#{{ project.id }}</span>
      </li>
    </ul>
    <p v-if="!loading && !error && projects.length === 0" class="muted">
      No projects yet. Use <RouterLink to="/">Import</RouterLink> to persist one.
    </p>
  </section>
</template>
