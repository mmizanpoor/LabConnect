import { Component, Inject, OnDestroy } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { SiteUsersService } from './site-users.service';
import { AddSiteMemberOtpScenario } from './site-users.types';

const OTP_VALIDITY_SECONDS = 120;

export interface AddSiteUserDialogData {
  existingMobiles: string[];
}

export interface AddSiteUserDialogResult {
  successMessage: string;
}

type AddSiteUserStep = 'form' | 'otp';

@Component({
  selector: 'app-add-site-user-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './add-site-user-dialog.component.html',
})
export class AddSiteUserDialogComponent implements OnDestroy {
  step: AddSiteUserStep = 'form';
  saving = false;
  error = '';
  success = '';
  otpScenario: AddSiteMemberOtpScenario | null = null;
  remainingSeconds = 0;

  private _countdownTimer: ReturnType<typeof setInterval> | null = null;

  form = new FormGroup({
    mobileNumber: new FormControl('', [
      Validators.required,
      Validators.pattern(/^09\d{9}$/),
    ]),
    code: new FormControl(''),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) private _data: AddSiteUserDialogData,
    private _dialogRef: MatDialogRef<
      AddSiteUserDialogComponent,
      AddSiteUserDialogResult
    >,
    private _siteUsersService: SiteUsersService,
    private _localization: LocalizationService
  ) {}

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

  close(): void {
    this._dialogRef.close();
  }

  async addMember(): Promise<void> {
    await this.requestAddMember(false);
  }

  async resendOtp(): Promise<void> {
    await this.requestAddMember(true);
  }

  async confirmAddMember(): Promise<void> {
    this.error = '';
    this.success = '';
    if (this.isOtpExpired) {
      this.error = this._localization.translate(
        'message.auth.otpExpiredResend'
      );
      return;
    }

    this.form.controls.code.setValidators([
      Validators.required,
      Validators.minLength(4),
    ]);
    this.form.controls.code.updateValueAndValidity();
    if (this.form.controls.code.invalid) {
      this.form.controls.code.markAsTouched();
      return;
    }

    this.saving = true;
    try {
      await this._siteUsersService.confirmAddMember({
        mobileNumber: this.form.controls.mobileNumber.value!,
        code: this.form.controls.code.value!,
      });
      this._dialogRef.close({
        successMessage: this._localization.translate(
          'modules.admin.siteUsers.success.added'
        ),
      });
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.siteUsers.errors.confirmFailed'
            );
    } finally {
      this.saving = false;
    }
  }

  goBackToForm(): void {
    this.stopCountdown();
    this.remainingSeconds = 0;
    this.otpScenario = null;
    this.step = 'form';
    this.form.controls.code.clearValidators();
    this.form.controls.code.reset('');
    this.error = '';
    this.success = '';
  }

  getOtpHint(): string {
    if (this.otpScenario === 'create') {
      return this._localization.translate(
        'modules.admin.siteUsers.hints.create'
      );
    }
    return this._localization.translate(
      'modules.admin.siteUsers.hints.ownership'
    );
  }

  private async requestAddMember(isResend: boolean): Promise<void> {
    this.error = '';
    this.success = '';
    if (this.form.controls.mobileNumber.invalid) {
      this.form.controls.mobileNumber.markAsTouched();
      return;
    }

    const mobileNumber = this.form.controls.mobileNumber.value!.trim();
    if (!isResend && this._data.existingMobiles.includes(mobileNumber)) {
      this.error = this._localization.translate(
        'modules.admin.siteUsers.errors.duplicateMobile'
      );
      return;
    }

    this.saving = true;
    try {
      const result = await this._siteUsersService.addMember({ mobileNumber });
      if (!result.requiresOtp) {
        throw new Error(
          this._localization.translate(
            'modules.admin.siteUsers.errors.otpRequired'
          )
        );
      }

      this.otpScenario = result.otpScenario ?? 'create';
      this.form.controls.code.reset('');
      this.form.controls.code.setValidators([
        Validators.required,
        Validators.minLength(4),
      ]);
      this.step = 'otp';
      this.startCountdown();
      this.success = isResend
        ? this._localization.translate(
            'modules.admin.siteUsers.success.codeResent'
          )
        : this.getOtpSentMessage(this.otpScenario);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.siteUsers.errors.addFailed'
            );
    } finally {
      this.saving = false;
    }
  }

  private getOtpSentMessage(scenario: AddSiteMemberOtpScenario): string {
    if (scenario === 'create') {
      return this._localization.translate(
        'modules.admin.siteUsers.success.createCodeSent'
      );
    }
    return this._localization.translate(
      'modules.admin.siteUsers.success.userCodeSent'
    );
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

  private stopCountdown(): void {
    if (this._countdownTimer) {
      clearInterval(this._countdownTimer);
      this._countdownTimer = null;
    }
  }
}
