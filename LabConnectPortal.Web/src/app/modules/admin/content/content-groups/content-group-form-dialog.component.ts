import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { ContentService } from '../content.service';
import { ContentGroupDto } from '../content.types';

export interface ContentGroupFormDialogData {
  group?: ContentGroupDto;
}

@Component({
  selector: 'app-content-group-form-dialog',
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
    <base-dialog
      [title]="
        (data.group
          ? 'modules.admin.contentGroups.editTitle'
          : 'modules.admin.contentGroups.addTitle'
        ) | transloco
      "
    >
      <div class="min-w-[320px]" dir="rtl">
        @if (error) {
          <div class="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{{ error }}</div>
        }
        <form [formGroup]="form" class="flex flex-col gap-5">
          <base-form-field
            layout="floating"
            [required]="true"
            [label]="'modules.admin.contentGroups.fields.title' | transloco"
            [control]="form.controls.title"
          />
        </form>
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button color="green" size="sm" [disabled]="saving || form.invalid" [loading]="saving" (click)="save()">
          {{ 'shared.save' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class ContentGroupFormDialogComponent implements OnInit {
  saving = false;
  error = '';

  form = new FormGroup({
    title: new FormControl('', Validators.required),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ContentGroupFormDialogData = {},
    private _dialogRef: MatDialogRef<ContentGroupFormDialogComponent>,
    private _contentService: ContentService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    if (this.data.group) {
      this.form.patchValue({
        title: this.data.group.title,
      });
    }
  }

  cancel(): void {
    this._dialogRef.close();
  }

  async save(): Promise<void> {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const title = this.form.controls.title.value ?? '';
    const code = this.data.group?.code ?? null;
    try {
      const result = this.data.group
        ? await this._contentService.updateGroup({
            contentGroupId: this.data.group.contentGroupId,
            code,
            title,
          })
        : await this._contentService.createGroup({ code: null, title });

      if (!result.success) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.contentGroups.errors.saveFailed'),
        );
      }

      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.contentGroups.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}
