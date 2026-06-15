<script setup lang="ts">
import { ref } from 'vue'

const progress = ref(62)

function bumpProgress() {
  progress.value = progress.value >= 100 ? 12 : progress.value + 11
}
</script>

<template>
  <!-- Hybrid: semantic panel + Tailwind utilities for one-off layout -->
  <section class="panel flex flex-col gap-4 lg:flex-row lg:items-stretch">
    <div class="flex-1">
      <p class="eyebrow">Hybrid styling</p>
      <h2 class="text-xl font-bold text-white">Semantic + utilities</h2>
      <p class="muted mt-2">
        <code>.panel</code> carries surface, border, and radius. Layout gaps use Tailwind:
        <code>flex flex-col gap-4 lg:flex-row</code>.
      </p>
    </div>

    <div class="w-full lg:w-72">
      <div class="progress-block">
        <p class="progress-block__label">Import progress</p>
        <div class="progress-bar">
          <div class="progress-bar__fill" :style="{ width: `${progress}%` }" />
        </div>
        <div class="progress-block__meta">
          <span>Actor pipeline</span>
          <span>{{ progress }}%</span>
        </div>
      </div>
      <button type="button" class="btn btn--primary mt-3 w-full" @click="bumpProgress">
        Advance
      </button>
    </div>
  </section>
</template>
