import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthUtils } from '@core/services/auth/auth.utils';

export const adminGuard: CanActivateFn = () => {
  const authUtils = inject(AuthUtils);
  const router = inject(Router);
  if (authUtils.isAuthenticated && authUtils.canAccessAdminPortal()) return true;
  if (authUtils.isAuthenticated) {
    return router.createUrlTree(['/profile']);
  }
  return router.createUrlTree(['/auth/login']);
};
