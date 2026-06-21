import { EnvironmentProviders, InjectionToken, makeEnvironmentProviders } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { correlationInterceptor } from './correlation.interceptor';

/** Empty string = same origin (use proxy.conf.json in dev). */
export const F2P_API_BASE_URL = new InjectionToken<string>('F2P_API_BASE_URL', {
  factory: () => '',
});

export function provideF2pApiCore(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideHttpClient(withInterceptors([correlationInterceptor])),
    { provide: F2P_API_BASE_URL, useValue: '' },
  ]);
}
