// libs/<context>/data-access/src/lib/<context>-status.api.ts

import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { F2P_API_BASE_URL } from '@f2p/shared/api-core';
import { <Context>StatusDto } from './<context>-status.dto';

@Injectable({ providedIn: 'root' })
export class <Context>StatusApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(F2P_API_BASE_URL);

  getStatus(): Observable<<Context>StatusDto> {
    return this.http.get<<Context>StatusDto>(`${this.baseUrl}/api/<context>/status`);
  }
}
