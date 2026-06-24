export type HomeTileStatus = 'primary' | 'success' | 'warning' | 'inprogress' | 'planned' | 'neutral';

export type HomeTileModuleColor = 'blue' | 'green' | 'orange' | 'purple' | 'generic';

export interface HomeTile {
  label: string;
  abbreviation: string;
  meta: string;
  route: string;
  status?: HomeTileStatus;
  moduleColor?: HomeTileModuleColor;
}
