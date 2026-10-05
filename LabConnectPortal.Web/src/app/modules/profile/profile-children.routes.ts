import { Routes } from '@angular/router';
import { authGuard } from '@core/guards/auth.guard';
import { centerProfileGuard } from '@core/guards/center-profile.guard';
import { approvedCenterProfileGuard } from '@core/guards/approved-center-profile.guard';
import { resumeGuard } from '@core/guards/resume.guard';
import { labPortalEntityGuard } from '@core/guards/lab-portal-entity.guard';
import { SystemEntity } from '@core/system-entity/system-entity';

export const profileHomeRoute: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./profile.component').then((m) => m.ProfileComponent),
  },
];

export const profileResumeRoute: Routes = [
  {
    path: 'resume',
    canActivate: [resumeGuard],
    loadComponent: () =>
      import('./resume/resume.component').then((m) => m.ResumeComponent),
  },
];

export const profileResumeApplicationsRoute: Routes = [
  {
    path: 'resume-applications',
    canActivate: [authGuard, resumeGuard],
    loadComponent: () =>
      import('./my-resume-applications/my-resume-applications.component').then(
        (m) => m.MyResumeApplicationsComponent,
      ),
  },
];

export const profileAccountRoutes: Routes = [
  {
    path: 'change-username',
    canActivate: [authGuard],
    data: { mode: 'username' },
    loadComponent: () =>
      import('./account-security/account-security.component').then(
        (m) => m.AccountSecurityComponent,
      ),
  },
  {
    path: 'change-password',
    canActivate: [authGuard],
    data: { mode: 'password' },
    loadComponent: () =>
      import('./account-security/account-security.component').then(
        (m) => m.AccountSecurityComponent,
      ),
  },
  {
    path: 'login-report',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./login-report/login-report.component').then(
        (m) => m.ProfileLoginReportComponent,
      ),
  },
];

export const profileCenterRoutes: Routes = [
  {
    path: 'center',
    canActivate: [centerProfileGuard],
    data: { systemEntity: SystemEntity.CenterProfile },
    loadComponent: () =>
      import('./center-profile/center-profile.component').then(
        (m) => m.CenterProfileComponent,
      ),
  },
  {
    path: 'center/view',
    canActivate: [centerProfileGuard],
    data: { systemEntity: SystemEntity.CenterProfile, viewMode: true },
    loadComponent: () =>
      import('./center-profile/center-profile.component').then(
        (m) => m.CenterProfileComponent,
      ),
  },
  {
    path: 'center/sms-charge',
    canActivate: [centerProfileGuard],
    data: { systemEntity: SystemEntity.CenterProfile },
    loadComponent: () =>
      import('./sms-charge/sms-charge.component').then(
        (m) => m.SmsChargeComponent,
      ),
  },
  {
    path: 'locations',
    canActivate: [centerProfileGuard],
    data: { systemEntity: SystemEntity.Location },
    loadComponent: () =>
      import('./organization-locations/organization-locations.component').then(
        (m) => m.OrganizationLocationsComponent,
      ),
  },
];

export const profileJobPostingRoutes: Routes = [
  {
    path: 'job-postings',
    canActivate: [centerProfileGuard],
    data: { systemEntity: SystemEntity.JobPosting },
    loadComponent: () =>
      import('./job-postings/job-postings-list.component').then(
        (m) => m.JobPostingsListComponent,
      ),
  },
  {
    path: 'job-postings/new',
    canActivate: [
      centerProfileGuard,
      approvedCenterProfileGuard,
      labPortalEntityGuard(SystemEntity.JobPosting, 'create'),
    ],
    data: { systemEntity: SystemEntity.JobPosting },
    loadComponent: () =>
      import('./job-postings/job-posting-form.component').then(
        (m) => m.JobPostingFormComponent,
      ),
  },
  {
    path: 'job-postings/:id/applications',
    canActivate: [
      centerProfileGuard,
      labPortalEntityGuard(SystemEntity.JobPosting, 'view'),
    ],
    data: { systemEntity: SystemEntity.JobPosting },
    loadComponent: () =>
      import('./job-applications/job-applications-review.component').then(
        (m) => m.JobApplicationsReviewComponent,
      ),
  },
  {
    path: 'job-postings/:id',
    canActivate: [
      centerProfileGuard,
      labPortalEntityGuard(SystemEntity.JobPosting, 'update'),
    ],
    data: { systemEntity: SystemEntity.JobPosting },
    loadComponent: () =>
      import('./job-postings/job-posting-form.component').then(
        (m) => m.JobPostingFormComponent,
      ),
  },
];

