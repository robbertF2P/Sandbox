import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PortalLinksResponse } from './portal-links';

@Injectable({ providedIn: 'root' })
export class LinksService {
  private readonly apiBase = (window as unknown as { __API_URL__?: string }).__API_URL__
    ?? '';

  constructor(private readonly http: HttpClient) {}

  getLinks(): Observable<PortalLinksResponse> {
    return this.http.get<PortalLinksResponse>(`${this.apiBase}/api/links`);
  }
}
