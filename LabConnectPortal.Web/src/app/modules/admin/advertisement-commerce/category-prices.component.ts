import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { CategoryPricesService } from './category-prices.service';
import { ProductCategoryPriceItemDto } from './category-prices.types';

type CategoryPriceRow = ProductCategoryPriceItemDto & {
  control: FormControl<number | null>;
};

@Component({
  selector: 'app-category-prices',
  standalone: true,
  imports: [ReactiveFormsModule, TranslocoPipe, BaseButtonComponent, BaseFormFieldComponent],
  templateUrl: './category-prices.component.html',
  styles: `
    :host { display: block; }
    :host ::ng-deep .apply-bulk-btn button.base-button-root.base-button--green {
      background-color: #4ade80 !important;
    }
  `,
})
export class CategoryPricesComponent implements OnInit {
  readonly entity = SystemEntity.ContentAds;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ContentAds);

  loading = false;
  saving = false;
  error = '';
  success = '';
  rows: CategoryPriceRow[] = [];
  bulkPrice = new FormControl<number | null>(null, { validators: [Validators.min(0)] });

  constructor(
    private _service: CategoryPricesService,
    private _localization: LocalizationService,
    private _sitePermission: SitePermissionService,
  ) {}

  get canUpdate(): boolean {
    return this._sitePermission.can(this.entity, 'update');
  }

  ngOnInit(): void {
    void this.load();
  }

  groupedRows(): { groupTitle: string; rows: CategoryPriceRow[] }[] {
    const map = new Map<string, CategoryPriceRow[]>();
    for (const row of this.rows) {
      const key =
        row.groupTitle?.trim() ||
        this._localization.translate('modules.admin.adCommerce.categoryPrices.ungrouped');
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(row);
    }
    return Array.from(map.entries()).map(([groupTitle, rows]) => ({ groupTitle, rows }));
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
            this._localization.translate('modules.admin.adCommerce.categoryPrices.errors.loadFailed'),
        );
      }

      this.rows = result.data.map((item) => this.toRow(item));
      this.bulkPrice.reset();
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.adCommerce.categoryPrices.errors.loadFailed');
      this.rows = [];
    } finally {
      this.loading = false;
    }
  }

  applyBulkPrice(): void {
    if (!this.canUpdate) return;

    if (this.bulkPrice.invalid || this.bulkPrice.value == null || `${this.bulkPrice.value}` === '') {
      this.bulkPrice.markAsTouched();
      this.error = this._localization.translate(
        'modules.admin.adCommerce.categoryPrices.errors.invalidPrice',
      );
      return;
    }

    const price = Number(this.bulkPrice.value);
    for (const row of this.rows) {
      row.control.setValue(price);
      row.control.markAsDirty();
    }
    this.error = '';
    this.success = '';
  }

  async save(): Promise<void> {
    if (!this.canUpdate) return;

    for (const row of this.rows) {
      if (row.control.invalid) {
        row.control.markAsTouched();
        this.error = this._localization.translate(
          'modules.admin.adCommerce.categoryPrices.errors.invalidPrice',
        );
        return;
      }
    }

    const items = this.rows
      .filter((row) => row.control.value != null && `${row.control.value}` !== '')
      .map((row) => ({
        productCategoryId: row.productCategoryId,
        price: Number(row.control.value),
      }));

    if (!items.length) {
      this.error = this._localization.translate(
        'modules.admin.adCommerce.categoryPrices.errors.noPrices',
      );
      return;
    }

    this.saving = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._service.saveAll({ items });
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.adCommerce.categoryPrices.errors.saveFailed'),
        );
      }

      this.rows = result.data.map((item) => this.toRow(item));
      this.success = this._localization.translate('modules.admin.adCommerce.categoryPrices.saved');
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.adCommerce.categoryPrices.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  private toRow(item: ProductCategoryPriceItemDto): CategoryPriceRow {
    const control = new FormControl<number | null>(item.price, {
      validators: [Validators.min(0)],
      nonNullable: false,
    });
    if (!this.canUpdate) control.disable({ emitEvent: false });
    return { ...item, control };
  }
}
