import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { AzmoonProContextService } from '@core/services/site/azmoon-pro-context.service';
import { PortalContextService } from '@core/portal/portal-context.service';
import { TokenService } from '@core/services/token/token.service';
import { AuthUtils } from './auth.utils';
import { LabPermissionService } from './lab-permission.service';
import { SitePermissionService } from './site-permission.service';
import {
  ChangePasswordCommand,
  CheckUsernameAvailableCommand,
  ConfirmEmailOtpCommand,
  LoginWithPasswordCommand,
  ProfileDto,
  RefreshTokenCommand,
  RegisterStoreCommand,
  SendEmailOtpCommand,
  SendOtpCommand,
  SendPasswordChangeOtpCommand,
  SendUsernameChangeOtpCommand,
  TokenResult,
  UpdateProfileCommand,
  UpdateUsernameCommand,
  VerifyOtpCommand,
} from './auth.types';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    private _http: ApiHttpService,
    private _tokenService: TokenService,
    private _router: Router,
    private _authUtils: AuthUtils,
    private _labPermission: LabPermissionService,
    private _sitePermission: SitePermissionService,
    private _localization: LocalizationService,
    private _azmoonPro: AzmoonProContextService,
    private _portalContext: PortalContextService,
  ) {}

  async sendOtp(command: SendOtpCommand): Promise<void> {
    const result = await this._http.post('Auth', 'SendOtp', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate('message.auth.sendCodeFailed')
      );
    }
  }

  async verifyOtp(command: VerifyOtpCommand): Promise<void> {
    const result = await this._http.post<TokenResult>(
      'Auth',
      'VerifyOtp',
      command
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('message.auth.invalidCode')
      );
    }
    this._tokenService.accessToken = result.data.accessToken;
    this._tokenService.refreshToken = result.data.refreshToken;
    await this.loadPermissionsAfterLogin();
  }

  async loginApprovedLaboratory(labCodeNew: number): Promise<void> {
    const result = await this._http.post<TokenResult>('Auth', 'LoginApprovedLaboratory', {
      labCodeNew,
    });
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('modules.home.portalLoginConfirm.loginFailed')
      );
    }
    this._tokenService.accessToken = result.data.accessToken;
    this._tokenService.refreshToken = result.data.refreshToken;
    await this.loadPermissionsAfterLogin();
  }

  async ensurePortalLaboratory(labCode: number, labCodeNew: number): Promise<void> {
    const result = await this._http.post<TokenResult>('Auth', 'EnsurePortalLaboratory', {
      labCode,
      labCodeNew,
    });
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('modules.home.portalLoginConfirm.loginFailed')
      );
    }
    this._tokenService.accessToken = result.data.accessToken;
    this._tokenService.refreshToken = result.data.refreshToken;
    await this.loadPermissionsAfterLogin();
  }

  async loginWithPassword(command: LoginWithPasswordCommand): Promise<void> {
    const result = await this._http.post<TokenResult>(
      'Auth',
      'LoginWithPassword',
      command
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('message.auth.invalidUsernameOrPassword')
      );
    }
    this._tokenService.accessToken = result.data.accessToken;
    this._tokenService.refreshToken = result.data.refreshToken;
    await this.loadPermissionsIfNeeded();
  }

  async registerStore(command: RegisterStoreCommand): Promise<void> {
    const result = await this._http.post<TokenResult>(
      'Auth',
      'RegisterStore',
      command
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('message.auth.registerStoreFailed')
      );
    }
    this._tokenService.accessToken = result.data.accessToken;
    this._tokenService.refreshToken = result.data.refreshToken;
    await this.loadPermissionsIfNeeded();
  }

  async getProfile(): Promise<ProfileDto> {
    const result = await this._http.get<ProfileDto>('Auth', 'GetProfile');
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('message.auth.loadProfileFailed')
      );
    }
    return result.data;
  }

  async updateProfile(command: UpdateProfileCommand): Promise<void> {
    const result = await this._http.post('Auth', 'UpdateProfile', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate('message.auth.saveProfileFailed')
      );
    }
  }

  async updateUsername(command: UpdateUsernameCommand): Promise<void> {
    const result = await this._http.post<TokenResult>(
      'Auth',
      'UpdateUsername',
      command
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.profile.accountSecurity.errors.usernameSaveFailed'
          )
      );
    }
    this._tokenService.accessToken = result.data.accessToken;
    this._tokenService.refreshToken = result.data.refreshToken;
    await this.loadPermissionsIfNeeded();
  }

  async sendUsernameChangeOtp(command: SendUsernameChangeOtpCommand): Promise<void> {
    const result = await this._http.post('Auth', 'SendUsernameChangeOtp', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.profile.accountSecurity.errors.usernameOtpSendFailed'
          )
      );
    }
  }

  async checkUsernameAvailable(command: CheckUsernameAvailableCommand): Promise<void> {
    const result = await this._http.post('Auth', 'CheckUsernameAvailable', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.profile.accountSecurity.errors.usernameTaken'
          )
      );
    }
  }

  async sendEmailOtp(command: SendEmailOtpCommand): Promise<void> {
    const result = await this._http.post('Auth', 'SendEmailOtp', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.profile.accountSecurity.errors.emailOtpSendFailed'
          )
      );
    }
  }

  async confirmEmailOtp(command: ConfirmEmailOtpCommand): Promise<void> {
    const result = await this._http.post('Auth', 'ConfirmEmailOtp', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.profile.accountSecurity.errors.emailOtpConfirmFailed'
          )
      );
    }
  }

  async changePassword(command: ChangePasswordCommand): Promise<void> {
    const result = await this._http.post('Auth', 'ChangePassword', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.profile.accountSecurity.errors.passwordSaveFailed'
          )
      );
    }
  }

  async sendPasswordChangeOtp(command: SendPasswordChangeOtpCommand): Promise<void> {
    const result = await this._http.post('Auth', 'SendPasswordChangeOtp', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.profile.accountSecurity.errors.passwordOtpSendFailed'
          )
      );
    }
  }

  async refresh(): Promise<void> {
    const refreshToken = this._tokenService.refreshToken;
    if (!refreshToken) {
      throw new Error(
        this._localization.translate('message.auth.noRefreshToken')
      );
    }
    const command: RefreshTokenCommand = { refreshToken };
    const result = await this._http.post<TokenResult>(
      'Auth',
      'Refresh',
      command
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('message.auth.refreshSessionFailed')
      );
    }
    this._tokenService.accessToken = result.data.accessToken;
    this._tokenService.refreshToken = result.data.refreshToken;
    await this.loadPermissionsIfNeeded();
  }

  async logout(): Promise<void> {
    await this.clearSession();
    await this._router.navigate(['/auth/login'], {
      queryParams: {},
      replaceUrl: true,
    });
  }

  /** Clears tokens (and best-effort server logout) without navigating. */
  async clearSession(): Promise<void> {
    try {
      await this._http.post('Auth', 'Logout', null);
    } catch {
      // Ignore API logout failures; local tokens are always cleared.
    } finally {
      this._tokenService.clear();
      this._labPermission.clear();
      this._sitePermission.clear();
      this._azmoonPro.clearCapturedUrlParams();
      this._portalContext.clear();
      try {
        sessionStorage.removeItem('labconnect.azmoonAuthenticatedLabCodeNew');
      } catch {
        // Ignore storage failures in restricted iframe contexts.
      }
    }
  }

  async loginAndNavigate(
    command: VerifyOtpCommand,
    returnUrl?: string | null
  ): Promise<void> {
    await this.verifyOtp(command);
    await this.navigateAfterLogin(returnUrl);
  }

  async loginWithPasswordAndNavigate(
    command: LoginWithPasswordCommand,
    returnUrl?: string | null
  ): Promise<void> {
    await this.loginWithPassword(command);
    await this.navigateAfterLogin(returnUrl);
  }

  async registerStoreAndNavigate(
    command: RegisterStoreCommand,
    returnUrl?: string | null
  ): Promise<void> {
    await this.registerStore(command);
    await this.navigateAfterLogin(returnUrl);
  }

  /** Redirect after auth: honor returnUrl, else role default home. */
  async navigateAfterLogin(returnUrl?: string | null): Promise<void> {
    const safeReturnUrl = this.getSafeReturnUrl(returnUrl);
    if (safeReturnUrl) {
      await this._router.navigateByUrl(safeReturnUrl);
      return;
    }
    if (this._authUtils.canAccessAdminPortal()) {
      await this._router.navigate(['/admin/dashboard']);
      return;
    }
    if (this._authUtils.isStore() || this._authUtils.isLabPortalUser()) {
      await this._router.navigate(['/profile/dashboard']);
      return;
    }
    await this._router.navigate(['/profile']);
  }

  private getSafeReturnUrl(returnUrl?: string | null): string | null {
    if (
      !returnUrl ||
      !returnUrl.startsWith('/') ||
      returnUrl.startsWith('//')
    ) {
      return null;
    }
    if (returnUrl.startsWith('/auth/')) {
      return null;
    }
    return returnUrl;
  }

  async loadPermissionsIfNeeded(): Promise<void> {
    await Promise.all([
      this.loadLabPermissionsIfNeeded(),
      this.loadSitePermissionsIfNeeded(),
    ]);
  }

  async loadLabPermissionsIfNeeded(): Promise<void> {
    if (this._authUtils.isUserLab()) {
      await this._labPermission.load();
      return;
    }
    this._labPermission.clear();
  }

  async loadSitePermissionsIfNeeded(): Promise<void> {
    if (this._authUtils.isAdmin()) {
      await this._sitePermission.load();
      return;
    }
    this._sitePermission.clear();
  }

  private async loadPermissionsAfterLogin(): Promise<void> {
    try {
      await this.loadPermissionsIfNeeded();
    } catch {
      this._labPermission.clear();
      this._sitePermission.clear();
    }
  }
}
