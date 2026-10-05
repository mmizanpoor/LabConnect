import { Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';

@Component({
  selector: 'app-product-reject-dialog',
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
    <base-dialog [title]="'modules.admin.products.rejectDialogTitle' | transloco">
      <div class="min-w-[min(100vw-2rem,24rem)]" dir="rtl">
        <p class="mb-3 mt-0 text-sm leading-7 text-slate-600">
          {{ 'modules.admin.products.rejectDialogHint' | transloco }}
        </p>
        <base-form-field
          layout="floating"
          [required]="true"
          [multiline]="true"
          [rows]="5"
          [label]="'modules.admin.products.rejectionReason' | transloco"
          [control]="reasonControl"
        />
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button color="warn" size="sm" [disabled]="reasonControl.invalid" (click)="confirm()">
          {{ 'modules.admin.products.rejectConfirm' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class ProductRejectDialogComponent {
  private readonly _dialogRef = inject(MatDialogRef<ProductRejectDialogComponent, string | null>);

  reasonControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(3)],
  });

  cancel(): void {
    this._dialogRef.close(null);
  }

  confirm(): void {
    if (this.reasonControl.invalid) {
      this.reasonControl.markAsTouched();
      return;
    }
    this._dialogRef.close(this.reasonControl.value.trim());
  }
}
