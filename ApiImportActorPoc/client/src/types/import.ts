export interface ImportEventNotification {
  eventType: string
  sessionId: string
  payload?: Record<string, unknown>
  occurredAt: string
}

export interface StartImportResponse {
  sessionId: string
  accepted: boolean
  errorMessage?: string
}
