import { Component, Inject, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';

export interface DeleteUsefulLinkDialogData {
  title: string;
}

@Component({
  selector: 'app-delete-useful-link-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
  ],
  template: `
    <base-dialog [title]="'modules.admin.settings.deleteUsefulLinkTitle' | transloco">
      <p class="m-0 text-sm leading-7 text-slate-600">
        {{
          'modules.admin.settings.confirmDeleteUsefulLink'
            | transloco: { title: data.title }
        }}
      </p>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="close(false)">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button color="warn" size="sm" (click)="close(true)">
          {{ 'shared.delete' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class DeleteUsefulLinkDialogComponent {
  private _dialogRef = inject(MatDialogRef<DeleteUsefulLinkDialogComponent, boolean>);

  constructor(@Inject(MAT_DIALOG_DATA) readonly data: DeleteUsefulLinkDialogData) {}

  close(confirmed: boolean): void {
    this._dialogRef.close(confirmed);
  }
}
