import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef, inject } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { AgreementSettingsService } from './agreement-settings.service';
import { LabAgreementSettingsDto, LabAgreementSettingsFileKind } from './agreement-settings.types';

@Component({
  selector: 'app-agreement-settings',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseCheckboxComponent,
    BaseFormFieldComponent,
    BaseImageUploadComponent,
  ],
  templateUrl: './agreement-settings.component.html',
})
export class AgreementSettingsComponent implements OnInit, OnDestroy {
  readonly entity = SystemEntity.LabAgreement;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.LabAgreement);
  private readonly _destroyRef = inject(DestroyRef);

  loading = false;
  saving = false;
  uploading: LabAgreementSettingsFileKind | null = null;
  error = '';
  success = '';
  settings: LabAgreementSettingsDto | null = null;

  headerImageUrl = '';
  headerLogoUrl = '';
  headerImageFileName = '';
  headerLogoFileName = '';

  form = new FormGroup({
    useHeaderImage: new FormControl(false, { nonNullable: true }),
    labName: new FormControl('', { nonNullable: true }),
    headerAddress: new FormControl('', { nonNullable: true }),
    description1: new FormControl('', { nonNullable: true }),
  });

  constructor(
    private _service: AgreementSettingsService,
    private _localization: LocalizationService,
    private _labPermission: LabPermissionService,
  ) {
    this.form.controls.useHeaderImage.valueChanges
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this.applyFieldState());
  }

  get canUpdate(): boolean {
    return this._labPermission.can(this.entity, 'update');
  }

  get useHeaderImage(): boolean {
    return this.form.controls.useHeaderImage.value;
  }

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  ngOnDestroy(): void {
    this.revokeUrls();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getMine();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.profile.agreementSettings.errors.loadFailed'),
        );
      }
      this.settings = result.data;
      this.form.patchValue(
        {
          useHeaderImage: result.data.useHeaderImage,
          labName: result.data.labName ?? '',
          headerAddress: result.data.headerAddress ?? '',
          description1: result.data.description1 ?? '',
        },
        { emitEvent: false },
      );
      this.applyFieldState();
      await this.loadPreviews(result.data);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreementSettings.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  async save(): Promise<void> {
    if (!this.canUpdate || this.saving) return;
    this.saving = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._service.upsert({
        useHeaderImage: this.form.controls.useHeaderImage.value,
        labName: this.form.controls.labName.value,
        headerAddress: this.form.controls.headerAddress.value,
        description1: this.form.controls.description1.value,
      });
      if (!result.success || !result.data) {
        this.error =
          result.message ??
          this._localization.translate('modules.profile.agreementSettings.errors.saveFailed');
        return;
      }
      this.settings = result.data;
      this.success = this._localization.translate('modules.profile.agreementSettings.saved');
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreementSettings.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  async onHeaderImageSelected(file: File): Promise<void> {
    await this.upload('HeaderImage', file);
  }

  async onHeaderLogoSelected(file: File): Promise<void> {
    await this.upload('HeaderLogo', file);
  }

  private async upload(kind: LabAgreementSettingsFileKind, file: File): Promise<void> {
    if (!this.canUpdate) return;
    this.uploading = kind;
    this.error = '';
    this.success = '';
    try {
      const result =
        kind === 'HeaderImage'
          ? await this._service.uploadHeaderImage(file)
          : await this._service.uploadHeaderLogo(file);
      if (!result.success || !result.data) {
        this.error =
          result.message ??
          this._localization.translate('modules.profile.agreementSettings.errors.uploadFailed');
        return;
      }
      this.settings = result.data;
      await this.loadPreviews(result.data);
      this.success = this._localization.translate('modules.profile.agreementSettings.uploaded');
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreementSettings.errors.uploadFailed');
    } finally {
      this.uploading = null;
    }
  }

  private applyFieldState(): void {
    const useImage = this.form.controls.useHeaderImage.value;
    const readOnly = !this.canUpdate;

    if (readOnly) {
      this.form.disable({ emitEvent: false });
      return;
    }

    this.form.controls.useHeaderImage.enable({ emitEvent: false });

    if (useImage) {
      this.form.controls.labName.disable({ emitEvent: false });
      this.form.controls.headerAddress.disable({ emitEvent: false });
      this.form.controls.description1.disable({ emitEvent: false });
    } else {
      this.form.controls.labName.enable({ emitEvent: false });
      this.form.controls.headerAddress.enable({ emitEvent: false });
      this.form.controls.description1.enable({ emitEvent: false });
    }
  }

  private async loadPreviews(data: LabAgreementSettingsDto): Promise<void> {
    this.revokeUrls();
    this.headerImageFileName = data.hasHeaderImage ? 'header-image' : '';
    this.headerLogoFileName = data.hasHeaderLogo ? 'header-logo' : '';

    if (data.hasHeaderImage) {
      try {
        const blob = await this._service.getFileBlob('HeaderImage');
        this.headerImageUrl = URL.createObjectURL(blob);
      } catch {
        this.headerImageUrl = '';
      }
    }

    if (data.hasHeaderLogo) {
      try {
        const blob = await this._service.getFileBlob('HeaderLogo');
        this.headerLogoUrl = URL.createObjectURL(blob);
      } catch {
        this.headerLogoUrl = '';
      }
    }
  }

  private revokeUrls(): void {
    if (this.headerImageUrl) URL.revokeObjectURL(this.headerImageUrl);
    if (this.headerLogoUrl) URL.revokeObjectURL(this.headerLogoUrl);
    this.headerImageUrl = '';
    this.headerLogoUrl = '';
  }
}
