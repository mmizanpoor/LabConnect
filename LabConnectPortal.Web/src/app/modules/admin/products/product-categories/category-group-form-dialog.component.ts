import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
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
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { ProductCatalogService } from '../product-catalog.service';
import {
  ProductCategoryDto,
  ProductCategoryGroupDto,
} from '../product-catalog.types';

export interface CategoryGroupFormDialogData {
  group?: ProductCategoryGroupDto;
}

@Component({
  selector: 'app-category-group-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    TranslocoPipe,
    BaseFormFieldComponent,
    BaseButtonComponent,
    BaseCheckboxComponent,
    BaseDialogComponent,
    BaseImageUploadComponent,
  ],
  template: `
    <base-dialog
      [title]="
        (data.group
          ? 'modules.admin.productCategories.categoryGroups.editTitle'
          : 'modules.admin.productCategories.categoryGroups.addTitle'
        ) | transloco
      "
    >
      <div class="min-w-[360px]" dir="rtl">
        @if (error) {
          <div class="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
            {{ error }}
          </div>
        }
        <form [formGroup]="form" class="flex flex-col gap-5">
          <base-form-field
            layout="floating"
            [required]="true"
            [label]="
              'modules.admin.productCategories.categoryGroups.fields.name'
                | transloco
            "
            [control]="form.controls.name"
          />
          <base-checkbox formControlName="showOnHomePage">
            {{
              'modules.admin.productCategories.categoryGroups.fields.showOnHomePage'
                | transloco
            }}
          </base-checkbox>
          <base-form-field
            layout="floating"
            [select]="true"
            [multiple]="true"
            [options]="categoryOptions"
            [label]="
              'modules.admin.productCategories.categoryGroups.fields.categories'
                | transloco
            "
            [control]="form.controls.productCategoryIds"
          />
        </form>

        <base-image-upload
          class="mt-5"
          [label]="
            'modules.admin.productCategories.categoryGroups.fields.homePageImage'
              | transloco
          "
          previewShape="circle"
          [previewSize]="96"
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
          [disabled]="saving || uploadingImage || form.invalid || loadingOptions"
          [loading]="saving || uploadingImage"
          (click)="save()"
        >
          {{ 'shared.save' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class CategoryGroupFormDialogComponent implements OnInit, OnDestroy {
  saving = false;
  uploadingImage = false;
  loadingOptions = false;
  error = '';
  imagePreviewUrl = '';
  imageFileName = '';
  categoryOptions: BaseFormSelectOption[] = [];
  private _pendingFile: File | null = null;
  private _objectPreviewUrl = '';

  form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    showOnHomePage: new FormControl(false, { nonNullable: true }),
    productCategoryIds: new FormControl<number[]>([], { nonNullable: true }),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: CategoryGroupFormDialogData = {},
    private _dialogRef: MatDialogRef<CategoryGroupFormDialogComponent>,
    private _catalogService: ProductCatalogService,
    private _localization: LocalizationService,
  ) {}

  async ngOnInit(): Promise<void> {
    if (this.data.group) {
      this.form.patchValue({
        name: this.data.group.name,
        showOnHomePage: this.data.group.showOnHomePage ?? false,
        productCategoryIds: this.data.group.categories.map(
          (c) => c.productCategoryId,
        ),
      });
      if (this.data.group.homePageImagePath) {
        this.imagePreviewUrl =
          this._catalogService.getCategoryGroupHomePageImageUrl(
            this.data.group.homePageImagePath,
          );
      }
    }
    await this.loadCategoryOptions();
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
      if (this.data.group?.homePageImagePath) {
        this.imagePreviewUrl =
          this._catalogService.getCategoryGroupHomePageImageUrl(
            this.data.group.homePageImagePath,
          );
      } else {
        this.imagePreviewUrl = '';
      }
      return;
    }

    if (
      !this.data.group?.productCategoryGroupId ||
      !this.data.group.hasHomePageImage
    ) {
      this.imagePreviewUrl = '';
      return;
    }

    this.uploadingImage = true;
    this.error = '';
    try {
      const result =
        await this._catalogService.deleteCategoryGroupHomePageImage(
          this.data.group.productCategoryGroupId,
        );
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.admin.productCategories.categoryGroups.errors.uploadFailed',
            ),
        );
      }
      this.data.group = result.data;
      this.imagePreviewUrl = '';
      this.imageFileName = '';
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.productCategories.categoryGroups.errors.uploadFailed',
            );
    } finally {
      this.uploadingImage = false;
    }
  }

  async save(): Promise<void> {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const name = this.form.controls.name.value.trim();
    const showOnHomePage = this.form.controls.showOnHomePage.value;
    const productCategoryIds =
      this.form.controls.productCategoryIds.value ?? [];

    try {
      const result = this.data.group
        ? await this._catalogService.updateCategoryGroup({
            productCategoryGroupId: this.data.group.productCategoryGroupId,
            name,
            showOnHomePage,
            productCategoryIds,
          })
        : await this._catalogService.createCategoryGroup({
            name,
            showOnHomePage,
            productCategoryIds,
          });

      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.admin.productCategories.categoryGroups.errors.saveFailed',
            ),
        );
      }

      let group = result.data;
      if (this._pendingFile) {
        this.uploadingImage = true;
        const uploadResult =
          await this._catalogService.uploadCategoryGroupHomePageImage(
            group.productCategoryGroupId,
            this._pendingFile,
          );
        this.uploadingImage = false;
        if (!uploadResult.success || !uploadResult.data) {
          throw new Error(
            uploadResult.message ??
              this._localization.translate(
                'modules.admin.productCategories.categoryGroups.errors.uploadFailed',
              ),
          );
        }
        group = uploadResult.data;
      }

      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.productCategories.categoryGroups.errors.saveFailed',
            );
    } finally {
      this.saving = false;
      this.uploadingImage = false;
    }
  }

  private async loadCategoryOptions(): Promise<void> {
    this.loadingOptions = true;
    try {
      const result = await this._catalogService.getCategoryOptions();
      if (!result.success || !result.data) {
        this.error =
          result.message ??
          this._localization.translate(
            'modules.admin.productCategories.categoryGroups.errors.loadFailed',
          );
        return;
      }

      const currentGroupId = this.data.group?.productCategoryGroupId ?? null;
      const assignable = result.data.filter(
        (c: ProductCategoryDto) =>
          !c.productCategoryGroupId ||
          c.productCategoryGroupId === currentGroupId,
      );

      this.categoryOptions = assignable.map((c) => ({
        value: c.productCategoryId,
        label: c.title,
      }));
    } finally {
      this.loadingOptions = false;
    }
  }

  private revokeObjectPreview(): void {
    if (this._objectPreviewUrl) {
      URL.revokeObjectURL(this._objectPreviewUrl);
      this._objectPreviewUrl = '';
    }
  }
}
