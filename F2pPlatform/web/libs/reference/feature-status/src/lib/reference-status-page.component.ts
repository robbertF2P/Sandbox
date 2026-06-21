import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ReferenceStatusApi, ReferenceStatusDto } from '@f2p/reference/data-access';
import { PlatformEventsService } from '@f2p/shared/platform-events';

@Component({
  selector: 'f2p-reference-status-page',
  imports: [RouterLink],
  templateUrl: './reference-status-page.component.html',
})
export class ReferenceStatusPageComponent implements OnInit {
  private readonly referenceApi = inject(ReferenceStatusApi);
  private readonly platformEvents = inject(PlatformEventsService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly status = signal<ReferenceStatusDto | null>(null);

  readonly connected = this.platformEvents.connected;
  readonly recentEvents = this.platformEvents.recentEvents;

  ngOnInit(): void {
    this.platformEvents.connect();
    this.loadStatus();
  }

  loadStatus(): void {
    this.loading.set(true);
    this.error.set(null);

    this.referenceApi.getStatus().subscribe({
      next: status => {
        this.status.set(status);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load reference module status. Is F2pPlatform.Host running on :5080?');
        this.loading.set(false);
      },
    });
  }
}
