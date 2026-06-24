import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { AuthSession } from './identity-auth.dto';
import { IdentityAuthApi } from './identity-auth.api';

const SESSION_KEY = 'f2p.auth.session';

@Injectable({ providedIn: 'root' })
export class IdentityAuthService {
  private readonly api = inject(IdentityAuthApi);
  private readonly router = inject(Router);

  isAuthenticated(): boolean {
    const session = this.getSession();
    if (!session) {
      return false;
    }

    return new Date(session.expiresAtUtc).getTime() > Date.now();
  }

  getSession(): AuthSession | null {
    const raw = localStorage.getItem(SESSION_KEY) ?? sessionStorage.getItem(SESSION_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as AuthSession;
    } catch {
      return null;
    }
  }

  getDisplayName(): string {
    return this.getSession()?.displayName ?? '';
  }

  getPermissions(): string[] {
    return this.getSession()?.permissions ?? [];
  }

  canApproveHoursProgress(): boolean {
    return this.getPermissions().includes('ApproveHoursProgress');
  }

  login(userName: string, password: string, rememberMe: boolean): Observable<boolean> {
    return this.api.login({ userName, password, rememberMe }).pipe(
      tap((response) => this.persistSession(response, rememberMe)),
      map(() => true),
      catchError(() => of(false)),
    );
  }

  logout(): void {
    localStorage.removeItem(SESSION_KEY);
    sessionStorage.removeItem(SESSION_KEY);
    void this.router.navigateByUrl('/account/login');
  }

  private persistSession(
    response: { userName: string; displayName: string; token: string; expiresAtUtc: string; permissions?: string[] },
    rememberMe: boolean,
  ): void {
    const session: AuthSession = {
      userName: response.userName,
      displayName: response.displayName,
      token: response.token,
      expiresAtUtc: response.expiresAtUtc,
      permissions: response.permissions ?? [],
    };

    localStorage.removeItem(SESSION_KEY);
    sessionStorage.removeItem(SESSION_KEY);

    const storage = rememberMe ? localStorage : sessionStorage;
    storage.setItem(SESSION_KEY, JSON.stringify(session));
  }
}
