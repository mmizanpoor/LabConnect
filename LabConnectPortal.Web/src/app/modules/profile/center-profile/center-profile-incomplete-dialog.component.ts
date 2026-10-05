import { Component, Inject, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';

export interface CenterProfileIncompleteItem {
  labelKey: string;
  tabLabelKey: string;
}

export interface CenterProfileIncompleteDialogData {
  title: string;
  items: CenterProfileIncompleteItem[];
}

@Component({
  selector: 'app-center-profile-incomplete-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
  ],
  template: `
    <base-dialog [title]="data.title">
      <p class="m-0 mb-4 text-sm leading-7 text-slate-600">
        {{ 'modules.profile.center.incompleteDialog.intro' | transloco }}
      </p>

      <ul class="m-0 grid list-none grid-cols-1 gap-2 p-0 sm:grid-cols-2">
        @for (item of data.items; track item.labelKey) {
          <li
            class="flex min-w-0 items-start gap-2 rounded-[5px] border border-rose-100 bg-rose-50/70 px-3 py-2.5 text-sm text-rose-700"
          >
            <mat-icon class="mt-0.5 !h-4 !w-4 shrink-0 !text-[1rem] text-rose-500">error_outline</mat-icon>
            <div class="min-w-0 flex-1">
              <div class="font-semibold">{{ item.labelKey | transloco }}</div>
              <div class="mt-0.5 text-xs text-rose-500/90">
                {{ item.tabLabelKey | transloco }}
              </div>
            </div>
          </li>
        }
      </ul>

      <div baseDialogActions class="flex justify-end">
        <div class="w-28">
          <base-button color="blue" (click)="close()">
            {{ 'shared.close' | transloco }}
          </base-button>
        </div>
      </div>
    </base-dialog>
  `,
})
export class CenterProfileIncompleteDialogComponent {
  private _dialogRef = inject(MatDialogRef<CenterProfileIncompleteDialogComponent>);

  constructor(@Inject(MAT_DIALOG_DATA) readonly data: CenterProfileIncompleteDialogData) {}

  close(): void {
    this._dialogRef.close();
  }
}
