<script setup lang="ts">
export type HomeTile = {
  label: string
  meta: string
  accent?: 'brand' | 'success' | 'warning'
}

defineProps<{
  tiles: HomeTile[]
}>()
</script>

<template>
  <!-- Semantic aliases only: f2ps-tile + f2p-tile-grid -->
  <div class="f2p-tile-grid">
    <article
      v-for="tile in tiles"
      :key="tile.label"
      class="f2ps-tile f2ps-clickable flex flex-col justify-end p-4"
      :class="{
        'border-t-4 border-t-f2p-brand': tile.accent === 'brand',
        'border-t-4 border-t-f2p-success': tile.accent === 'success',
        'border-t-4 border-t-f2p-warning': tile.accent === 'warning',
      }"
    >
      <p class="f2ps-tile__label">{{ tile.label }}</p>
      <p class="f2ps-tile__meta">{{ tile.meta }}</p>
    </article>
  </div>
</template>
