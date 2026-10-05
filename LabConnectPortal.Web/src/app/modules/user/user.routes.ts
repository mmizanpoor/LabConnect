import { Routes } from '@angular/router';
import { ProfileLayoutComponent } from '@modules/profile/profile-layout/profile-layout.component';
import {
  profileAccountRoutes,
  profileHomeRoute,
  profileResumeRoute,
  profileResumeApplicationsRoute,
  profileUserMessagesRoute,
} from '@modules/profile/profile-children.routes';

export const userRoutes: Routes = [
  {
    path: '',
    component: ProfileLayoutComponent,
    data: { portal: 'user' },
    children: [
      ...profileHomeRoute,
      ...profileResumeRoute,
      ...profileResumeApplicationsRoute,
      ...profileAccountRoutes,
      ...profileUserMessagesRoute,
    ],
  },
];
