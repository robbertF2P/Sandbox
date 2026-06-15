<script setup lang="ts">
export type CatalogItem = {
  name: string
  description?: string
  meta: string
}

defineProps<{
  items: CatalogItem[]
}>()

const emit = defineEmits<{
  edit: [item: CatalogItem]
  remove: [item: CatalogItem]
}>()
</script>

<template>
  <!-- Dark POC pattern: catalog-list + btn modifiers -->
  <section class="catalog-list">
    <div class="catalog-toolbar">
      <h2>Projects</h2>
      <button type="button" class="btn btn--primary">Add project</button>
    </div>

    <ul>
      <li v-for="item in items" :key="item.meta">
        <div class="catalog-list__primary">
          <strong>{{ item.name }}</strong>
          <span v-if="item.description" class="catalog-list__description">{{ item.description }}</span>
          <span class="catalog-list__meta">{{ item.meta }}</span>
        </div>
        <div class="catalog-list__aside">
          <div class="catalog-list__actions">
            <button type="button" class="btn btn--ghost" @click="emit('edit', item)">Edit</button>
            <button type="button" class="btn btn--danger" @click="emit('remove', item)">Delete</button>
          </div>
        </div>
      </li>
    </ul>
  </section>
</template>
