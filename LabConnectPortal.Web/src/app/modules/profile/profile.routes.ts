import { Routes } from '@angular/router';
import { adminProfileRedirectGuard } from '@core/guards/admin-profile-redirect.guard';
import {
  labPortalCanMatch,
  storePortalCanMatch,
  userPortalCanMatch,
} from '@core/portal/portal.can-match';

export const profileRoutes: Routes = [
  {
    path: '',
    canActivate: [adminProfileRedirectGuard],
    children: [
      {
        path: 'api-keys/guide',
        loadComponent: () =>
          import('../api-docs/product-api-guide.component').then(
            (m) => m.ProductApiGuideComponent,
          ),
      },
      {
        path: '',
        canMatch: [storePortalCanMatch],
        loadChildren: () => import('../store/store.routes').then((m) => m.storeRoutes),
      },
      {
        path: '',
        canMatch: [labPortalCanMatch],
        loadChildren: () => import('../lab/lab.routes').then((m) => m.labRoutes),
      },
      {
        path: '',
        canMatch: [userPortalCanMatch],
        loadChildren: () => import('../user/user.routes').then((m) => m.userRoutes),
      },
    ],
  },
];
