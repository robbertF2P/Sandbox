import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { correlationInterceptor, F2P_API_BASE_URL } from '@f2p/shared/api-core';
import { identityAuthInterceptor } from '@f2p/identity/data-access';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([correlationInterceptor, identityAuthInterceptor])),
    { provide: F2P_API_BASE_URL, useValue: '' },
  ],
};
