import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { AdPositionsService } from './ad-positions.service';
import { AdvertisementPositionDto } from './advertisement-commerce.types';

@Component({
  selector: 'app-ad-position-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseCheckboxComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './ad-position-form.component.html',
})
export class AdPositionFormComponent implements OnInit {
  readonly entity = SystemEntity.ContentAds;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ContentAds);

  loading = false;
  saving = false;
  error = '';
  isEdit = false;
  positionId = 0;

  form = new FormGroup({
    code: new FormControl('', [Validators.required, Validators.maxLength(50)]),
    title: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    description: new FormControl('', Validators.maxLength(500)),
    maxConcurrentSlots: new FormControl(1, [Validators.required, Validators.min(1)]),
    maxDisplayCount: new FormControl<number | null>(null),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _service: AdPositionsService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    const idParam = this._route.snapshot.paramMap.get('id') ?? '';
    this.isEdit = idParam !== '' && idParam !== 'new';
    this.positionId = this.isEdit ? Number(idParam) : 0;
    void this.load();
  }

  async load(): Promise<void> {
    if (!this.isEdit) return;

    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getById(this.positionId);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.adCommerce.positions.errors.loadFailed'),
        );
      }
      this.patchForm(result.data);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.adCommerce.positions.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  patchForm(item: AdvertisementPositionDto): void {
    this.form.patchValue({
      code: item.code,
      title: item.title,
      description: item.description,
      maxConcurrentSlots: item.maxConcurrentSlots,
      maxDisplayCount: item.maxDisplayCount ?? null,
      isActive: item.isActive,
    });
    this.form.controls.code.disable();
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();

    try {
      const result = this.isEdit
        ? await this._service.update({
            id: this.positionId,
            code: value.code ?? '',
            title: value.title ?? '',
            description: value.description ?? '',
            maxConcurrentSlots: value.maxConcurrentSlots ?? 1,
            maxDisplayCount: value.maxDisplayCount,
            isActive: value.isActive,
          })
        : await this._service.create({
            code: value.code ?? '',
            title: value.title ?? '',
            description: value.description ?? '',
            maxConcurrentSlots: value.maxConcurrentSlots ?? 1,
            maxDisplayCount: value.maxDisplayCount,
            isActive: value.isActive,
          });

      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.adCommerce.positions.errors.saveFailed'),
        );
      }

      void this._router.navigate(['/admin/ad-positions']);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.adCommerce.positions.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}
