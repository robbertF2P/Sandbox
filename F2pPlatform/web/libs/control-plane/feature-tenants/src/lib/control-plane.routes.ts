import { Routes } from '@angular/router';

export const controlPlaneRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./tenants-page.component').then(m => m.TenantsPageComponent),
  },
];
