export type TenantDeploymentMode = 'LegacyHosted' | 'Native';
export type TenantDataTier = 'SharedSqlServer' | 'DedicatedSqlServer';
export type TenantLifecycleStatus =
  | 'Provisioning'
  | 'Active'
  | 'Suspended'
  | 'Migrating'
  | 'Retired';

export interface TenantRecordDto {
  tenantId: string;
  slug: string;
  displayName: string;
  status: TenantLifecycleStatus;
  deploymentProfile: {
    mode: TenantDeploymentMode;
    dataTier: TenantDataTier;
    region: string;
    legacy?: {
      buildProfileId: string;
      runtimeUrl: string;
      databaseConnectionRef: string;
    };
    native?: {
      databaseConnectionRef: string;
      apiBaseUrl: string;
    };
  };
  packEntitlements: {
    integrationPacks: string[];
    customizationPacks: string[];
  };
  billing: {
    tier: string;
    seatLimit: number;
  };
  createdAt: string;
  provisionedAt: string | null;
  lastSyncedToPlatformAt: string | null;
  lastPlatformSyncError: string | null;
}

export interface ProvisionTenantRequestDto {
  slug: string;
  displayName: string;
  mode: TenantDeploymentMode;
  dataTier: TenantDataTier;
  region: string;
  legacyBuildProfileId?: string | null;
  legacyRuntimeUrl?: string | null;
  legacyDatabaseConnectionRef?: string | null;
  nativeDatabaseConnectionRef?: string | null;
  nativeApiBaseUrl?: string | null;
  integrationPacks?: string[] | null;
  customizationPacks?: string[] | null;
  billingTier?: string;
  seatLimit?: number;
}
