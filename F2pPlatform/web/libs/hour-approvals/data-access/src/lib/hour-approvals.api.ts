import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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

@Injectable({ providedIn: 'root' })
export class HourApprovalsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(F2P_API_BASE_URL);

  getCapabilities(): Observable<HourApprovalsCapabilitiesDto> {
    return this.http.get<HourApprovalsCapabilitiesDto>(`${this.baseUrl}/api/hour-approvals/capabilities`);
  }

  listTasks(filter: ApprovalStatusFilter): Observable<HourApprovalTaskDto[]> {
    const query = filter === 'all' ? '' : `?approvalStatus=${filter}`;
    return this.http.get<HourApprovalTaskDto[]>(`${this.baseUrl}/api/hour-approvals/tasks${query}`);
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

    return this.http.get<ApprovalQueueRowDto[]>(`${this.baseUrl}/api/hour-approvals/queue`, { params });
  }

  saveTask(taskId: string, values: ApprovalValuesDto): Observable<HourApprovalTaskDto> {
    return this.http.put<HourApprovalTaskDto>(`${this.baseUrl}/api/hour-approvals/tasks/${taskId}`, values);
  }

  approveTask(taskId: string): Observable<HourApprovalTaskDto> {
    return this.http.post<HourApprovalTaskDto>(`${this.baseUrl}/api/hour-approvals/tasks/${taskId}/approve`, {});
  }

  submitTasks(taskIds: string[]): Observable<SubmitTasksResultDto> {
    return this.http.post<SubmitTasksResultDto>(`${this.baseUrl}/api/hour-approvals/submit`, { taskIds });
  }
}
