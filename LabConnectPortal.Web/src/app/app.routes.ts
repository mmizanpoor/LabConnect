import { Routes } from '@angular/router';
import { authGuard } from '@core/guards/auth.guard';
import { adminGuard } from '@core/guards/admin.guard';
import { noAuthGuard } from '@core/guards/no-auth.guard';
import { BREADCRUMB_DATA_KEY } from '@core/services/breadcrumb/breadcrumb.types';

export const routes: Routes = [
  {
    path: 'auth',
    canActivate: [noAuthGuard],
    loadChildren: () =>
      import('./modules/auth/auth.routes').then((m) => m.authRoutes),
  },
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadChildren: () =>
      import('./modules/admin/admin.routes').then((m) => m.adminRoutes),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./modules/profile/profile.routes').then((m) => m.profileRoutes),
  },
  {
    path: '',
    loadComponent: () =>
      import('./modules/base/layouts/public-layout/public-layout.component').then(
        (m) => m.PublicLayoutComponent,
      ),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      {
        path: 'home',
        loadChildren: () =>
          import('./modules/home/home.routes').then((m) => m.homeRoutes),
      },
      {
        path: 'jobs',
        loadChildren: () =>
          import('./modules/jobs/jobs.routes').then((m) => m.jobsRoutes),
      },
      {
        path: 'posts',
        loadChildren: () =>
          import('./modules/posts/posts.routes').then((m) => m.postsRoutes),
      },
      {
        path: 'news',
        loadChildren: () =>
          import('./modules/posts/posts.routes').then((m) => m.newsRoutes),
      },
      {
        path: 'articles',
        loadChildren: () =>
          import('./modules/posts/posts.routes').then((m) => m.articlesRoutes),
      },
      {
        path: 'documents',
        loadChildren: () =>
          import('./modules/posts/posts.routes').then((m) => m.documentsRoutes),
      },
      {
        path: 'products',
        loadChildren: () =>
          import('./modules/products/products.routes').then(
            (m) => m.productsRoutes,
          ),
      },
      {
        path: 'special-offers',
        loadChildren: () =>
          import('./modules/special-offers/special-offers.routes').then(
            (m) => m.specialOffersPublicRoutes,
          ),
      },
      {
        path: 'contact-us',
        loadChildren: () =>
          import('./modules/contact-us/contact-us.routes').then(
            (m) => m.contactUsRoutes,
          ),
      },
      {
        path: 'company-regulations',
        loadChildren: () =>
          import('./modules/company-regulations/company-regulations.routes').then(
            (m) => m.companyRegulationsRoutes,
          ),
      },
      {
        path: 'cart',
        data: { [BREADCRUMB_DATA_KEY]: 'modules.cart.title' },
        loadComponent: () =>
          import('./modules/cart/cart.component').then((m) => m.CartComponent),
      },
      {
        path: 'checkout',
        canActivate: [authGuard],
        data: {
          [BREADCRUMB_DATA_KEY]: [
            { i18nKey: 'modules.cart.title', url: '/cart' },
            { i18nKey: 'modules.checkout.title' },
          ],
        },
        loadComponent: () =>
          import('./modules/cart/checkout.component').then(
            (m) => m.CheckoutComponent,
          ),
      },
      {
        path: 'orders',
        canActivate: [authGuard],
        loadChildren: () =>
          import('./modules/orders/orders.routes').then((m) => m.ordersRoutes),
      },
    ],
  },
  { path: 'login', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'payment-response',
    loadComponent: () =>
      import('./modules/profile/payment-response/payment-response.component').then(
        (m) => m.PaymentResponseComponent,
      ),
  },
  { path: '**', redirectTo: 'home' },
];
