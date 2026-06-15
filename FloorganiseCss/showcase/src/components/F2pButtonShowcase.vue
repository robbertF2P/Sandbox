<script setup lang="ts">
const variants = [
  { class: 'f2ps-btn f2ps-btn-primary f2ps-btn-md', label: 'Primary' },
  { class: 'f2ps-btn f2ps-btn-secondary f2ps-btn-md', label: 'Secondary' },
  { class: 'f2ps-btn f2ps-btn-tertiary f2ps-btn-md', label: 'Tertiary' },
  { class: 'f2ps-btn f2ps-btn-approve f2ps-btn-md', label: 'Approve' },
  { class: 'f2ps-btn f2ps-btn-decline f2ps-btn-md', label: 'Decline' },
]
</script>

<template>
  <!-- f2ps-btn-* aliases — one class per intent, no utility soup -->
  <div class="flex flex-wrap gap-3">
    <button
      v-for="variant in variants"
      :key="variant.label"
      type="button"
      :class="variant.class"
    >
      {{ variant.label }}
    </button>
  </div>

  <div class="mt-4 flex flex-wrap items-center gap-3">
    <button type="button" class="f2ps-btn f2ps-btn-primary f2ps-btn-sm">Small</button>
    <button type="button" class="f2ps-btn f2ps-btn-primary f2ps-btn-md">Medium</button>
    <button type="button" class="f2ps-btn f2ps-btn-primary f2ps-btn-lg">Large</button>
  </div>
</template>
