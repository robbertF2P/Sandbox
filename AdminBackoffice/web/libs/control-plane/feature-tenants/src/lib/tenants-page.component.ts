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
    switch (status) {
      case 'Active':
        return 'admin-status admin-status--active';
      case 'Provisioning':
        return 'admin-status admin-status--provisioning';
      default:
        return 'admin-status';
    }
  }
}
