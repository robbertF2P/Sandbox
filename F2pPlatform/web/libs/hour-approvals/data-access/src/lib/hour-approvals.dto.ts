export interface ApprovalValuesDto {
  hoursToGo: number;
  progress: number;
  workedHours: number;
  plannedStart: string | null;
  plannedFinish: string | null;
}

export interface LastApprovalDto {
  id: string;
  approvedBy: string;
  approvedAtUtc: string;
  approvedValues: ApprovalValuesDto;
}

export interface HourApprovalTaskDto {
  id: string;
  title: string;
  activityCode: string;
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
