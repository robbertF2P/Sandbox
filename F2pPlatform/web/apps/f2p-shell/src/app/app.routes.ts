import { Routes } from '@angular/router';
import { authGuard, guestGuard } from '@f2p/identity/data-access';

export const routes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/home-page.component').then(m => m.HomePageComponent),
  },
  {
    path: 'account/login',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/login-page.component').then(m => m.LoginPageComponent),
  },
  {
    path: 'reference',
    canActivate: [authGuard],
    loadChildren: () =>
      import('@f2p/reference/feature-status').then(m => m.referenceRoutes),
  },
  {
    path: 'hour-approvals',
    canActivate: [authGuard],
    loadChildren: () =>
      import('@f2p/hour-approvals/feature-tasks').then(m => m.hourApprovalsRoutes),
  },
  { path: '**', redirectTo: '' },
];
