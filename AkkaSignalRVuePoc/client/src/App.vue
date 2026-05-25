<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="app">
    <header class="app-nav">
      <RouterLink to="/" class="app-nav__brand">Akka + SignalR POC</RouterLink>
      <nav class="app-nav__links">
        <RouterLink to="/">Live messages</RouterLink>
        <RouterLink to="/organisations">Organisations</RouterLink>
        <RouterLink to="/projects">Projects</RouterLink>
      </nav>
    </header>
    <main class="shell">
      <RouterView />
    </main>
  </div>
</template>
