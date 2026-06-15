<script setup lang="ts">
import { ref } from 'vue'

const username = ref('')
const password = ref('')
const rememberMe = ref(false)
const notification = ref<'danger' | 'warning' | 'success' | null>('warning')

function cycleNotification() {
  const order: Array<'danger' | 'warning' | 'success' | null> = ['danger', 'warning', 'success', null]
  const index = order.indexOf(notification.value)
  notification.value = order[(index + 1) % order.length]
}
</script>

<template>
  <!-- Production login pattern: f2ps-tile-fluid + form.login + f2ps-btn-* -->
  <div class="f2ps-tile f2ps-tile-fluid max-w-2xl p-6">
    <div class="mb-6 border-b border-f2p-border pb-6 text-center">
      <p class="text-lg font-bold tracking-wide text-f2p-ink">Floor2Plan</p>
      <p class="text-sm text-f2p-ink-muted">Service credentials</p>
    </div>

    <div
      v-if="notification"
      class="login-notification mb-4"
      :class="`login-notification-${notification}`"
    >
      <span class="mr-2 inline-block size-6 rounded-full bg-current opacity-80" />
      <span class="uppercase">{{ notification }} environment</span>
    </div>

    <form class="login" @submit.prevent>
      <label class="field">
        <span class="text-sm text-f2p-ink-muted">Username</span>
        <input id="userName" v-model="username" type="text" placeholder="Username" />
      </label>
      <label class="field">
        <span class="text-sm text-f2p-ink-muted">Password</span>
        <input id="password" v-model="password" type="password" placeholder="Password" />
      </label>
      <div class="flex items-center justify-between gap-4">
        <label class="flex items-center gap-2 text-sm text-f2p-ink-muted">
          <input v-model="rememberMe" type="checkbox" />
          Stay logged in?
        </label>
        <button class="f2ps-btn f2ps-btn-primary f2ps-btn-md" type="submit">Login</button>
      </div>
    </form>

    <button class="f2ps-btn f2ps-btn-tertiary f2ps-btn-sm mt-4" type="button" @click="cycleNotification">
      Cycle notification
    </button>
  </div>
</template>