export const profileProductRoutes: Routes = [
  {
    path: 'product-listings',
    canActivate: [centerProfileGuard, approvedCenterProfileGuard],
    data: { systemEntity: SystemEntity.Product },
    loadComponent: () =>
      import('./catalog-products/profile-catalog-products.component').then(
        (m) => m.ProfileCatalogProductsComponent,
      ),
  },
  {
    path: 'product-listings/new',
    canActivate: [
      centerProfileGuard,
      approvedCenterProfileGuard,
      labPortalEntityGuard(SystemEntity.Product, 'create'),
    ],
    data: {
      systemEntity: SystemEntity.Product,
      listLink: '/profile/product-listings',
    },
    loadComponent: () =>
      import('../admin/products/products/product-form/product-form.component').then(
        (m) => m.ProductFormComponent,
      ),
  },
  {
    path: 'product-listings/:id/resumes',
    canActivate: [
      centerProfileGuard,
      approvedCenterProfileGuard,
      labPortalEntityGuard(SystemEntity.Product, 'view'),
    ],
    data: { systemEntity: SystemEntity.Product },
    loadComponent: () =>
      import('./product-resume-applications/product-resume-applications-review.component').then(
        (m) => m.ProductResumeApplicationsReviewComponent,
      ),
  },
  {
    path: 'product-listings/:id',
    canActivate: [
      centerProfileGuard,
      approvedCenterProfileGuard,
      labPortalEntityGuard(SystemEntity.Product, 'update'),
    ],
    data: {
      systemEntity: SystemEntity.Product,
      listLink: '/profile/product-listings',
    },
    loadComponent: () =>
      import('../admin/products/products/product-form/product-form.component').then(
        (m) => m.ProductFormComponent,
      ),
  },
  {
    path: 'catalog-products',
    redirectTo: 'product-listings',
    pathMatch: 'full',
  },
  {
    path: 'catalog-products/new',
    redirectTo: 'product-listings/new',
    pathMatch: 'full',
  },
  {
    path: 'catalog-products/:id',
    redirectTo: 'product-listings/:id',
  },
  {
    path: 'product-orders',
    canActivate: [centerProfileGuard],
    data: { systemEntity: SystemEntity.ProductOrder },
    loadComponent: () =>
      import('./product-orders/product-orders-list.component').then(
        (m) => m.ProductOrdersListComponent,
      ),
  },
  {
    path: 'product-orders/:id',
    canActivate: [centerProfileGuard],
    data: { systemEntity: SystemEntity.ProductOrder },
    loadComponent: () =>
      import('./product-orders/product-order-detail.component').then(
        (m) => m.ProductOrderDetailComponent,
      ),
  },
];

export const profileActivityLogsRoute: Routes = [
  {
    path: 'activity-logs',
    canActivate: [
      centerProfileGuard,
      labPortalEntityGuard(SystemEntity.ActivityLog, 'view'),
    ],
    data: { systemEntity: SystemEntity.ActivityLog },
    loadComponent: () =>
      import('./activity-logs/activity-logs.component').then(
        (m) => m.ProfileActivityLogsComponent,
      ),
  },
];

export const profileApiKeysRoute: Routes = [
  {
    path: 'api-keys',
    canActivate: [centerProfileGuard],
    data: { systemEntity: SystemEntity.CenterProfile },
    loadComponent: () =>
      import('./api-keys/api-keys-list.component').then(
        (m) => m.ApiKeysListComponent,
      ),
  },
];

export const profileLabMessagesRoute: Routes = [
  {
    path: 'messages',
    canActivate: [
      authGuard,
      labPortalEntityGuard(SystemEntity.Message, 'view'),
    ],
    data: { systemEntity: SystemEntity.Message },
    loadComponent: () =>
      import('./messages/messages.component').then((m) => m.MessagesComponent),
  },
];

export const profileUserMessagesRoute: Routes = [
  {
    path: 'messages',
    canActivate: [authGuard],
    data: { systemEntity: SystemEntity.Message },
    loadComponent: () =>
      import('./messages/messages.component').then((m) => m.MessagesComponent),
  },
];
