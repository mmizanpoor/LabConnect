import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';

export const centerProfileGuard: CanActivateFn = (route) => {
  const authUtils = inject(AuthUtils);
  const labPermission = inject(LabPermissionService);
  const router = inject(Router);

  if (!authUtils.isAuthenticated) {
    return router.createUrlTree(['/auth/login']);
  }

  if (authUtils.canManageCenterProfile()) {
    return true;
  }

  const entity =
    (route.data?.['systemEntity'] as string | undefined) ??
    SystemEntity.CenterProfile;
  if (authUtils.isUserLab() && labPermission.can(entity, 'view')) {
    return true;
  }

  return router.createUrlTree(['/profile']);
};
