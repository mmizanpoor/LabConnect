import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { SitePermissionAction } from '@core/services/auth/site-permission.types';

export function siteAdminEntityGuard(
  entity: string,
  action: SitePermissionAction = 'view'
): CanActivateFn {
  return () => {
    const authUtils = inject(AuthUtils);
    const sitePermission = inject(SitePermissionService);
    const router = inject(Router);

    if (!authUtils.isAuthenticated) {
      return router.createUrlTree(['/auth/login']);
    }

    if (authUtils.isAdministrator()) {
      return true;
    }

    if (authUtils.isAdmin() && sitePermission.can(entity, action)) {
      return true;
    }

    return router.createUrlTree(['/admin/dashboard']);
  };
}

/** Only full site owner (Administrator). */
export const administratorOnlyGuard: CanActivateFn = () => {
  const authUtils = inject(AuthUtils);
  const router = inject(Router);
  if (authUtils.isAuthenticated && authUtils.isAdministrator()) return true;
  if (authUtils.isAuthenticated) {
    return router.createUrlTree(['/admin/dashboard']);
  }
  return router.createUrlTree(['/auth/login']);
};
