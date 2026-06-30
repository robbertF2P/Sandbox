import { DatePipe, NgClass } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { F2pAppNavbarComponent } from '@floorganise/ui';
import { forkJoin, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { shouldProceedToSubmit } from './hour-approvals-submit';
import { IdentityAuthService } from '@f2p/identity/data-access';
import {
  ApprovalStatusFilter,
  ApprovalValuesDto,
  ColumnDefDto,
  formatHourApprovalsValue,
  HOUR_APPROVALS_EDITABLE_FIELDS,
  HourApprovalsApi,
  HourApprovalsCapabilitiesDto,
  HourApprovalTaskDto,
  HourApprovalsLocale,
  resolveHourApprovalsLocale,
  TaskId,
  translateHourApprovalsLabel,
  visibleColumns,
} from '@f2p/hour-approvals/data-access';

@Component({
  selector: 'f2p-hour-approvals-page',
  imports: [F2pAppNavbarComponent, FormsModule, DatePipe, NgClass],
  templateUrl: './hour-approvals-page.component.html',
})
export class HourApprovalsPageComponent implements OnInit {
  private readonly api = inject(HourApprovalsApi);
  private readonly auth = inject(IdentityAuthService);

  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly capabilities = signal<HourApprovalsCapabilitiesDto | null>(null);
  readonly tasks = signal<HourApprovalTaskDto[]>([]);
  readonly statusFilter = signal<ApprovalStatusFilter>('all');
  readonly searchTerm = signal('');
  readonly selectedTaskIds = signal<Set<TaskId>>(new Set());
  readonly drafts = signal<Partial<Record<TaskId, ApprovalValuesDto>>>({});

  readonly displayName = this.auth.getDisplayName();
  readonly locale = signal<HourApprovalsLocale>(resolveHourApprovalsLocale());
  readonly canApprove = computed(() => this.capabilities()?.canApprove ?? false);
  readonly queueView = computed(() => this.capabilities()?.queueView);
  readonly editableColumns = computed(() =>
    visibleColumns(this.queueView(), 'Core')
      .filter(column => HOUR_APPROVALS_EDITABLE_FIELDS.has(column.id as keyof ApprovalValuesDto)),
  );
  readonly extensionColumns = computed(() => visibleColumns(this.queueView(), 'Extension'));

  readonly visibleTasks = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) {
      return this.tasks();
    }

    return this.tasks().filter(task =>
      task.title.toLowerCase().includes(term)
      || task.activityCode.toLowerCase().includes(term)
      || task.currentValues.assignedUser.toLowerCase().includes(term));
  });

  readonly allVisibleTaskIds = computed(() => this.visibleTasks().map(task => task.id));
  readonly allVisibleSelected = computed(() => {
    const visible = this.allVisibleTaskIds();
    return visible.length > 0 && visible.every(taskId => this.selectedTaskIds().has(taskId));
  });
  readonly hasSelection = computed(() => this.selectedTaskIds().size > 0);
  readonly submitEnabled = computed(() => this.canApprove() && this.hasSelection() && !this.submitting());

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
    this.api.listTasks(this.statusFilter()).subscribe({
      next: tasks => {
        this.tasks.set(tasks);
        this.selectedTaskIds.set(new Set());
        this.drafts.set(Object.fromEntries(tasks.map(task => [task.id, { ...task.currentValues }])));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load tasks.');
        this.loading.set(false);
      },
    });
  }

  onStatusFilterChange(value: ApprovalStatusFilter): void {
    this.statusFilter.set(value);
    this.loading.set(true);
    this.loadTasks();
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
  }

  toggleSelectAll(checked: boolean): void {
    this.selectedTaskIds.set(checked ? new Set(this.allVisibleTaskIds()) : new Set());
  }

  toggleRowSelection(taskId: TaskId, checked: boolean): void {
    const next = new Set(this.selectedTaskIds());
    if (checked) {
      next.add(taskId);
    } else {
      next.delete(taskId);
    }

    this.selectedTaskIds.set(next);
  }

  isSelected(taskId: TaskId): boolean {
    return this.selectedTaskIds().has(taskId);
  }

  draftFor(taskId: TaskId): ApprovalValuesDto | undefined {
    return this.drafts()[taskId];
  }

  onDraftChange(taskId: TaskId, field: keyof ApprovalValuesDto, value: string | number): void {
    const current = this.drafts()[taskId];
    if (!current) {
      return;
    }

    this.drafts.set({
      ...this.drafts(),
      [taskId]: { ...current, [field]: value },
    });

    const next = new Set(this.selectedTaskIds());
    next.add(taskId);
    this.selectedTaskIds.set(next);
  }

  fieldClass(task: HourApprovalTaskDto, field: string): string {
    if (!this.isEditableField(field)) {
      return '';
    }

    return this.isFieldApproved(task, field) ? 'floorboard-field--approved' : 'floorboard-field--pending';
  }

  extensionValue(task: HourApprovalTaskDto, column: ColumnDefDto): string {
    const extensions = (task as HourApprovalTaskDto & { extensions?: Record<string, unknown> }).extensions ?? {};
    const value = extensions[column.id];
    return formatHourApprovalsValue(value, column.format, this.locale());
  }

  columnLabel(column: ColumnDefDto): string {
    return translateHourApprovalsLabel(column.labelKey, this.locale());
  }

  isEditableField(columnId: string): columnId is keyof ApprovalValuesDto {
    return HOUR_APPROVALS_EDITABLE_FIELDS.has(columnId as keyof ApprovalValuesDto);
  }

  isFieldApproved(task: HourApprovalTaskDto, field: string): boolean {
    if (!task.isApproved || !this.isEditableField(field)) {
      return false;
    }

    const draft = this.drafts()[task.id] ?? task.currentValues;
    return draft[field as keyof ApprovalValuesDto] === task.currentValues[field as keyof ApprovalValuesDto];
  }

  submitSelected(): void {
    if (!this.submitEnabled()) {
      return;
    }

    const selected = [...this.selectedTaskIds()];
    const dirtySaves = selected
      .map(taskId => {
        const task = this.tasks().find(item => item.id === taskId);
        const draft = this.drafts()[taskId];
        if (!task || !draft || this.valuesEqual(draft, task.currentValues)) {
          return null;
        }

        return this.api.saveTask(taskId, draft).pipe(
          map(() => true),
          catchError(() => of(false)),
        );
      })
      .filter((call): call is ReturnType<HourApprovalsApi['saveTask']> => call !== null);

    this.submitting.set(true);
    this.error.set(null);

    const saveStep = dirtySaves.length > 0 ? forkJoin(dirtySaves) : of([] as boolean[]);

    saveStep
      .pipe(
        switchMap(results => {
          if (!shouldProceedToSubmit(results)) {
            throw new Error('Save failed');
          }

          return this.api.submitTasks(selected);
        }),
      )
      .subscribe({
        next: result => {
          this.submitting.set(false);
          if (result.failures.length > 0) {
            this.error.set(`Some tasks failed to submit (${result.failures.length}).`);
          }

          this.loadTasks();
        },
        error: () => {
          this.submitting.set(false);
          this.error.set('Submit failed. Save your changes and check you have Approve Hours/Progress permission.');
        },
      });
  }

  private valuesEqual(left: ApprovalValuesDto, right: ApprovalValuesDto): boolean {
    return left.hoursToGo === right.hoursToGo
      && left.plannedStart === right.plannedStart
      && left.plannedFinish === right.plannedFinish
      && left.assignedUser === right.assignedUser;
  }
}
