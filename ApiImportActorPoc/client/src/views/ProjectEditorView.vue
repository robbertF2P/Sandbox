<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ComponentTreeEditor from '../components/ComponentTreeEditor.vue'
import { fetchComponentTemplates, type ComponentTemplateSummary } from '../api/components'
import { fetchProject } from '../api/projects'
import type { EditableProject } from '../types/project'
import { createEmptyProject, downloadJson, fromApiModel, toImportPayload } from '../utils/projectEditor'

const route = useRoute()
const router = useRouter()
const project = ref<EditableProject>(createEmptyProject())
const loading = ref(false)
const error = ref('')
const exportMessage = ref('')
const templates = ref<ComponentTemplateSummary[]>([])

const isNew = () => route.params.id === 'new'

async function loadProject() {
  if (isNew()) {
    return
  }

  loading.value = true
  error.value = ''
  try {
    const projectId = route.params.id as string
    const model = await fetchProject(projectId)
    project.value = fromApiModel(model)
    templates.value = await fetchComponentTemplates(projectId)
  } catch {
    error.value = 'Project not found.'
  } finally {
    loading.value = false
  }
}

onMounted(loadProject)

function exportProject() {
  const payload = toImportPayload(project.value)
  const slug = project.value.name.replace(/[^\w-]+/g, '-').toLowerCase() || 'vessel-export'
  downloadJson(`${slug}-import.json`, payload)
  exportMessage.value = 'Export downloaded. Paste into Import page or Thunder Client POST /api/import.'
}

async function copyExport() {
  const payload = toImportPayload(project.value)
  await navigator.clipboard.writeText(JSON.stringify(payload, null, 2))
  exportMessage.value = 'Import JSON copied to clipboard.'
}

function goToImport() {
  const payload = toImportPayload(project.value)
  sessionStorage.setItem('importPayload', JSON.stringify(payload))
  router.push({ name: 'import' })
}
</script>

<template>
  <section class="panel editor">
    <div class="page-header">
      <div>
        <h1>{{ isNew() ? 'New vessel project' : 'Edit project' }}</h1>
        <p class="muted">Edit components, activities, and assignments. Mark components as templates to spawn new ones with open assignments and budgeted hours.</p>
      </div>
      <div class="button-row">
        <button type="button" class="btn btn--primary" @click="exportProject">Export JSON</button>
        <button type="button" class="btn btn--ghost" @click="copyExport">Copy for import</button>
        <button type="button" class="btn btn--ghost" @click="goToImport">Open in Import</button>
      </div>
    </div>

    <p v-if="loading" class="muted">Loading…</p>
    <p v-if="error" class="error">{{ error }}</p>
    <p v-if="exportMessage" class="success">{{ exportMessage }}</p>

    <label class="field">
      <span>Project name</span>
      <input v-model="project.name" placeholder="e.g. MV Northern Star — Hull 247" />
    </label>

    <ComponentTreeEditor
      :components="project.components"
      :project-id="isNew() ? undefined : String(route.params.id)"
      :templates="templates"
      @update="project.components = $event"
      @reload="loadProject"
    />
  </section>
</template>
