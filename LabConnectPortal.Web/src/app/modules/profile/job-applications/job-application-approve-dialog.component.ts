import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';

export interface JobApplicationApproveDialogData {
  applicantName: string;
}

@Component({
  selector: 'app-job-application-approve-dialog',
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
  templateUrl: './job-application-approve-dialog.component.html',
})
export class JobApplicationApproveDialogComponent {
  form = new FormGroup({
    reviewNotes: new FormControl(''),
    visitScheduledAt: new FormControl(''),
    visitLocation: new FormControl(''),
  });

  constructor(
    private _dialogRef: MatDialogRef<JobApplicationApproveDialogComponent>,
    @Inject(MAT_DIALOG_DATA) readonly data: JobApplicationApproveDialogData,
  ) {}

  submit(): void {
    const visitAt = this.form.controls.visitScheduledAt.value;
    this._dialogRef.close({
      reviewNotes: this.form.controls.reviewNotes.value ?? '',
      visitScheduledAt: visitAt ? new Date(visitAt).toISOString() : null,
      visitLocation: this.form.controls.visitLocation.value ?? '',
    });
  }

  cancel(): void {
    this._dialogRef.close(null);
  }
}
