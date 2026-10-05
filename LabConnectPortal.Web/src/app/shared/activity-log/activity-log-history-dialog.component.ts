import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { ActivityLogHistoryComponent } from './activity-log-history.component';

export interface ActivityLogHistoryDialogData {
  entityName: string;
  recordKey: string;
}

@Component({
  selector: 'app-activity-log-history-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
    ActivityLogHistoryComponent,
  ],
  template: `
    <base-dialog [title]="'modules.activityLog.historyTitle' | transloco">
      <div class="min-w-[min(100%,36rem)] max-h-[min(70vh,32rem)] overflow-y-auto" dir="rtl">
        <app-activity-log-history
          [entityName]="data.entityName"
          [recordKey]="data.recordKey"
        />
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="close()">
          {{ 'shared.close' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class ActivityLogHistoryDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ActivityLogHistoryDialogData,
    private _dialogRef: MatDialogRef<ActivityLogHistoryDialogComponent>,
  ) {}

  close(): void {
    this._dialogRef.close();
  }
}
