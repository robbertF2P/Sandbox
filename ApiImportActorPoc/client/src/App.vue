<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="f2p-app-dark app">
    <header class="app-nav app-nav--wide">
      <RouterLink to="/" class="app-nav__brand">Shipbuilding Import POC</RouterLink>
      <nav class="app-nav__links">
        <RouterLink to="/">Import</RouterLink>
        <RouterLink to="/projects">Projects</RouterLink>
        <RouterLink to="/book-hours">Book hours</RouterLink>
      </nav>
    </header>
    <main class="shell shell--wide">
      <RouterView />
    </main>
  </div>
</template>
