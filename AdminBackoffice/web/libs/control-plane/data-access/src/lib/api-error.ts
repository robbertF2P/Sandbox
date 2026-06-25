/** Maps API / Problem Details / network failures to operator-facing messages. */
export function formatControlPlaneApiError(
  err: unknown,
  options?: {
    fallback?: string;
    action?: 'provision' | 'sync' | 'list';
  },
): string {
  const fallback = options?.fallback ?? 'Request failed.';
  const action = options?.action ?? 'list';

  if (!err || typeof err !== 'object') {
    return fallback;
  }

  const http = err as { status?: number; statusText?: string; error?: unknown; message?: string };

  if (http.status === 0 || http.message === 'Http failure response for (unknown url): 0 Unknown Error') {
    return (
      'Cannot reach the admin API. Start AdminBackoffice on :5090 and ensure the UI proxy is running (:5190).'
    );
  }

  const body = http.error;
  const detail = readString(body, 'detail') ?? readString(body, 'error');
  const title = readString(body, 'title');

  let message = detail ?? title ?? fallback;

  if (http.status === 502 || title?.toLowerCase().includes('platform sync')) {
    message = appendPlatformSyncHint(message, action);
  }

  if (http.status === 409) {
    message = detail ?? 'A tenant with this slug already exists.';
  }

  if (http.status === 400 && detail) {
    message = detail;
  }

  return message;
}

function readString(body: unknown, key: string): string | null {
  if (!body || typeof body !== 'object') {
    return null;
  }

  const value = (body as Record<string, unknown>)[key];
  return typeof value === 'string' && value.trim().length > 0 ? value.trim() : null;
}

function appendPlatformSyncHint(message: string, action: 'provision' | 'sync' | 'list'): string {
  const lower = message.toLowerCase();
  const connectionHint =
    lower.includes('connection refused') ||
    lower.includes('no connection') ||
    lower.includes('actively refused') ||
    lower.includes('name or service not known') ||
    lower.includes('failed to connect');

  if (!connectionHint && action === 'list') {
    return message;
  }

  const intro =
    action === 'provision'
      ? 'Tenant was saved in the control plane, but configuration could not be pushed to the v2 platform.'
      : 'Platform sync failed.';

  const steps =
    'Start F2pPlatform first: `cd F2pPlatform && docker compose up -d && dotnet run --project host/F2pPlatform.Host` (:5080), then retry.';

  if (connectionHint) {
    return `${intro} F2pPlatform API is not reachable on :5080 (${message}). ${steps}`;
  }

  return `${message} ${steps}`;
}
