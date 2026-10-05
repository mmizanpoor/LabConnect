import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@core/services/auth/auth.service';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { PortalLabAccessService } from '@core/services/site/portal-lab-access.service';
import { AzmoonProContextService } from '@core/services/site/azmoon-pro-context.service';

export const authGuard: CanActivateFn = async (_route, state) => {
  const authUtils = inject(AuthUtils);
  const authService = inject(AuthService);
  const router = inject(Router);
  const portalLabAccess = inject(PortalLabAccessService);
  const azmoonPro = inject(AzmoonProContextService);
  const path = state.url.split('?')[0];

  azmoonPro.syncFromUrl(state.url);

  const fromAzmoonProfile =
    azmoonPro.isAzmoonProfileChromePath(state.url) &&
    azmoonPro.isLoggedInFromAzmoonPro(state.url);

  if (path === '/profile/product-listings/new' || fromAzmoonProfile) {
    const portalResult = await portalLabAccess.ensureFromUrl(state.url);
    if (portalResult === 'authenticated') {
      await authService.loadLabPermissionsIfNeeded();
      return true;
    }
    if (portalResult !== 'skipped') {
      return router.createUrlTree(['/home'], {
        queryParams: router.parseUrl(state.url).queryParams,
      });
    }
  }

  if (!authUtils.isAuthenticated) {
    return router.createUrlTree(['/auth/login'], {
      queryParams: { returnUrl: state.url },
    });
  }
  await authService.loadLabPermissionsIfNeeded();
  return true;
};
