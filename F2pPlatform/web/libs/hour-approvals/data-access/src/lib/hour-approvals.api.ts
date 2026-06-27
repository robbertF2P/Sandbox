import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { F2P_API_BASE_URL } from '@f2p/shared/api-core';
import {
  ApprovalQueueFilter,
  ApprovalQueueRowDto,
  ApprovalStatusFilter,
  ApprovalValuesDto,
  HourApprovalsCapabilitiesDto,
  HourApprovalTaskDto,
  SubmitTasksResultDto,
} from './hour-approvals.dto';
import {
  asActivityCode,
  asAssignmentId,
  asOrganisationId,
  asTaskId,
  TaskId,
} from './hour-approvals.ids';

@Injectable({ providedIn: 'root' })
export class HourApprovalsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(F2P_API_BASE_URL);

  getCapabilities(): Observable<HourApprovalsCapabilitiesDto> {
    return this.http.get<HourApprovalsCapabilitiesDto>(`${this.baseUrl}/api/hour-approvals/capabilities`);
  }

  listTasks(filter: ApprovalStatusFilter): Observable<HourApprovalTaskDto[]> {
    const query = filter === 'all' ? '' : `?approvalStatus=${filter}`;
    return this.http
      .get<HourApprovalTaskDto[]>(`${this.baseUrl}/api/hour-approvals/tasks${query}`)
      .pipe(map(tasks => tasks.map(task => this.mapTask(task))));
  }

  getQueue(filter: ApprovalQueueFilter): Observable<ApprovalQueueRowDto[]> {
    let params = new HttpParams();

    if (filter.organisationIds.length > 0) {
      params = params.set('organisationIds', filter.organisationIds.join(','));
    }

    if (filter.submissionCategories.length > 0) {
      params = params.set('submissionCategories', filter.submissionCategories.join(','));
    }

    const search = filter.search.trim();
    if (search) {
      params = params.set('search', search);
    }

    if (filter.timeWindow) {
      params = params.set('timeWindow', filter.timeWindow);
    }

    return this.http
      .get<ApprovalQueueRowDto[]>(`${this.baseUrl}/api/hour-approvals/queue`, { params })
      .pipe(map(rows => rows.map(row => this.mapQueueRow(row))));
  }

  saveTask(taskId: TaskId, values: ApprovalValuesDto): Observable<HourApprovalTaskDto> {
    return this.http
      .put<HourApprovalTaskDto>(`${this.baseUrl}/api/hour-approvals/tasks/${taskId}`, values)
      .pipe(map(task => this.mapTask(task)));
  }

  approveTask(taskId: TaskId): Observable<HourApprovalTaskDto> {
    return this.http
      .post<HourApprovalTaskDto>(`${this.baseUrl}/api/hour-approvals/tasks/${taskId}/approve`, {})
      .pipe(map(task => this.mapTask(task)));
  }

  submitTasks(taskIds: TaskId[]): Observable<SubmitTasksResultDto> {
    return this.http
      .post<SubmitTasksResultDto>(`${this.baseUrl}/api/hour-approvals/submit`, { taskIds })
      .pipe(
        map(result => ({
          approved: result.approved.map(task => this.mapTask(task)),
          failures: result.failures.map(failure => ({
            taskId: asTaskId(failure.taskId as unknown as string),
            error: failure.error,
          })),
        })),
      );
  }

  private mapTask(task: HourApprovalTaskDto): HourApprovalTaskDto {
    return {
      ...task,
      id: asTaskId(task.id as unknown as string),
      activityCode: asActivityCode(task.activityCode as unknown as string),
    };
  }

  private mapQueueRow(row: ApprovalQueueRowDto): ApprovalQueueRowDto {
    return {
      ...row,
      taskId: asTaskId(row.taskId as unknown as string),
      assignmentId: asAssignmentId(row.assignmentId as unknown as string),
      organisationId: asOrganisationId(row.organisationId as unknown as number),
      activityCode: asActivityCode(row.activityCode as unknown as string),
      taskNumber: row.taskNumber ?? 0,
      locationPath: row.locationPath ?? '',
      disciplineLabel: row.disciplineLabel ?? '',
      teamCount: row.teamCount ?? 0,
      totalHoursBooked: row.totalHoursBooked ?? 0,
      lookbackValues: row.lookbackValues ?? row.currentValues,
      extensions: row.extensions ?? {},
      computed: row.computed ?? { daysSinceLastSubmission: null },
    };
  }
}
