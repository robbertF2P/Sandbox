import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { HourApprovalsApi } from './hour-approvals.api';

export const hourApprovalsFeatureGuard: CanActivateFn = () => {
  const api = inject(HourApprovalsApi);
  const router = inject(Router);

  return api.getCapabilities().pipe(
    map(capabilities => capabilities.featureEnabled),
    catchError(() => of(false)),
    map(enabled => enabled || router.createUrlTree(['/'])),
  );
};
