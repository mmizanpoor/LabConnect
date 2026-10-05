import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { ProductCatalogService } from '../product-catalog.service';
import { ProductCategoryDto } from '../product-catalog.types';

export interface CategoryFormDialogData {
  category?: ProductCategoryDto;
}

@Component({
  selector: 'app-category-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    TranslocoPipe,
    BaseFormFieldComponent,
    BaseButtonComponent,
    BaseDialogComponent,
    BaseCheckboxComponent,
  ],
  template: `
    <base-dialog
      [title]="
        (data.category
          ? 'modules.admin.productCategories.editTitle'
          : 'modules.admin.productCategories.addTitle'
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
            [label]="'modules.admin.productCategories.fields.title' | transloco"
            [control]="form.controls.title"
          />
          <base-checkbox formControlName="acceptsResume">
            {{ 'modules.admin.productCategories.fields.acceptsResume' | transloco }}
          </base-checkbox>
        </form>
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button
          color="green"
          size="sm"
          [disabled]="saving || form.invalid"
          [loading]="saving"
          (click)="save()"
        >
          {{ 'shared.save' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class CategoryFormDialogComponent implements OnInit {
  saving = false;
  error = '';

  form = new FormGroup({
    title: new FormControl('', Validators.required),
    acceptsResume: new FormControl(false, { nonNullable: true }),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: CategoryFormDialogData = {},
    private _dialogRef: MatDialogRef<CategoryFormDialogComponent>,
    private _catalogService: ProductCatalogService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    if (this.data.category) {
      this.form.patchValue({
        title: this.data.category.title,
        acceptsResume: this.data.category.acceptsResume ?? false,
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
    const title = this.form.controls.title.value ?? '';
    const acceptsResume = this.form.controls.acceptsResume.value;
    try {
      const result = this.data.category
        ? await this._catalogService.updateCategory({
            productCategoryId: this.data.category.productCategoryId,
            title,
            acceptsResume,
          })
        : await this._catalogService.createCategory({ title, acceptsResume });

      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.productCategories.errors.saveFailed'),
        );
      }

      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.productCategories.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}
