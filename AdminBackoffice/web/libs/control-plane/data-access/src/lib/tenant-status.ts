import { TenantLifecycleStatus } from './tenant.dto';

const STATUS_LABELS: Record<TenantLifecycleStatus, string> = {
  Provisioning: 'Provisioning',
  Active: 'Active',
  Suspended: 'Suspended',
  Migrating: 'Migrating',
  Retired: 'Retired',
};

/** API may return enum strings or numeric enum values depending on serializer settings. */
export function formatTenantStatus(status: TenantLifecycleStatus | number | string): string {
  if (typeof status === 'number') {
    const byIndex: TenantLifecycleStatus[] = [
      'Provisioning',
      'Active',
      'Suspended',
      'Migrating',
      'Retired',
    ];
    const mapped = byIndex[status];
    return mapped ? STATUS_LABELS[mapped] : `Status ${status}`;
  }

  if (typeof status === 'string' && status in STATUS_LABELS) {
    return STATUS_LABELS[status as TenantLifecycleStatus];
  }

  return String(status);
}

export function tenantStatusTone(status: TenantLifecycleStatus | number | string): 'active' | 'provisioning' | 'other' {
  const normalized = typeof status === 'string' ? status : formatTenantStatus(status);
  if (normalized === 'Active') {
    return 'active';
  }

  if (normalized === 'Provisioning') {
    return 'provisioning';
  }

  return 'other';
}
