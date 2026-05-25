<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getApiBaseUrl, listProjects } from '../api/catalog'
import { useDataEventHub } from '../composables/useDataEventHub'
import { useToast } from '../composables/useToast'
import type { DataEventNotification, Project } from '../types/catalog'

const projects = ref<Project[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const apiBaseUrl = getApiBaseUrl()
const { showToast } = useToast()

async function loadProjects() {
  try {
    projects.value = await listProjects()
    error.value = null
  } catch (fetchError) {
    error.value =
      fetchError instanceof Error ? fetchError.message : 'Unable to load projects from the API.'
  }
}

onMounted(async () => {
  await loadProjects()
  loading.value = false
})

useDataEventHub((notification: DataEventNotification) => {
  if (notification.eventType !== 'ProjectCreated' && notification.eventType !== 'ProjectUpdated') {
    return
  }

  const label =
    notification.eventType === 'ProjectCreated'
      ? `Project created: ${notification.project.name}`
      : `Project updated: ${notification.project.name}`

  showToast(label, 'success')
  void loadProjects()
})

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}
</script>

<template>
  <section class="hero">
    <p class="eyebrow">Catalog</p>
    <h1>Projects</h1>
    <p class="description">
      Projects returned by the Akka.NET data actors via
      <code>GET /api/projects</code>. The list refreshes when
      <code>ProjectCreated</code> or <code>ProjectUpdated</code> events arrive on SignalR.
    </p>
  </section>

  <section class="status-card">
    <div>
      <span class="label">API endpoint</span>
      <strong>{{ apiBaseUrl }}/api/projects</strong>
    </div>
    <div>
      <span class="label">Count</span>
      <strong>{{ loading ? '…' : projects.length }}</strong>
    </div>
  </section>

  <p v-if="error" class="error">{{ error }}</p>

  <section class="catalog-list">
    <h2>All projects</h2>
    <p v-if="loading" class="empty">Loading projects…</p>
    <ul v-else-if="projects.length">
      <li v-for="project in projects" :key="project.id">
        <div class="catalog-list__primary">
          <strong>{{ project.name }}</strong>
          <span v-if="project.description" class="catalog-list__description">{{ project.description }}</span>
          <span class="catalog-list__meta">Organisation {{ project.organisationId }}</span>
        </div>
        <time>{{ formatDate(project.createdAt) }}</time>
      </li>
    </ul>
    <p v-else class="empty">No projects found.</p>
  </section>
</template>
