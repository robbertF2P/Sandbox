<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { Organisation, Project, ProjectFormPayload } from '../types/catalog'

export type ProjectFormMode = 'create' | 'edit'

const props = defineProps<{
  mode: ProjectFormMode
  project?: Project | null
  organisations: Organisation[]
  saving?: boolean
}>()

const emit = defineEmits<{
  save: [payload: ProjectFormPayload]
  cancel: []
}>()

const organisationId = ref('')
const name = ref('')
const description = ref('')

const title = computed(() => (props.mode === 'create' ? 'Add project' : 'Edit project'))
const submitLabel = computed(() => (props.mode === 'create' ? 'Create project' : 'Save changes'))

watch(
  () => [props.mode, props.project, props.organisations] as const,
  () => {
    if (props.mode === 'edit' && props.project) {
      organisationId.value = props.project.organisationId
      name.value = props.project.name
      description.value = props.project.description ?? ''
      return
    }

    organisationId.value = props.organisations[0]?.id ?? ''
    name.value = ''
    description.value = ''
  },
  { immediate: true },
)

function onSubmit() {
  emit('save', {
    organisationId: organisationId.value,
    name: name.value.trim(),
    description: description.value.trim(),
  })
}
</script>

<template>
  <section class="project-form">
    <header class="project-form__header">
      <h2>{{ title }}</h2>
      <button type="button" class="btn btn--ghost" @click="emit('cancel')">Cancel</button>
    </header>

    <form class="project-form__body" @submit.prevent="onSubmit">
      <label class="field">
        <span class="label">Organisation</span>
        <select
          v-model="organisationId"
          :disabled="mode === 'edit' || saving || organisations.length === 0"
          required
        >
          <option v-for="organisation in organisations" :key="organisation.id" :value="organisation.id">
            {{ organisation.name }}
          </option>
        </select>
      </label>

      <label class="field">
        <span class="label">Name</span>
        <input v-model="name" type="text" required maxlength="200" :disabled="saving" />
      </label>

      <label class="field">
        <span class="label">Description</span>
        <textarea v-model="description" rows="3" maxlength="1000" :disabled="saving" />
      </label>

      <div class="project-form__actions">
        <button type="submit" class="btn btn--primary" :disabled="saving || !name.trim()">
          {{ saving ? 'Saving…' : submitLabel }}
        </button>
      </div>
    </form>
  </section>
</template>
