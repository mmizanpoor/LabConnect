import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { DeviceGroupsService } from '../device-groups.service';
import { DeviceGroupDto } from '../device-groups.types';

export interface DeviceGroupFormDialogData {
  group?: DeviceGroupDto;
}

@Component({
  selector: 'app-device-group-form-dialog',
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
  templateUrl: './device-group-form-dialog.component.html',
})
export class DeviceGroupFormDialogComponent {
  saving = false;
  error = '';
  readonly data: DeviceGroupFormDialogData;

  form = new FormGroup({
    title: new FormControl('', Validators.required),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) data: DeviceGroupFormDialogData | null,
    private _dialogRef: MatDialogRef<DeviceGroupFormDialogComponent>,
    private _service: DeviceGroupsService,
    private _localization: LocalizationService,
  ) {
    this.data = data ?? {};
    if (this.data.group) {
      this.form.patchValue({ title: this.data.group.title });
    }
  }

  cancel(): void {
    this._dialogRef.close();
  }

  async save(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.saving = true;
    this.error = '';
    const title = this.form.controls.title.value?.trim() ?? '';

    try {
      const result = this.data.group
        ? await this._service.update({ id: this.data.group.id, title })
        : await this._service.create({ title });

      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.profile.deviceGroups.errors.saveFailed'),
        );
      }
      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.deviceGroups.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}
