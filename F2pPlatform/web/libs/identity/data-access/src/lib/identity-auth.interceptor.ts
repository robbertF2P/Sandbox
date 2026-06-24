import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { IdentityAuthService } from './identity-auth.service';

export const identityAuthInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(IdentityAuthService);
  const session = auth.getSession();

  if (!session || !request.url.includes('/api/')) {
    return next(request);
  }

  return next(
    request.clone({
      setHeaders: {
        Authorization: `Bearer ${session.token}`,
        'X-User-Name': session.userName,
        'X-User-Permissions': session.permissions.join(','),
      },
    }),
  );
};
