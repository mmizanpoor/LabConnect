import { Component, Inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { TranslocoPipe } from '@jsverse/transloco';

export interface SpecialOfferRejectDialogData {
  requestId: number;
  userDisplayName: string;
}

@Component({
  selector: 'app-special-offer-reject-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
  ],
  templateUrl: './special-offer-reject-dialog.component.html',
})
export class SpecialOfferRejectDialogComponent {
  reasonControl = new FormControl('', [
    Validators.required,
    Validators.maxLength(2000),
  ]);

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: SpecialOfferRejectDialogData,
    private _dialogRef: MatDialogRef<SpecialOfferRejectDialogComponent>
  ) {}

  cancel(): void {
    this._dialogRef.close(null);
  }

  confirm(): void {
    if (this.reasonControl.invalid) {
      this.reasonControl.markAsTouched();
      return;
    }

    this._dialogRef.close({
      requestId: this.data.requestId,
      reason: this.reasonControl.value?.trim() ?? '',
    });
  }
}
