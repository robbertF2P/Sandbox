import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'admin-root',
  imports: [RouterOutlet, RouterLink],
  template: `
    <div class="admin-shell">
      <header class="admin-topbar">
        <div class="admin-brand">Floorganise Admin</div>
        <nav>
          <a routerLink="/tenants">Tenants</a>
        </nav>
      </header>
      <main class="admin-main">
        <router-outlet />
      </main>
    </div>
  `,
})
export class AppComponent {}
