import { EnvironmentProviders, InjectionToken, makeEnvironmentProviders } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { correlationInterceptor } from './correlation.interceptor';

/** Empty string = same origin (use proxy.conf.json in dev). */
export const ADMIN_API_BASE_URL = new InjectionToken<string>('ADMIN_API_BASE_URL', {
  factory: () => '',
});

export function provideAdminApiCore(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideHttpClient(withInterceptors([correlationInterceptor])),
    { provide: ADMIN_API_BASE_URL, useValue: '' },
  ]);
}
