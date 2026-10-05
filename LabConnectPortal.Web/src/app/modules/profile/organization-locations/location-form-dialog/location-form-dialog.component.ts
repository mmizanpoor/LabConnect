import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { ReferenceItemDto } from '../../resume/resume.types';
import { OrganizationLocationsService } from '../organization-locations.service';
import { OrganizationLocationDto } from '../organization-locations.types';

export interface LocationFormDialogData {
  provinces: ReferenceItemDto[];
  location?: OrganizationLocationDto;
}

@Component({
  selector: 'app-location-form-dialog',
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
  templateUrl: './location-form-dialog.component.html',
})
export class LocationFormDialogComponent implements OnInit {
  saving = false;
  error = '';

  form = new FormGroup({
    locationName: new FormControl('', Validators.required),
    provinceId: new FormControl<number | null>(null, Validators.required),
    address: new FormControl('', Validators.required),
    phoneNumber: new FormControl('', Validators.required),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: LocationFormDialogData,
    private _dialogRef: MatDialogRef<LocationFormDialogComponent>,
    private _locationsService: OrganizationLocationsService,
    private _localization: LocalizationService,
  ) {}

  get provinceOptions(): BaseFormSelectOption[] {
    return this.data.provinces.map((province) => ({
      value: province.id,
      label: province.name,
    }));
  }

  ngOnInit(): void {
    if (this.data.location) {
      this.form.patchValue({
        locationName: this.data.location.locationName,
        provinceId: this.data.location.provinceId,
        address: this.data.location.address,
        phoneNumber: this.data.location.phoneNumber,
      });
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
    const value = this.form.getRawValue();
    try {
      const result = this.data.location
        ? await this._locationsService.update({
            locationId: this.data.location.locationId,
            locationName: value.locationName ?? '',
            provinceId: value.provinceId!,
            address: value.address ?? '',
            phoneNumber: value.phoneNumber ?? '',
          })
        : await this._locationsService.create({
            locationName: value.locationName ?? '',
            provinceId: value.provinceId!,
            address: value.address ?? '',
            phoneNumber: value.phoneNumber ?? '',
          });

      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.profile.locations.errors.saveFailed'),
        );
      }
      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.locations.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}
