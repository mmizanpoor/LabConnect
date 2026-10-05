import { Routes } from '@angular/router';
import { BREADCRUMB_DATA_KEY } from '@core/services/breadcrumb/breadcrumb.types';

export const contactUsRoutes: Routes = [
  {
    path: '',
    data: { [BREADCRUMB_DATA_KEY]: 'modules.home.nav.contactUs' },
    loadComponent: () => import('./contact-us.component').then(m => m.ContactUsComponent),
  },
];
