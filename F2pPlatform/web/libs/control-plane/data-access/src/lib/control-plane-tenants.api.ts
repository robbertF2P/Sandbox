import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { F2P_API_BASE_URL } from '@f2p/shared/api-core';
import { CreateTenantRequestDto, TenantSummaryDto } from './tenant.dto';

@Injectable({ providedIn: 'root' })
export class ControlPlaneTenantsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(F2P_API_BASE_URL);

  listTenants(): Observable<TenantSummaryDto[]> {
    return this.http.get<TenantSummaryDto[]>(`${this.baseUrl}/admin/tenants`);
  }

  createTenant(request: CreateTenantRequestDto): Observable<TenantSummaryDto> {
    return this.http.post<TenantSummaryDto>(`${this.baseUrl}/admin/tenants`, request);
  }
}
