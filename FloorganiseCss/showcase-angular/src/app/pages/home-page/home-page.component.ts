import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { F2pHomeTilesComponent } from '../../components/f2p-home-tiles/f2p-home-tiles.component';
import { HomeTile } from '../../models/home-tile.model';

@Component({
  selector: 'app-home-page',
  imports: [F2pHomeTilesComponent],
  templateUrl: './home-page.component.html',
})
export class HomePageComponent {
  private readonly router = inject(Router);

  readonly tiles: HomeTile[] = [
    { label: 'Planning', meta: '12 open activities', route: '/modules/planning', accent: 'brand' },
    { label: 'Production', meta: 'Hull 247 — Block 204', route: '/modules/production', accent: 'success' },
    { label: 'Quality', meta: '3 inspections due', route: '/modules/quality', accent: 'warning' },
    { label: 'Logistics', meta: 'Warehouse A', route: '/modules/logistics', accent: 'brand' },
  ];

  onTileSelect(tile: HomeTile): void {
    void this.router.navigateByUrl(tile.route);
  }
}
