import { Injectable, inject } from '@angular/core';
import { Params, Router } from '@angular/router';
import { AuthService } from '@core/services/auth/auth.service';
import { AuthUtils } from '@core/services/auth/auth.utils';
import {
  AzmoonProContextService,
  parseAzmoonProQuery,
} from '@core/services/site/azmoon-pro-context.service';
import { PublicLaboratoryService } from '@core/services/site/public-laboratory.service';

const AUTHED_LAB_KEY = 'labconnect.azmoonAuthenticatedLabCodeNew';

export type PortalLabAccessResult =
  | 'skipped'
  | 'authenticated'
  | 'pending'
  | 'registered'
  | 'cancelled'
  | 'unresolved';

@Injectable({ providedIn: 'root' })
export class PortalLabAccessService {
  private _router = inject(Router);
  private _authService = inject(AuthService);
  private _authUtils = inject(AuthUtils);
  private _azmoonPro = inject(AzmoonProContextService);
  private _publicLaboratory = inject(PublicLaboratoryService);
  private _inFlight: Promise<PortalLabAccessResult> | null = null;

  async ensureFromUrl(url?: string): Promise<PortalLabAccessResult> {
    const tree = this._router.parseUrl(url ?? this._router.url);
    return this.ensure(tree.queryParams);
  }

  async ensure(queryParams?: Params): Promise<PortalLabAccessResult> {
    if (this._inFlight) {
      return this._inFlight;
    }

    this._inFlight = this.run(queryParams).finally(() => {
      this._inFlight = null;
    });
    return this._inFlight;
  }

  private async run(queryParams?: Params): Promise<PortalLabAccessResult> {
    const params = queryParams ?? this._router.parseUrl(this._router.url).queryParams;
    const azmoonPro = parseAzmoonProQuery(params);
    const labCodeNewFromQuery = Number(azmoonPro.labCodeNew);
    const labCodeNew = Number.isFinite(labCodeNewFromQuery) && labCodeNewFromQuery > 0
      ? labCodeNewFromQuery
      : this._azmoonPro.isLoggedInFromAzmoonPro()
        ? Number(this._azmoonPro.labCodeNew)
        : Number.NaN;
    const labCode = Number(azmoonPro.labCode);
    if (!Number.isFinite(labCodeNew) || labCodeNew <= 0) {
      return 'skipped';
    }

    try {
      const lookupResult = await this._publicLaboratory.getByLabCodeNew(labCodeNew);
      const lookup = lookupResult.success ? lookupResult.data : null;

      if (lookup) {
        return this.loginLaboratory(labCodeNew);
      }

      if (!Number.isFinite(labCode) || labCode <= 0) {
        return 'unresolved';
      }

      await this._authService.ensurePortalLaboratory(labCode, labCodeNew);
      this.writeAuthedLabCodeNew(String(labCodeNew));
      return 'authenticated';
    } catch {
      return 'unresolved';
    }
  }

  private async loginLaboratory(labCodeNew: number): Promise<PortalLabAccessResult> {
    const alreadyForThisLab =
      this._authUtils.isAuthenticated &&
      this.readAuthedLabCodeNew() === String(labCodeNew) &&
      this._authUtils.isLabPortalUser();

    if (alreadyForThisLab) {
      return 'authenticated';
    }

    await this._authService.loginApprovedLaboratory(labCodeNew);
    this.writeAuthedLabCodeNew(String(labCodeNew));
    return 'authenticated';
  }

  private readAuthedLabCodeNew(): string {
    try {
      return sessionStorage.getItem(AUTHED_LAB_KEY)?.trim() ?? '';
    } catch {
      return '';
    }
  }

  private writeAuthedLabCodeNew(value: string): void {
    try {
      sessionStorage.setItem(AUTHED_LAB_KEY, value);
    } catch {
      // Ignore storage failures in restricted iframe contexts.
    }
  }
}
