<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import ToastContainer from './components/ToastContainer.vue'
</script>

<template>
  <div class="f2p-app-dark app">
    <ToastContainer />
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
