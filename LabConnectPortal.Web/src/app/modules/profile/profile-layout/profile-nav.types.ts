import { LabPermissionAction } from '@core/services/auth/lab-permission.types';

export type ProfileNavVisibility =
  | 'always'
  | 'isPersonalProfile'
  | 'canAccessResume'
  | 'canManageCenterProfile'
  | 'isAdminLab'
  | 'canViewReceptions'
  | 'canViewAgreements'
  | 'canViewLabServices'
  | 'labPortalAccess'
  | 'canViewActivityLogs';

export interface ProfileNavLink {
  route: string;
  icon: string;
  labelKey: string;
  exact?: boolean;
  systemEntity?: string;
  requiredAction?: LabPermissionAction;
  visibility?: ProfileNavVisibility;
}

export interface ProfileNavGroup {
  id: string;
  icon: string;
  labelKey: string;
  visibility: ProfileNavVisibility;
  route?: string;
  items: ProfileNavLink[];
}
