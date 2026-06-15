<script setup lang="ts">
import { computed, ref } from 'vue'

const states = ['Connected', 'Reconnecting', 'Disconnected'] as const
const stateIndex = ref(0)

const connectionState = computed(() => states[stateIndex.value])
const stateClass = computed(() => connectionState.value.toLowerCase())

function cycleState() {
  stateIndex.value = (stateIndex.value + 1) % states.length
}
</script>

<template>
  <!-- hero + status-card + state.* aliases -->
  <section class="hero">
    <p class="eyebrow">Live hub</p>
    <h1>SignalR status</h1>
    <p class="description">
      Connection state uses semantic <code>.state.*</code> classes mapped to f2p-dark tokens.
    </p>
  </section>

  <section class="status-card">
    <div>
      <span class="label">Hub endpoint</span>
      <strong><code>/hubs/data-events</code></strong>
    </div>
    <div>
      <span class="label">Connection</span>
      <strong :class="['state', stateClass]">{{ connectionState }}</strong>
    </div>
    <div>
      <span class="label">Messages</span>
      <strong>128</strong>
    </div>
  </section>

  <button type="button" class="btn btn--ghost mt-4" @click="cycleState">Cycle connection state</button>
</template>
