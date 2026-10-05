import { Component, OnDestroy } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthService } from '@core/services/auth/auth.service';
import { LoginUserType } from '@core/services/auth/auth.types';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { LoginStep } from '@modules/auth/login/login.types';

const OTP_VALIDITY_SECONDS = 120;

@Component({
  selector: 'app-contact-login-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  host: { class: 'block overflow-hidden' },
  template: `
    <div class="overflow-hidden bg-white" dir="rtl">
      <header class="flex items-center justify-between border-b border-gray-200 bg-white px-5 py-4">
        <h2 class="m-0 text-base font-bold text-gray-900">
          {{ 'modules.auth.login.title' | transloco }}
        </h2>
        <button
          type="button"
          class="inline-flex h-8 w-8 items-center justify-center rounded-full border-0 bg-transparent text-gray-400 transition hover:bg-gray-100 hover:text-gray-600"
          (click)="closeDialog(false)"
          [attr.aria-label]="'shared.close' | transloco"
        >
          <mat-icon class="!h-5 !w-5 !text-[20px]">close</mat-icon>
        </button>
      </header>

      <div class="px-6 pb-6 pt-5 sm:px-8 sm:pb-8 sm:pt-6">
        @if (error) {
          <div class="mb-4 rounded-xl border border-red-100 bg-red-50 px-4 py-3 text-sm text-red-700">
            {{ error }}
          </div>
        }
        @if (success) {
          <div class="mb-4 rounded-xl border border-green-100 bg-green-50 px-4 py-3 text-sm text-green-700">
            {{ success }}
          </div>
        }

        <form [formGroup]="form" class="flex flex-col gap-4">
          @if (step === 'mobile') {
            <div class="mb-2 text-center sm:text-right">
              <p class="m-0 text-base font-bold text-gray-900">
                {{ 'modules.products.contactLogin.mobilePrompt' | transloco }}
              </p>
              <p class="m-0 mt-2 text-sm text-gray-500">
                {{ 'modules.products.contactLogin.mobileHint' | transloco }}
              </p>
            </div>

            <base-form-field
              [label]="'shared.mobileNumber' | transloco"
              [floatLabel]="true"
              placeholder="09123456789"
              [control]="form.controls.mobileNumber"
              errorMessage="message.validation.invalidMobile"
            />

            <base-button [color]="'primary'" [disabled]="loading" (click)="sendOtp()">
              {{ 'modules.auth.login.sendVerificationCode' | transloco }}
            </base-button>

            <div class="mt-1 flex flex-col items-center gap-3 border-t border-gray-100 pt-4">
              <a
                href="#"
                class="text-sm font-medium text-[#6b849e] no-underline transition-colors hover:text-[#35507c] hover:no-underline"
                (click)="goToPasswordLogin(); $event.preventDefault()"
              >
                {{ 'modules.auth.login.loginWithPassword' | transloco }}
              </a>

              <p class="m-0 text-center text-xs leading-6 text-gray-500">
                {{ 'modules.products.contactLogin.termsPrefix' | transloco }}
                <a
                  href="/company-regulations"
                  class="font-semibold text-[#35507c] underline decoration-[#35507c]/40 underline-offset-2 transition hover:text-[#2a4063]"
                  (click)="goToCompanyRegulations($event)"
                >
                  {{ 'modules.products.contactLogin.termsLink' | transloco }}
                </a>
                {{ 'modules.products.contactLogin.termsSuffix' | transloco }}
              </p>
            </div>
          } @else if (step === 'password') {
            <div class="mb-2 text-center sm:text-right">
              <p class="m-0 text-base font-bold text-gray-900">
                {{ 'modules.auth.login.passwordLoginTitle' | transloco }}
              </p>
              <p class="m-0 mt-2 text-sm text-gray-500">
                {{ 'modules.auth.login.enterUsernamePasswordHint' | transloco }}
              </p>
            </div>

            <base-form-field
              [label]="'modules.auth.login.username' | transloco"
              [floatLabel]="true"
              [control]="form.controls.username"
              errorMessage="message.validation.required"
            />
            <base-form-field
              [label]="'modules.auth.login.password' | transloco"
              [floatLabel]="true"
              type="password"
              [control]="form.controls.password"
              errorMessage="message.validation.required"
            />

            <base-button [color]="'primary'" [disabled]="loading" (click)="loginWithPassword()">
              {{ 'modules.auth.login.submit' | transloco }}
            </base-button>

            <div class="mt-1 flex justify-center border-t border-gray-100 pt-4">
              <a
                href="#"
                class="text-sm font-medium text-[#6b849e] no-underline transition-colors hover:text-[#35507c] hover:no-underline"
                (click)="goBackToMobile(); $event.preventDefault()"
              >
                {{ 'modules.auth.login.loginWithMobile' | transloco }}
              </a>
            </div>
          } @else {
            <div class="text-center sm:text-right">
              <h2 class="m-0 mb-1.5 text-lg font-bold text-gray-900">
                {{ 'shared.verificationCode' | transloco }}
              </h2>
              <p class="m-0 text-sm text-gray-500">
                {{
                  'modules.auth.login.codeSentTo'
                    | transloco : { mobile: form.controls.mobileNumber.value }
                }}
              </p>
            </div>

            @if (isOtpExpired) {
              <p class="m-0 rounded-xl bg-amber-50 px-4 py-2.5 text-center text-sm text-amber-800">
                {{ 'shared.codeExpired' | transloco }}
              </p>
            } @else {
              <p class="m-0 rounded-xl bg-[#eef3f9] px-4 py-2.5 text-center text-sm font-medium text-[#35507c]">
                {{ 'shared.codeValidity' | transloco : { time: remainingTimeLabel } }}
              </p>
            }

            <base-form-field
              [label]="'shared.verificationCode' | transloco"
              [control]="form.controls.code"
              type="tel"
              [maxLength]="6"
              errorMessage="message.validation.enterCode"
            />

            <base-button [color]="'primary'" [disabled]="loading || isOtpExpired" (click)="verifyOtp()">
              {{ 'modules.auth.login.submit' | transloco }}
            </base-button>

            <button
              type="button"
              class="w-full rounded-xl border-0 bg-gray-50 py-2.5 text-sm font-semibold text-[#35507c] transition hover:bg-[#eef3f9] disabled:cursor-not-allowed disabled:opacity-60"
              [disabled]="loading"
              (click)="resendOtp()"
            >
              {{ 'shared.resendCode' | transloco }}
            </button>

            <button
              type="button"
              class="w-full rounded-xl border-0 bg-transparent py-2 text-sm text-gray-500 transition hover:text-gray-700"
              (click)="goBackToMobile()"
            >
              {{ 'modules.auth.login.changeMobile' | transloco }}
            </button>
          }
        </form>
      </div>
    </div>
  `,
})
export class ContactLoginDialogComponent implements OnDestroy {
  step: LoginStep = 'mobile';
  loading = false;
  error = '';
  success = '';
  remainingSeconds = 0;

