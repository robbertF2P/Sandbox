import { Routes } from '@angular/router';

export const tenantRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./tenants-page.component').then(m => m.TenantsPageComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./create-tenant-page.component').then(m => m.CreateTenantPageComponent),
  },
];
