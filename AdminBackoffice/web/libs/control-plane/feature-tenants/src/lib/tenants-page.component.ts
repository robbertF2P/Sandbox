import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ControlPlaneApi,
  formatControlPlaneApiError,
  formatTenantStatus,
  TenantRecordDto,
  tenantStatusTone,
} from '@admin/control-plane/data-access';

@Component({
  selector: 'admin-tenants-page',
  imports: [RouterLink, DatePipe],
  templateUrl: './tenants-page.component.html',
})
export class TenantsPageComponent implements OnInit {
  private readonly api = inject(ControlPlaneApi);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly tenants = signal<TenantRecordDto[]>([]);
  readonly syncingId = signal<string | null>(null);

  readonly formatTenantStatus = formatTenantStatus;
  readonly tenantStatusTone = tenantStatusTone;

  ngOnInit(): void {
    const created = this.route.snapshot.queryParamMap.get('created');
    if (created) {
      this.success.set(`Tenant "${created}" is active on the v2 platform.`);
    }

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
      error: err => {
        this.error.set(
          formatControlPlaneApiError(err, {
            fallback: 'Could not load tenants from the control plane API.',
            action: 'list',
          }),
        );
        this.loading.set(false);
      },
    });
  }

  syncTenant(tenant: TenantRecordDto): void {
    this.success.set(null);
    this.syncingId.set(tenant.tenantId);
    this.api.syncTenant(tenant.tenantId).subscribe({
      next: () => {
        this.syncingId.set(null);
        this.success.set(`Tenant "${tenant.slug}" synced to the v2 platform.`);
        this.load();
      },
      error: err => {
        this.syncingId.set(null);
        this.error.set(
          formatControlPlaneApiError(err, {
            fallback: `Sync failed for tenant "${tenant.slug}".`,
            action: 'sync',
          }),
        );
      },
    });
  }

  statusClass(status: TenantRecordDto['status']): string {
    const base = 'inline-block rounded-f2p-pill px-2 py-0.5 text-xs font-semibold';
    switch (tenantStatusTone(status)) {
      case 'active':
        return `${base} bg-f2p-success/15 text-f2p-success`;
      case 'provisioning':
        return `${base} bg-f2p-warning/15 text-f2p-warning`;
      default:
        return `${base} bg-f2p-surface-subtle text-f2p-ink-muted`;
    }
  }
}
