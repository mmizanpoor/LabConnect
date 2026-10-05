import { Routes } from '@angular/router';
import { labPortalEntityGuard } from '@core/guards/lab-portal-entity.guard';
import { SystemEntity } from '@core/system-entity/system-entity';

export const specialOffersRoutes: Routes = [
  {
    path: '',
    canActivate: [labPortalEntityGuard(SystemEntity.SpecialOffer)],
    loadComponent: () => import('./special-offers-list.component').then((m) => m.SpecialOffersListComponent),
  },
  {
    path: 'new',
    canActivate: [labPortalEntityGuard(SystemEntity.SpecialOffer, 'create')],
    loadComponent: () => import('./special-offer-form.component').then((m) => m.SpecialOfferFormComponent),
  },
  {
    path: ':id',
    canActivate: [labPortalEntityGuard(SystemEntity.SpecialOffer, 'update')],
    loadComponent: () => import('./special-offer-form.component').then((m) => m.SpecialOfferFormComponent),
  },
];
