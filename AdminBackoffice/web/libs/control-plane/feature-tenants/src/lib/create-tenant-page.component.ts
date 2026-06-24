import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  ControlPlaneApi,
  ProvisionTenantRequestDto,
  TenantDataTier,
  TenantDeploymentMode,
} from '@admin/control-plane/data-access';

@Component({
  selector: 'admin-create-tenant-page',
  imports: [RouterLink, FormsModule],
  templateUrl: './create-tenant-page.component.html',
})
export class CreateTenantPageComponent {
  private readonly api = inject(ControlPlaneApi);
  private readonly router = inject(Router);

  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  slug = '';
  displayName = '';
  mode: TenantDeploymentMode = 'Native';
  dataTier: TenantDataTier = 'SharedSqlServer';
  region = 'eu-west';
  legacyBuildProfileId = '';
  legacyRuntimeUrl = '';
  legacyDatabaseConnectionRef = '';
  nativeDatabaseConnectionRef = 'control-plane/native-db';
  nativeApiBaseUrl = 'http://f2p-platform-api:5080';
  integrationPacks = '';
  customizationPacks = 'acme-hour-approvals-v1';
  billingTier = 'standard';
  seatLimit = 50;

  readonly isLegacy = computed(() => this.mode === 'LegacyHosted');

  submit(): void {
    this.saving.set(true);
    this.error.set(null);

    const request: ProvisionTenantRequestDto = {
      slug: this.slug.trim(),
      displayName: this.displayName.trim(),
      mode: this.mode,
      dataTier: this.dataTier,
      region: this.region.trim(),
      legacyBuildProfileId: this.isLegacy() ? this.legacyBuildProfileId.trim() : null,
      legacyRuntimeUrl: this.isLegacy() ? this.legacyRuntimeUrl.trim() : null,
      legacyDatabaseConnectionRef: this.isLegacy() ? this.legacyDatabaseConnectionRef.trim() : null,
      nativeDatabaseConnectionRef: this.isLegacy() ? null : this.nativeDatabaseConnectionRef.trim(),
      nativeApiBaseUrl: this.isLegacy() ? null : this.nativeApiBaseUrl.trim(),
      integrationPacks: this.parseList(this.integrationPacks),
      customizationPacks: this.parseList(this.customizationPacks),
      billingTier: this.billingTier.trim() || 'standard',
      seatLimit: this.seatLimit,
    };

    this.api.provisionTenant(request).subscribe({
      next: tenant => {
        this.saving.set(false);
        void this.router.navigate(['/tenants'], {
          queryParams: { created: tenant.slug },
        });
      },
      error: err => {
        this.saving.set(false);
        const message = err?.error?.error ?? err?.error?.title ?? 'Provisioning failed.';
        this.error.set(typeof message === 'string' ? message : 'Provisioning failed.');
      },
    });
  }

  private parseList(value: string): string[] | null {
    const items = value
      .split(',')
      .map(item => item.trim())
      .filter(Boolean);
    return items.length > 0 ? items : null;
  }
}
