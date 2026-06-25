import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'admin-root',
  imports: [RouterOutlet, RouterLink],
  template: `
    <div class="f2p-app-light min-h-screen">
      <header class="bg-f2p-navbar px-6 py-4 text-white">
        <div class="mx-auto flex max-w-6xl items-center justify-between gap-4">
          <span class="text-lg font-bold tracking-wide">Floorganise Admin</span>
          <nav class="flex gap-4 text-sm">
            <a routerLink="/tenants" class="text-white/80 transition-colors hover:text-white">Tenants</a>
          </nav>
        </div>
      </header>
      <main class="mx-auto max-w-6xl px-6 py-8">
        <router-outlet />
      </main>
    </div>
  `,
})
export class AppComponent {}
