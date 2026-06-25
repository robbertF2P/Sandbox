import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  ControlPlaneApi,
  formatControlPlaneApiError,
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
  /** Local dev default — Docker stack uses http://f2p-platform-api:5080 */
  nativeApiBaseUrl = 'http://localhost:5080';
  integrationPacks = '';
  customizationPacks = 'acme-hour-approvals-v1';
  billingTier = 'standard';
  seatLimit = 50;

  readonly isLegacy = computed(() => this.mode === 'LegacyHosted');

  normalizeSlug(): void {
    this.slug = this.slug.trim().toLowerCase();
  }

  submit(): void {
    this.normalizeSlug();

    if (!this.slug || !this.displayName.trim()) {
      this.error.set('Slug and display name are required.');
      return;
    }

    if (!/^[a-z0-9-]+$/.test(this.slug)) {
      this.error.set('Slug must use lowercase letters, numbers, and hyphens only.');
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const request: ProvisionTenantRequestDto = {
      slug: this.slug,
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
        this.error.set(
          formatControlPlaneApiError(err, {
            fallback: 'Provisioning failed.',
            action: 'provision',
          }),
        );
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
