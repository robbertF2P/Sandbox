import { DatePipe, NgClass, UpperCasePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { F2pAppNavbarComponent } from '@floorganise/ui';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { IdentityAuthService } from '@f2p/identity/data-access';
import {
  ApprovalQueueFilter,
  ApprovalQueueRowDto,
  ApprovalValuesDto,
  ColumnDefDto,
  formatHourApprovalsValue,
  HOUR_APPROVALS_EDITABLE_FIELDS,
  HourApprovalsApi,
  HourApprovalsCapabilitiesDto,
  HourApprovalsLocale,
  OrganisationId,
  resolveHourApprovalsLocale,
  SubmissionCategory,
  TaskId,
  TimeWindow,
  translateHourApprovalsLabel,
  visibleColumns,
} from '@f2p/hour-approvals/data-access';

interface FilterOption<T> {
  value: T;
  label: string;
}

interface ProjectGroup {
  projectLabel: string;
  rows: ApprovalQueueRowDto[];
}

interface CategorySection {
  category: SubmissionCategory;
  title: string;
  projectGroups: ProjectGroup[];
}

const SUBMISSION_CATEGORY_OPTIONS: FilterOption<SubmissionCategory>[] = [
  { value: 'worked_on', label: 'Tasks worked on' },
  { value: 'other_active', label: 'Other active task' },
  { value: 'never_submitted', label: 'Never submitted' },
];

const GROUP_TITLES: Record<SubmissionCategory, string> = {
  worked_on: 'Task worked on',
  other_active: 'Other active task',
  never_submitted: 'Never submitted',
};

const GROUP_ORDER: SubmissionCategory[] = ['worked_on', 'other_active', 'never_submitted'];

const TIME_WINDOW_OPTIONS: { value: TimeWindow; label: string }[] = [
  { value: 'since_last_submission', label: 'Since last submission' },
  { value: 'last_week', label: 'Last week' },
  { value: 'current_week', label: 'Current week' },
  { value: 'custom', label: 'Custom' },
];

@Component({
  selector: 'f2p-hour-approvals-page',
  imports: [F2pAppNavbarComponent, FormsModule, DatePipe, NgClass, UpperCasePipe],
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
  readonly organisationOptions = signal<FilterOption<OrganisationId>[]>([]);
  readonly disciplineOptions = signal<FilterOption<string>[]>([]);
  readonly projectOptions = signal<FilterOption<string>[]>([]);
  readonly locationOptions = signal<FilterOption<string>[]>([]);
  readonly selectedOrganisationIds = signal<OrganisationId[]>([]);
  readonly selectedDisciplines = signal<string[]>([]);
  readonly selectedProjects = signal<string[]>([]);
  readonly selectedLocations = signal<string[]>([]);
  readonly selectedCategories = signal<SubmissionCategory[]>([]);
  readonly searchTerm = signal('');
  readonly timeWindow = signal<TimeWindow>('current_week');
  readonly selectedTaskIds = signal<Set<TaskId>>(new Set());
  readonly drafts = signal<Partial<Record<TaskId, ApprovalValuesDto>>>({});
  readonly at100Percent = signal<Partial<Record<TaskId, boolean>>>({});

  readonly displayName = this.auth.getDisplayName();
  readonly locale = signal<HourApprovalsLocale>(resolveHourApprovalsLocale());
  readonly categoryOptions = SUBMISSION_CATEGORY_OPTIONS;
  readonly timeWindowOptions = TIME_WINDOW_OPTIONS;
  readonly canApprove = computed(() => this.capabilities()?.canApprove ?? false);
  readonly queueView = computed(() => this.capabilities()?.queueView);
  readonly editableColumns = computed(() =>
    visibleColumns(this.queueView(), 'Core')
      .filter(column => HOUR_APPROVALS_EDITABLE_FIELDS.has(column.id as keyof ApprovalValuesDto)),
  );
  readonly extensionColumns = computed(() => visibleColumns(this.queueView(), 'Extension'));
  readonly computedColumns = computed(() => visibleColumns(this.queueView(), 'Computed'));

  readonly primaryOrganisation = computed(() => {
    const options = this.organisationOptions();
    return options.length === 1 ? options[0].label : options[0]?.label ?? 'Organisation';
  });

  readonly queueFilter = computed<ApprovalQueueFilter>(() => ({
    organisationIds: this.selectedOrganisationIds(),
    submissionCategories: this.selectedCategories(),
    search: this.searchTerm().trim(),
  }));

  readonly visibleRows = computed(() => {
    const disciplines = new Set(this.selectedDisciplines());
    const projects = new Set(this.selectedProjects());
    const locations = new Set(this.selectedLocations());

    return this.rows().filter(row => {
      if (disciplines.size > 0 && !disciplines.has(row.disciplineLabel)) {
        return false;
      }

      if (projects.size > 0 && !projects.has(row.projectLabel)) {
        return false;
      }

      if (locations.size > 0 && !locations.has(row.locationPath)) {
        return false;
      }

      return this.matchesTimeWindow(row);
    });
  });

  readonly groupedSections = computed<CategorySection[]>(() => {
    const byCategory = new Map<SubmissionCategory, Map<string, ApprovalQueueRowDto[]>>();

    for (const row of this.visibleRows()) {
      const projects = byCategory.get(row.submissionCategory) ?? new Map<string, ApprovalQueueRowDto[]>();
      const group = projects.get(row.projectLabel) ?? [];
      group.push(row);
      projects.set(row.projectLabel, group);
      byCategory.set(row.submissionCategory, projects);
    }

    return GROUP_ORDER
      .filter(category => byCategory.has(category))
      .map(category => {
        const projects = byCategory.get(category) ?? new Map<string, ApprovalQueueRowDto[]>();
        return {
          category,
          title: GROUP_TITLES[category],
          projectGroups: [...projects.entries()]
            .sort(([left], [right]) => left.localeCompare(right))
            .map(([projectLabel, rows]) => ({
              projectLabel,
              rows: rows.sort((left, right) => left.taskNumber - right.taskNumber),
            })),
        };
      });
  });

  readonly allVisibleTaskIds = computed(() => this.visibleRows().map(row => row.taskId));
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
        this.syncFilterOptions(rows);
        this.selectedTaskIds.set(new Set());
        this.drafts.set(Object.fromEntries(rows.map(row => [row.taskId, { ...row.currentValues }])));
        this.at100Percent.set(Object.fromEntries(
          rows.map(row => [row.taskId, row.currentValues.progress >= 100]),
        ));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load approval queue.');
        this.loading.set(false);
      },
    });
  }

  onOrganisationToggle(orgId: OrganisationId, checked: boolean): void {
    this.toggleFilterValue(this.selectedOrganisationIds, orgId, checked);
    this.refetchQueue();
  }

  onDisciplineToggle(label: string, checked: boolean): void {
    this.toggleFilterValue(this.selectedDisciplines, label, checked);
  }

  onProjectToggle(label: string, checked: boolean): void {
    this.toggleFilterValue(this.selectedProjects, label, checked);
  }

  onLocationToggle(label: string, checked: boolean): void {
    this.toggleFilterValue(this.selectedLocations, label, checked);
  }

  onCategoryToggle(category: SubmissionCategory, checked: boolean): void {
    this.toggleFilterValue(this.selectedCategories, category, checked);
    this.refetchQueue();
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.refetchQueue();
  }

  onTimeWindowChange(value: TimeWindow): void {
    this.timeWindow.set(value);
  }

  toggleSelectAll(checked: boolean): void {
    if (!checked) {
      this.selectedTaskIds.set(new Set());
      return;
    }

    this.selectedTaskIds.set(new Set(this.allVisibleTaskIds()));
  }

  toggleGroupSelection(rows: ApprovalQueueRowDto[], checked: boolean): void {
    const next = new Set(this.selectedTaskIds());
    for (const row of rows) {
      if (checked) {
        next.add(row.taskId);
      } else {
        next.delete(row.taskId);
      }
    }

    this.selectedTaskIds.set(next);
  }

  isGroupSelected(rows: ApprovalQueueRowDto[]): boolean {
    return rows.length > 0 && rows.every(row => this.selectedTaskIds().has(row.taskId));
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

    if (field === 'progress') {
      this.at100Percent.set({
        ...this.at100Percent(),
        [taskId]: Number(value) >= 100,
      });
    }

    const next = new Set(this.selectedTaskIds());
    next.add(taskId);
    this.selectedTaskIds.set(next);
  }

  onAt100Toggle(taskId: TaskId, checked: boolean): void {
    this.at100Percent.set({ ...this.at100Percent(), [taskId]: checked });
    if (checked) {
      this.onDraftChange(taskId, 'progress', 100);
    }
  }

  isAt100(taskId: TaskId): boolean {
    return this.at100Percent()[taskId] ?? false;
  }

  fieldClass(row: ApprovalQueueRowDto, field: keyof ApprovalValuesDto): string {
    return this.isFieldApproved(row, field) ? 'floorboard-field--approved' : 'floorboard-field--pending';
  }

  extensionValue(row: ApprovalQueueRowDto, column: ColumnDefDto): string {
    const value = row.extensions[column.id];
    return formatHourApprovalsValue(value, column.format, this.locale());
  }

  computedValue(row: ApprovalQueueRowDto, column: ColumnDefDto): string {
    const value = row.computed[column.id as keyof ApprovalQueueRowDto['computed']];
    return formatHourApprovalsValue(value, column.format, this.locale());
  }

  columnLabel(column: ColumnDefDto): string {
    return translateHourApprovalsLabel(column.labelKey, this.locale());
  }

  isEditableField(columnId: string): columnId is keyof ApprovalValuesDto {
    return HOUR_APPROVALS_EDITABLE_FIELDS.has(columnId as keyof ApprovalValuesDto);
  }

  isFieldApproved(row: ApprovalQueueRowDto, field: keyof ApprovalValuesDto): boolean {
    if (!row.isApproved) {
      return false;
    }

    const draft = this.drafts()[row.taskId] ?? row.currentValues;
    return draft[field] === row.currentValues[field];
  }

  hoursWindowLabel(row: ApprovalQueueRowDto): string {
    const baseline = Math.max(0, Math.round(row.lookbackValues.workedHours));
    const current = Math.round(row.hoursWorkedInWindow);
    return `${current} / ${baseline}`;
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

  private matchesTimeWindow(row: ApprovalQueueRowDto): boolean {
    switch (this.timeWindow()) {
      case 'since_last_submission':
        return row.hoursWorkedInWindow > 0 || row.lastApproval === null;
      case 'last_week':
        return row.hoursWorkedInWindow >= 0;
      case 'current_week':
        return true;
      case 'custom':
        return true;
      default:
        return true;
    }
  }

  private refetchQueue(): void {
    this.loading.set(true);
    this.loadQueue();
  }

  private syncFilterOptions(rows: ApprovalQueueRowDto[]): void {
    this.organisationOptions.set(this.toOptions(
      rows.map(row => ({ value: row.organisationId, label: row.organisationLabel })),
      option => option.value,
    ));
    this.disciplineOptions.set(this.toOptions(
      rows.map(row => ({ value: row.disciplineLabel, label: row.disciplineLabel })),
      option => option.value,
    ));
    this.projectOptions.set(this.toOptions(
      rows.map(row => ({ value: row.projectLabel, label: row.projectLabel })),
      option => option.value,
    ));
    this.locationOptions.set(this.toOptions(
      rows.map(row => ({ value: row.locationPath, label: row.locationPath })),
      option => option.value,
    ));
  }

  private toOptions<T>(
    items: FilterOption<T>[],
    keySelector: (item: FilterOption<T>) => T,
  ): FilterOption<T>[] {
    const map = new Map<string, FilterOption<T>>();
    for (const item of items) {
      map.set(String(keySelector(item)), item);
    }

    return [...map.values()].sort((left, right) => left.label.localeCompare(right.label));
  }

  private toggleFilterValue<T>(signalRef: { (): T[]; set(value: T[]): void }, value: T, checked: boolean): void {
    const current = new Set(signalRef());
    if (checked) {
      current.add(value);
    } else {
      current.delete(value);
    }

    signalRef.set([...current]);
  }

  private valuesEqual(left: ApprovalValuesDto, right: ApprovalValuesDto): boolean {
    return left.hoursToGo === right.hoursToGo
      && left.progress === right.progress
      && left.workedHours === right.workedHours
      && left.plannedStart === right.plannedStart
      && left.plannedFinish === right.plannedFinish;
  }
}
