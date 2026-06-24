import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAdminApiCore } from '@admin/shared/api-core';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAdminApiCore(),
  ],
};
