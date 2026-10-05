import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthUtils } from '@core/services/auth/auth.utils';

export const resumeGuard: CanActivateFn = () => {
  const authUtils = inject(AuthUtils);
  const router = inject(Router);
  if (authUtils.isAuthenticated && authUtils.canAccessResume()) return true;
  return router.createUrlTree(['/profile']);
};
