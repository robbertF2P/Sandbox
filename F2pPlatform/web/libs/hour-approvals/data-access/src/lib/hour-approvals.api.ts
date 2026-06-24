import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { F2P_API_BASE_URL } from '@f2p/shared/api-core';
import {
  ApprovalStatusFilter,
  HourApprovalsCapabilitiesDto,
  HourApprovalTaskDto,
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

  saveTask(taskId: string, values: HourApprovalTaskDto['currentValues']): Observable<HourApprovalTaskDto> {
    return this.http.put<HourApprovalTaskDto>(`${this.baseUrl}/api/hour-approvals/tasks/${taskId}`, values);
  }

  approveTask(taskId: string): Observable<HourApprovalTaskDto> {
    return this.http.post<HourApprovalTaskDto>(`${this.baseUrl}/api/hour-approvals/tasks/${taskId}/approve`, {});
  }
}
