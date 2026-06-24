import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { IdentityAuthService } from './identity-auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(IdentityAuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/account/login']);
};

export const guestGuard: CanActivateFn = () => {
  const auth = inject(IdentityAuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/']);
};
