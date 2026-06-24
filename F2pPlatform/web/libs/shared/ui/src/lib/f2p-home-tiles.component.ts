import { Component, input, output } from '@angular/core';
import { NgClass } from '@angular/common';
import { HomeTile, HomeTileIcon, HomeTileModuleColor } from './home-tile.model';

@Component({
  selector: 'f2p-home-tiles',
  imports: [NgClass],
  templateUrl: './f2p-home-tiles.component.html',
})
export class F2pHomeTilesComponent {
  readonly tiles = input.required<HomeTile[]>();
  readonly tileSelect = output<HomeTile>();

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

  iconClass(tile: HomeTile): Record<string, boolean> {
    const icon: HomeTileIcon = tile.icon ?? 'grid';
    return {
      'f2ps-icon-module-grid': icon === 'grid',
      'f2ps-icon-module-ship': icon === 'ship',
      'f2ps-icon-module-clock': icon === 'clock',
      'f2ps-icon-module-check': icon === 'check',
    };
  }

  onTileClick(tile: HomeTile): void {
    this.tileSelect.emit(tile);
  }
}
