export type Brand<T, BrandId extends string> = T & { readonly __brand: BrandId };

export type TaskId = Brand<string, 'TaskId'>;
export type AssignmentId = Brand<string, 'AssignmentId'>;
export type OrganisationId = Brand<number, 'OrganisationId'>;
export type ActivityCode = Brand<string, 'ActivityCode'>;

export function asTaskId(value: string): TaskId {
  return value as TaskId;
}

export function asAssignmentId(value: string): AssignmentId {
  return value as AssignmentId;
}

export function asOrganisationId(value: number): OrganisationId {
  return value as OrganisationId;
}

export function asActivityCode(value: string): ActivityCode {
  return value as ActivityCode;
}
