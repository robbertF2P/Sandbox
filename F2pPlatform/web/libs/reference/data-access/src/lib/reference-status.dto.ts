export interface ReferenceStatusDto {
  moduleName: string;
  health: string;
  moduleRegistered: boolean;
  stranglerAdapterPresent: boolean;
  checkedAtUtc: string;
}
