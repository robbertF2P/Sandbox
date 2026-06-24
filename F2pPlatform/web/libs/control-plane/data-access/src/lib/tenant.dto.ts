export interface TenantSummaryDto {
  tenantId: string;
  slug: string;
  displayName: string;
  status: string;
  deploymentMode: string;
  dataTier: string;
  region: string;
  billingTier: string;
  seatLimit: number;
  createdAtUtc: string;
}

export interface CreateTenantRequestDto {
  slug: string;
  displayName: string;
  dataTier: 'shared_sql_server' | 'dedicated_sql_server';
  region: string;
  databaseConnectionRef: string;
  apiBaseUrl?: string;
  billingTier: string;
  seatLimit: number;
}
