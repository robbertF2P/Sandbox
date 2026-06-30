import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { F2P_API_BASE_URL } from '@f2p/shared/api-core';
import {
  ApprovalStatusFilter,
  ApprovalValuesDto,
  HourApprovalsCapabilitiesDto,
  HourApprovalTaskDto,
  SubmitTasksResultDto,
} from './hour-approvals.dto';
import { asActivityCode, asTaskId, TaskId } from './hour-approvals.ids';

@Injectable({ providedIn: 'root' })
export class HourApprovalsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(F2P_API_BASE_URL);

  getCapabilities(): Observable<HourApprovalsCapabilitiesDto> {
    return this.http.get<HourApprovalsCapabilitiesDto>(`${this.baseUrl}/api/hour-approvals/capabilities`);
  }

  listTasks(filter: ApprovalStatusFilter): Observable<HourApprovalTaskDto[]> {
    let params = new HttpParams();
    if (filter !== 'all') {
      params = params.set('approvalStatus', filter);
    }

    return this.http
      .get<HourApprovalTaskDto[]>(`${this.baseUrl}/api/hour-approvals/tasks`, { params })
      .pipe(map(tasks => tasks.map(task => this.mapTask(task))));
  }

  saveTask(taskId: TaskId, values: ApprovalValuesDto): Observable<HourApprovalTaskDto> {
    return this.http
      .put<HourApprovalTaskDto>(`${this.baseUrl}/api/hour-approvals/tasks/${taskId}`, values)
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
}
