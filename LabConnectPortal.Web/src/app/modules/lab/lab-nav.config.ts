import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { filterNavGroups } from '@modules/profile/profile-layout/profile-nav.helpers';
import {
  ProfileNavGroup,
  ProfileNavLink,
} from '@modules/profile/profile-layout/profile-nav.types';

export const LAB_DASHBOARD_LINK: ProfileNavLink = {
  route: '/profile/dashboard',
  icon: 'grid_view',
  labelKey: 'modules.profile.layout.dashboard',
  visibility: 'always',
};

export const LAB_PRODUCT_LISTINGS_LINK: ProfileNavLink = {
  route: '/profile/product-listings',
  icon: 'campaign',
  labelKey: 'modules.profile.layout.products',
  systemEntity: SystemEntity.Product,
  requiredAction: 'view',
  visibility: 'canManageCenterProfile',
};

export const LAB_ACTIVITY_LOGS_LINK: ProfileNavLink = {
  route: '/profile/activity-logs',
  icon: 'history',
  labelKey: 'modules.profile.layout.activityLogs',
  systemEntity: SystemEntity.ActivityLog,
  requiredAction: 'view',
  visibility: 'canViewActivityLogs',
};

export const LAB_CENTER_PROFILE_LINK: ProfileNavLink = {
  route: '/profile/center',
  icon: 'apartment',
  labelKey: 'modules.profile.layout.centerProfile',
  systemEntity: SystemEntity.CenterProfile,
  requiredAction: 'view',
  visibility: 'canManageCenterProfile',
};

const LAB_NAV_GROUPS: ProfileNavGroup[] = [
  {
    id: 'apiKeys',
    icon: 'key',
    labelKey: 'shared.apiKeys.title',
    visibility: 'canManageCenterProfile',
    route: '/profile/api-keys',
    items: [
      {
        route: '/profile/api-keys',
        icon: 'key',
        labelKey: 'shared.apiKeys.title',
        systemEntity: SystemEntity.CenterProfile,
        requiredAction: 'view',
        visibility: 'canManageCenterProfile',
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
      },
    ],
  },
  {
    id: 'special-offers',
    icon: 'local_offer',
    labelKey: 'modules.profile.layout.specialOffers',
    visibility: 'canManageCenterProfile',
    route: '/profile/special-offers',
    items: [
      {
        route: '/profile/special-offers',
        icon: 'local_offer',
        labelKey: 'modules.profile.layout.specialOffers',
        systemEntity: SystemEntity.SpecialOffer,
        requiredAction: 'view',
      },
    ],
  },
  {
    id: 'testInfos',
    icon: 'science',
    labelKey: 'modules.profile.layout.testInfos',
    visibility: 'labPortalAccess',
    route: '/profile/test-infos',
    items: [
      {
        route: '/profile/test-infos',
        icon: 'science',
        labelKey: 'modules.profile.layout.testInfos',
        systemEntity: SystemEntity.TestInfo,
        requiredAction: 'view',
      },
    ],
  },
  {
    id: 'agreements',
    icon: 'description',
    labelKey: 'modules.profile.layout.agreements',
    visibility: 'canViewAgreements',
    route: '/profile/agreements',
    items: [
      {
        route: '/profile/agreements',
        icon: 'description',
        labelKey: 'modules.profile.layout.agreements',
        systemEntity: SystemEntity.LabAgreement,
        requiredAction: 'view',
        visibility: 'canViewAgreements',
      },
    ],
  },
  {
    id: 'agreement-settings',
    icon: 'tune',
    labelKey: 'modules.profile.layout.agreementSettings',
    visibility: 'canViewAgreements',
    route: '/profile/agreement-settings',
    items: [
      {
        route: '/profile/agreement-settings',
        icon: 'tune',
        labelKey: 'modules.profile.layout.agreementSettings',
        systemEntity: SystemEntity.LabAgreement,
        requiredAction: 'view',
        visibility: 'canViewAgreements',
      },
    ],
  },
];

export function getLabNavGroups(
  auth: AuthUtils,
  labPermission: LabPermissionService,
): ProfileNavGroup[] {
  return filterNavGroups(LAB_NAV_GROUPS, auth, labPermission);
}
