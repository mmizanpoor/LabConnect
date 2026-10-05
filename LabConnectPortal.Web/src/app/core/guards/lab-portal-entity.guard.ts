import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionAction } from '@core/services/auth/lab-permission.types';

export function labPortalEntityGuard(
  entity: string,
  action: LabPermissionAction = 'view'
): CanActivateFn {
  return () => {
    const authUtils = inject(AuthUtils);
    const labPermission = inject(LabPermissionService);
    const router = inject(Router);

    if (!authUtils.isAuthenticated) {
      return router.createUrlTree(['/auth/login']);
    }

    if (authUtils.isAdminLab()) {
      return true;
    }

    if (authUtils.isUserLab() && labPermission.can(entity, action)) {
      return true;
    }

    if (authUtils.canManageCenterProfile()) {
      return true;
    }

    return router.createUrlTree(['/profile']);
  };
}
