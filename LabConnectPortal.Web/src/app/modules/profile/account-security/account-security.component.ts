import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '@core/services/auth/auth.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import {
  UsernameChangeChannel,
  UsernameChangeChannelDialogComponent,
} from './username-change-channel-dialog.component';

type AccountSecurityMode = 'username' | 'password';

@Component({
  selector: 'app-account-security',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './account-security.component.html',
  styleUrl: './account-security.component.scss',
})
export class AccountSecurityComponent implements OnInit, OnDestroy {
  readonly entity = SystemEntity.Profile;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Profile);
  private readonly _dialog = inject(MatDialog);

  mode: AccountSecurityMode = 'username';
  loading = false;
  saving = false;
  sendingEmailOtp = false;
  confirmingEmailOtp = false;
  emailOtpSent = false;
  emailConfirmed = false;
  /** وقتی ایمیل تأییدشده است و کاربر هنوز «تغییر ایمیل» نزده، فیلد قفل است. */
  emailEditing = false;
  emailOtpCountdown = 0;
  emailError = '';
  emailSuccess = '';

  originalUsername = '';
  profileEmail = '';
  profileMobile = '';
  usernameOtpSent = false;
  usernameOtpChannel: UsernameChangeChannel | null = null;
  sendingUsernameOtp = false;
  confirmingUsernameOtp = false;
  usernameOtpCountdown = 0;
  pendingUsername = '';

  passwordForgotMode = false;
  passwordOtpChannel: UsernameChangeChannel | null = null;
  sendingPasswordOtp = false;
  passwordOtpCountdown = 0;

  private _countdownTimer: ReturnType<typeof setInterval> | null = null;
  private _usernameCountdownTimer: ReturnType<typeof setInterval> | null = null;
  private _passwordCountdownTimer: ReturnType<typeof setInterval> | null = null;

  usernameForm = new FormGroup({
    username: new FormControl('', [Validators.required, Validators.maxLength(50)]),
    code: new FormControl('', [Validators.minLength(6), Validators.maxLength(6)]),
  });

  emailForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email, Validators.maxLength(200)]),
    code: new FormControl('', [Validators.minLength(6), Validators.maxLength(6)]),
  });

  passwordForm = new FormGroup({
    currentPassword: new FormControl(''),
    newPassword: new FormControl('', [Validators.required, Validators.minLength(6)]),
    confirmPassword: new FormControl('', [Validators.required]),
    code: new FormControl('', [Validators.minLength(6), Validators.maxLength(6)]),
  });

  constructor(
    private _authService: AuthService,
    private _route: ActivatedRoute,
    private _localization: LocalizationService,
  ) {}

  get isUsernameMode(): boolean {
    return this.mode === 'username';
  }

  get passwordMismatch(): boolean {
    const newPassword = this.passwordForm.controls.newPassword.value ?? '';
    const confirmPassword = this.passwordForm.controls.confirmPassword.value ?? '';
    return !!confirmPassword && newPassword !== confirmPassword;
  }

  get canResendEmailOtp(): boolean {
    return this.emailOtpCountdown <= 0 && !this.sendingEmailOtp;
  }

  get canResendUsernameOtp(): boolean {
    return this.usernameOtpCountdown <= 0 && !this.sendingUsernameOtp;
  }

  get canResendPasswordOtp(): boolean {
    return this.passwordOtpCountdown <= 0 && !this.sendingPasswordOtp;
  }

  get hasProfileEmail(): boolean {
    return !!this.profileEmail.trim();
  }

  get hasProfileMobile(): boolean {
    return !!this.profileMobile.trim();
  }

  /** ایمیل تأییدشده و هنوز در حالت ویرایش نیست → اینپوت غیرفعال + دکمه تغییر ایمیل */
  get isEmailLocked(): boolean {
    return this.emailConfirmed && !this.emailEditing;
  }

  async ngOnInit(): Promise<void> {
    this.mode = (this._route.snapshot.data['mode'] as AccountSecurityMode | undefined) ?? 'username';
    await this.loadProfile();
  }

  ngOnDestroy(): void {
    this.clearCountdown();
    this.clearUsernameCountdown();
    this.clearPasswordCountdown();
  }

  async loadProfile(): Promise<void> {
    this.loading = true;
    this.clearEmailError();
    this.clearEmailSuccess();
    try {
      const profile = await this._authService.getProfile();
      this.originalUsername = profile.username ?? '';
      this.profileEmail = profile.email ?? '';
      this.profileMobile = profile.mobileNumber ?? '';
      if (this.isUsernameMode) {
        this.usernameForm.patchValue({
          username: this.originalUsername,
          code: '',
        });
        this.resetUsernameOtpState();
        this.emailForm.patchValue({
          email: profile.email ?? '',
          code: '',
        });
        this.emailConfirmed = !!profile.emailConfirmed;
        this.emailEditing = false;
        this.emailOtpSent = false;
        this.syncEmailInputLock();
      } else {
        this.resetPasswordForgotState();
      }
    } catch (e: unknown) {
      this.emailError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.accountSecurity.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  async saveUsername(): Promise<void> {
    this.clearEmailError();
    this.clearEmailSuccess();

    if (this.usernameForm.controls.username.invalid) {
      this.usernameForm.controls.username.markAsTouched();
      return;
    }

    const username = this.usernameForm.controls.username.value?.trim() ?? '';
    if (!username) {
      this.usernameForm.controls.username.markAsTouched();
      return;
    }

    if (username.toLowerCase() === this.originalUsername.trim().toLowerCase()) {
      this.emailSuccess = this._localization.translate(
        'modules.profile.accountSecurity.errors.usernameUnchanged',
      );
      return;
    }

    this.saving = true;
    try {
      await this._authService.checkUsernameAvailable({ username });
    } catch (e: unknown) {
      this.emailError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.accountSecurity.errors.usernameTaken');
      this.saving = false;
      return;
    } finally {
      this.saving = false;
    }

    if (this.hasProfileEmail && this.hasProfileMobile) {
      const channel = await this.askUsernameOtpChannel();
      if (!channel) return;
      await this.sendUsernameOtp(channel, username);
      return;
    }

    if (this.hasProfileMobile) {
      await this.sendUsernameOtp('Mobile', username);
      return;
    }

    if (this.hasProfileEmail) {
      await this.sendUsernameOtp('Email', username);
      return;
    }

    await this.commitUsername(username);
  }

  async resendUsernameOtp(): Promise<void> {
    if (!this.usernameOtpChannel || !this.pendingUsername || !this.canResendUsernameOtp) return;
    await this.sendUsernameOtp(this.usernameOtpChannel, this.pendingUsername);
  }

  async confirmUsernameOtp(): Promise<void> {
    this.clearEmailError();
    this.clearEmailSuccess();

    const codeControl = this.usernameForm.controls.code;
    const code = codeControl.value?.trim() ?? '';
    if (code.length !== 6 || !this.usernameOtpChannel || !this.pendingUsername) {
      codeControl.setErrors({ required: true });
      codeControl.markAsTouched();
      return;
    }

    this.confirmingUsernameOtp = true;
    try {
      await this._authService.updateUsername({
        username: this.pendingUsername,
        code,
        channel: this.usernameOtpChannel,
      });
      this.originalUsername = this.pendingUsername;
      this.usernameForm.patchValue({ username: this.pendingUsername, code: '' });
      this.resetUsernameOtpState();
      this.emailSuccess = this._localization.translate(
        'modules.profile.accountSecurity.success.usernameSaved',
      );
    } catch (e: unknown) {
      this.emailError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.accountSecurity.errors.usernameSaveFailed');
    } finally {
      this.confirmingUsernameOtp = false;
    }
  }

  beginChangeEmail(): void {
    this.emailEditing = true;
    this.emailOtpSent = false;
    this.clearEmailError();
    this.clearEmailSuccess();
    this.emailForm.controls.code.setValue('');
    this.clearCountdown();
    this.syncEmailInputLock();
  }

  async sendEmailOtp(): Promise<void> {
    this.clearEmailError();
    this.clearEmailSuccess();

    const emailControl = this.emailForm.controls.email;
    if (emailControl.invalid) {
      emailControl.markAsTouched();
      return;
    }

    this.sendingEmailOtp = true;
    try {
      await this._authService.sendEmailOtp({
        email: emailControl.value?.trim() ?? '',
      });
      this.emailConfirmed = false;
      this.emailEditing = true;
      this.emailOtpSent = true;
      this.emailForm.controls.code.setValue('');
      this.syncEmailInputLock();
      this.startCountdown(120);
      this.emailSuccess = this._localization.translate(
        'modules.profile.accountSecurity.success.emailOtpSent',
      );
    } catch (e: unknown) {
      this.emailError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.accountSecurity.errors.emailOtpSendFailed');
    } finally {
      this.sendingEmailOtp = false;
    }
  }

  async confirmEmailOtp(): Promise<void> {
    this.clearEmailError();
    this.clearEmailSuccess();

    const email = this.emailForm.controls.email;
    const code = this.emailForm.controls.code;
    const codeValue = code.value?.trim() ?? '';
    if (email.invalid || codeValue.length !== 6) {
      email.markAsTouched();
      code.setErrors({ required: true });
      code.markAsTouched();
      return;
    }

    this.confirmingEmailOtp = true;
    try {
      await this._authService.confirmEmailOtp({
        email: email.value?.trim() ?? '',
        code: codeValue,
      });
      this.emailConfirmed = true;
      this.emailEditing = false;
      this.emailOtpSent = false;
      this.profileEmail = email.value?.trim() ?? '';
      code.setValue('');
      this.clearCountdown();
      this.syncEmailInputLock();
      this.emailSuccess = this._localization.translate(
        'modules.profile.accountSecurity.success.emailConfirmed',
      );
    } catch (e: unknown) {
      this.emailError =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.accountSecurity.errors.emailOtpConfirmFailed',
            );
    } finally {
      this.confirmingEmailOtp = false;
    }
  }

  async changePassword(): Promise<void> {
    this.clearEmailError();
    this.clearEmailSuccess();

    if (this.passwordMismatch) {
      this.passwordForm.markAllAsTouched();
      this.emailError = this._localization.translate(
        'modules.profile.accountSecurity.errors.passwordMismatch',
      );
      return;
    }

    const newPassword = this.passwordForm.controls.newPassword;
    const confirmPassword = this.passwordForm.controls.confirmPassword;
    if (newPassword.invalid || confirmPassword.invalid) {
      this.passwordForm.markAllAsTouched();
      this.emailError = this._localization.translate(
        'modules.profile.accountSecurity.errors.passwordInvalid',
      );
      return;
    }

    if (this.passwordForgotMode) {
      const code = this.passwordForm.controls.code.value?.trim() ?? '';
      if (code.length !== 6 || !this.passwordOtpChannel) {
        this.passwordForm.controls.code.setErrors({ required: true });
        this.passwordForm.controls.code.markAsTouched();
        this.emailError = this._localization.translate(
          'modules.profile.accountSecurity.errors.passwordOtpRequired',
        );
        return;
      }
    }

    this.saving = true;
    try {
      await this._authService.changePassword({
        currentPassword: this.passwordForgotMode
          ? ''
          : (this.passwordForm.controls.currentPassword.value ?? ''),
        newPassword: newPassword.value ?? '',
        code: this.passwordForgotMode
          ? (this.passwordForm.controls.code.value?.trim() ?? '')
          : null,
        channel: this.passwordForgotMode ? this.passwordOtpChannel : null,
      });
      this.passwordForm.reset();
      this.resetPasswordForgotState();
      this.emailSuccess = this._localization.translate(
        'modules.profile.accountSecurity.success.passwordSaved',
      );
    } catch (e: unknown) {
      this.emailError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.accountSecurity.errors.passwordSaveFailed');
    } finally {
      this.saving = false;
    }
  }

  async beginForgotPassword(event: Event): Promise<void> {
    event.preventDefault();
    this.clearEmailError();
    this.clearEmailSuccess();

    if (!this.hasProfileEmail && !this.hasProfileMobile) {
      this.emailError = this._localization.translate(
        'modules.profile.accountSecurity.errors.passwordNoContact',
      );
      return;
    }

    let channel: UsernameChangeChannel | null = null;
    if (this.hasProfileEmail && this.hasProfileMobile) {
      channel = await this.askPasswordOtpChannel();
    } else if (this.hasProfileMobile) {
      channel = 'Mobile';
    } else {
      channel = 'Email';
    }

    if (!channel) return;
    await this.sendPasswordOtp(channel);
  }

  async resendPasswordOtp(): Promise<void> {
    if (!this.passwordOtpChannel || !this.canResendPasswordOtp) return;
    await this.sendPasswordOtp(this.passwordOtpChannel);
  }

  clearEmailSuccess(): void {
    this.emailSuccess = '';
  }

  clearEmailError(): void {
    this.emailError = '';
  }

  private async askUsernameOtpChannel(): Promise<UsernameChangeChannel | null> {
    return this.openOtpChannelDialog();
  }

  private async askPasswordOtpChannel(): Promise<UsernameChangeChannel | null> {
    return this.openOtpChannelDialog({
      titleKey: 'modules.profile.accountSecurity.passwordOtpChannelTitle',
      hintKey: 'modules.profile.accountSecurity.passwordOtpChannelHint',
      subhintKey: 'modules.profile.accountSecurity.passwordOtpChannelSubhint',
    });
  }

  private async openOtpChannelDialog(options?: {
    titleKey?: string;
    hintKey?: string;
    subhintKey?: string;
  }): Promise<UsernameChangeChannel | null> {
    const ref = this._dialog.open(UsernameChangeChannelDialogComponent, {
      width: '26.5rem',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        hasEmail: this.hasProfileEmail,
        hasMobile: this.hasProfileMobile,
        email: this.profileEmail,
        mobileNumber: this.profileMobile,
        titleKey: options?.titleKey,
        hintKey: options?.hintKey,
        subhintKey: options?.subhintKey,
      },
    });
    return (await firstValueFrom(ref.afterClosed())) ?? null;
  }

  private async sendPasswordOtp(channel: UsernameChangeChannel): Promise<void> {
    this.sendingPasswordOtp = true;
    this.clearEmailError();
    this.clearEmailSuccess();
    try {
      await this._authService.sendPasswordChangeOtp({ channel });
      this.passwordForgotMode = true;
      this.passwordOtpChannel = channel;
      this.passwordForm.controls.code.setValue('');
      this.passwordForm.controls.currentPassword.setValue('');
      this.syncCurrentPasswordLock();
      this.startPasswordCountdown(120);
      this.emailSuccess = this._localization.translate(
        channel === 'Mobile'
          ? 'modules.profile.accountSecurity.passwordOtpSentMobile'
          : 'modules.profile.accountSecurity.passwordOtpSentEmail',
      );
    } catch (e: unknown) {
      this.emailError =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.accountSecurity.errors.passwordOtpSendFailed',
            );
    } finally {
      this.sendingPasswordOtp = false;
    }
  }

  private async sendUsernameOtp(
    channel: UsernameChangeChannel,
    username: string,
  ): Promise<void> {
    this.sendingUsernameOtp = true;
    this.clearEmailError();
    this.clearEmailSuccess();
    try {
      await this._authService.sendUsernameChangeOtp({ channel, username });
      this.pendingUsername = username;
      this.usernameOtpChannel = channel;
      this.usernameOtpSent = true;
      this.usernameForm.controls.code.setValue('');
      this.startUsernameCountdown(120);
      this.emailSuccess = this._localization.translate(
        channel === 'Mobile'
          ? 'modules.profile.accountSecurity.usernameOtpSentMobile'
          : 'modules.profile.accountSecurity.usernameOtpSentEmail',
      );
    } catch (e: unknown) {
      this.emailError =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.accountSecurity.errors.usernameOtpSendFailed',
            );
    } finally {
      this.sendingUsernameOtp = false;
    }
  }

  private async commitUsername(username: string): Promise<void> {
    this.saving = true;
    try {
      await this._authService.updateUsername({ username });
      this.originalUsername = username;
      this.resetUsernameOtpState();
      this.emailSuccess = this._localization.translate(
        'modules.profile.accountSecurity.success.usernameSaved',
      );
    } catch (e: unknown) {
      this.emailError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.accountSecurity.errors.usernameSaveFailed');
    } finally {
      this.saving = false;
    }
  }

  private resetUsernameOtpState(): void {
    this.usernameOtpSent = false;
    this.usernameOtpChannel = null;
    this.pendingUsername = '';
    this.usernameForm.controls.code.setValue('');
    this.clearUsernameCountdown();
  }

  private resetPasswordForgotState(): void {
    this.passwordForgotMode = false;
    this.passwordOtpChannel = null;
    this.passwordForm.controls.code.setValue('');
    this.clearPasswordCountdown();
    this.syncCurrentPasswordLock();
  }

  private syncCurrentPasswordLock(): void {
    const control = this.passwordForm.controls.currentPassword;
    if (this.passwordForgotMode) {
      control.disable({ emitEvent: false });
    } else {
      control.enable({ emitEvent: false });
    }
  }

  private syncEmailInputLock(): void {
    const emailControl = this.emailForm.controls.email;
    if (this.isEmailLocked) {
      emailControl.disable({ emitEvent: false });
    } else {
      emailControl.enable({ emitEvent: false });
    }
  }

  private startCountdown(seconds: number): void {
    this.clearCountdown();
    this.emailOtpCountdown = seconds;
    this._countdownTimer = setInterval(() => {
      this.emailOtpCountdown -= 1;
      if (this.emailOtpCountdown <= 0) {
        this.clearCountdown();
      }
    }, 1000);
  }

  private clearCountdown(): void {
    if (this._countdownTimer) {
      clearInterval(this._countdownTimer);
      this._countdownTimer = null;
    }
    this.emailOtpCountdown = 0;
  }

  private startUsernameCountdown(seconds: number): void {
    this.clearUsernameCountdown();
    this.usernameOtpCountdown = seconds;
    this._usernameCountdownTimer = setInterval(() => {
      this.usernameOtpCountdown -= 1;
      if (this.usernameOtpCountdown <= 0) {
        this.clearUsernameCountdown();
      }
    }, 1000);
  }

  private clearUsernameCountdown(): void {
    if (this._usernameCountdownTimer) {
      clearInterval(this._usernameCountdownTimer);
      this._usernameCountdownTimer = null;
    }
    this.usernameOtpCountdown = 0;
  }

  private startPasswordCountdown(seconds: number): void {
    this.clearPasswordCountdown();
    this.passwordOtpCountdown = seconds;
    this._passwordCountdownTimer = setInterval(() => {
      this.passwordOtpCountdown -= 1;
      if (this.passwordOtpCountdown <= 0) {
        this.clearPasswordCountdown();
      }
    }, 1000);
  }

  private clearPasswordCountdown(): void {
    if (this._passwordCountdownTimer) {
      clearInterval(this._passwordCountdownTimer);
      this._passwordCountdownTimer = null;
    }
    this.passwordOtpCountdown = 0;
  }
}
