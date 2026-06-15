<script setup lang="ts">
import { ref } from 'vue'
import ActivityListEditor from './ActivityListEditor.vue'
import type { ComponentTemplateSummary } from '../api/components'
import { instantiateFromTemplate, setComponentTemplate } from '../api/components'
import type { EditableComponent } from '../types/project'
import { createEmptyComponent } from '../utils/projectEditor'

const props = defineProps<{
  components: EditableComponent[]
  depth?: number
  projectId?: string
  templates?: ComponentTemplateSummary[]
}>()

const marginLeft = `${(props.depth ?? 0) * 1}rem`

const emit = defineEmits<{
  update: [components: EditableComponent[]]
  reload: []
}>()

const templateMessage = ref('')
const templateError = ref('')

function templateNameValue(templateId: number, fallback: string) {
  return (document.getElementById(`template-name-${templateId}`) as HTMLInputElement | null)?.value || fallback
}

function addComponent(current: EditableComponent[]) {
  emit('update', [...current, createEmptyComponent('New module')])
}

function removeComponent(current: EditableComponent[], index: number) {
  emit('update', current.filter((_, i) => i !== index))
}

function patchComponent(
  current: EditableComponent[],
  index: number,
  patch: Partial<EditableComponent>,
) {
  emit(
    'update',
    current.map((component, i) => (i === index ? { ...component, ...patch } : component)),
  )
}

async function toggleTemplate(index: number, component: EditableComponent, isTemplate: boolean) {
  if (!props.projectId || component.id <= 0) {
    patchComponent(props.components, index, { isTemplate })
    return
  }

  templateError.value = ''
  templateMessage.value = ''

  try {
    await setComponentTemplate(component.id, isTemplate)
    patchComponent(props.components, index, { isTemplate })
    templateMessage.value = isTemplate ? 'Marked as template.' : 'Template flag removed.'
    emit('reload')
  } catch {
    templateError.value = 'Could not update template flag.'
  }
}

async function createFromTemplate(
  _current: EditableComponent[],
  templateId: number,
  name: string,
  parentComponentId?: number,
) {
  templateError.value = ''
  templateMessage.value = ''

  try {
    const result = await instantiateFromTemplate(templateId, name, parentComponentId)
    templateMessage.value = `Created component #${result.componentId} with ${result.activityCount} activities and ${result.assignmentCount} open assignments.`
    emit('reload')
  } catch (error) {
    templateError.value = error instanceof Error ? error.message : 'Instantiation failed.'
  }
}
</script>

<template>
  <div class="component-tree" :style="{ marginLeft }">
    <div v-if="(depth ?? 0) === 0" class="sub-list__header">
      <h3>Components</h3>
      <button type="button" class="btn-secondary" @click="addComponent(components)">Add component</button>
    </div>

    <p v-if="templateMessage" class="success">{{ templateMessage }}</p>
    <p v-if="templateError" class="error">{{ templateError }}</p>

    <div
      v-if="projectId && (depth ?? 0) === 0 && templates && templates.length > 0"
      class="template-create panel-inset"
    >
      <h4>Create from template</h4>
      <p class="muted">New components get activities and open assignments with budgeted hours copied from the template.</p>
      <div
        v-for="template in templates"
        :key="template.id"
        class="list-row"
      >
        <span>{{ template.name }} ({{ template.activityCount }} activities)</span>
        <input
          :id="`template-name-${template.id}`"
          :placeholder="`${template.name} copy`"
          class="template-name-input"
        />
        <button
          type="button"
          class="btn-secondary"
          @click="createFromTemplate(
            components,
            template.id,
            templateNameValue(template.id, `${template.name} copy`),
          )"
        >
          Create
        </button>
      </div>
    </div>

    <article
      v-for="(component, index) in components"
      :key="component.id"
      class="nested-card"
      :class="{ 'nested-card--template': component.isTemplate }"
    >
      <div class="list-row">
        <input
          :value="component.name"
          placeholder="Component name (block, module, zone)"
          @input="patchComponent(components, index, { name: ($event.target as HTMLInputElement).value })"
        />
        <label class="template-toggle">
          <input
            type="checkbox"
            :checked="component.isTemplate"
            @change="toggleTemplate(index, component, ($event.target as HTMLInputElement).checked)"
          />
          Template
        </label>
        <span class="id-chip">#{{ component.id }}</span>
        <button type="button" class="btn-danger" @click="removeComponent(components, index)">Remove</button>
      </div>

      <ActivityListEditor
        :activities="component.activities"
        @update="patchComponent(components, index, { activities: $event })"
      />

      <div class="sub-list__header">
        <h4>Child components</h4>
        <button
          type="button"
          class="btn-secondary"
          @click="patchComponent(components, index, { childComponents: [...component.childComponents, createEmptyComponent('Child module')] })"
        >
          Add child
        </button>
      </div>

      <ComponentTreeEditor
        v-if="component.childComponents.length > 0"
        :components="component.childComponents"
        :depth="(depth ?? 0) + 1"
        :project-id="projectId"
        :templates="templates"
        @update="patchComponent(components, index, { childComponents: $event })"
        @reload="emit('reload')"
      />
      <p v-else class="muted">No nested components.</p>
    </article>
  </div>
</template>
