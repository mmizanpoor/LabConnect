import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '@env/environment';
import { SystemEntity } from '@core/system-entity/system-entity';
import { SystemEntityContextService } from '@core/system-entity/system-entity-context.service';
import { resolveSystemEntityFromUrl } from '@core/system-entity/system-entity-routes';

function currentAppUrl(router: Router): string {
  const nav = router.getCurrentNavigation();
  if (nav?.finalUrl) {
    return nav.finalUrl.toString();
  }
  if (nav?.extractedUrl) {
    return nav.extractedUrl.toString();
  }
  return router.url;
}

export const systemEntityInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  // These endpoints are intentionally global and do not need SystemEntity scoping.
  if (req.url.includes('/SiteService/') || req.url.includes('/PublicSite/') || req.url.includes('/Public')) {
    return next(req);
  }

  const context = inject(SystemEntityContextService);
  const router = inject(Router);
  const entity =
    context.current ?? resolveSystemEntityFromUrl(currentAppUrl(router));

  if (!entity) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: {
        [SystemEntity.HeaderName]: entity,
      },
    })
  );
};
