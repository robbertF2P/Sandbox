<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getApiBaseUrl, listOrganisations } from '../api/catalog'
import type { Organisation } from '../types/catalog'

const organisations = ref<Organisation[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const apiBaseUrl = getApiBaseUrl()

onMounted(async () => {
  try {
    organisations.value = await listOrganisations()
  } catch (fetchError) {
    error.value =
      fetchError instanceof Error
        ? fetchError.message
        : 'Unable to load organisations from the API.'
  } finally {
    loading.value = false
  }
})

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}
</script>

<template>
  <section class="hero">
    <p class="eyebrow">Catalog</p>
    <h1>Organisations</h1>
    <p class="description">
      Organisations returned by the Akka.NET data actors via
      <code>GET /api/organisations</code>.
    </p>
  </section>

  <section class="status-card">
    <div>
      <span class="label">API endpoint</span>
      <strong>{{ apiBaseUrl }}/api/organisations</strong>
    </div>
    <div>
      <span class="label">Count</span>
      <strong>{{ loading ? '…' : organisations.length }}</strong>
    </div>
  </section>

  <p v-if="error" class="error">{{ error }}</p>

  <section class="catalog-list">
    <h2>All organisations</h2>
    <p v-if="loading" class="empty">Loading organisations…</p>
    <ul v-else-if="organisations.length">
      <li v-for="organisation in organisations" :key="organisation.id">
        <div class="catalog-list__primary">
          <strong>{{ organisation.name }}</strong>
          <span class="catalog-list__meta">{{ organisation.id }}</span>
        </div>
        <time>{{ formatDate(organisation.createdAt) }}</time>
      </li>
    </ul>
    <p v-else class="empty">No organisations found.</p>
  </section>
</template>
