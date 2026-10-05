import { Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';

export interface ShipOrderDialogResult {
  shippingCompany?: string;
  trackingCode?: string;
}

@Component({
  selector: 'app-ship-order-dialog',
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
    <base-dialog [title]="'modules.profile.productOrders.shipOrder' | transloco">
      <div class="flex min-w-[min(100%,22rem)] flex-col gap-3" dir="rtl">
        <p class="m-0 text-sm leading-7 text-slate-600">
          {{ 'modules.profile.productOrders.confirmShip' | transloco }}
        </p>
        <base-form-field
          class="w-full"
          layout="floating"
          icon="local_shipping"
          [label]="'modules.profile.productOrders.shipCompany' | transloco"
          [control]="shippingCompany"
        />
        <base-form-field
          class="w-full"
          layout="floating"
          icon="qr_code"
          [label]="'modules.profile.productOrders.shipTracking' | transloco"
          [control]="trackingCode"
        />
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button color="primary" size="sm" (click)="confirm()">
          {{ 'modules.profile.productOrders.shipOrder' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class ShipOrderDialogComponent {
  private _dialogRef = inject(MatDialogRef<ShipOrderDialogComponent, ShipOrderDialogResult | null>);

  shippingCompany = new FormControl('', { nonNullable: true });
  trackingCode = new FormControl('', { nonNullable: true });

  cancel(): void {
    this._dialogRef.close(null);
  }

  confirm(): void {
    this._dialogRef.close({
      shippingCompany: this.shippingCompany.value.trim() || undefined,
      trackingCode: this.trackingCode.value.trim() || undefined,
    });
  }
}
