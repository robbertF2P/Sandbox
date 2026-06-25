import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ReferenceStatusApi, ReferenceStatusDto } from '@f2p/reference/data-access';

@Component({
  selector: 'f2p-reference-status-page',
  imports: [RouterLink],
  templateUrl: './reference-status-page.component.html',
})
export class ReferenceStatusPageComponent implements OnInit {
  private readonly referenceApi = inject(ReferenceStatusApi);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly status = signal<ReferenceStatusDto | null>(null);

  ngOnInit(): void {
    this.loadStatus();
  }

  protected loadStatus(): void {
    this.loading.set(true);
    this.error.set(null);

    this.referenceApi.getStatus().subscribe({
      next: status => {
        this.status.set(status);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load reference module status. Check that the API host is running.');
        this.loading.set(false);
      },
    });
  }
}
