import { Component, OnInit } from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { SiteChargeServicesService } from './site-charge-services.service';
import {
  SITE_CHARGE_PRICING_MODES,
  SITE_CHARGE_PRICING_MODE_LABELS,
  SITE_CHARGE_SERVICE_CODES,
  SITE_CHARGE_SERVICE_CODE_LABELS,
  SiteChargePricingMode,
  SiteChargeServiceAdminDto,
  SiteChargeServiceCode,
  toSiteChargePricingMode,
  toSiteChargeServiceCode,
} from './site-charge-services.types';

@Component({
  selector: 'app-site-charge-services',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseCheckboxComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './site-charge-services.component.html',
  styleUrl: './site-charge-services.component.scss',
})
export class SiteChargeServicesComponent implements OnInit {
  readonly entity = SystemEntity.Settings;
  private readonly _systemEntityBinding = bindSystemEntity(
    SystemEntity.Settings,
  );

  loading = false;
  saving = false;
  error = '';
  success = '';
  services: SiteChargeServiceAdminDto[] = [];
  selectedId: number | null = null;
  isCreating = false;

  form = new FormGroup({
    siteChargeServiceId: new FormControl<number | null>(null),
    code: new FormControl<SiteChargeServiceCode>(SiteChargeServiceCode.Sms, {
      nonNullable: true,
      validators: [Validators.required],
    }),
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    description: new FormControl(''),
    pricingMode: new FormControl<SiteChargePricingMode>(
      SiteChargePricingMode.Fixed,
      {
        nonNullable: true,
        validators: [Validators.required],
      },
    ),
    isActive: new FormControl(true, { nonNullable: true }),
    prices: new FormArray<FormGroup>([]),
  });

  constructor(
    private _service: SiteChargeServicesService,
    private _localization: LocalizationService,
    private _sitePermission: SitePermissionService,
  ) {}

  get canUpdate(): boolean {
    return this._sitePermission.can(this.entity, 'update');
  }

  get canCreate(): boolean {
    return this._sitePermission.can(this.entity, 'create') || this.canUpdate;
  }

  get canDelete(): boolean {
    return this._sitePermission.can(this.entity, 'delete');
  }

  get prices(): FormArray<FormGroup> {
    return this.form.controls.prices;
  }

  get pricingMode(): SiteChargePricingMode {
    return toSiteChargePricingMode(this.form.controls.pricingMode.value);
  }

  get isFixed(): boolean {
    return this.pricingMode === SiteChargePricingMode.Fixed;
  }

  get isRange(): boolean {
    return this.pricingMode === SiteChargePricingMode.Range;
  }

  get isPackage(): boolean {
    return this.pricingMode === SiteChargePricingMode.Package;
  }

  get codeOptions(): BaseFormSelectOption[] {
    const used = new Set(
      this.services.map((s) => toSiteChargeServiceCode(s.code)),
    );
    const current = toSiteChargeServiceCode(this.form.controls.code.value);
    return SITE_CHARGE_SERVICE_CODES.filter((code) => {
      if (this.isCreating) return !used.has(code);
      return !used.has(code) || code === current;
    }).map((code) => ({
      value: code,
      label: this._localization.translate(
        SITE_CHARGE_SERVICE_CODE_LABELS[code],
      ),
    }));
  }

  get pricingModeOptions(): BaseFormSelectOption[] {
    return SITE_CHARGE_PRICING_MODES.map((mode) => ({
      value: mode,
      label: this._localization.translate(
        SITE_CHARGE_PRICING_MODE_LABELS[mode],
      ),
    }));
  }

  get availableCodesToCreate(): boolean {
    const used = new Set(
      this.services.map((s) => toSiteChargeServiceCode(s.code)),
    );
    return SITE_CHARGE_SERVICE_CODES.some((code) => !used.has(code));
  }

