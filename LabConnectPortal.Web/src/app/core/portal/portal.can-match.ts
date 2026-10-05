import { inject } from '@angular/core';
import { CanMatchFn } from '@angular/router';
import { PortalContextService } from './portal-context.service';
import { PortalKind } from './portal.types';

function portalCanMatch(kind: PortalKind): CanMatchFn {
  return async () => (await inject(PortalContextService).resolve()) === kind;
}

export const labPortalCanMatch = portalCanMatch('lab');
export const storePortalCanMatch = portalCanMatch('store');
export const userPortalCanMatch = portalCanMatch('user');
