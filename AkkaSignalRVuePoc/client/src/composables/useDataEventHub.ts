import * as signalR from '@microsoft/signalr'
import { onBeforeUnmount, onMounted } from 'vue'
import type { DataEventNotification } from '../types/catalog'

const hubUrl = import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/hubs/live-messages'

export function useDataEventHub(onDataEvent: (notification: DataEventNotification) => void) {
  let connection: signalR.HubConnection | undefined

  onMounted(async () => {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build()

    connection.on('dataEvent', onDataEvent)

    try {
      await connection.start()
    } catch {
      // Connection errors are surfaced by the host page if needed.
    }
  })

  onBeforeUnmount(() => {
    void connection?.stop()
  })
}
