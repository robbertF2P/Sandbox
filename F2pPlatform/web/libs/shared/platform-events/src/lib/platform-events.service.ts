import { Injectable, OnDestroy, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { PlatformEventMessage } from './platform-event-message';

@Injectable({ providedIn: 'root' })
export class PlatformEventsService implements OnDestroy {
  private connection: signalR.HubConnection | null = null;

  readonly connected = signal(false);
  readonly lastEvent = signal<PlatformEventMessage | null>(null);
  readonly recentEvents = signal<PlatformEventMessage[]>([]);

  connect(hubUrl = '/hubs/platform-events'): void {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    this.connection.on('platformEvent', (message: PlatformEventMessage) => {
      this.lastEvent.set(message);
      this.recentEvents.update(events => [message, ...events].slice(0, 20));
    });

    void this.connection
      .start()
      .then(() => this.connected.set(true))
      .catch(() => this.connected.set(false));
  }

  ngOnDestroy(): void {
    void this.connection?.stop();
    this.connection = null;
    this.connected.set(false);
  }
}
