import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthService } from '@core/services/auth/auth.service';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LocalizationService } from '@core/services/localization/localization.service';
import { PublicSiteService } from '@core/services/site/public-site.service';
import { SiteContextService } from '@core/services/site/site-context.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { SiteSettingsPublicDto } from '@modules/admin/settings/site-settings.types';

@Component({
  selector: 'app-register-store',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    RouterLink,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './register-store.component.html',
})
export class RegisterStoreComponent implements OnInit {
  loading = false;
  error = '';
  siteSettings: SiteSettingsPublicDto | null = null;

  form = new FormGroup({
    username: new FormControl('', [Validators.required, Validators.maxLength(50)]),
    mobileNumber: new FormControl('', [Validators.required, Validators.pattern(/^09\d{9}$/)]),
    password: new FormControl('', [Validators.required, Validators.minLength(6)]),
    confirmPassword: new FormControl('', [Validators.required]),
    storeName: new FormControl('', [Validators.required, Validators.maxLength(200)]),
  });

  constructor(
    private _authService: AuthService,
    private _authUtils: AuthUtils,
    private _localization: LocalizationService,
    private _route: ActivatedRoute,
    private _siteContext: SiteContextService,
  ) {}

  get passwordMismatch(): boolean {
    const password = this.form.controls.password.value ?? '';
    const confirmPassword = this.form.controls.confirmPassword.value ?? '';
    return !!confirmPassword && password !== confirmPassword;
  }

  get siteTitle(): string {
    return this.siteSettings?.siteTitle?.trim() || 'LabConnect';
  }

  get logoUrl(): string | null {
    return this.siteSettings?.hasLogo ? PublicSiteService.logoUrl() : null;
  }

  ngOnInit(): void {
    void this.loadSiteSettings();

    if (this._authUtils.isAuthenticated) {
      void this._authService.navigateAfterLogin(
        this._route.snapshot.queryParamMap.get('returnUrl')
      );
    }
  }

  async loadSiteSettings(): Promise<void> {
    try {
      this.siteSettings = await this._siteContext.ensureLoaded();
    } catch {
      // Page still renders with defaults when settings fail to load.
    }
  }

  async register(): Promise<void> {
    this.error = '';
    if (this.form.invalid || this.passwordMismatch) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    try {
      await this._authService.registerStoreAndNavigate(
        {
          username: this.form.controls.username.value!.trim(),
          mobileNumber: this.form.controls.mobileNumber.value!,
          password: this.form.controls.password.value!,
          storeName: this.form.controls.storeName.value!.trim(),
        },
        this._route.snapshot.queryParamMap.get('returnUrl'),
      );
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('message.auth.registerStoreFailed');
    } finally {
      this.loading = false;
    }
  }
}
