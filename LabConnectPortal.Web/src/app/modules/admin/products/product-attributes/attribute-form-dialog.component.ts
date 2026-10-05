import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { ProductCatalogService } from '../product-catalog.service';
import { ProductAttributeDto, ProductCategoryDto, AttributeFieldType } from '../product-catalog.types';

export interface AttributeFormDialogData {
  categories: ProductCategoryDto[];
  attribute?: ProductAttributeDto;
}

@Component({
  selector: 'app-attribute-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    TranslocoPipe,
    BaseFormFieldComponent,
    BaseButtonComponent,
    BaseDialogComponent,
  ],
  template: `
    <base-dialog
      [title]="
        (data.attribute
          ? 'modules.admin.productAttributes.editTitle'
          : 'modules.admin.productAttributes.addTitle'
        ) | transloco
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
            [select]="true"
            [options]="categoryOptions"
            [label]="'modules.admin.productAttributes.fields.category' | transloco"
            [control]="form.controls.productCategoryId"
          />
          <base-form-field
            layout="floating"
            [required]="true"
            [label]="'modules.admin.productAttributes.fields.title' | transloco"
            [control]="form.controls.title"
          />
          <base-form-field
            layout="floating"
            [select]="true"
            [options]="fieldTypeOptions"
            [label]="'modules.admin.productAttributes.fields.fieldType' | transloco"
            [control]="form.controls.fieldType"
          />
        </form>
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button color="green" size="sm" [disabled]="saving || form.invalid" [loading]="saving" (click)="save()">
          {{ 'shared.save' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class AttributeFormDialogComponent implements OnInit {
  saving = false;
  error = '';

  form = new FormGroup({
    productCategoryId: new FormControl<number | null>(null, Validators.required),
    title: new FormControl('', Validators.required),
    fieldType: new FormControl<AttributeFieldType | null>(null),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: AttributeFormDialogData = { categories: [] },
    private _dialogRef: MatDialogRef<AttributeFormDialogComponent>,
    private _catalogService: ProductCatalogService,
    private _localization: LocalizationService,
  ) {}

  get categoryOptions(): BaseFormSelectOption[] {
    return this.data.categories.map((category) => ({
      value: category.productCategoryId,
      label: category.title,
    }));
  }

  get fieldTypeOptions(): BaseFormSelectOption[] {
    return [
      {
        value: null,
        label: this._localization.translate('modules.admin.productAttributes.fieldTypes.none'),
      },
      {
        value: 'Date',
        label: this._localization.translate('modules.admin.productAttributes.fieldTypes.date'),
      },
      {
        value: 'ExpiryDate',
        label: this._localization.translate('modules.admin.productAttributes.fieldTypes.expiryDate'),
      },
    ];
  }

  ngOnInit(): void {
    if (this.data.attribute) {
      this.form.patchValue({
        productCategoryId: this.data.attribute.productCategoryId,
        title: this.data.attribute.title,
        fieldType: (this.data.attribute.fieldType as AttributeFieldType | null) ?? null,
      });
    }
  }

  cancel(): void {
    this._dialogRef.close();
  }

  async save(): Promise<void> {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();
    try {
      const result = this.data.attribute
        ? await this._catalogService.updateAttribute({
            productAttributeId: this.data.attribute.productAttributeId,
            productCategoryId: value.productCategoryId!,
            title: value.title ?? '',
            fieldType: value.fieldType,
          })
        : await this._catalogService.createAttribute({
            productCategoryId: value.productCategoryId!,
            title: value.title ?? '',
            fieldType: value.fieldType,
          });

      if (!result.success) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.productAttributes.errors.saveFailed'),
        );
      }
      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.productAttributes.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}
