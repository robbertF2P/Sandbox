import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { IdentityAuthService } from '@f2p/identity/data-access';
import {
  ApprovalStatusFilter,
  HourApprovalsApi,
  HourApprovalsCapabilitiesDto,
  HourApprovalTaskDto,
} from '@f2p/hour-approvals/data-access';

@Component({
  selector: 'f2p-hour-approvals-page',
  imports: [RouterLink, FormsModule, DatePipe],
  templateUrl: './hour-approvals-page.component.html',
})
export class HourApprovalsPageComponent implements OnInit {
  private readonly api = inject(HourApprovalsApi);
  private readonly auth = inject(IdentityAuthService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly capabilities = signal<HourApprovalsCapabilitiesDto | null>(null);
  readonly tasks = signal<HourApprovalTaskDto[]>([]);
  readonly filter = signal<ApprovalStatusFilter>('all');
  readonly selectedTaskId = signal<string | null>(null);
  readonly saving = signal(false);

  readonly displayName = this.auth.getDisplayName();
  readonly canApprove = computed(() => this.capabilities()?.canApprove ?? false);

  readonly selectedTask = computed(() =>
    this.tasks().find(task => task.id === this.selectedTaskId()) ?? null,
  );

  readonly draftValues = signal<HourApprovalTaskDto['currentValues'] | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.getCapabilities().subscribe({
      next: capabilities => {
        this.capabilities.set(capabilities);
        this.loadTasks();
      },
      error: () => {
        this.error.set('Hour approvals feature is not enabled for this tenant.');
        this.loading.set(false);
      },
    });
  }

  loadTasks(): void {
    this.api.listTasks(this.filter()).subscribe({
      next: tasks => {
        this.tasks.set(tasks);
        const current = this.selectedTaskId();
        const next = current && tasks.some(task => task.id === current)
          ? current
          : tasks[0]?.id ?? null;

        this.selectedTaskId.set(next);
        this.syncDraftFromSelection();
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load tasks.');
        this.loading.set(false);
      },
    });
  }

  onFilterChange(value: ApprovalStatusFilter): void {
    this.filter.set(value);
    this.loading.set(true);
    this.loadTasks();
  }

  selectTask(taskId: string): void {
    this.selectedTaskId.set(taskId);
    this.syncDraftFromSelection();
  }

  isFieldApproved(field: keyof HourApprovalTaskDto['currentValues']): boolean {
    const task = this.selectedTask();
    if (!task?.lastApproval) {
      return false;
    }

    return task.currentValues[field] === task.lastApproval.approvedValues[field];
  }

  inputClass(field: keyof HourApprovalTaskDto['currentValues']): string {
    return this.isFieldApproved(field)
      ? 'f2p-approved-field'
      : 'f2p-pending-field';
  }

  saveSelected(): void {
    const task = this.selectedTask();
    const draft = this.draftValues();
    if (!task || !draft || !this.canApprove()) {
      return;
    }

    this.saving.set(true);
    this.api.saveTask(task.id, draft).subscribe({
      next: () => {
        this.saving.set(false);
        this.loadTasks();
      },
      error: () => {
        this.saving.set(false);
        this.error.set('Save failed. Check you have Approve Hours/Progress permission.');
      },
    });
  }

  approveSelected(): void {
    const task = this.selectedTask();
    if (!task || !this.canApprove()) {
      return;
    }

    this.saving.set(true);
    this.api.approveTask(task.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.loadTasks();
      },
      error: () => {
        this.saving.set(false);
        this.error.set('Approval failed. Check permissions.');
      },
    });
  }

  private syncDraftFromSelection(): void {
    const task = this.selectedTask();
    this.draftValues.set(task ? { ...task.currentValues } : null);
  }
}
