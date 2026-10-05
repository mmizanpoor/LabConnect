import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SiteSettingsService } from './site-settings.service';
import { SiteSettingsDto } from './site-settings.types';
import { toUpdateSiteSettingsCommand } from './site-settings.mapper';

@Component({
  selector: 'app-settings-seo',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './settings-seo.component.html',
  styles: `
    :host {
      display: block;
    }
  `,
})
export class SettingsSeoComponent implements OnInit {
  readonly entity = SystemEntity.Settings;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Settings);

  loading = false;
  saving = false;
  error = '';
  success = '';
  settings: SiteSettingsDto | null = null;

  form = new FormGroup({
    metaTitle: new FormControl(''),
    metaDescription: new FormControl(''),
    metaKeywords: new FormControl(''),
    metaViewport: new FormControl(''),
    metaCanonical: new FormControl(''),
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
        metaTitle: result.data.metaTitle,
        metaDescription: result.data.metaDescription,
        metaKeywords: result.data.metaKeywords,
        metaViewport: result.data.metaViewport ?? '',
        metaCanonical: result.data.metaCanonical ?? '',
      });
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
          metaTitle: value.metaTitle ?? '',
          metaDescription: value.metaDescription ?? '',
          metaKeywords: value.metaKeywords ?? '',
          metaViewport: value.metaViewport || null,
          metaCanonical: value.metaCanonical || null,
        })
      );
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.settings.errors.saveFailed')
        );
      }
      this.settings = result.data;
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
}
