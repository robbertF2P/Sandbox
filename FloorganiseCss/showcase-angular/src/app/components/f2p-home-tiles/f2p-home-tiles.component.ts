import { Component, input, output } from '@angular/core';
import { NgClass } from '@angular/common';
import { HomeTile } from '../../models/home-tile.model';

@Component({
  selector: 'app-f2p-home-tiles',
  imports: [NgClass],
  templateUrl: './f2p-home-tiles.component.html',
})
export class F2pHomeTilesComponent {
  readonly tiles = input.required<HomeTile[]>();
  readonly tileSelect = output<HomeTile>();

  accentClass(tile: HomeTile): Record<string, boolean> {
    return {
      'border-t-4 border-t-f2p-brand': tile.accent === 'brand',
      'border-t-4 border-t-f2p-success': tile.accent === 'success',
      'border-t-4 border-t-f2p-warning': tile.accent === 'warning',
    };
  }

  onTileClick(tile: HomeTile): void {
    this.tileSelect.emit(tile);
  }
}
