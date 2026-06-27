import { describe, expect, it } from 'vitest';
import { shouldProceedToSubmit } from './hour-approvals-submit';

describe('shouldProceedToSubmit', () => {
  it('returns false when any save failed', () => {
    expect(shouldProceedToSubmit([true, false])).toBe(false);
  });

  it('returns true when all saves succeeded', () => {
    expect(shouldProceedToSubmit([true, true])).toBe(true);
  });

  it('returns true when there were no dirty saves', () => {
    expect(shouldProceedToSubmit([])).toBe(true);
  });
});
