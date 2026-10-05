import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';

export const labAgreementGuard: CanActivateFn = () => {
  const authUtils = inject(AuthUtils);
  const labPermission = inject(LabPermissionService);
  const router = inject(Router);
  if (
    authUtils.isAuthenticated &&
    (authUtils.isAdminLab() ||
      labPermission.can(SystemEntity.LabAgreement, 'view'))
  ) {
    return true;
  }
  return router.createUrlTree(['/profile']);
};