  ngOnInit(): void {
    this.form.controls.pricingMode.valueChanges.subscribe(() =>
      this.onPricingModeChanged(),
    );
    if (!this.canUpdate) this.form.disable({ emitEvent: false });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._service.getAll();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.admin.siteChargeServices.errors.loadFailed',
            ),
        );
      }
      this.services = result.data.map((s) => ({
        ...s,
        code: toSiteChargeServiceCode(s.code),
        pricingMode: toSiteChargePricingMode(s.pricingMode),
      }));
      if (this.services.length) {
        this.selectService(this.services[0]);
      } else {
        this.startCreate();
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.siteChargeServices.errors.loadFailed',
            );
      this.services = [];
    } finally {
      this.loading = false;
    }
  }

  selectService(service: SiteChargeServiceAdminDto): void {
    this.isCreating = false;
    this.selectedId = service.siteChargeServiceId;
    this.patchForm(service);
    this.form.controls.code.disable({ emitEvent: false });
  }

  startCreate(): void {
    if (!this.canCreate || !this.availableCodesToCreate) return;
    this.isCreating = true;
    this.selectedId = null;
    const used = new Set(
      this.services.map((s) => toSiteChargeServiceCode(s.code)),
    );
    const unused = SITE_CHARGE_SERVICE_CODES.find((code) => !used.has(code));
    this.form.reset({
      siteChargeServiceId: null,
      code: unused ?? SiteChargeServiceCode.Sms,
      title: '',
      description: '',
      pricingMode: SiteChargePricingMode.Fixed,
      isActive: true,
    });
    this.prices.clear();
    this.addPriceRow();
    if (!this.canUpdate) {
      this.form.disable({ emitEvent: false });
    } else {
      this.form.enable({ emitEvent: false });
    }
  }

  addPriceRow(): void {
    if (!this.canUpdate) return;
    if (this.isFixed && this.prices.length >= 1) return;
    this.prices.push(this.createPriceGroup());
  }

  removePriceRow(index: number): void {
    if (!this.canUpdate || this.prices.length <= 1) return;
    this.prices.removeAt(index);
  }

  private onPricingModeChanged(): void {
    if (this.isFixed) {
      while (this.prices.length > 1)
        this.prices.removeAt(this.prices.length - 1);
      if (this.prices.length === 0) this.addPriceRow();
    } else if (this.prices.length === 0) {
      this.addPriceRow();
    }
  }

  async save(): Promise<void> {
    if (!this.canUpdate) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.error = this._localization.translate(
        'modules.admin.siteChargeServices.errors.invalidForm',
      );
      return;
    }

    const raw = this.form.getRawValue();
    const command = {
      siteChargeServiceId: this.isCreating ? null : raw.siteChargeServiceId,
      code: toSiteChargeServiceCode(raw.code),
      title: raw.title.trim(),
      description: raw.description?.trim() || null,
      pricingMode: this.pricingMode,
      isActive: !!raw.isActive,
      prices: this.prices.controls.map((group, index) => {
        const v = group.getRawValue() as {
          title?: string | null;
          minQuantity?: number | null;
          maxQuantity?: number | null;
          packageQuantity?: number | null;
          price: number | null;
        };
        return {
          title: v.title?.trim() || null,
          minQuantity: this.isRange ? Number(v.minQuantity ?? 1) : null,
          maxQuantity: this.isRange
            ? v.maxQuantity == null || `${v.maxQuantity}` === ''
              ? null
              : Number(v.maxQuantity)
            : null,
          packageQuantity: this.isPackage ? Number(v.packageQuantity) : null,
          price: Number(v.price ?? 0),
          sortOrder: index,
        };
      }),
    };

    this.saving = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._service.save(command);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.admin.siteChargeServices.errors.saveFailed',
            ),
        );
      }
      this.success = this._localization.translate(
        'modules.admin.siteChargeServices.saved',
      );
      await this.load();
      const saved = this.services.find(
        (s) => s.siteChargeServiceId === result.data!.siteChargeServiceId,
      );
      if (saved) this.selectService(saved);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.siteChargeServices.errors.saveFailed',
            );
    } finally {
      this.saving = false;
    }
  }

  async deleteSelected(): Promise<void> {
    if (!this.canDelete || this.selectedId == null || this.isCreating) return;
    const title = this.form.controls.title.value || '';
    const message = this._localization.translate(
      'modules.admin.siteChargeServices.confirmDelete',
      {
        title,
      },
    );
    if (!confirm(message)) return;

    this.error = '';
    this.success = '';
    const result = await this._service.delete(this.selectedId);
    if (!result.success) {
      this.error =
        result.message ??
        this._localization.translate(
          'modules.admin.siteChargeServices.errors.deleteFailed',
        );
      return;
    }
    this.success = this._localization.translate(
      'modules.admin.siteChargeServices.deleted',
    );
    await this.load();
  }

  codeLabel(code: SiteChargeServiceCode | string | number): string {
    const value = toSiteChargeServiceCode(code);
    const key = SITE_CHARGE_SERVICE_CODE_LABELS[value];
    return key ? this._localization.translate(key) : String(code);
  }

  modeLabel(mode: SiteChargePricingMode | string | number): string {
    const value = toSiteChargePricingMode(mode);
    const key = SITE_CHARGE_PRICING_MODE_LABELS[value];
    return key ? this._localization.translate(key) : String(mode);
  }

  private patchForm(service: SiteChargeServiceAdminDto): void {
    this.form.reset({
      siteChargeServiceId: service.siteChargeServiceId,
      code: toSiteChargeServiceCode(service.code),
      title: service.title,
      description: service.description ?? '',
      pricingMode: toSiteChargePricingMode(service.pricingMode),
      isActive: service.isActive,
    });
    this.prices.clear();
    const rows = service.prices?.length
      ? service.prices
      : [{ price: 0 } as never];
    for (const price of rows) {
      this.prices.push(
        this.createPriceGroup({
          title: price.title ?? '',
          minQuantity: price.minQuantity ?? null,
          maxQuantity: price.maxQuantity ?? null,
          packageQuantity: price.packageQuantity ?? null,
          price: price.price ?? 0,
        }),
      );
    }
    if (!this.canUpdate) this.form.disable({ emitEvent: false });
    else {
      this.form.enable({ emitEvent: false });
      this.form.controls.code.disable({ emitEvent: false });
    }
  }

  private createPriceGroup(initial?: {
    title?: string;
    minQuantity?: number | null;
    maxQuantity?: number | null;
    packageQuantity?: number | null;
    price?: number;
  }): FormGroup {
    return new FormGroup({
      title: new FormControl(initial?.title ?? ''),
      minQuantity: new FormControl<number | null>(initial?.minQuantity ?? 1),
      maxQuantity: new FormControl<number | null>(initial?.maxQuantity ?? null),
      packageQuantity: new FormControl<number | null>(
        initial?.packageQuantity ?? 100,
      ),
      price: new FormControl<number | null>(initial?.price ?? 0, {
        validators: [Validators.required, Validators.min(0)],
      }),
    });
  }
}
