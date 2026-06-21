import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideF2pApiCore } from '@f2p/shared/api-core';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [provideRouter(routes), provideF2pApiCore()],
};
