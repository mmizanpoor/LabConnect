import { Routes } from '@angular/router';
import { BREADCRUMB_DATA_KEY } from '@core/services/breadcrumb/breadcrumb.types';

export const companyRegulationsRoutes: Routes = [
  {
    path: '',
    data: { [BREADCRUMB_DATA_KEY]: 'modules.companyRegulations.title' },
    loadComponent: () =>
      import('./company-regulations-page.component').then(
        (m) => m.CompanyRegulationsPageComponent,
      ),
  },
];
