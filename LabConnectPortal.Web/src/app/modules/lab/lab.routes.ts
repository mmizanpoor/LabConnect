import { Routes } from '@angular/router';
import { pendingCenterProfileGuard } from '@core/guards/pending-center-profile.guard';
import { labAgreementGuard } from '@core/guards/lab-agreement.guard';
import { labPortalEntityGuard } from '@core/guards/lab-portal-entity.guard';
import { SystemEntity } from '@core/system-entity/system-entity';
import { ProfileLayoutComponent } from '../profile/profile-layout/profile-layout.component';
import {
  profileAccountRoutes,
  profileActivityLogsRoute,
  profileApiKeysRoute,
  profileCenterRoutes,
  profileHomeRoute,
  profileJobPostingRoutes,
  profileLabMessagesRoute,
  profileProductRoutes,
  profileResumeRoute,
} from '../profile/profile-children.routes';

export const labRoutes: Routes = [
  {
    path: '',
    component: ProfileLayoutComponent,
    data: { portal: 'lab' },
    canActivate: [pendingCenterProfileGuard],
    canActivateChild: [pendingCenterProfileGuard],
    children: [
      ...profileHomeRoute,
      ...profileResumeRoute,
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
          import('../profile/dashboard/lab-dashboard.component').then(
            (m) => m.LabDashboardComponent,
          ),
      },
      {
        path: 'lab-users',
        loadChildren: () =>
          import('../profile/lab-users/lab-users.routes').then((m) => m.labUsersRoutes),
      },
      {
        path: 'agreement-settings',
        canActivate: [labAgreementGuard, labPortalEntityGuard(SystemEntity.LabAgreement, 'view')],
        data: { systemEntity: SystemEntity.LabAgreement },
        loadComponent: () =>
          import('../profile/agreement-settings/agreement-settings.component').then(
            (m) => m.AgreementSettingsComponent,
          ),
      },
      {
        path: 'agreements/new',
        canActivate: [labAgreementGuard, labPortalEntityGuard(SystemEntity.LabAgreement, 'create')],
        data: { systemEntity: SystemEntity.LabAgreement },
        loadComponent: () =>
          import('../profile/agreements/agreement-form.component').then(
            (m) => m.AgreementFormComponent,
          ),
      },
      {
        path: 'agreements',
        loadChildren: () =>
          import('../profile/agreements/agreements.routes').then((m) => m.agreementsRoutes),
      },
      {
        path: 'device-groups',
        loadChildren: () =>
          import('../profile/device-groups/device-groups.routes').then((m) => m.deviceGroupsRoutes),
      },
      {
        path: 'kit-groups',
        loadChildren: () =>
          import('../profile/kit-groups/kit-groups.routes').then((m) => m.kitGroupsRoutes),
      },
      {
        path: 'test-infos',
        loadChildren: () =>
          import('../profile/test-infos/test-infos.routes').then((m) => m.testInfosRoutes),
      },
      {
        path: 'special-offers',
        loadChildren: () =>
          import('../profile/special-offers/special-offers.routes').then(
            (m) => m.specialOffersRoutes,
          ),
      },
    ],
  },
];
