import { Routes } from '@angular/router';
import { hourApprovalsFeatureGuard } from '@f2p/hour-approvals/data-access';

export const hourApprovalsRoutes: Routes = [
  {
    path: '',
    canActivate: [hourApprovalsFeatureGuard],
    loadComponent: () =>
      import('./hour-approvals-page.component').then(m => m.HourApprovalsPageComponent),
  },
];
