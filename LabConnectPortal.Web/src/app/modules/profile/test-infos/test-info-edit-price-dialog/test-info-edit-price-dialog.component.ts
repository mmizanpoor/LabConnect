import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { DeviceGroupsService } from '../../device-groups/device-groups.service';
import { KitGroupsService } from '../../kit-groups/kit-groups.service';
import { TestInfosService } from '../test-infos.service';
import { TestInfoListItemDto } from '../test-infos.types';

export interface TestInfoEditPriceDialogData {
  row: TestInfoListItemDto;
}

@Component({
  selector: 'app-test-info-edit-price-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './test-info-edit-price-dialog.component.html',
})
export class TestInfoEditPriceDialogComponent implements OnInit {
  loading = true;
  saving = false;
  error = '';

  deviceGroupOptions: BaseFormSelectOption[] = [];
  kitGroupOptions: BaseFormSelectOption[] = [];

  form = new FormGroup({
    cpnCode: new FormControl({ value: '', disabled: true }),
    nationalCode: new FormControl({ value: '', disabled: true }),
    approvePrice: new FormControl<number | null>(null, [Validators.min(0)]),
    deviceGroupId: new FormControl<number | null>(null),
    kitGroupId: new FormControl<number | null>(null),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: TestInfoEditPriceDialogData,
    private _dialogRef: MatDialogRef<TestInfoEditPriceDialogComponent>,
    private _service: TestInfosService,
    private _deviceGroupsService: DeviceGroupsService,
    private _kitGroupsService: KitGroupsService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    void this.load();
  }

  cancel(): void {
    this._dialogRef.close();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';

    try {
      const [detailResult, deviceResult, kitResult] = await Promise.all([
        this._service.getById(this.data.row.id),
        this._deviceGroupsService.getAll(),
        this._kitGroupsService.getAll(),
      ]);

      if (!detailResult.success || !detailResult.data) {
        throw new Error(
          detailResult.message ?? this._localization.translate('modules.profile.testInfos.errors.loadFailed'),
        );
      }
      if (!deviceResult.success || !deviceResult.data) {
        throw new Error(
          deviceResult.message ??
            this._localization.translate('modules.profile.testInfos.errors.loadDeviceGroupsFailed'),
        );
      }
      if (!kitResult.success || !kitResult.data) {
        throw new Error(
          kitResult.message ?? this._localization.translate('modules.profile.testInfos.errors.loadKitGroupsFailed'),
        );
      }

      const detail = detailResult.data;
      this.deviceGroupOptions = this.toSelectOptions(
        deviceResult.data,
        this._localization.translate('modules.profile.testInfos.editDialog.noSelection'),
      );
      this.kitGroupOptions = this.toSelectOptions(
        kitResult.data,
        this._localization.translate('modules.profile.testInfos.editDialog.noSelection'),
      );

      this.form.patchValue({
        cpnCode: detail.cpnCode ?? this.data.row.cpnCode ?? '',
        nationalCode: detail.nationalCode ?? this.data.row.nationalCode ?? '',
        approvePrice: detail.approvePrice ?? this.data.row.approvePrice ?? null,
        deviceGroupId: detail.deviceGroupId ?? null,
        kitGroupId: detail.kitGroupId ?? null,
      });
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.testInfos.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  async save(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.saving = true;
    this.error = '';

    try {
      const value = this.form.getRawValue();
      const result = await this._service.update({
        id: this.data.row.id,
        approvePrice: value.approvePrice,
        deviceGroupId: value.deviceGroupId,
        kitGroupId: value.kitGroupId,
      });

      if (!result.success) {
        throw new Error(
          result.message ?? this._localization.translate('modules.profile.testInfos.errors.saveFailed'),
        );
      }
      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.testInfos.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  private toSelectOptions(
    items: { id: number; title: string }[],
    emptyLabel: string,
  ): BaseFormSelectOption[] {
    return [
      { value: null, label: emptyLabel },
      ...items.map((item) => ({ value: item.id, label: item.title })),
    ];
  }
}
