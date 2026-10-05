import { Component, Inject, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';

export interface BaseConfirmDialogData {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  warnConfirm?: boolean;
  /** بدنه با آیکون و استایل حرفه‌ای‌تر */
  premium?: boolean;
  icon?: string;
}

@Component({
  selector: 'base-confirm-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
  ],
  template: `
    <base-dialog [title]="data.title">
      @if (data.premium) {
        <div class="confirm-premium" dir="rtl">
          <div class="confirm-premium__intro">
            <span class="confirm-premium__badge" aria-hidden="true">
              <mat-icon>{{ data.icon || 'warning_amber' }}</mat-icon>
            </span>
            <p class="confirm-premium__message">{{ data.message }}</p>
          </div>
        </div>
      } @else {
        <p class="m-0 text-sm leading-7 text-slate-600" dir="rtl">
          {{ data.message }}
        </p>
      }

      <div
        baseDialogActions
        class="flex items-center justify-end gap-2"
        [class.confirm-premium__actions]="data.premium"
      >
        <button mat-button type="button" (click)="close(false)">
          {{ data.cancelLabel || ('shared.cancel' | transloco) }}
        </button>
        <div class="min-w-28">
          <base-button
            [color]="data.warnConfirm === false ? 'primary' : 'warn'"
            (click)="close(true)"
          >
            {{ data.confirmLabel || ('shared.delete' | transloco) }}
          </base-button>
        </div>
      </div>
    </base-dialog>
  `,
  styles: `
    .confirm-premium {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .confirm-premium__intro {
      display: flex;
      align-items: flex-start;
      gap: 0.85rem;
    }

    .confirm-premium__badge {
      display: grid;
      place-items: center;
      width: 2.5rem;
      height: 2.5rem;
      flex-shrink: 0;
      border-radius: 5px;
      background: linear-gradient(160deg, #b45309 0%, #d97706 100%);
      color: #fff;
      box-shadow: 0 6px 16px rgba(180, 83, 9, 0.22);
    }

    .confirm-premium__badge mat-icon {
      width: 1.25rem !important;
      height: 1.25rem !important;
      font-size: 1.25rem !important;
    }

    .confirm-premium__message {
      margin: 0;
      padding-top: 0.15rem;
      font-size: 0.875rem;
      font-weight: 500;
      line-height: 1.85;
      color: #334155;
    }

    .confirm-premium__actions {
      margin-top: 0.35rem;
    }
  `,
})
export class BaseConfirmDialogComponent {
  private _dialogRef = inject(MatDialogRef<BaseConfirmDialogComponent, boolean>);

  constructor(@Inject(MAT_DIALOG_DATA) readonly data: BaseConfirmDialogData) {}

  close(confirmed: boolean): void {
    this._dialogRef.close(confirmed);
  }
}
