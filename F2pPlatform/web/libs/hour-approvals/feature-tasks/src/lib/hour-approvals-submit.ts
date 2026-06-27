export function shouldProceedToSubmit(saveResults: readonly boolean[]): boolean {
  return saveResults.every(Boolean);
}
