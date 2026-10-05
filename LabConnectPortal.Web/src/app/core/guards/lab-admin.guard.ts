import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthUtils } from '@core/services/auth/auth.utils';

export const labAdminGuard: CanActivateFn = () => {
  const authUtils = inject(AuthUtils);
  const router = inject(Router);
  if (authUtils.isAuthenticated && authUtils.isAdminLab()) return true;
  return router.createUrlTree(['/profile']);
};
