import {
  ActivityCode,
  AssignmentId,
  OrganisationId,
  TaskId,
} from './hour-approvals.ids';

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
  progress: number;
  workedHours: number;
  plannedStart: string | null;
  plannedFinish: string | null;
}

export interface LastApprovalDto {
  id?: string;
  approvedBy: string;
  approvedAtUtc: string;
  approvedValues?: ApprovalValuesDto;
}

export interface HourApprovalTaskDto {
  id: TaskId;
  title: string;
  activityCode: ActivityCode;
  isActiveForCurrentUser: boolean;
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

export type SubmissionCategory = 'worked_on' | 'other_active' | 'never_submitted';

export type TimeWindow = 'since_last_submission' | 'last_week' | 'current_week' | 'custom';

export interface ApprovalQueueFilter {
  organisationIds: OrganisationId[];
  submissionCategories: SubmissionCategory[];
  search: string;
  timeWindow: TimeWindow;
}

export interface ApprovalQueueComputedDto {
  daysSinceLastSubmission: number | null;
}

export interface ApprovalQueueRowDto {
  taskId: TaskId;
  assignmentId: AssignmentId;
  organisationId: OrganisationId;
  title: string;
  activityCode: ActivityCode;
  organisationLabel: string;
  projectLabel: string;
  taskNumber: number;
  locationPath: string;
  disciplineLabel: string;
  teamCount: number;
  totalHoursBooked: number;
  hoursWorkedInWindow: number;
  submissionCategory: SubmissionCategory;
  approvalState: 'Approved' | 'NotApproved';
  isApproved: boolean;
  currentValues: ApprovalValuesDto;
  lookbackValues: ApprovalValuesDto;
  extensions: Record<string, string | number | boolean | null>;
  computed: ApprovalQueueComputedDto;
  lastApproval: LastApprovalDto | null;
}

export interface SubmitTasksResultDto {
  approved: HourApprovalTaskDto[];
  failures: { taskId: TaskId; error: string }[];
}

export const HOUR_APPROVALS_EDITABLE_FIELDS = new Set<keyof ApprovalValuesDto>([
  'hoursToGo',
  'plannedStart',
  'plannedFinish',
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
