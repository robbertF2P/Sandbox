import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />',
  styles: [':host { display: block; min-height: 100vh; background: linear-gradient(160deg, #0f172a 0%, #1e293b 45%, #0f766e 100%); color: #e2e8f0; font-family: Inter, system-ui, sans-serif; }']
})
export class AppComponent {}
