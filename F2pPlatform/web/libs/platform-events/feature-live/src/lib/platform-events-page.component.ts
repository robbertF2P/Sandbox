import { Component, inject, OnInit } from '@angular/core';
import { F2pPageHeaderComponent } from '@floorganise/ui';
import { PlatformEventsService } from '@f2p/shared/platform-events';

@Component({
  selector: 'f2p-platform-events-page',
  imports: [F2pPageHeaderComponent],
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
