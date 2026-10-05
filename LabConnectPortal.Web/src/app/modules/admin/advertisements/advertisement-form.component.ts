import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { AdvertisementsService } from './advertisements.service';
import { AdvertisementDto } from './advertisements.types';

@Component({
  selector: 'app-advertisement-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseCheckboxComponent,
    BaseFormFieldComponent,
    BaseImageUploadComponent,
  ],
  templateUrl: './advertisement-form.component.html',
})
export class AdvertisementFormComponent implements OnInit, OnDestroy {
  readonly entity = SystemEntity.ContentAds;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ContentAds);

  loading = false;
  saving = false;
  uploadingImage = false;
  error = '';
  isEdit = false;
  advertisementId = '';
  imagePreviewUrl = '';
  imageFileName = '';
  private _pendingFile: File | null = null;
  private _objectPreviewUrl = '';
  private _existing: AdvertisementDto | null = null;

  form = new FormGroup({
    title: new FormControl('', [Validators.required, Validators.maxLength(300)]),
    shortDescription: new FormControl('', Validators.maxLength(500)),
    startAt: new FormControl<Moment | null>(null, Validators.required),
    endAt: new FormControl<Moment | null>(null, Validators.required),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _service: AdvertisementsService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    this.advertisementId = this._route.snapshot.paramMap.get('id') ?? '';
    this.isEdit = this.advertisementId !== '' && this.advertisementId !== 'new';
    void this.load();
  }

  ngOnDestroy(): void {
    this.revokeObjectPreview();
  }

  async load(): Promise<void> {
    if (!this.isEdit) return;

    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getById(this.advertisementId);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.advertisements.errors.loadFailed'),
        );
      }
      this._existing = result.data;
      this.form.patchValue({
        title: result.data.title,
        shortDescription: result.data.shortDescription,
        startAt: this.toMoment(result.data.startAt),
        endAt: this.toMoment(result.data.endAt),
        isActive: result.data.isActive,
      });
      if (result.data.imagePath) {
        this.imagePreviewUrl = this._service.getImageUrl(result.data.imagePath);
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.advertisements.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  onImageSelected(file: File): void {
    this.revokeObjectPreview();
    this._pendingFile = file;
    this.imageFileName = file.name;
    this._objectPreviewUrl = URL.createObjectURL(file);
    this.imagePreviewUrl = this._objectPreviewUrl;
  }

  async removeImage(): Promise<void> {
    if (this._pendingFile) {
      this._pendingFile = null;
      this.imageFileName = '';
      this.revokeObjectPreview();
      if (this._existing?.imagePath) {
        this.imagePreviewUrl = this._service.getImageUrl(this._existing.imagePath);
      } else {
        this.imagePreviewUrl = '';
      }
      return;
    }

    if (!this.isEdit || !this._existing?.hasImage) {
      this.imagePreviewUrl = '';
      return;
    }

    this.uploadingImage = true;
    this.error = '';
    try {
      const result = await this._service.deleteImage(this.advertisementId);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.advertisements.errors.uploadFailed'),
        );
      }
      this._existing = result.data;
      this.imagePreviewUrl = '';
      this.imageFileName = '';
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.advertisements.errors.uploadFailed');
    } finally {
      this.uploadingImage = false;
    }
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = this._localization.translate('modules.admin.advertisements.errors.validationFailed');
      return;
    }

    const value = this.form.getRawValue();
    const startAt = this.toIso(value.startAt);
    const endAt = this.toIso(value.endAt);
    if (!startAt || !endAt) {
      this.error = this._localization.translate('modules.admin.advertisements.errors.validationFailed');
      return;
    }
    if (new Date(endAt) < new Date(startAt)) {
      this.error = this._localization.translate('modules.admin.advertisements.errors.dateRange');
      return;
    }

    this.saving = true;
    this.error = '';
    const command = {
      title: value.title ?? '',
      shortDescription: value.shortDescription ?? '',
      startAt,
      endAt,
      isActive: value.isActive,
    };

    try {
      const result = this.isEdit
        ? await this._service.update({ ...command, advertisementId: this.advertisementId })
        : await this._service.create(command);

      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.advertisements.errors.saveFailed'),
        );
      }

      let item = result.data;
      if (this._pendingFile) {
        this.uploadingImage = true;
        const upload = await this._service.uploadImage(item.advertisementId, this._pendingFile);
        this.uploadingImage = false;
        if (!upload.success || !upload.data) {
          throw new Error(
            upload.message ?? this._localization.translate('modules.admin.advertisements.errors.uploadFailed'),
          );
        }
        item = upload.data;
        this._pendingFile = null;
        this.revokeObjectPreview();
      }

      void this._router.navigate(['/admin/ads']);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.advertisements.errors.saveFailed');
    } finally {
      this.saving = false;
      this.uploadingImage = false;
    }
  }

  backToList(): void {
    void this._router.navigate(['/admin/ads']);
  }

  private toMoment(value?: string | null): Moment | null {
    if (!value) return null;
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.clone() : null;
  }

  private toIso(date: Moment | null): string | null {
    if (!date) return null;
    return date.clone().hours(0).minutes(0).seconds(0).milliseconds(0).toISOString();
  }

  private revokeObjectPreview(): void {
    if (this._objectPreviewUrl) {
      URL.revokeObjectURL(this._objectPreviewUrl);
      this._objectPreviewUrl = '';
    }
  }
}
