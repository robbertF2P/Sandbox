<script setup lang="ts">
import { ref } from 'vue'

const visible = ref(true)

function dismiss() {
  visible.value = false
  window.setTimeout(() => {
    visible.value = true
  }, 1800)
}
</script>

<template>
  <div v-if="visible" class="toast success max-w-sm">
    <p>Template saved. Semantic <code>.toast.success</code> — no utility chain needed.</p>
    <button type="button" class="toast__dismiss" @click="dismiss">Dismiss</button>
  </div>
</template>
