import { Routes } from '@angular/router';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { ModulePageComponent } from './pages/module-page/module-page.component';

export const routes: Routes = [
  { path: '', component: HomePageComponent },
  { path: 'modules/:name', component: ModulePageComponent },
  { path: '**', redirectTo: '' },
];
