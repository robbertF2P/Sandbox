import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { F2P_API_BASE_URL } from '@f2p/shared/api-core';
import { ReferenceStatusDto } from './reference-status.dto';

@Injectable({ providedIn: 'root' })
export class ReferenceStatusApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(F2P_API_BASE_URL);

  getStatus(): Observable<ReferenceStatusDto> {
    return this.http.get<ReferenceStatusDto>(`${this.baseUrl}/api/reference/status`);
  }
}
