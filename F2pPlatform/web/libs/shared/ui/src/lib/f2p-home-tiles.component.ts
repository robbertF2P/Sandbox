import { Component, input, output } from '@angular/core';
import { NgClass } from '@angular/common';
import { HomeTile, HomeTileModuleColor, HomeTileStatus } from './home-tile.model';

@Component({
  selector: 'f2p-home-tiles',
  imports: [NgClass],
  templateUrl: './f2p-home-tiles.component.html',
})
export class F2pHomeTilesComponent {
  readonly tiles = input.required<HomeTile[]>();
  readonly tileSelect = output<HomeTile>();

  statusClass(tile: HomeTile): Record<string, boolean> {
    const status: HomeTileStatus = tile.status ?? 'primary';
    return {
      'f2ps-tile-status-primary': status === 'primary',
      'f2ps-tile-status-success': status === 'success',
      'f2ps-tile-status-warning': status === 'warning',
      'f2ps-tile-status-inprogress': status === 'inprogress',
      'f2ps-tile-status-planned': status === 'planned',
    };
  }

  moduleClass(tile: HomeTile): Record<string, boolean> {
    const color: HomeTileModuleColor = tile.moduleColor ?? 'generic';
    return {
      'module-tile-blue': color === 'blue',
      'module-tile-green': color === 'green',
      'module-tile-orange': color === 'orange',
      'module-tile-purple': color === 'purple',
      'module-tile-generic': color === 'generic',
    };
  }

  onTileClick(tile: HomeTile): void {
    this.tileSelect.emit(tile);
  }
}
