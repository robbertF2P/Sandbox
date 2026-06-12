<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="app">
    <header class="app-nav">
      <RouterLink to="/" class="app-nav__brand">API Import Actor POC</RouterLink>
    </header>
    <main class="shell">
      <RouterView />
    </main>
  </div>
</template>
