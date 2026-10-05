import { Routes } from '@angular/router';
import { BREADCRUMB_DATA_KEY } from '@core/services/breadcrumb/breadcrumb.types';

export const homeRoutes: Routes = [
  {
    path: '',
    data: { [BREADCRUMB_DATA_KEY]: 'skip' },
    loadComponent: () => import('./home.component').then(m => m.HomeComponent),
  },
];
