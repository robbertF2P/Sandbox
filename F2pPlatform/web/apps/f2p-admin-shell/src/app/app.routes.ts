import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('@f2p/control-plane/feature-tenants').then(m => m.controlPlaneRoutes),
  },
  { path: '**', redirectTo: '' },
];
