import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { ProductExpertReviewDto } from '../../product-catalog.types';

export interface ExpertReviewFormDialogResult {
  title: string;
  description: string;
}

@Component({
  selector: 'app-expert-review-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
    BaseFormFieldComponent,
  ],
  template: `
    <base-dialog [title]="'modules.admin.products.addReview' | transloco">
      <div class="min-w-[min(100%,22rem)]" dir="rtl">
        <form [formGroup]="form" class="flex flex-col gap-5">
          <base-form-field
            class="w-full"
            layout="floating"
            [required]="true"
            [label]="'modules.admin.products.fields.reviewTitle' | transloco"
            [control]="form.controls.title"
          />
          <base-form-field
            class="w-full"
            layout="floating"
            [required]="true"
            [multiline]="true"
            [rows]="4"
            [label]="'modules.admin.products.fields.reviewDescription' | transloco"
            [control]="form.controls.description"
          />
        </form>
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button
          color="green"
          size="sm"
          [disabled]="form.invalid"
          (click)="save()"
        >
          {{ 'shared.add' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class ExpertReviewFormDialogComponent {
  form = new FormGroup({
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ProductExpertReviewDto | null,
    private _dialogRef: MatDialogRef<
      ExpertReviewFormDialogComponent,
      ExpertReviewFormDialogResult | null
    >,
  ) {
    if (data) {
      this.form.patchValue({
        title: data.title ?? '',
        description: data.description ?? '',
      });
    }
  }

  cancel(): void {
    this._dialogRef.close(null);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this._dialogRef.close({
      title: this.form.controls.title.value.trim(),
      description: this.form.controls.description.value.trim(),
    });
  }
}
