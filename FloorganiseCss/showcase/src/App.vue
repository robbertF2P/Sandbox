<script setup lang="ts">
import F2pHomeTiles from './components/F2pHomeTiles.vue'
import F2pLoginPanel from './components/F2pLoginPanel.vue'
import F2pButtonShowcase from './components/F2pButtonShowcase.vue'
import F2pCatalogPanel from './components/F2pCatalogPanel.vue'
import F2pStatusPanel from './components/F2pStatusPanel.vue'
import F2pHybridPanel from './components/F2pHybridPanel.vue'
import F2pToastSample from './components/F2pToastSample.vue'

const homeTiles = [
  { label: 'Planning', meta: '12 open activities', accent: 'brand' as const },
  { label: 'Production', meta: 'Hull 247 — Block 204', accent: 'success' as const },
  { label: 'Quality', meta: '3 inspections due', accent: 'warning' as const },
  { label: 'Logistics', meta: 'Warehouse A', accent: 'brand' as const },
]

const catalogItems = [
  { name: 'MV Northern Star', description: 'Hull outfitting programme', meta: 'Organisation 42 · 18 components' },
  { name: 'MV Baltic Wind', description: 'Engine room module', meta: 'Organisation 7 · 6 components' },
]
</script>

<template>
  <div class="f2p-app-dark app">
    <F2pToastSample />

    <header class="app-nav">
      <span class="app-nav__brand">Floorganise CSS Showcase</span>
    </header>

    <main class="shell space-y-12">
      <section class="hero">
        <p class="eyebrow">@floorganise/css</p>
        <h1>Semantic aliases on Tailwind</h1>
        <p class="description">
          Typical Floor2Plan components use short class names. Tailwind tokens and utilities fill the gaps.
        </p>
      </section>

      <section class="space-y-4">
        <h2 class="text-lg font-bold text-f2p-dark-link">Production — home tiles</h2>
        <p class="muted text-sm">Light shell · <code>f2ps-tile</code> + <code>f2p-tile-grid</code></p>
        <div class="f2p-app-light rounded-f2p-card p-6">
          <F2pHomeTiles :tiles="homeTiles" />
        </div>
      </section>

      <section class="space-y-4">
        <h2 class="text-lg font-bold text-f2p-dark-link">Production — login panel</h2>
        <p class="muted text-sm"><code>form.login</code> · <code>f2ps-btn-primary</code> · notifications</p>
        <div class="f2p-app-light rounded-f2p-card p-6">
          <F2pLoginPanel />
        </div>
      </section>

      <section class="space-y-4">
        <h2 class="text-lg font-bold text-f2p-dark-link">Production — action buttons</h2>
        <p class="muted text-sm"><code>f2ps-btn-*</code> size and intent variants</p>
        <div class="f2p-app-light rounded-f2p-card p-6">
          <F2pButtonShowcase />
        </div>
      </section>

      <section class="space-y-4">
        <h2 class="text-lg font-bold text-f2p-dark-link">POC — catalog list</h2>
        <p class="muted text-sm"><code>catalog-list</code> · <code>btn--primary</code> / <code>btn--danger</code></p>
        <F2pCatalogPanel :items="catalogItems" />
      </section>

      <section class="space-y-4">
        <h2 class="text-lg font-bold text-f2p-dark-link">POC — live status</h2>
        <p class="muted text-sm"><code>hero</code> · <code>status-card</code> · <code>state.*</code></p>
        <F2pStatusPanel />
      </section>

      <section class="space-y-4">
        <h2 class="text-lg font-bold text-f2p-dark-link">Hybrid — panel + utilities</h2>
        <p class="muted text-sm">Semantic surface + Tailwind layout (<code>flex</code>, <code>gap-4</code>)</p>
        <F2pHybridPanel />
      </section>
    </main>
  </div>
</template>
