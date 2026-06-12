import * as signalR from '@microsoft/signalr'
import { onBeforeUnmount, onMounted } from 'vue'
import type { ImportEventNotification } from '../types/import'

const hubUrl = import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5001/hubs/import'

export function useImportHub(onImportEvent: (notification: ImportEventNotification) => void) {
  let connection: signalR.HubConnection | undefined

  onMounted(async () => {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build()

    connection.on('importEvent', onImportEvent)

    try {
      await connection.start()
    } catch {
      // Host page surfaces connection issues.
    }
  })

  onBeforeUnmount(() => {
    void connection?.stop()
  })
}
