import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthService } from '@core/services/auth/auth.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { PublicSiteService } from '@core/services/site/public-site.service';
import { SiteContextService } from '@core/services/site/site-context.service';
import { LoginUserType } from '@core/services/auth/auth.types';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { SiteSettingsPublicDto } from '@modules/admin/settings/site-settings.types';
import { LoginStep } from './login.types';

const OTP_VALIDITY_SECONDS = 120;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnInit, OnDestroy {
  step: LoginStep = 'mobile';
  loading = false;
  error = '';
  success = '';
  remainingSeconds = 0;
  siteSettings: SiteSettingsPublicDto | null = null;

  private _countdownTimer: ReturnType<typeof setInterval> | null = null;
  private _loginUserType: LoginUserType = LoginUserType.User;
  private _labCode: number | null = null;
  private _labCodeNew: number | null = null;
  private _autoOtpTriggered = false;

  form = new FormGroup({
    mobileNumber: new FormControl('', [
      Validators.required,
      Validators.pattern(/^09\d{9}$/),
    ]),
    code: new FormControl('', [
      Validators.required,
      Validators.pattern(/^\d{6}$/),
    ]),
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required]),
  });

  constructor(
    private _authService: AuthService,
    private _localization: LocalizationService,
    private _route: ActivatedRoute,
    private _authUtils: AuthUtils,
    private _siteContext: SiteContextService
  ) {}

  get siteTitle(): string {
    return this.siteSettings?.siteTitle?.trim() || 'LabConnect';
  }

  get logoUrl(): string | null {
    return this.siteSettings?.hasLogo ? PublicSiteService.logoUrl() : null;
  }

  ngOnInit(): void {
    void this.loadSiteSettings();
    this.applyLabPortalQueryParams();

    if (this._authUtils.isAuthenticated) {
      void this._authService.navigateAfterLogin(
        this._route.snapshot.queryParamMap.get('returnUrl')
      );
      return;
    }

    if (this._autoOtpTriggered) {
      void this.requestOtp(false);
    }
  }

  private applyLabPortalQueryParams(): void {
    const params = this._route.snapshot.queryParamMap;
    const mobileNumber = this.normalizeMobileNumber(params.get('mobileNumber'));
    const labCode = Number(params.get('labCode'));
    const labCodeNew = Number(params.get('labCodeNew'));

    if (
      !mobileNumber ||
      !Number.isFinite(labCode) ||
      labCode <= 0 ||
      !Number.isFinite(labCodeNew) ||
      labCodeNew <= 0
    ) {
      return;
    }

    this._labCode = labCode;
    this._labCodeNew = labCodeNew;
    this._loginUserType = this.parseIsAdminLab(params.get('isAdminLab'))
      ? LoginUserType.AdminLab
      : LoginUserType.UserLab;

    this.form.controls.mobileNumber.setValue(mobileNumber);
    this._autoOtpTriggered = true;
  }

  private normalizeMobileNumber(value: string | null): string | null {
    if (!value?.trim()) return null;
    let mobile = value.trim().replace(/[\s\-]/g, '');
    if (mobile.startsWith('+98')) mobile = `0${mobile.slice(3)}`;
    else if (mobile.startsWith('98') && mobile.length === 12)
      mobile = `0${mobile.slice(2)}`;
    return /^09\d{9}$/.test(mobile) ? mobile : null;
  }

  private parseIsAdminLab(value: string | null): boolean {
    if (!value?.trim()) return false;
    const normalized = value.trim().toLowerCase();
    return normalized === 'true' || normalized === '1' || normalized === 'yes';
  }

  async loadSiteSettings(): Promise<void> {
    try {
      this.siteSettings = await this._siteContext.ensureLoaded();
    } catch {
      // Login still renders with defaults when settings fail to load.
    }
  }

  ngOnDestroy(): void {
    this.stopCountdown();
  }

  get isOtpExpired(): boolean {
    return this.step === 'otp' && this.remainingSeconds <= 0;
  }

  get remainingTimeLabel(): string {
    const minutes = Math.floor(this.remainingSeconds / 60);
    const seconds = this.remainingSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  async sendOtp(): Promise<void> {
    await this.requestOtp(false);
  }

  async resendOtp(): Promise<void> {
    await this.requestOtp(true);
  }

  async verifyOtp(): Promise<void> {
    this.error = '';
    this.success = '';
    const normalizedCode = this.normalizeOtpCode(
      this.form.controls.code.value
    );
    this.form.controls.code.setValue(normalizedCode);
    if (this.isOtpExpired) {
      this.error = this._localization.translate(
        'message.auth.otpExpiredResend'
      );
      return;
    }
    if (this.form.controls.code.invalid) {
      this.form.controls.code.markAsTouched();
      return;
    }
    this.loading = true;
    try {
      await this._authService.loginAndNavigate(
        {
          mobileNumber: this.form.controls.mobileNumber.value!,
          code: normalizedCode,
          loginUserType: this._loginUserType,
        },
        this._route.snapshot.queryParamMap.get('returnUrl')
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('message.auth.invalidCode');
    } finally {
      this.loading = false;
    }
  }

  async loginWithPassword(): Promise<void> {
    this.error = '';
    this.success = '';
    const usernameControl = this.form.controls.username;
    const passwordControl = this.form.controls.password;

    if (usernameControl.invalid || passwordControl.invalid) {
      usernameControl.markAsTouched();
      passwordControl.markAsTouched();
      return;
    }

    this.loading = true;
    try {
      await this._authService.loginWithPasswordAndNavigate(
        {
          username: usernameControl.value!.trim(),
          password: passwordControl.value!,
        },
        this._route.snapshot.queryParamMap.get('returnUrl')
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'message.auth.invalidUsernameOrPassword'
            );
    } finally {
      this.loading = false;
    }
  }

  goBackToMobile(): void {
    this.stopCountdown();
    this.remainingSeconds = 0;
    this.success = '';
    this.error = '';
    this.form.controls.code.reset('');
    this.step = 'mobile';
  }

  goToPasswordLogin(): void {
    this.stopCountdown();
    this.remainingSeconds = 0;
    this.success = '';
    this.error = '';
    this.form.controls.code.reset('');
    this.step = 'password';
  }

  private async requestOtp(isResend: boolean): Promise<void> {
    this.error = '';
    this.success = '';
    if (this.form.controls.mobileNumber.invalid) {
      this.form.controls.mobileNumber.markAsTouched();
      return;
    }
    this.loading = true;
    try {
      await this._authService.sendOtp({
        mobileNumber: this.form.controls.mobileNumber.value!,
        loginUserType: this._loginUserType,
        labCode: this._labCode,
        labCodeNew: this._labCodeNew,
      });
      this.form.controls.code.reset('');
      this.step = 'otp';
      this.startCountdown();
      this.success = isResend
        ? this._localization.translate('modules.auth.login.success.codeResent')
        : this._localization.translate('modules.auth.login.success.codeSent');
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('message.auth.sendCodeFailed');
    } finally {
      this.loading = false;
    }
  }

  private startCountdown(): void {
    this.stopCountdown();
    this.remainingSeconds = OTP_VALIDITY_SECONDS;
    this._countdownTimer = setInterval(() => {
      if (this.remainingSeconds > 0) {
        this.remainingSeconds--;
      } else {
        this.stopCountdown();
      }
    }, 1000);
  }

  private normalizeOtpCode(value: string | null): string {
    return (value ?? '')
      .trim()
      .replace(/[۰-۹]/g, (digit) =>
        String.fromCharCode(digit.charCodeAt(0) - 1728)
      )
      .replace(/[٠-٩]/g, (digit) =>
        String.fromCharCode(digit.charCodeAt(0) - 1584)
      )
      .replace(/[\s-]/g, '');
  }

  private stopCountdown(): void {
    if (this._countdownTimer) {
      clearInterval(this._countdownTimer);
      this._countdownTimer = null;
    }
  }
}
