import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { SiteSettingsService } from './site-settings.service';
import { SiteSettingsDto } from './site-settings.types';
import { toUpdateSiteSettingsCommand } from './site-settings.mapper';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SitePermissionService } from '@core/services/auth/site-permission.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseImageUploadComponent,
  ],
  templateUrl: './settings.component.html',
  styles: `
    :host {
      display: block;
    }
  `,
})
export class SettingsComponent implements OnInit, OnDestroy {
  readonly entity = SystemEntity.Settings;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Settings);

  loading = false;
  saving = false;
  uploadingLogo = false;
  uploadingFooterLogo = false;
  error = '';
  success = '';
  logoUrl = '';
  footerLogoUrl = '';
  logoFileName = '';
  footerLogoFileName = '';
  settings: SiteSettingsDto | null = null;

  readonly imageAccept = 'image/jpeg,image/png,image/webp,image/svg+xml,.svg';

  form = new FormGroup({
    siteTitle: new FormControl('', Validators.required),
    tagline: new FormControl(''),
    supportLandline: new FormControl(''),
    supportMobile: new FormControl(''),
    enamadEmbedCode: new FormControl(''),
    enamadLinkUrl: new FormControl(''),
    googleMapEmbedCode: new FormControl(''),
    footerAddress: new FormControl(''),
    footerEmail: new FormControl(''),
    footerCopyrightText: new FormControl(''),
    footerAboutText: new FormControl(''),
  });

  constructor(
    private _siteSettingsService: SiteSettingsService,
    private _localization: LocalizationService,
    private _sitePermission: SitePermissionService
  ) {}

  get canUpdate(): boolean {
    return this._sitePermission.can(this.entity, 'update');
  }

  ngOnInit(): void {
    void this.load();
  }

  ngOnDestroy(): void {
    if (this.logoUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.logoUrl);
    }
    if (this.footerLogoUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.footerLogoUrl);
    }
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._siteSettingsService.get();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.settings.errors.loadFailed')
        );
      }
      this.settings = result.data;
      this.form.patchValue({
        siteTitle: result.data.siteTitle,
        tagline: result.data.tagline,
        supportLandline: result.data.supportLandline,
        supportMobile: result.data.supportMobile,
        enamadEmbedCode: result.data.enamadEmbedCode ?? '',
        enamadLinkUrl: result.data.enamadLinkUrl ?? '',
        googleMapEmbedCode: result.data.googleMapEmbedCode ?? '',
        footerAddress: result.data.footerAddress ?? '',
        footerEmail: result.data.footerEmail ?? '',
        footerCopyrightText: result.data.footerCopyrightText ?? '',
        footerAboutText: result.data.footerAboutText ?? '',
      });
      await Promise.all([
        this.loadLogoPreview(result.data.hasLogo),
        this.loadFooterLogoPreview(result.data.hasFooterLogo),
      ]);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.settings.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  async save(): Promise<void> {
    if (!this.canUpdate || !this.settings || this.form.invalid) return;
    this.saving = true;
    this.error = '';
    this.success = '';
    const value = this.form.getRawValue();
    try {
      const result = await this._siteSettingsService.update(
        toUpdateSiteSettingsCommand(this.settings, {
          siteTitle: value.siteTitle ?? '',
          tagline: value.tagline ?? '',
          supportLandline: value.supportLandline ?? '',
          supportMobile: value.supportMobile ?? '',
          enamadEmbedCode: value.enamadEmbedCode || null,
          enamadLinkUrl: value.enamadLinkUrl || null,
          googleMapEmbedCode: value.googleMapEmbedCode || null,
          footerAboutText: value.footerAboutText ?? '',
          footerAddress: value.footerAddress ?? '',
          footerEmail: value.footerEmail ?? '',
          footerCopyrightText: value.footerCopyrightText ?? '',
        })
      );
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.settings.errors.saveFailed')
        );
      }
      this.settings = result.data;
      await this.loadFooterLogoPreview(result.data.hasFooterLogo);
      this.success = this._localization.translate('modules.admin.settings.success.saved');
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.settings.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  async onLogoSelected(file: File): Promise<void> {
    this.uploadingLogo = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._siteSettingsService.uploadLogo(file);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.settings.errors.uploadFailed')
        );
      }
      this.settings = result.data;
      this.logoFileName = file.name;
      await this.loadLogoPreview(true);
      this.success = this._localization.translate(
        'modules.admin.settings.success.logoUploaded'
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.settings.errors.uploadFailed');
    } finally {
      this.uploadingLogo = false;
    }
  }

  async onFooterLogoSelected(file: File): Promise<void> {
    this.uploadingFooterLogo = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._siteSettingsService.uploadFooterLogo(file);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.settings.errors.uploadFailed')
        );
      }
      this.settings = result.data;
      this.footerLogoFileName = file.name;
      await this.loadFooterLogoPreview(true);
      this.success = this._localization.translate(
        'modules.admin.settings.success.footerLogoUploaded'
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.settings.errors.uploadFailed');
    } finally {
      this.uploadingFooterLogo = false;
    }
  }

  private async loadLogoPreview(hasLogo: boolean): Promise<void> {
    if (this.logoUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.logoUrl);
    }
    this.logoUrl = '';
    if (!hasLogo) return;

    try {
      const blob = await this._siteSettingsService.getLogoBlob();
      this.logoUrl = URL.createObjectURL(blob);
    } catch {
      this.logoUrl = SiteSettingsService.logoUrl();
    }
  }

  private async loadFooterLogoPreview(hasFooterLogo: boolean): Promise<void> {
    if (this.footerLogoUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.footerLogoUrl);
    }
    this.footerLogoUrl = '';
    if (!hasFooterLogo) return;

    try {
      const blob = await this._siteSettingsService.getFooterLogoBlob();
      this.footerLogoUrl = URL.createObjectURL(blob);
    } catch {
      this.footerLogoUrl = SiteSettingsService.footerLogoUrl();
    }
  }
}
