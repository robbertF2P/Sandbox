export interface PlatformEventMessage {
  eventType: string;
  payload: unknown;
  occurredAt: string;
  correlationId?: string | null;
  useCase?: string | null;
}
