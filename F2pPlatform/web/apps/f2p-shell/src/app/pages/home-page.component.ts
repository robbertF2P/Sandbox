import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { IdentityAuthService } from '@f2p/identity/data-access';
import { F2pHomeTilesComponent, HomeTile } from '@f2p/shared/ui';

@Component({
  selector: 'f2p-home-page',
  imports: [F2pHomeTilesComponent],
  templateUrl: './home-page.component.html',
})
export class HomePageComponent {
  private readonly auth = inject(IdentityAuthService);
  private readonly router = inject(Router);

  readonly displayName = this.auth.getDisplayName();

  readonly tiles: HomeTile[] = [
    {
      label: 'Reference',
      abbreviation: 'Re',
      meta: 'Module status + SignalR events',
      route: '/reference',
      moduleColor: 'blue',
    },
    {
      label: 'Planning',
      abbreviation: 'Pl',
      meta: '12 open activities',
      route: '/modules/planning',
      moduleColor: 'blue',
    },
    {
      label: 'Production',
      abbreviation: 'Pr',
      meta: 'Hull 247 — Block 204',
      route: '/modules/production',
      moduleColor: 'green',
    },
    {
      label: 'Quality',
      abbreviation: 'Qu',
      meta: '3 inspections due',
      route: '/modules/quality',
      moduleColor: 'purple',
    },
  ];

  onTileSelect(tile: HomeTile): void {
    if (tile.route.startsWith('/modules/')) {
      return;
    }

    void this.router.navigateByUrl(tile.route);
  }

  logout(): void {
    this.auth.logout();
  }
}
