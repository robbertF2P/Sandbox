import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ControlPlaneApi, TenantRecordDto } from '@admin/control-plane/data-access';

@Component({
  selector: 'admin-tenants-page',
  imports: [RouterLink, DatePipe],
  templateUrl: './tenants-page.component.html',
})
export class TenantsPageComponent implements OnInit {
  private readonly api = inject(ControlPlaneApi);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly tenants = signal<TenantRecordDto[]>([]);
  readonly syncingId = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.listTenants().subscribe({
      next: tenants => {
        this.tenants.set(tenants);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load tenants from the control plane API.');
        this.loading.set(false);
      },
    });
  }

  syncTenant(tenant: TenantRecordDto): void {
    this.syncingId.set(tenant.tenantId);
    this.api.syncTenant(tenant.tenantId).subscribe({
      next: () => {
        this.syncingId.set(null);
        this.load();
      },
      error: () => {
        this.syncingId.set(null);
        this.error.set(`Sync failed for tenant "${tenant.slug}".`);
      },
    });
  }

  statusClass(status: string): string {
    const base = 'inline-block rounded-f2p-pill px-2 py-0.5 text-xs font-semibold';
    switch (status) {
      case 'Active':
        return `${base} bg-f2p-success/15 text-f2p-success`;
      case 'Provisioning':
        return `${base} bg-f2p-warning/15 text-f2p-warning`;
      default:
        return `${base} bg-f2p-surface-subtle text-f2p-ink-muted`;
    }
  }
}
