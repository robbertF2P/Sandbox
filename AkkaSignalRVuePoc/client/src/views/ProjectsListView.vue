<script setup lang="ts">
import { onMounted, ref } from 'vue'
import {
  createProject,
  deleteProject,
  getApiBaseUrl,
  listOrganisations,
  listProjects,
  updateProject,
} from '../api/catalog'
import ProjectForm from '../components/ProjectForm.vue'
import { useDataEventHub } from '../composables/useDataEventHub'
import { useToast } from '../composables/useToast'
import type { DataEventNotification, Organisation, Project, ProjectFormPayload } from '../types/catalog'

const projects = ref<Project[]>([])
const organisations = ref<Organisation[]>([])
const loading = ref(true)
const saving = ref(false)
const error = ref<string | null>(null)
const apiBaseUrl = getApiBaseUrl()
const { showToast } = useToast()

const editingProject = ref<Project | null>(null)
const showCreateForm = ref(false)

async function loadProjects() {
  try {
    projects.value = await listProjects()
    error.value = null
  } catch (fetchError) {
    error.value =
      fetchError instanceof Error ? fetchError.message : 'Unable to load projects from the API.'
  }
}

async function loadOrganisations() {
  organisations.value = await listOrganisations()
}

onMounted(async () => {
  try {
    await Promise.all([loadProjects(), loadOrganisations()])
  } catch (fetchError) {
    error.value =
      fetchError instanceof Error ? fetchError.message : 'Unable to load catalog data from the API.'
  } finally {
    loading.value = false
  }
})

useDataEventHub((notification: DataEventNotification) => {
  const labels: Record<DataEventNotification['eventType'], string> = {
    ProjectCreated: `Project created: ${notification.project.name}`,
    ProjectUpdated: `Project updated: ${notification.project.name}`,
    ProjectDeleted: `Project deleted: ${notification.project.name}`,
  }

  const label = labels[notification.eventType]
  if (!label) {
    return
  }

  showToast(label, 'success')
  void loadProjects()
})

function openCreateForm() {
  editingProject.value = null
  showCreateForm.value = true
}

function openEditForm(project: Project) {
  showCreateForm.value = false
  editingProject.value = project
}

function closeForm() {
  showCreateForm.value = false
  editingProject.value = null
}

async function handleSave(payload: ProjectFormPayload) {
  saving.value = true
  error.value = null

  try {
    if (editingProject.value) {
      await updateProject(editingProject.value.id, {
        name: payload.name,
        description: payload.description || null,
      })
      showToast(`Project updated: ${payload.name}`, 'success')
    } else {
      await createProject({
        organisationId: payload.organisationId,
        name: payload.name,
        description: payload.description || null,
      })
      showToast(`Project created: ${payload.name}`, 'success')
    }

    closeForm()
    await loadProjects()
  } catch (saveError) {
    error.value = saveError instanceof Error ? saveError.message : 'Unable to save the project.'
  } finally {
    saving.value = false
  }
}

async function handleDelete(project: Project) {
  const confirmed = window.confirm(`Delete project "${project.name}"?`)
  if (!confirmed) {
    return
  }

  saving.value = true
  error.value = null

  try {
    await deleteProject(project.id)
    if (editingProject.value?.id === project.id) {
      closeForm()
    }
    showToast(`Project deleted: ${project.name}`, 'success')
    await loadProjects()
  } catch (deleteError) {
    error.value = deleteError instanceof Error ? deleteError.message : 'Unable to delete the project.'
  } finally {
    saving.value = false
  }
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}
</script>

<template>
  <section class="hero">
    <p class="eyebrow">Catalog</p>
    <h1>Projects</h1>
    <p class="description">
      Manage projects via the REST API. Changes publish
      <code>ProjectCreated</code>, <code>ProjectUpdated</code>, and
      <code>ProjectDeleted</code> events over SignalR.
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

  <section class="catalog-toolbar">
    <h2>All projects</h2>
    <button type="button" class="btn btn--primary" :disabled="loading || saving" @click="openCreateForm">
      Add project
    </button>
  </section>

  <ProjectForm
    v-if="showCreateForm"
    mode="create"
    :organisations="organisations"
    :saving="saving"
    @save="handleSave"
    @cancel="closeForm"
  />

  <ProjectForm
    v-else-if="editingProject"
    mode="edit"
    :project="editingProject"
    :organisations="organisations"
    :saving="saving"
    @save="handleSave"
    @cancel="closeForm"
  />

  <section class="catalog-list">
    <p v-if="loading" class="empty">Loading projects…</p>
    <ul v-else-if="projects.length">
      <li v-for="project in projects" :key="project.id">
        <div class="catalog-list__primary">
          <strong>{{ project.name }}</strong>
          <span v-if="project.description" class="catalog-list__description">{{ project.description }}</span>
          <span class="catalog-list__meta">Organisation {{ project.organisationId }}</span>
        </div>
        <div class="catalog-list__aside">
          <time>{{ formatDate(project.createdAt) }}</time>
          <div class="catalog-list__actions">
            <button type="button" class="btn btn--ghost" :disabled="saving" @click="openEditForm(project)">
              Edit
            </button>
            <button type="button" class="btn btn--danger" :disabled="saving" @click="handleDelete(project)">
              Delete
            </button>
          </div>
        </div>
      </li>
    </ul>
    <p v-else class="empty">No projects found.</p>
  </section>
</template>
