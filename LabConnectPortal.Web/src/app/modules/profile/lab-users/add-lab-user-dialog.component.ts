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
import { LabUsersService } from './lab-users.service';
import { AddLabMemberOtpScenario } from './lab-users.types';

const OTP_VALIDITY_SECONDS = 120;

export interface AddLabUserDialogData {
  existingMobiles: string[];
}

export interface AddLabUserDialogResult {
  successMessage: string;
}

type AddLabUserStep = 'form' | 'otp';

@Component({
  selector: 'app-add-lab-user-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './add-lab-user-dialog.component.html',
})
export class AddLabUserDialogComponent implements OnDestroy {
  step: AddLabUserStep = 'form';
  saving = false;
  error = '';
  success = '';
  otpScenario: AddLabMemberOtpScenario | null = null;
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
    @Inject(MAT_DIALOG_DATA) private _data: AddLabUserDialogData,
    private _dialogRef: MatDialogRef<
      AddLabUserDialogComponent,
      AddLabUserDialogResult
    >,
    private _labUsersService: LabUsersService,
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
      await this._labUsersService.confirmAddMember({
        mobileNumber: this.form.controls.mobileNumber.value!,
        code: this.form.controls.code.value!,
      });
      const successMessage = this._localization.translate(
        this.otpScenario === 'transfer'
          ? 'modules.profile.labUsers.success.transferred'
          : 'modules.profile.labUsers.success.added'
      );
      this._dialogRef.close({ successMessage });
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.labUsers.errors.confirmFailed'
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
    if (this.otpScenario === 'transfer') {
      return this._localization.translate(
        'modules.profile.labUsers.hints.transfer'
      );
    }
    if (this.otpScenario === 'create') {
      return this._localization.translate(
        'modules.profile.labUsers.hints.create'
      );
    }
    return this._localization.translate(
      'modules.profile.labUsers.hints.ownership'
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
        'modules.profile.labUsers.errors.duplicateMobile'
      );
      return;
    }

    this.saving = true;
    try {
      const result = await this._labUsersService.addMember({ mobileNumber });
      if (!result.requiresOtp) {
        throw new Error(
          this._localization.translate(
            'modules.profile.labUsers.errors.otpRequired'
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
            'modules.profile.labUsers.success.codeResent'
          )
        : this.getOtpSentMessage(this.otpScenario);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.labUsers.errors.addFailed'
            );
    } finally {
      this.saving = false;
    }
  }

  private getOtpSentMessage(scenario: AddLabMemberOtpScenario): string {
    if (scenario === 'transfer') {
      return this._localization.translate(
        'modules.profile.labUsers.success.transferCodeSent'
      );
    }
    if (scenario === 'create') {
      return this._localization.translate(
        'modules.profile.labUsers.success.createCodeSent'
      );
    }
    return this._localization.translate(
      'modules.profile.labUsers.success.userCodeSent'
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
