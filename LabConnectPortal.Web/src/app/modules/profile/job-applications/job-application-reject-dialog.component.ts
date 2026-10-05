import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';

export interface JobApplicationRejectDialogData {
  applicantName: string;
}

@Component({
  selector: 'app-job-application-reject-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    TranslocoPipe,
    BaseButtonComponent,
  ],
  templateUrl: './job-application-reject-dialog.component.html',
})
export class JobApplicationRejectDialogComponent {
  form = new FormGroup({
    reviewNotes: new FormControl('', Validators.required),
  });

  constructor(
    private _dialogRef: MatDialogRef<JobApplicationRejectDialogComponent>,
    @Inject(MAT_DIALOG_DATA) readonly data: JobApplicationRejectDialogData,
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.controls.reviewNotes.markAsTouched();
      return;
    }
    this._dialogRef.close({
      reviewNotes: this.form.controls.reviewNotes.value ?? '',
    });
  }

  cancel(): void {
    this._dialogRef.close(null);
  }
}
