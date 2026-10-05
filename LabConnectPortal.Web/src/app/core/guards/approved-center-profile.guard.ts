import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { CenterProfileService } from '@modules/profile/center-profile/center-profile.service';

export const approvedCenterProfileGuard: CanActivateFn = async (_route, state) => {
  const centerProfileService = inject(CenterProfileService);
  const router = inject(Router);

  const result = await centerProfileService.getMyProfile();
  if (result.success && result.data?.isApproved) {
    return true;
  }

  return router.createUrlTree(['/profile/center'], {
    queryParams: router.parseUrl(state.url).queryParams,
  });
};
