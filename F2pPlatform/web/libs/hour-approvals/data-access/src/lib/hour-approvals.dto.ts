import {
  ActivityCode,
  AssignmentId,
  OrganisationId,
  TaskId,
} from './hour-approvals.ids';

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
  displaySettings: {
    showPlannedStart: boolean;
    showPlannedFinish: boolean;
  };
  canApprove: boolean;
  permissions: string[];
}

export type ApprovalStatusFilter = 'all' | 'approved' | 'not_approved';

export type SubmissionCategory = 'worked_on' | 'other_active' | 'never_submitted';

export interface ApprovalQueueFilter {
  organisationIds: OrganisationId[];
  submissionCategories: SubmissionCategory[];
  search: string;
}

export interface ApprovalQueueRowDto {
  taskId: TaskId;
  assignmentId: AssignmentId;
  organisationId: OrganisationId;
  title: string;
  activityCode: ActivityCode;
  organisationLabel: string;
  projectLabel: string;
  hoursWorkedInWindow: number;
  submissionCategory: SubmissionCategory;
  approvalState: 'Approved' | 'NotApproved';
  isApproved: boolean;
  currentValues: ApprovalValuesDto;
  lastApproval: LastApprovalDto | null;
}

export interface SubmitTasksResultDto {
  approved: HourApprovalTaskDto[];
  failures: { taskId: TaskId; error: string }[];
}
