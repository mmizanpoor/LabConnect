import { Routes } from '@angular/router';
import { BREADCRUMB_DATA_KEY } from '@core/services/breadcrumb/breadcrumb.types';

export const specialOffersPublicRoutes: Routes = [
  {
    path: '',
    data: { [BREADCRUMB_DATA_KEY]: 'modules.home.specialOffers.title' },
    loadComponent: () =>
      import('./special-offers-list.component').then((m) => m.PublicSpecialOffersListComponent),
  },
  {
    path: ':id',
    data: { [BREADCRUMB_DATA_KEY]: 'modules.specialOffers.detailTitle' },
    loadComponent: () => import('./special-offer-detail.component').then((m) => m.SpecialOfferDetailComponent),
  },
];
