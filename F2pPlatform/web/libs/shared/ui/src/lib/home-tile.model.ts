export type HomeTileModuleColor = 'blue' | 'green' | 'orange' | 'purple' | 'generic';

export type HomeTileIcon = 'grid' | 'ship' | 'clock' | 'check';

export interface HomeTile {
  label: string;
  abbreviation: string;
  meta: string;
  route: string;
  moduleColor?: HomeTileModuleColor;
  icon?: HomeTileIcon;
}
