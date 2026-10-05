import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { TranslocoPipe } from '@jsverse/transloco';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    RouterLink,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './forgot-password.component.html',
})
export class ForgotPasswordComponent {
  step: 'request' | 'reset' = 'request';
  loading = false;
  error = '';
  success = '';

  form = new FormGroup({
    mobileNumber: new FormControl('', [Validators.required, Validators.pattern(/^09\d{9}$/)]),
    code: new FormControl(''),
    newPassword: new FormControl(''),
  });

  constructor(
    private _http: ApiHttpService,
    private _localization: LocalizationService,
  ) {}

  async sendOtp(): Promise<void> {
    this.error = '';
    if (this.form.controls.mobileNumber.invalid) return;
    this.loading = true;
    try {
      const result = await this._http.post('Auth', 'ForgotPassword', {
        mobileNumber: this.form.controls.mobileNumber.value,
      });
      if (!result.success) throw new Error(result.message);
      this.step = 'reset';
      this.form.controls.code.setValidators([Validators.required]);
      this.form.controls.newPassword.setValidators([Validators.required, Validators.minLength(6)]);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('shared.error');
    } finally {
      this.loading = false;
    }
  }

  async resetPassword(): Promise<void> {
    this.error = '';
    if (this.form.invalid) return;
    this.loading = true;
    try {
      const result = await this._http.post('Auth', 'ResetPassword', {
        mobileNumber: this.form.controls.mobileNumber.value,
        code: this.form.controls.code.value,
        newPassword: this.form.controls.newPassword.value,
      });
      if (!result.success) throw new Error(result.message);
      this.success = this._localization.translate('modules.auth.forgotPassword.success.passwordChanged');
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('shared.error');
    } finally {
      this.loading = false;
    }
  }
}
