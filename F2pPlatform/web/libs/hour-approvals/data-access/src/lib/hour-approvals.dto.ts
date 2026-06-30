import { ActivityCode, TaskId } from './hour-approvals.ids';

export type ColumnSourceDto = 'Core' | 'Extension' | 'Computed';

export interface ColumnDefDto {
  id: string;
  labelKey: string;
  source: ColumnSourceDto;
  visible: boolean;
  order: number;
  format?: string | null;
}

export interface ViewDefinitionDto {
  screenId: string;
  columns: ColumnDefDto[];
}

export interface ApprovalValuesDto {
  hoursToGo: number;
  plannedStart: string | null;
  plannedFinish: string | null;
  assignedUser: string;
}

export interface LastApprovalDto {
  id?: string;
  approvedBy: string;
  approvedAtUtc: string;
  approvalDay?: string;
  approvedValues?: ApprovalValuesDto;
}

export interface HourApprovalTaskDto {
  id: TaskId;
  title: string;
  activityCode: ActivityCode;
  approvalState: 'Approved' | 'NotApproved';
  isApproved: boolean;
  currentValues: ApprovalValuesDto;
  lastApproval: LastApprovalDto | null;
}

export interface HourApprovalsCapabilitiesDto {
  featureEnabled: boolean;
  customizationPackId: string;
  queueView: ViewDefinitionDto;
  canApprove: boolean;
  permissions: string[];
}

export type ApprovalStatusFilter = 'all' | 'approved' | 'not_approved';

export interface SubmitTasksResultDto {
  approved: HourApprovalTaskDto[];
  failures: { taskId: TaskId; error: string }[];
}

export const HOUR_APPROVALS_EDITABLE_FIELDS = new Set<keyof ApprovalValuesDto>([
  'hoursToGo',
  'plannedStart',
  'plannedFinish',
  'assignedUser',
]);

export function visibleColumns(
  view: ViewDefinitionDto | undefined,
  source?: ColumnSourceDto,
): ColumnDefDto[] {
  if (!view) {
    return [];
  }

  return view.columns
    .filter(column => column.visible && (source === undefined || column.source === source))
    .sort((left, right) => left.order - right.order);
}

export function isColumnVisible(view: ViewDefinitionDto | undefined, columnId: string): boolean {
  return view?.columns.some(column => column.id === columnId && column.visible) ?? false;
}
