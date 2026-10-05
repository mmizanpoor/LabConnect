import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import {
  ProfileNavGroup,
  ProfileNavLink,
  ProfileNavVisibility,
} from './profile-nav.types';

export function isNavVisible(
  visibility: ProfileNavVisibility,
  auth: AuthUtils,
  labPermission: LabPermissionService,
): boolean {
  switch (visibility) {
    case 'always':
      return true;
    case 'isPersonalProfile':
      return !auth.isAdminLab() && !auth.isStore() && !auth.isUserLab();
    case 'canAccessResume':
      return auth.canAccessResume();
    case 'canManageCenterProfile':
      return (
        auth.canManageCenterProfile() ||
        (auth.isUserLab() && hasAnyBusinessPermission(labPermission))
      );
    case 'isAdminLab':
      return auth.isAdminLab();
    case 'canViewReceptions':
      return auth.isAdminLab() || labPermission.can(SystemEntity.Reception, 'view');
    case 'canViewAgreements':
      return auth.isAdminLab() || labPermission.can(SystemEntity.LabAgreement, 'view');
    case 'canViewLabServices':
      return (
        auth.isAdminLab() ||
        labPermission.can(SystemEntity.Reception, 'view') ||
        labPermission.can(SystemEntity.LabAgreement, 'view')
      );
    case 'canViewActivityLogs':
      return (
        auth.isAdminLab() ||
        auth.isStore() ||
        labPermission.can(SystemEntity.ActivityLog, 'view')
      );
    case 'labPortalAccess':
      return auth.isAdminLab() || labPermission.hasAnyLabPortalAccess();
    default:
      return false;
  }
}

export function isProfileLinkVisible(
  link: ProfileNavLink,
  auth: AuthUtils,
  labPermission: LabPermissionService,
): boolean {
  return isLinkVisible(link, link.visibility ?? 'always', auth, labPermission);
}

export function filterNavGroups(
  defs: ProfileNavGroup[],
  auth: AuthUtils,
  labPermission: LabPermissionService,
): ProfileNavGroup[] {
  const seenRoutes = new Set<string>();
  return defs
    .map((group) => ({
      ...group,
      items: group.items.filter((item) =>
        isLinkVisible(item, group.visibility, auth, labPermission),
      ),
    }))
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => {
        if (seenRoutes.has(item.route)) {
          return false;
        }
        seenRoutes.add(item.route);
        return true;
      }),
    }))
    .filter(
      (group) =>
        isNavVisible(group.visibility, auth, labPermission) && group.items.length > 0,
    );
}

function isLinkVisible(
  link: ProfileNavLink,
  groupVisibility: ProfileNavVisibility,
  auth: AuthUtils,
  labPermission: LabPermissionService,
): boolean {
  const visibility = link.visibility ?? groupVisibility;
  if (!isNavVisible(visibility, auth, labPermission)) {
    return false;
  }

  if (link.systemEntity) {
    if (auth.isAdminLab() || auth.isStore()) {
      return true;
    }
    if (
      auth.canManageCenterProfile() &&
      (visibility === 'canManageCenterProfile' || visibility === 'canViewActivityLogs')
    ) {
      return true;
    }
    return labPermission.can(link.systemEntity, link.requiredAction ?? 'view');
  }

  return true;
}

function hasAnyBusinessPermission(labPermission: LabPermissionService): boolean {
  return [
    SystemEntity.CenterProfile,
    SystemEntity.Location,
    SystemEntity.JobPosting,
    SystemEntity.Product,
    SystemEntity.SpecialOffer,
  ].some((entity) => labPermission.can(entity, 'view'));
}
