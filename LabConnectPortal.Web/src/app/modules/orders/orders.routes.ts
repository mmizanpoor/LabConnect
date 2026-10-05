import { Routes } from '@angular/router';
import { BREADCRUMB_DATA_KEY } from '@core/services/breadcrumb/breadcrumb.types';

export const ordersRoutes: Routes = [
  {
    path: '',
    data: { [BREADCRUMB_DATA_KEY]: 'modules.orders.title' },
    loadComponent: () => import('./orders-list.component').then(m => m.OrdersListComponent),
  },
  {
    path: ':id',
    data: {
      [BREADCRUMB_DATA_KEY]: [
        { i18nKey: 'modules.orders.title', url: '/orders' },
        { dynamic: true },
      ],
    },
    loadComponent: () => import('./order-detail.component').then(m => m.OrderDetailComponent),
  },
];
