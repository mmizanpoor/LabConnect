import { Routes } from '@angular/router';
import { ProfileLayoutComponent } from '@modules/profile/profile-layout/profile-layout.component';
import {
  profileAccountRoutes,
  profileActivityLogsRoute,
  profileApiKeysRoute,
  profileCenterRoutes,
  profileHomeRoute,
  profileJobPostingRoutes,
  profileLabMessagesRoute,
  profileProductRoutes,
} from '@modules/profile/profile-children.routes';

export const storeRoutes: Routes = [
  {
    path: '',
    component: ProfileLayoutComponent,
    data: { portal: 'store' },
    children: [
      ...profileHomeRoute,
      ...profileAccountRoutes,
      ...profileCenterRoutes,
      ...profileJobPostingRoutes,
      ...profileProductRoutes,
      ...profileLabMessagesRoute,
      ...profileActivityLogsRoute,
      ...profileApiKeysRoute,
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./dashboard/store-dashboard.component').then((m) => m.StoreDashboardComponent),
      },
    ],
  },
];
