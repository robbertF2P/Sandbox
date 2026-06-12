<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ComponentTreeEditor from '../components/ComponentTreeEditor.vue'
import { fetchProject } from '../api/projects'
import type { EditableProject } from '../types/project'
import { createEmptyProject, downloadJson, fromApiModel, toImportPayload } from '../utils/projectEditor'

const route = useRoute()
const router = useRouter()
const project = ref<EditableProject>(createEmptyProject())
const loading = ref(false)
const error = ref('')
const exportMessage = ref('')

const isNew = () => route.params.id === 'new'

onMounted(async () => {
  if (isNew()) {
    return
  }

  loading.value = true
  try {
    const model = await fetchProject(route.params.id as string)
    project.value = fromApiModel(model)
  } catch {
    error.value = 'Project not found.'
  } finally {
    loading.value = false
  }
})

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
        <p class="muted">Edit components, activities, and assignments. Export to test the import round-trip.</p>
      </div>
      <div class="button-row">
        <button type="button" @click="exportProject">Export JSON</button>
        <button type="button" class="btn-secondary" @click="copyExport">Copy for import</button>
        <button type="button" class="btn-secondary" @click="goToImport">Open in Import</button>
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
      @update="project.components = $event"
    />
  </section>
</template>
