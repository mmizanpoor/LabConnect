import { Routes } from '@angular/router';
import { BREADCRUMB_DATA_KEY } from '@core/services/breadcrumb/breadcrumb.types';

export const productsRoutes: Routes = [
  {
    path: '',
    data: { [BREADCRUMB_DATA_KEY]: 'modules.products.title' },
    loadComponent: () => import('./products-list.component').then(m => m.ProductsListComponent),
  },
  {
    path: ':id',
    data: {
      [BREADCRUMB_DATA_KEY]: [
        { i18nKey: 'modules.products.title', url: '/products' },
        { dynamic: true },
      ],
    },
    loadComponent: () => import('./product-detail.component').then(m => m.ProductDetailComponent),
  },
];
