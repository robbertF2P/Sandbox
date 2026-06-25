export type HomeTileModuleColor = 'blue' | 'green' | 'orange' | 'purple' | 'generic';

export interface HomeTile {
  label: string;
  abbreviation: string;
  meta: string;
  route: string;
  moduleColor?: HomeTileModuleColor;
}
