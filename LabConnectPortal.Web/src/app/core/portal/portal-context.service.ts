import { Injectable } from '@angular/core';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { CenterProfileService } from '@modules/profile/center-profile/center-profile.service';
import { isStoreCenterType, PortalKind } from './portal.types';

@Injectable({ providedIn: 'root' })
export class PortalContextService {
  private _kind: PortalKind | null = null;
  private _inFlight: Promise<PortalKind> | null = null;

  constructor(
    private _auth: AuthUtils,
    private _centerProfile: CenterProfileService,
  ) {}

  peek(): PortalKind | null {
    return this._kind;
  }

  async resolve(): Promise<PortalKind> {
    if (this._kind) {
      return this._kind;
    }
    if (!this._inFlight) {
      this._inFlight = this.detect();
    }
    try {
      this._kind = await this._inFlight;
      return this._kind;
    } finally {
      this._inFlight = null;
    }
  }

  clear(): void {
    this._kind = null;
    this._inFlight = null;
  }

  private async detect(): Promise<PortalKind> {
    if (this._auth.isStore()) {
      return 'store';
    }
    if (this._auth.isUser() || this._auth.canAccessAdminPortal()) {
      return 'user';
    }
    if (this._auth.isAdminLab() || this._auth.isUserLab()) {
      try {
        const result = await this._centerProfile.getMyProfile();
        if (result.success && isStoreCenterType(result.data?.centerType)) {
          return 'store';
        }
      } catch {
        return 'lab';
      }
      return 'lab';
    }
    return 'user';
  }
}
