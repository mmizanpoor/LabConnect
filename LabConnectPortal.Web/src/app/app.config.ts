import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { DOCUMENT } from '@angular/common';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from '@core/interceptors/auth.interceptor';
import { systemEntityInterceptor } from '@core/interceptors/system-entity.interceptor';
import { provideIcons } from '@core/services/icons/icons.provider';
import { provideTranslocoConfig } from '@core/services/localization/transloco.provider';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(
      routes,
      withInMemoryScrolling({
        scrollPositionRestoration: 'top',
        anchorScrolling: 'enabled',
      }),
    ),
    provideAnimationsAsync(),
    { provide: DOCUMENT, useFactory: () => document },
    provideHttpClient(withInterceptors([authInterceptor, systemEntityInterceptor])),
    provideIcons(),
    provideTranslocoConfig(),
  ],
};
