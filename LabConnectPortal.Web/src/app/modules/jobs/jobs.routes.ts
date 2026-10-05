import { Routes } from '@angular/router';
import { BREADCRUMB_DATA_KEY } from '@core/services/breadcrumb/breadcrumb.types';

export const jobsRoutes: Routes = [
  {
    path: '',
    data: { [BREADCRUMB_DATA_KEY]: 'modules.home.nav.jobs' },
    loadComponent: () => import('./jobs-list.component').then(m => m.JobsListComponent),
  },
  {
    path: ':id',
    data: {
      [BREADCRUMB_DATA_KEY]: [
        { i18nKey: 'modules.home.nav.jobs', url: '/jobs' },
        { dynamic: true },
      ],
    },
    loadComponent: () => import('./job-detail.component').then(m => m.JobDetailComponent),
  },
];
