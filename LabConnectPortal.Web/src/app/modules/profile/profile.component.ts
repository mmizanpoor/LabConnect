import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { LocationMapPickerComponent } from './location-map-picker/location-map-picker.component';
import { ProfileService } from './profile.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    LocationMapPickerComponent,
  ],
  templateUrl: './profile.component.html',
})
export class ProfileComponent implements OnInit {
  readonly entity = SystemEntity.Profile;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Profile);

  loading = false;
  saving = false;
  error = '';
  success = '';

  form = new FormGroup({
    firstName: new FormControl(''),
    lastName: new FormControl(''),
    mobileNumber: new FormControl({ value: '', disabled: true }),
    email: new FormControl(''),
    address: new FormControl(''),
    phone: new FormControl(''),
    latitude: new FormControl<number | null>(null),
    longitude: new FormControl<number | null>(null),
  });

  constructor(
    private _profileService: ProfileService,
    private _localization: LocalizationService,
  ) {}

  async ngOnInit(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const profile = await this._profileService.getProfile();
      this.form.patchValue({
        ...profile,
        latitude: profile.latitude ?? null,
        longitude: profile.longitude ?? null,
      });
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.profile.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  onLocationChange(location: { latitude: number; longitude: number }): void {
    this.form.patchValue({
      latitude: location.latitude,
      longitude: location.longitude,
    });
  }

  async save(): Promise<void> {
    this.error = '';
    this.success = '';
    this.saving = true;
    try {
      const value = this.form.getRawValue();
      await this._profileService.updateProfile({
        firstName: value.firstName ?? '',
        lastName: value.lastName ?? '',
        email: value.email ?? '',
        address: value.address ?? '',
        phone: value.phone ?? '',
        latitude: this.toNullableNumber(value.latitude),
        longitude: this.toNullableNumber(value.longitude),
      });
      this.success = this._localization.translate('modules.profile.profile.success.saved');
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.profile.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  private toNullableNumber(value: number | string | null | undefined): number | null {
    if (value === null || value === undefined || value === '') return null;
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }
}
