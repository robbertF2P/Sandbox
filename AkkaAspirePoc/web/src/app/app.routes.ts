import { Routes } from '@angular/router';
import { LandingComponent } from './landing/landing.component';
import { TodosComponent } from './todos/todos.component';

export const routes: Routes = [
  { path: '', component: LandingComponent },
  { path: 'todos', component: TodosComponent }
];
