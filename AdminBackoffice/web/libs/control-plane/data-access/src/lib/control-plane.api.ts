import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ADMIN_API_BASE_URL } from '@admin/shared/api-core';
import { ProvisionTenantRequestDto, TenantRecordDto } from './tenant.dto';

@Injectable({ providedIn: 'root' })
export class ControlPlaneApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(ADMIN_API_BASE_URL);

  listTenants(): Observable<TenantRecordDto[]> {
    return this.http.get<TenantRecordDto[]>(`${this.baseUrl}/admin/tenants`);
  }

  provisionTenant(request: ProvisionTenantRequestDto): Observable<TenantRecordDto> {
    return this.http.post<TenantRecordDto>(`${this.baseUrl}/admin/tenants`, request);
  }

  syncTenant(tenantId: string): Observable<TenantRecordDto> {
    return this.http.post<TenantRecordDto>(`${this.baseUrl}/admin/tenants/${tenantId}/sync`, {});
  }
}
