import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthUtils } from '@core/services/auth/auth.utils';

export const adminProfileRedirectGuard: CanActivateFn = (_route, state) => {
  const authUtils = inject(AuthUtils);
  const router = inject(Router);

  if (!authUtils.isAuthenticated || !authUtils.canAccessAdminPortal()) {
    return true;
  }

  const url = state.url.split('?')[0];
  if (url === '/profile/messages' || url.startsWith('/profile/messages/')) {
    return router.createUrlTree(['/admin/messages']);
  }

  if (url === '/profile' || url.startsWith('/profile/')) {
    return router.createUrlTree(['/admin/account']);
  }

  return true;
};
