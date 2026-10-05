import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { ProductCatalogService } from '../product-catalog.service';
import { BrandDto } from '../product-catalog.types';

export interface BrandFormDialogData {
  brand?: BrandDto;
}

@Component({
  selector: 'app-brand-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    BaseCheckboxComponent,
    TranslocoPipe,
    BaseFormFieldComponent,
    BaseButtonComponent,
    BaseDialogComponent,
    BaseImageUploadComponent,
  ],
  template: `
    <base-dialog
      [title]="
        (data.brand ? 'modules.admin.brands.editTitle' : 'modules.admin.brands.addTitle') | transloco
      "
    >
      <div class="min-w-[320px]" dir="rtl">
        @if (error) {
          <div class="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{{ error }}</div>
        }
        <form [formGroup]="form" class="flex flex-col gap-5">
          <base-form-field
            layout="floating"
            [required]="true"
            [label]="'modules.admin.brands.fields.title' | transloco"
            [control]="form.controls.title"
          />
          <base-checkbox formControlName="showOnHomePage">
            {{ 'modules.admin.brands.fields.showOnHomePage' | transloco }}
          </base-checkbox>
        </form>

        <base-image-upload
          class="mt-5"
          [label]="'modules.admin.brands.fields.image' | transloco"
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
export class BrandFormDialogComponent implements OnInit, OnDestroy {
  saving = false;
  uploadingImage = false;
  error = '';
  imagePreviewUrl = '';
  imageFileName = '';
  private _pendingFile: File | null = null;
  private _objectPreviewUrl = '';

  form = new FormGroup({
    title: new FormControl('', Validators.required),
    showOnHomePage: new FormControl(false, { nonNullable: true }),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: BrandFormDialogData = {},
    private _dialogRef: MatDialogRef<BrandFormDialogComponent>,
    private _catalogService: ProductCatalogService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    if (this.data.brand) {
      this.form.patchValue({
        title: this.data.brand.title,
        showOnHomePage: this.data.brand.showOnHomePage ?? false,
      });
      if (this.data.brand.imagePath) {
        this.imagePreviewUrl = this._catalogService.getBrandImageUrl(this.data.brand.imagePath);
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
      if (this.data.brand?.imagePath) {
        this.imagePreviewUrl = this._catalogService.getBrandImageUrl(this.data.brand.imagePath);
      } else {
        this.imagePreviewUrl = '';
      }
      return;
    }

    if (!this.data.brand?.brandId || !this.data.brand.hasImage) {
      this.imagePreviewUrl = '';
      return;
    }

    this.uploadingImage = true;
    this.error = '';
    try {
      const result = await this._catalogService.deleteBrandImage(this.data.brand.brandId);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.brands.errors.uploadFailed'),
        );
      }
      this.data.brand = result.data;
      this.imagePreviewUrl = '';
      this.imageFileName = '';
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.admin.brands.errors.uploadFailed');
    } finally {
      this.uploadingImage = false;
    }
  }

  async save(): Promise<void> {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const title = this.form.controls.title.value ?? '';
    const showOnHomePage = this.form.controls.showOnHomePage.value;
    try {
      const result = this.data.brand
        ? await this._catalogService.updateBrand({
            brandId: this.data.brand.brandId,
            title,
            showOnHomePage,
          })
        : await this._catalogService.createBrand({ title, showOnHomePage });

      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.admin.brands.errors.saveFailed'));
      }

      let brand = result.data;
      if (this._pendingFile) {
        this.uploadingImage = true;
        const uploadResult = await this._catalogService.uploadBrandImage(brand.brandId, this._pendingFile);
        this.uploadingImage = false;
        if (!uploadResult.success || !uploadResult.data) {
          throw new Error(
            uploadResult.message ?? this._localization.translate('modules.admin.brands.errors.uploadFailed'),
          );
        }
        brand = uploadResult.data;
        this._pendingFile = null;
        this.imageFileName = '';
        this.revokeObjectPreview();
      }

      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.admin.brands.errors.saveFailed');
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

