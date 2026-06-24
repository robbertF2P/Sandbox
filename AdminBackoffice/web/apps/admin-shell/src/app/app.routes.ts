import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'tenants' },
  {
    path: 'tenants',
    loadChildren: () =>
      import('@admin/control-plane/feature-tenants').then(m => m.tenantRoutes),
  },
  { path: '**', redirectTo: 'tenants' },
];
