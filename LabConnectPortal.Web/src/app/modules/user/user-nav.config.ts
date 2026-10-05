import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { filterNavGroups } from '@modules/profile/profile-layout/profile-nav.helpers';
import { ProfileNavGroup } from '@modules/profile/profile-layout/profile-nav.types';

const USER_NAV_GROUPS: ProfileNavGroup[] = [
  {
    id: 'profile',
    icon: 'badge',
    labelKey: 'modules.profile.layout.profile',
    visibility: 'always',
    route: '/profile',
    items: [
      {
        route: '/profile',
        icon: 'badge',
        labelKey: 'modules.profile.layout.profile',
        exact: true,
      },
    ],
  },
  {
    id: 'resume',
    icon: 'description',
    labelKey: 'modules.profile.resume.title',
    visibility: 'canAccessResume',
    route: '/profile/resume',
    items: [
      {
        route: '/profile/resume',
        icon: 'description',
        labelKey: 'modules.profile.resume.title',
        visibility: 'canAccessResume',
        exact: true,
      },
    ],
  },
  {
    id: 'resume-applications',
    icon: 'assignment',
    labelKey: 'modules.profile.layout.requestList',
    visibility: 'canAccessResume',
    route: '/profile/resume-applications',
    items: [
      {
        route: '/profile/resume-applications',
        icon: 'assignment',
        labelKey: 'modules.profile.layout.requestList',
        visibility: 'canAccessResume',
      },
    ],
  },
];

export function getUserNavGroups(
  auth: AuthUtils,
  labPermission: LabPermissionService,
): ProfileNavGroup[] {
  return filterNavGroups(USER_NAV_GROUPS, auth, labPermission);
}
