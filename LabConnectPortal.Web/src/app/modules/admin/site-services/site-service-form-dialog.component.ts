import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { SiteServicesService } from '../dashboard/site-services.service';
import { SiteServiceDto } from '../dashboard/site-services.types';

export interface SiteServiceFormDialogData {
  service?: SiteServiceDto;
}

@Component({
  selector: 'app-site-service-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatCheckboxModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseDialogComponent,
    BaseCheckboxComponent,
    BaseImageUploadComponent,
  ],
  template: `
    <base-dialog
      [title]="(data.service ? 'modules.admin.dashboard.services.edit' : 'modules.admin.dashboard.services.add') | transloco"
    >
      <div class="min-w-[320px]" dir="rtl">
        @if (error) {
          <div class="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{{ error }}</div>
        }
        <form [formGroup]="form" class="flex flex-col gap-4">
          <base-form-field
            layout="floating"
            [required]="true"
            [label]="'modules.admin.dashboard.services.columns.title' | transloco"
            [control]="form.controls.title"
          />
          <base-form-field
            layout="floating"
            [label]="'modules.admin.dashboard.services.columns.linkUrl' | transloco"
            [control]="form.controls.linkUrl"
          />
          <base-form-field
            layout="floating"
            type="number"
            [label]="'modules.admin.dashboard.services.columns.sortOrder' | transloco"
            [control]="form.controls.sortOrder"
          />
          <base-checkbox formControlName="isActive">
            {{ 'modules.admin.dashboard.services.columns.isActive' | transloco }}
          </base-checkbox>
        </form>

        <base-image-upload
          class="mt-5"
          [label]="'modules.admin.dashboard.services.columns.image' | transloco"
          previewShape="square"
          [previewSize]="120"
          [clearable]="true"
          [imageUrl]="imagePreviewUrl"
          [fileName]="imageFileName"
          [loading]="uploadingImage"
          (fileSelected)="onImageSelected($event)"
          (cleared)="removeImage()"
        />
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button
          color="green"
          size="sm"
          [disabled]="saving || uploadingImage || form.invalid"
          [loading]="saving || uploadingImage"
          (click)="save()"
        >
          {{ 'shared.save' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class SiteServiceFormDialogComponent implements OnInit, OnDestroy {
  saving = false;
  uploadingImage = false;
  error = '';
  imagePreviewUrl = '';
  imageFileName = '';
  private _pendingFile: File | null = null;
  private _objectPreviewUrl = '';

  form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    linkUrl: new FormControl(''),
    sortOrder: new FormControl(0, { nonNullable: true }),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: SiteServiceFormDialogData,
    private _dialogRef: MatDialogRef<SiteServiceFormDialogComponent>,
    private _service: SiteServicesService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    if (this.data.service) {
      this.form.patchValue({
        title: this.data.service.title,
        linkUrl: this.data.service.linkUrl ?? '',
        sortOrder: this.data.service.sortOrder,
        isActive: this.data.service.isActive,
      });
      if (this.data.service.imagePath) {
        this.imagePreviewUrl = this._service.getImageUrl(this.data.service.imagePath);
      }
    }
  }

  ngOnDestroy(): void {
    this.revokeObjectPreview();
  }

  cancel(): void {
    this._dialogRef.close();
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
      this.imagePreviewUrl = this.data.service?.imagePath ? this._service.getImageUrl(this.data.service.imagePath) : '';
      return;
    }

    if (!this.data.service?.siteServiceId || !this.data.service.imagePath) {
      this.imagePreviewUrl = '';
      return;
    }

    this.uploadingImage = true;
    this.error = '';
    try {
      const result = await this._service.deleteImage(this.data.service.siteServiceId);
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.admin.dashboard.services.saveFailed'));
      }
      this.data.service = result.data;
      this.imagePreviewUrl = '';
      this.imageFileName = '';
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.admin.dashboard.services.saveFailed');
    } finally {
      this.uploadingImage = false;
    }
  }

  async save(): Promise<void> {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    try {
      const value = this.form.getRawValue();
      const result = this.data.service
        ? await this._service.update({ ...value, siteServiceId: this.data.service.siteServiceId })
        : await this._service.create(value);

      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.admin.dashboard.services.saveFailed'));
      }
      let service = result.data;
      if (this._pendingFile) {
        this.uploadingImage = true;
        const uploadResult = await this._service.uploadImage(service.siteServiceId, this._pendingFile);
        this.uploadingImage = false;
        if (!uploadResult.success || !uploadResult.data) {
          throw new Error(uploadResult.message ?? this._localization.translate('modules.admin.dashboard.services.saveFailed'));
        }
        service = uploadResult.data;
        this._pendingFile = null;
        this.imageFileName = '';
        this.revokeObjectPreview();
      }
      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.admin.dashboard.services.saveFailed');
    } finally {
      this.saving = false;
      this.uploadingImage = false;
    }
  }

  private revokeObjectPreview(): void {
    if (this._objectPreviewUrl) {
      URL.revokeObjectURL(this._objectPreviewUrl);
      this._objectPreviewUrl = '';
    }
  }
}
