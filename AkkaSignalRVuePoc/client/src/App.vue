<script setup lang="ts">
import * as signalR from '@microsoft/signalr'
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

type ConnectionState = 'Connecting' | 'Connected' | 'Reconnecting' | 'Disconnected'

interface PushMessage {
  sequence: number
  text: string
  sentAt: string
  source: string
}

const hubUrl = import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/hubs/live-messages'
const connectionState = ref<ConnectionState>('Connecting')
const messages = ref<PushMessage[]>([])
const error = ref<string | null>(null)
let connection: signalR.HubConnection | undefined

const newestMessage = computed(() => messages.value[0])

function addMessage(message: PushMessage) {
  messages.value = [message, ...messages.value].slice(0, 20)
}

onMounted(async () => {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build()

  connection.on('serverMessage', addMessage)
  connection.on('actorMessage', addMessage)
  connection.onreconnecting(() => {
    connectionState.value = 'Reconnecting'
  })
  connection.onreconnected(() => {
    connectionState.value = 'Connected'
  })
  connection.onclose(() => {
    connectionState.value = 'Disconnected'
  })

  try {
    await connection.start()
    connectionState.value = 'Connected'
  } catch (startError) {
    connectionState.value = 'Disconnected'
    error.value = startError instanceof Error ? startError.message : 'Unable to connect to the SignalR hub.'
  }
})

onBeforeUnmount(() => {
  void connection?.stop()
})
</script>

<template>
  <main class="shell">
    <section class="hero">
      <p class="eyebrow">Akka.NET + SignalR + Vue</p>
      <h1>Actor-driven live messages</h1>
      <p class="description">
        The ASP.NET Core host starts an Akka.NET actor system in a background service.
        The actor publishes a SignalR message every five seconds to this Vue client.
      </p>
    </section>

    <section class="status-card">
      <div>
        <span class="label">Hub endpoint</span>
        <strong>{{ hubUrl }}</strong>
      </div>
      <div>
        <span class="label">Connection</span>
        <strong :class="['state', connectionState.toLowerCase()]">{{ connectionState }}</strong>
      </div>
    </section>

    <p v-if="error" class="error">{{ error }}</p>

    <section class="latest" v-if="newestMessage">
      <span class="label">Latest message</span>
      <h2>{{ newestMessage.text }}</h2>
      <p>From {{ newestMessage.source }} at {{ new Date(newestMessage.sentAt).toLocaleTimeString() }}</p>
    </section>

    <section class="messages">
      <h2>Message stream</h2>
      <ul v-if="messages.length">
        <li v-for="message in messages" :key="`${message.source}-${message.sequence}-${message.sentAt}`">
          <span class="sequence">#{{ message.sequence }}</span>
          <span>{{ message.text }}</span>
          <time>{{ new Date(message.sentAt).toLocaleTimeString() }}</time>
        </li>
      </ul>
      <p v-else class="empty">Waiting for the first SignalR event...</p>
    </section>
  </main>
</template>
