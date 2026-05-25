<script setup lang="ts">
import { useToast } from '../composables/useToast'

const { toasts, dismissToast } = useToast()
</script>

<template>
  <div class="toast-stack" aria-live="polite">
    <article
      v-for="toast in toasts"
      :key="toast.id"
      :class="['toast', toast.variant]"
    >
      <p>{{ toast.message }}</p>
      <button type="button" class="toast__dismiss" @click="dismissToast(toast.id)">Dismiss</button>
    </article>
  </div>
</template>
