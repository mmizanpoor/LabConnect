import { Component, Input, inject, output } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';

/** Use as `panelClass` when opening MatDialog so the header is edge-to-edge. */
export const BASE_DIALOG_PANEL_CLASS = 'base-dialog-panel';

/**
 * Shared dialog chrome: colored header (title + close) like the product dialogs.
 *
 * Usage:
 * ```html
 * <base-dialog [title]="'...' | transloco">
 *   ...body...
 *   <div baseDialogActions class="flex justify-end gap-2">
 *     ...
 *   </div>
 * </base-dialog>
 * ```
 *
 * Open with: `panelClass: BASE_DIALOG_PANEL_CLASS`
 */
@Component({
  selector: 'base-dialog',
  standalone: true,
  imports: [MatIconModule, TranslocoPipe],
  templateUrl: './base-dialog.component.html',
  host: { class: 'block h-full' },
})
export class BaseDialogComponent {
  private _dialogRef = inject(MatDialogRef, { optional: true });

  /** Dialog title shown in the header bar. */
  @Input() title = '';

  /** Show the header close (X) button. */
  @Input() showClose = true;

  /** Emitted when the header close button is clicked (before MatDialogRef.close). */
  readonly closed = output<void>();

  close(): void {
    this.closed.emit();
    this._dialogRef?.close();
  }
}
