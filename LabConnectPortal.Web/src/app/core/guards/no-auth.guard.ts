import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthUtils } from '@core/services/auth/auth.utils';

export const noAuthGuard: CanActivateFn = () => {
  const authUtils = inject(AuthUtils);
  const router = inject(Router);
  if (!authUtils.isAuthenticated) return true;
  if (authUtils.canAccessAdminPortal()) {
    return router.createUrlTree(['/admin/dashboard']);
  }
  if (authUtils.isStore() || authUtils.isLabPortalUser()) {
    return router.createUrlTree(['/profile/dashboard']);
  }
  return router.createUrlTree(['/profile']);
};
