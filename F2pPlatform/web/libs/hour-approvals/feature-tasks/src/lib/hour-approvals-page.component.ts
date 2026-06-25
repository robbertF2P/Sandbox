import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { F2pPageHeaderComponent } from '@floorganise/ui';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { IdentityAuthService } from '@f2p/identity/data-access';
import {
  ApprovalQueueFilter,
  ApprovalQueueRowDto,
  ApprovalValuesDto,
  HourApprovalsApi,
  HourApprovalsCapabilitiesDto,
  OrganisationId,
  SubmissionCategory,
  TaskId,
} from '@f2p/hour-approvals/data-access';

interface OrganisationOption {
  id: OrganisationId;
  label: string;
}

interface QueueGroup {
  category: SubmissionCategory;
  title: string;
  rows: ApprovalQueueRowDto[];
}

const SUBMISSION_CATEGORY_OPTIONS: { value: SubmissionCategory; label: string }[] = [
  { value: 'worked_on', label: 'Task worked on' },
  { value: 'other_active', label: 'Other active task' },
  { value: 'never_submitted', label: 'Never submitted' },
];

const GROUP_TITLES: Record<SubmissionCategory, string> = {
  worked_on: 'Task worked on',
  other_active: 'Other active task',
  never_submitted: 'Never submitted',
};

const GROUP_ORDER: SubmissionCategory[] = ['worked_on', 'other_active', 'never_submitted'];

@Component({
  selector: 'f2p-hour-approvals-page',
  imports: [F2pPageHeaderComponent, FormsModule, DatePipe],
  templateUrl: './hour-approvals-page.component.html',
})
export class HourApprovalsPageComponent implements OnInit {
  private readonly api = inject(HourApprovalsApi);
  private readonly auth = inject(IdentityAuthService);

  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly capabilities = signal<HourApprovalsCapabilitiesDto | null>(null);
  readonly rows = signal<ApprovalQueueRowDto[]>([]);
  readonly organisationOptions = signal<OrganisationOption[]>([]);
  readonly selectedOrganisationIds = signal<OrganisationId[]>([]);
  readonly selectedCategories = signal<SubmissionCategory[]>([]);
  readonly searchTerm = signal('');
  readonly selectedTaskIds = signal<Set<TaskId>>(new Set());
  readonly drafts = signal<Partial<Record<TaskId, ApprovalValuesDto>>>({});

  readonly displayName = this.auth.getDisplayName();
  readonly canApprove = computed(() => this.capabilities()?.canApprove ?? false);
  readonly categoryOptions = SUBMISSION_CATEGORY_OPTIONS;

  readonly queueFilter = computed<ApprovalQueueFilter>(() => ({
    organisationIds: this.selectedOrganisationIds(),
    submissionCategories: this.selectedCategories(),
    search: this.searchTerm(),
  }));

  readonly groupedRows = computed<QueueGroup[]>(() => {
    const byCategory = new Map<SubmissionCategory, ApprovalQueueRowDto[]>();

    for (const row of this.rows()) {
      const group = byCategory.get(row.submissionCategory) ?? [];
      group.push(row);
      byCategory.set(row.submissionCategory, group);
    }

    return GROUP_ORDER
      .filter(category => (byCategory.get(category)?.length ?? 0) > 0)
      .map(category => ({
        category,
        title: GROUP_TITLES[category],
        rows: byCategory.get(category) ?? [],
      }));
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
        this.loadQueue();
      },
      error: () => {
        this.error.set('Hour approvals feature is not enabled for this tenant.');
        this.loading.set(false);
      },
    });
  }

  loadQueue(): void {
    this.api.getQueue(this.queueFilter()).subscribe({
      next: rows => {
        this.rows.set(rows);
        this.syncOrganisationOptions(rows);
        this.selectedTaskIds.set(new Set());
        this.drafts.set(Object.fromEntries(rows.map(row => [row.taskId, { ...row.currentValues }])));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load approval queue.');
        this.loading.set(false);
      },
    });
  }

  onOrganisationToggle(orgId: OrganisationId, checked: boolean): void {
    const current = new Set(this.selectedOrganisationIds());
    if (checked) {
      current.add(orgId);
    } else {
      current.delete(orgId);
    }

    this.selectedOrganisationIds.set([...current]);
    this.refetchQueue();
  }

  onCategoryToggle(category: SubmissionCategory, checked: boolean): void {
    const current = new Set(this.selectedCategories());
    if (checked) {
      current.add(category);
    } else {
      current.delete(category);
    }

    this.selectedCategories.set([...current]);
    this.refetchQueue();
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.refetchQueue();
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

    const parsed = typeof value === 'number' ? value : value;
    this.drafts.set({
      ...this.drafts(),
      [taskId]: { ...current, [field]: parsed },
    });

    const next = new Set(this.selectedTaskIds());
    next.add(taskId);
    this.selectedTaskIds.set(next);
  }

  isFieldApproved(row: ApprovalQueueRowDto, field: keyof ApprovalValuesDto): boolean {
    if (!row.isApproved) {
      return false;
    }

    const draft = this.drafts()[row.taskId] ?? row.currentValues;
    return draft[field] === row.currentValues[field];
  }

  fieldClass(row: ApprovalQueueRowDto, field: keyof ApprovalValuesDto): string {
    return this.isFieldApproved(row, field) ? 'f2p-approved-field' : 'f2p-pending-field';
  }

  submitSelected(): void {
    if (!this.submitEnabled()) {
      return;
    }

    const selected = [...this.selectedTaskIds()];
    const dirtySaves = selected
      .map(taskId => {
        const row = this.rows().find(item => item.taskId === taskId);
        const draft = this.drafts()[taskId];
        if (!row || !draft || this.valuesEqual(draft, row.currentValues)) {
          return null;
        }

        return this.api.saveTask(taskId, draft).pipe(catchError(() => of(null)));
      })
      .filter((call): call is ReturnType<HourApprovalsApi['saveTask']> => call !== null);

    this.submitting.set(true);
    this.error.set(null);

    const saveStep = dirtySaves.length > 0 ? forkJoin(dirtySaves) : of([]);

    saveStep
      .pipe(switchMap(() => this.api.submitTasks(selected)))
      .subscribe({
        next: result => {
          this.submitting.set(false);
          if (result.failures.length > 0) {
            this.error.set(`Some tasks failed to submit (${result.failures.length}).`);
          }

          this.loadQueue();
        },
        error: () => {
          this.submitting.set(false);
          this.error.set('Submit failed. Check you have Approve Hours/Progress permission.');
        },
      });
  }

  private refetchQueue(): void {
    this.loading.set(true);
    this.loadQueue();
  }

  private syncOrganisationOptions(rows: ApprovalQueueRowDto[]): void {
    const map = new Map<OrganisationId, string>();
    for (const row of rows) {
      map.set(row.organisationId, row.organisationLabel);
    }

    for (const option of this.organisationOptions()) {
      if (!map.has(option.id)) {
        map.set(option.id, option.label);
      }
    }

    this.organisationOptions.set(
      [...map.entries()]
        .map(([id, label]) => ({ id, label }))
        .sort((a, b) => a.label.localeCompare(b.label)),
    );
  }

  private valuesEqual(left: ApprovalValuesDto, right: ApprovalValuesDto): boolean {
    return left.hoursToGo === right.hoursToGo
      && left.progress === right.progress
      && left.workedHours === right.workedHours
      && left.plannedStart === right.plannedStart
      && left.plannedFinish === right.plannedFinish;
  }
}
