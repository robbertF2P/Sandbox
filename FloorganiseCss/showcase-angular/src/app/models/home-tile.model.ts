export type HomeTileAccent = 'brand' | 'success' | 'warning';

export interface HomeTile {
  label: string;
  meta: string;
  route: string;
  accent?: HomeTileAccent;
}
