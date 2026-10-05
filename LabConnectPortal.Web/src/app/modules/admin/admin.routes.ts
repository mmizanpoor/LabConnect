import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { authGuard } from '@core/guards/auth.guard';
import {
  administratorOnlyGuard,
  siteAdminEntityGuard,
} from '@core/guards/site-admin-entity.guard';
import { SystemEntity } from '@core/system-entity/system-entity';

export const adminRoutes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'users',
        canActivate: [siteAdminEntityGuard(SystemEntity.User)],
        loadComponent: () => import('./users/users.component').then(m => m.UsersComponent),
      },
      {
        path: 'api-keys',
        canActivate: [siteAdminEntityGuard(SystemEntity.User)],
        data: { adminMode: true, systemEntity: SystemEntity.User },
        loadComponent: () =>
          import('../profile/api-keys/api-keys-list.component').then(
            (m) => m.ApiKeysListComponent,
          ),
      },
      {
        path: 'site-users',
        canActivate: [administratorOnlyGuard],
        loadComponent: () =>
          import('./site-users/site-users.component').then(m => m.SiteUsersComponent),
      },
      {
        path: 'laboratories',
        canActivate: [siteAdminEntityGuard(SystemEntity.Laboratory)],
        loadComponent: () => import('./laboratories/laboratories.component').then(m => m.LaboratoriesComponent),
      },
      {
        path: 'laboratories/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.Laboratory)],
        loadComponent: () => import('./laboratories/laboratory-detail/laboratory-detail.component').then(m => m.LaboratoryDetailComponent),
      },
      {
        path: 'receptions',
        canActivate: [administratorOnlyGuard],
        loadComponent: () =>
          import('../profile/receptions/receptions-list.component').then(
            (m) => m.ReceptionsListComponent,
          ),
      },
      {
        path: 'special-offers',
        canActivate: [siteAdminEntityGuard(SystemEntity.SpecialOffer)],
        loadComponent: () =>
          import('./special-offers/admin-special-offers-list.component').then(m => m.AdminSpecialOffersListComponent),
      },
      {
        path: 'special-offers/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.SpecialOffer)],
        loadComponent: () =>
          import('./special-offers/admin-special-offer-detail.component').then(m => m.AdminSpecialOfferDetailComponent),
      },
      {
        path: 'shops',
        canActivate: [siteAdminEntityGuard(SystemEntity.Shop)],
        loadComponent: () => import('./shops/shops.component').then(m => m.ShopsComponent),
      },
      {
        path: 'shops/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.Shop)],
        loadComponent: () => import('./shops/shop-detail/shop-detail.component').then(m => m.ShopDetailComponent),
      },
      {
        path: 'product-categories',
        canActivate: [siteAdminEntityGuard(SystemEntity.ProductCategory)],
        loadComponent: () =>
          import('./products/product-categories/product-categories.component').then(m => m.ProductCategoriesComponent),
      },
      {
        path: 'product-attributes',
        canActivate: [siteAdminEntityGuard(SystemEntity.ProductAttribute)],
        loadComponent: () =>
          import('./products/product-attributes/product-attributes.component').then(m => m.ProductAttributesComponent),
      },
      {
        path: 'brands',
        canActivate: [siteAdminEntityGuard(SystemEntity.Brand)],
        loadComponent: () => import('./products/brands/brands.component').then(m => m.BrandsComponent),
      },
      {
        path: 'products',
        canActivate: [siteAdminEntityGuard(SystemEntity.Product)],
        loadComponent: () => import('./products/products/products.component').then(m => m.ProductsComponent),
      },
      {
        path: 'products/new',
        redirectTo: 'products',
        pathMatch: 'full',
      },
      {
        path: 'products/:id/review',
        redirectTo: 'products/:id',
        pathMatch: 'full',
      },
      {
        path: 'products/:id/edit',
        canActivate: [siteAdminEntityGuard(SystemEntity.Product, 'update')],
        data: { listLink: '/admin/products' },
        loadComponent: () =>
          import('./products/products/product-form/product-form.component').then(m => m.ProductFormComponent),
      },
      {
        path: 'products/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.Product, 'view')],
        loadComponent: () =>
          import('./products/products/product-review/product-review.component').then(
            (m) => m.ProductReviewComponent,
          ),
      },
      {
        path: 'settings/seo',
        canActivate: [siteAdminEntityGuard(SystemEntity.Settings)],
        loadComponent: () =>
          import('./settings/settings-seo.component').then((m) => m.SettingsSeoComponent),
      },
      {
        path: 'settings/useful-links',
        canActivate: [siteAdminEntityGuard(SystemEntity.Settings)],
        loadComponent: () =>
          import('./settings/settings-useful-links.component').then(
            (m) => m.SettingsUsefulLinksComponent
          ),
      },
      {
        path: 'settings',
        canActivate: [siteAdminEntityGuard(SystemEntity.Settings)],
        loadComponent: () => import('./settings/settings.component').then(m => m.SettingsComponent),
      },
      {
        path: 'account',
        loadComponent: () => import('../profile/profile.component').then(m => m.ProfileComponent),
      },
      {
        path: 'change-username',
        data: { mode: 'username' },
        loadComponent: () =>
          import('../profile/account-security/account-security.component').then(m => m.AccountSecurityComponent),
      },
      {
        path: 'change-password',
        data: { mode: 'password' },
        loadComponent: () =>
          import('../profile/account-security/account-security.component').then(m => m.AccountSecurityComponent),
      },
      {
        path: 'messages',
        canActivate: [siteAdminEntityGuard(SystemEntity.Message)],
        loadComponent: () => import('../profile/messages/messages.component').then(m => m.MessagesComponent),
      },
      {
        path: 'activity-logs',
        canActivate: [siteAdminEntityGuard(SystemEntity.ActivityLog)],
        loadComponent: () =>
          import('./activity-logs/activity-logs.component').then((m) => m.AdminActivityLogsComponent),
      },
      {
        path: 'login-report',
        canActivate: [authGuard],
        loadComponent: () =>
          import('../profile/login-report/login-report.component').then(
            (m) => m.ProfileLoginReportComponent,
          ),
      },
      {
        path: 'login-reports',
        canActivate: [siteAdminEntityGuard(SystemEntity.LoginReport)],
        data: { systemEntity: SystemEntity.LoginReport },
        loadComponent: () =>
          import('./login-reports/admin-login-reports.component').then(
            (m) => m.AdminLoginReportsComponent,
          ),
      },
      {
        path: 'slider-groups',
        canActivate: [siteAdminEntityGuard(SystemEntity.SliderGroup)],
        loadComponent: () =>
          import('./slider-groups/slider-groups-list.component').then(m => m.SliderGroupsListComponent),
      },
      {
        path: 'slider-groups/new',
        canActivate: [siteAdminEntityGuard(SystemEntity.SliderGroup, 'create')],
        loadComponent: () =>
          import('./slider-groups/slider-group-form.component').then(m => m.SliderGroupFormComponent),
      },
      {
        path: 'slider-groups/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.SliderGroup, 'update')],
        loadComponent: () =>
          import('./slider-groups/slider-group-form.component').then(m => m.SliderGroupFormComponent),
      },
      {
        path: 'content-groups',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentGroup)],
        loadComponent: () =>
          import('./content/content-groups/content-groups.component').then(m => m.ContentGroupsComponent),
      },
      {
        path: 'posts',
        canActivate: [siteAdminEntityGuard(SystemEntity.Post)],
        data: { listLink: '/admin/posts' },
        loadComponent: () => import('./content/posts/posts.component').then(m => m.PostsComponent),
      },
      {
        path: 'posts/new',
        canActivate: [siteAdminEntityGuard(SystemEntity.Post, 'create')],
        data: { listLink: '/admin/posts' },
        loadComponent: () => import('./content/posts/post-form.component').then(m => m.PostFormComponent),
      },
      {
        path: 'posts/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.Post, 'update')],
        data: { listLink: '/admin/posts' },
        loadComponent: () => import('./content/posts/post-form.component').then(m => m.PostFormComponent),
      },
      {
        path: 'content/news',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentNews)],
        data: { listLink: '/admin/content/news', contentType: 1, systemEntity: SystemEntity.ContentNews },
        loadComponent: () => import('./content/posts/posts.component').then(m => m.PostsComponent),
      },
      {
        path: 'content/news/new',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentNews, 'create')],
        data: { listLink: '/admin/content/news', contentType: 1, systemEntity: SystemEntity.ContentNews },
        loadComponent: () => import('./content/posts/post-form.component').then(m => m.PostFormComponent),
      },
      {
        path: 'content/news/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentNews, 'update')],
        data: { listLink: '/admin/content/news', contentType: 1, systemEntity: SystemEntity.ContentNews },
        loadComponent: () => import('./content/posts/post-form.component').then(m => m.PostFormComponent),
      },
      {
        path: 'content/articles',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentArticles)],
        data: { listLink: '/admin/content/articles', contentType: 2, systemEntity: SystemEntity.ContentArticles },
        loadComponent: () => import('./content/posts/posts.component').then(m => m.PostsComponent),
      },
      {
        path: 'content/articles/new',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentArticles, 'create')],
        data: { listLink: '/admin/content/articles', contentType: 2, systemEntity: SystemEntity.ContentArticles },
        loadComponent: () => import('./content/posts/post-form.component').then(m => m.PostFormComponent),
      },
      {
        path: 'content/articles/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentArticles, 'update')],
        data: { listLink: '/admin/content/articles', contentType: 2, systemEntity: SystemEntity.ContentArticles },
        loadComponent: () => import('./content/posts/post-form.component').then(m => m.PostFormComponent),
      },
      {
        path: 'content/documents',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentDocuments)],
        data: { listLink: '/admin/content/documents', contentType: 3, systemEntity: SystemEntity.ContentDocuments },
        loadComponent: () => import('./content/posts/posts.component').then(m => m.PostsComponent),
      },
      {
        path: 'content/documents/new',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentDocuments, 'create')],
        data: { listLink: '/admin/content/documents', contentType: 3, systemEntity: SystemEntity.ContentDocuments },
        loadComponent: () => import('./content/posts/post-form.component').then(m => m.PostFormComponent),
      },
      {
        path: 'content/documents/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentDocuments, 'update')],
        data: { listLink: '/admin/content/documents', contentType: 3, systemEntity: SystemEntity.ContentDocuments },
        loadComponent: () => import('./content/posts/post-form.component').then(m => m.PostFormComponent),
      },
      {
        path: 'ads',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds)],
        loadComponent: () =>
          import('./advertisements/advertisements.component').then((m) => m.AdvertisementsComponent),
      },
      {
        path: 'ads/new',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds, 'create')],
        loadComponent: () =>
          import('./advertisements/advertisement-form.component').then(
            (m) => m.AdvertisementFormComponent,
          ),
      },
      {
        path: 'ads/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds, 'update')],
        loadComponent: () =>
          import('./advertisements/advertisement-form.component').then(
            (m) => m.AdvertisementFormComponent,
          ),
      },
      {
        path: 'ad-positions',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds)],
        loadComponent: () =>
          import('./advertisement-commerce/ad-positions.component').then(
            (m) => m.AdPositionsComponent,
          ),
      },
      {
        path: 'ad-positions/new',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds, 'create')],
        loadComponent: () =>
          import('./advertisement-commerce/ad-position-form.component').then(
            (m) => m.AdPositionFormComponent,
          ),
      },
      {
        path: 'ad-positions/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds, 'update')],
        loadComponent: () =>
          import('./advertisement-commerce/ad-position-form.component').then(
            (m) => m.AdPositionFormComponent,
          ),
      },
      {
        path: 'ad-prices',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds)],
        loadComponent: () =>
          import('./advertisement-commerce/ad-prices.component').then((m) => m.AdPricesComponent),
      },
      {
        path: 'category-prices',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds)],
        loadComponent: () =>
          import('./advertisement-commerce/category-prices.component').then(
            (m) => m.CategoryPricesComponent,
          ),
      },
      {
        path: 'ad-orders',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds)],
        loadComponent: () =>
          import('./advertisement-commerce/ad-orders.component').then((m) => m.AdOrdersComponent),
      },
      {
        path: 'ad-orders/new',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds, 'create')],
        loadComponent: () =>
          import('./advertisement-commerce/ad-order-form.component').then(
            (m) => m.AdOrderFormComponent,
          ),
      },
      {
        path: 'ad-orders/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.ContentAds, 'update')],
        loadComponent: () =>
          import('./advertisement-commerce/ad-order-form.component').then(
            (m) => m.AdOrderFormComponent,
          ),
      },
      {
        path: 'company-regulations',
        canActivate: [siteAdminEntityGuard(SystemEntity.CompanyRegulation)],
        loadComponent: () =>
          import('./company-regulations/company-regulations.component').then(
            (m) => m.CompanyRegulationsComponent,
          ),
      },
      {
        path: 'company-regulations/new',
        canActivate: [siteAdminEntityGuard(SystemEntity.CompanyRegulation, 'create')],
        loadComponent: () =>
          import('./company-regulations/company-regulation-form.component').then(
            (m) => m.CompanyRegulationFormComponent,
          ),
      },
      {
        path: 'company-regulations/:id',
        canActivate: [siteAdminEntityGuard(SystemEntity.CompanyRegulation, 'update')],
        loadComponent: () =>
          import('./company-regulations/company-regulation-form.component').then(
            (m) => m.CompanyRegulationFormComponent,
          ),
      },
      {
        path: 'site-services',
        canActivate: [siteAdminEntityGuard(SystemEntity.SiteService, 'view')],
        loadComponent: () =>
          import('./site-services/site-services.component').then(
            (m) => m.SiteServicesComponent,
          ),
      },
      {
        path: 'site-charge-services',
        canActivate: [siteAdminEntityGuard(SystemEntity.Settings, 'view')],
        loadComponent: () =>
          import('./site-charge-services/site-charge-services.component').then(
            (m) => m.SiteChargeServicesComponent,
          ),
      },
    ],
  },
];
