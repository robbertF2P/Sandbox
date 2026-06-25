import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PlatformEventsService } from '@f2p/shared/platform-events';

@Component({
  selector: 'f2p-platform-events-page',
  imports: [RouterLink],
  templateUrl: './platform-events-page.component.html',
})
export class PlatformEventsPageComponent implements OnInit {
  private readonly platformEvents = inject(PlatformEventsService);

  readonly connected = this.platformEvents.connected;
  readonly recentEvents = this.platformEvents.recentEvents;

  ngOnInit(): void {
    this.platformEvents.connect();
  }
}
