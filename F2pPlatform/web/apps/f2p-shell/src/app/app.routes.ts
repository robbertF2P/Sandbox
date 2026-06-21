import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home-page.component').then(m => m.HomePageComponent),
  },
  {
    path: 'reference',
    loadChildren: () =>
      import('@f2p/reference/feature-status').then(m => m.referenceRoutes),
  },
  { path: '**', redirectTo: '' },
];