  private _countdownTimer: ReturnType<typeof setInterval> | null = null;

  form = new FormGroup({
    mobileNumber: new FormControl('', [
      Validators.required,
      Validators.pattern(/^09\d{9}$/),
    ]),
    code: new FormControl('', [Validators.required, Validators.pattern(/^\d{6}$/)]),
    username: new FormControl('', [Validators.required, Validators.maxLength(50)]),
    password: new FormControl('', [Validators.required]),
  });

  constructor(
    private _dialogRef: MatDialogRef<ContactLoginDialogComponent, boolean>,
    private _authService: AuthService,
    private _localization: LocalizationService,
    private _router: Router,
  ) {}

  get isOtpExpired(): boolean {
    return this.step === 'otp' && this.remainingSeconds <= 0;
  }

  get remainingTimeLabel(): string {
    const minutes = Math.floor(this.remainingSeconds / 60);
    const seconds = this.remainingSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  ngOnDestroy(): void {
    this.stopCountdown();
  }

  closeDialog(result = false): void {
    this._dialogRef.close(result);
  }

  goToCompanyRegulations(event: MouseEvent): void {
    event.preventDefault();
    this.closeDialog(false);
    void this._router.navigate(['/company-regulations']);
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
    const normalizedCode = this.normalizeOtpCode(this.form.controls.code.value);
    this.form.controls.code.setValue(normalizedCode);

    if (this.isOtpExpired) {
      this.error = this._localization.translate('message.auth.otpExpiredResend');
      return;
    }
    if (this.form.controls.code.invalid) {
      this.form.controls.code.markAsTouched();
      return;
    }

    this.loading = true;
    try {
      await this._authService.verifyOtp({
        mobileNumber: this.form.controls.mobileNumber.value!,
        code: normalizedCode,
        loginUserType: LoginUserType.User,
      });
      this.closeDialog(true);
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
      await this._authService.loginWithPassword({
        username: usernameControl.value!.trim(),
        password: passwordControl.value!,
      });
      this.closeDialog(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('message.auth.invalidUsernameOrPassword');
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
    this.form.controls.username.reset('');
    this.form.controls.password.reset('');
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
        loginUserType: LoginUserType.User,
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
      .replace(/[۰-۹]/g, (digit) => String.fromCharCode(digit.charCodeAt(0) - 1728))
      .replace(/[٠-٩]/g, (digit) => String.fromCharCode(digit.charCodeAt(0) - 1584))
      .replace(/[\s-]/g, '');
  }

  private stopCountdown(): void {
    if (this._countdownTimer) {
      clearInterval(this._countdownTimer);
      this._countdownTimer = null;
    }
  }
}
