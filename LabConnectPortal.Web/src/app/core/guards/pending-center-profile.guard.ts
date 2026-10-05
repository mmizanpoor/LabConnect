import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { CenterProfileService } from '@modules/profile/center-profile/center-profile.service';

/** When laboratory center is Pending/Suspended, only /profile/center is allowed. */
export const pendingCenterProfileGuard: CanActivateFn = async (_route, state) => {
  const authUtils = inject(AuthUtils);
  const router = inject(Router);
  const centerProfileService = inject(CenterProfileService);

  if (!authUtils.isAuthenticated || !authUtils.isAdminLab()) {
    return true;
  }

  const url = state.url.split('?')[0];
  if (url === '/profile/center' || url.startsWith('/profile/center/')) {
    return true;
  }

  const result = await centerProfileService.getMyProfile();
  const status = result.data?.status;
  if (status === 'Pending' || status === 'Suspended' || status === 0 || status === 2) {
    return router.createUrlTree(['/profile/center'], {
      queryParams: router.parseUrl(state.url).queryParams,
    });
  }

  return true;
};
