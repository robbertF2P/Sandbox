<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getImportModel, persistImport, startImport } from '../api/import'
import { useImportHub } from '../composables/useImportHub'
import type { ImportEventNotification } from '../types/import'

const samplePayload = {
  name: 'MV Northern Star — Hull 247',
  externalIds: { PLM: 'HULL-247' },
  components: [
    {
      name: 'Hull Block 204',
      externalIds: { PLM: 'BLOCK-204' },
      childComponents: [
        {
          name: 'Engine Room Module',
          externalIds: { PLM: 'MOD-ERM' },
          activities: [
            {
              name: 'Block Erection',
              externalIds: { PLM: 'ACT-ERECT' },
              assignments: [{ personName: 'Marco van Berg', description: 'Crane supervisor', budgetedHours: 40, externalIds: { HR: 'PERSON-1' } }],
              relations: [{ relatedActivityId: 'PLM:ACT-WELD', type: 'Successor' }],
            },
            {
              name: 'Structural Welding',
              externalIds: { PLM: 'ACT-WELD' },
              assignments: [{ personName: 'Elena Petrov', description: 'Certified welder', budgetedHours: 56, externalIds: { HR: 'PERSON-2' } }],
              relations: [{ relatedActivityId: 'PLM:ACT-ERECT', type: 'Predecessor' }],
            },
          ],
        },
      ],
    },
  ],
}

const jsonInput = ref(JSON.stringify(samplePayload, null, 2))

onMounted(() => {
  const stored = sessionStorage.getItem('importPayload')
  if (stored) {
    jsonInput.value = JSON.stringify(JSON.parse(stored), null, 2)
    sessionStorage.removeItem('importPayload')
  }
})
const sessionId = ref<string | null>(null)
const progress = ref<string[]>([])
const modelJson = ref('')
const statusMessage = ref('')
const isError = ref(false)
const busy = ref(false)

useImportHub((notification: ImportEventNotification) => {
  if (sessionId.value && notification.sessionId !== sessionId.value) {
    return
  }

  if (notification.eventType === 'ImportProgressUpdated' && notification.payload) {
    const step = notification.payload.step
    const total = notification.payload.totalSteps
    const message = notification.payload.message
    progress.value.push(`[${step}/${total}] ${message}`)
  }

  if (notification.eventType === 'ImportCompleted') {
    statusMessage.value = 'Import completed in memory.'
    isError.value = false
    void loadModel()
  }

  if (notification.eventType === 'ImportFailed' && notification.payload) {
    statusMessage.value = String(notification.payload.errorMessage ?? 'Import failed')
    isError.value = true
    busy.value = false
  }

  if (notification.eventType === 'ImportPersisted' && notification.payload) {
    statusMessage.value = `Persisted to database as project ${notification.payload.projectId}`
    isError.value = false
    busy.value = false
  }
})

async function loadModel() {
  if (!sessionId.value) {
    return
  }

  const response = await getImportModel(sessionId.value)
  if (!response.ok) {
    statusMessage.value = 'Model not available yet.'
    isError.value = true
    busy.value = false
    return
  }

  modelJson.value = JSON.stringify(await response.json(), null, 2)
  busy.value = false
}

async function runImport() {
  progress.value = []
  modelJson.value = ''
  statusMessage.value = ''
  isError.value = false
  busy.value = true

  let payload: unknown
  try {
    payload = JSON.parse(jsonInput.value)
  } catch {
    statusMessage.value = 'Invalid JSON payload.'
    isError.value = true
    busy.value = false
    return
  }

  const response = await startImport(payload)
  if (!response.ok) {
    statusMessage.value = 'Import request failed.'
    isError.value = true
    busy.value = false
    return
  }

  const body = await response.json()
  sessionId.value = body.sessionId
  statusMessage.value = `Import session ${body.sessionId} started.`
}

async function runPersist() {
  if (!sessionId.value) {
    return
  }

  busy.value = true
  const response = await persistImport(sessionId.value)
  if (!response.ok) {
    const body = await response.json()
    statusMessage.value = body.errorMessage ?? 'Persist failed.'
    isError.value = true
    busy.value = false
  }
}
</script>

<template>
  <div class="grid">
    <section class="panel">
      <h1>Import shipbuilding work breakdown</h1>
      <p>
        Vessel project → hull blocks / modules (nested) → outfitting activities → trade assignments.
        Activities may relate as child, predecessor, or successor on the build schedule.
      </p>
      <textarea v-model="jsonInput" spellcheck="false" />
      <button :disabled="busy" @click="runImport">Start import</button>
      <button v-if="sessionId && modelJson" :disabled="busy" @click="runPersist">
        Persist to database
      </button>
      <p v-if="statusMessage" :class="isError ? 'error' : 'success'">{{ statusMessage }}</p>
    </section>

    <section class="panel">
      <h2>Actor progress</h2>
      <ul class="progress-list">
        <li v-for="(line, index) in progress" :key="index">{{ line }}</li>
      </ul>
      <h2>In-memory model (JSON)</h2>
      <pre class="model-output">{{ modelJson || 'Waiting for import…' }}</pre>
    </section>
  </div>
</template>
