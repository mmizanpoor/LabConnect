import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { filterNavGroups } from '@modules/profile/profile-layout/profile-nav.helpers';
import {
  ProfileNavGroup,
  ProfileNavLink,
} from '@modules/profile/profile-layout/profile-nav.types';

export const STORE_DASHBOARD_LINK: ProfileNavLink = {
  route: '/profile/dashboard',
  icon: 'grid_view',
  labelKey: 'modules.profile.layout.dashboard',
  visibility: 'always',
};

export const STORE_PRODUCT_LISTINGS_LINK: ProfileNavLink = {
  route: '/profile/product-listings',
  icon: 'campaign',
  labelKey: 'modules.profile.layout.products',
  systemEntity: SystemEntity.Product,
  requiredAction: 'view',
  visibility: 'always',
};

export const STORE_ACTIVITY_LOGS_LINK: ProfileNavLink = {
  route: '/profile/activity-logs',
  icon: 'history',
  labelKey: 'modules.profile.layout.activityLogs',
  systemEntity: SystemEntity.ActivityLog,
  requiredAction: 'view',
  visibility: 'always',
};

export const STORE_CENTER_PROFILE_LINK: ProfileNavLink = {
  route: '/profile/center',
  icon: 'apartment',
  labelKey: 'modules.profile.layout.centerProfile',
  systemEntity: SystemEntity.CenterProfile,
  requiredAction: 'view',
  visibility: 'always',
};

const STORE_NAV_GROUPS: ProfileNavGroup[] = [
  {
    id: 'apiKeys',
    icon: 'key',
    labelKey: 'shared.apiKeys.title',
    visibility: 'always',
    route: '/profile/api-keys',
    items: [
      {
        route: '/profile/api-keys',
        icon: 'key',
        labelKey: 'shared.apiKeys.title',
        systemEntity: SystemEntity.CenterProfile,
        requiredAction: 'view',
        visibility: 'always',
      },
    ],
  },
];

export function getStoreNavGroups(
  auth: AuthUtils,
  labPermission: LabPermissionService,
): ProfileNavGroup[] {
  return filterNavGroups(STORE_NAV_GROUPS, auth, labPermission);
}
