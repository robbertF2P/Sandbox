import { HttpInterceptorFn } from '@angular/common/http';

let sequence = 0;

export const correlationInterceptor: HttpInterceptorFn = (request, next) => {
  const correlationId = crypto.randomUUID();
  const useCase = request.headers.get('X-Use-Case') ?? inferUseCase(request.url);

  return next(
    request.clone({
      setHeaders: {
        'X-Correlation-Id': correlationId,
        'X-Use-Case': useCase,
        'X-Causation-Id': `web-${++sequence}`,
      },
    }),
  );
};

function inferUseCase(url: string): string {
  if (url.includes('/api/reference/status')) {
    return 'Reference.GetStatus';
  }

  return 'Web.Request';
}
