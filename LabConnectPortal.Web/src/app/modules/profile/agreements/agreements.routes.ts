import { Routes } from '@angular/router';
import { labAgreementGuard } from '@core/guards/lab-agreement.guard';

export const agreementsRoutes: Routes = [
  {
    path: '',
    canActivate: [labAgreementGuard],
    loadComponent: () =>
      import('./agreements-list.component').then((m) => m.AgreementsListComponent),
  },
];
