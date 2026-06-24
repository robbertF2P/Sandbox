import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import {
  ControlPlaneTenantsApi,
  CreateTenantRequestDto,
  TenantSummaryDto,
} from '@f2p/control-plane/data-access';

@Component({
  selector: 'f2p-tenants-page',
  imports: [FormsModule, DatePipe],
  templateUrl: './tenants-page.component.html',
})
export class TenantsPageComponent implements OnInit {
  private readonly api = inject(ControlPlaneTenantsApi);

  readonly tenants = signal<TenantSummaryDto[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  form: CreateTenantRequestDto = {
    slug: '',
    displayName: '',
    dataTier: 'shared_sql_server',
    region: 'eu-west',
    databaseConnectionRef: '',
    apiBaseUrl: 'https://api.platform.example/v1',
    billingTier: 'standard',
    seatLimit: 10,
  };

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.listTenants().subscribe({
      next: tenants => {
        this.tenants.set(tenants);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load tenants.');
        this.loading.set(false);
      },
    });
  }

  onSubmit(): void {
    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);

    const payload: CreateTenantRequestDto = {
      ...this.form,
      slug: this.form.slug.trim().toLowerCase(),
      displayName: this.form.displayName.trim(),
      region: this.form.region.trim(),
      databaseConnectionRef: this.form.databaseConnectionRef.trim(),
      apiBaseUrl: this.form.apiBaseUrl?.trim() || undefined,
      billingTier: this.form.billingTier.trim(),
    };

    this.api.createTenant(payload).subscribe({
      next: tenant => {
        this.success.set(`Tenant "${tenant.displayName}" created.`);
        this.form = {
          slug: '',
          displayName: '',
          dataTier: 'shared_sql_server',
          region: 'eu-west',
          databaseConnectionRef: '',
          apiBaseUrl: 'https://api.platform.example/v1',
          billingTier: 'standard',
          seatLimit: 10,
        };
        this.saving.set(false);
        this.reload();
      },
      error: err => {
        const message =
          err?.error?.error ??
          (err?.status === 409 ? 'A tenant with this slug already exists.' : 'Failed to create tenant.');
        this.error.set(message);
        this.saving.set(false);
      },
    });
  }
}
