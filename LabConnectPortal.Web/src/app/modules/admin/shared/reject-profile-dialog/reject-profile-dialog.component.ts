import { Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';

@Component({
  selector: 'app-reject-profile-dialog',
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
    <base-dialog [title]="'modules.admin.shared.rejectTitle' | transloco">
      <div class="min-w-[320px]" dir="rtl">
        <base-form-field
          layout="floating"
          [required]="true"
          [multiline]="true"
          [rows]="5"
          [label]="'modules.admin.shared.rejectReason' | transloco"
          [control]="reasonControl"
        />
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button color="warn" size="sm" [disabled]="reasonControl.invalid" (click)="confirm()">
          {{ 'modules.admin.shared.rejectConfirm' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class RejectProfileDialogComponent {
  private _dialogRef = inject(MatDialogRef<RejectProfileDialogComponent>);
  reasonControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(3)],
  });

  cancel(): void {
    this._dialogRef.close();
  }

  confirm(): void {
    if (this.reasonControl.invalid) {
      this.reasonControl.markAsTouched();
      return;
    }
    this._dialogRef.close(this.reasonControl.value.trim());
  }
}
