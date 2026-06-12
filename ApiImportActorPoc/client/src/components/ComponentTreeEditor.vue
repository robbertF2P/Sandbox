<script setup lang="ts">
import ActivityListEditor from './ActivityListEditor.vue'
import type { EditableComponent } from '../types/project'
import { createEmptyComponent } from '../utils/projectEditor'

const props = defineProps<{
  components: EditableComponent[]
  depth?: number
}>()

const marginLeft = `${(props.depth ?? 0) * 1}rem`

const emit = defineEmits<{
  update: [components: EditableComponent[]]
}>()

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
</script>

<template>
  <div class="component-tree" :style="{ marginLeft }">
    <div v-if="(depth ?? 0) === 0" class="sub-list__header">
      <h3>Components</h3>
      <button type="button" class="btn-secondary" @click="addComponent(components)">Add component</button>
    </div>

    <article
      v-for="(component, index) in components"
      :key="component.id"
      class="nested-card"
    >
      <div class="list-row">
        <input
          :value="component.name"
          placeholder="Component name (block, module, zone)"
          @input="patchComponent(components, index, { name: ($event.target as HTMLInputElement).value })"
        />
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
        @update="patchComponent(components, index, { childComponents: $event })"
      />
      <p v-else class="muted">No nested components.</p>
    </article>
  </div>
</template>
