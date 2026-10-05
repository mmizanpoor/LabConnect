import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BASE_DIALOG_PANEL_CLASS,
  BaseDialogComponent,
} from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { AdPricesService } from './ad-prices.service';
import {
  AdvertisementDurationDto,
  AdvertisementPositionDto,
  AdvertisementPriceDto,
  AdvertisementPriceMatrixCellDto,
  AdvertisementPriceMatrixDto,
} from './advertisement-commerce.types';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

export type SetAdPriceDialogData = {
  positionId: number;
  durationId: number;
  positionTitle: string;
  durationTitle: string;
  currentPrice?: number | null;
};

@Component({
  selector: 'app-set-ad-price-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
    BaseFormFieldComponent,
    DecimalPipe,
  ],
  template: `
    <base-dialog [title]="'modules.admin.adCommerce.prices.setPrice' | transloco">
      <div class="flex min-w-[min(100%,22rem)] flex-col gap-4" dir="rtl">
        <p class="m-0 text-sm text-gray-500">{{ data.positionTitle }} — {{ data.durationTitle }}</p>

        @if (error) {
          <div class="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{{ error }}</div>
        }

        <form class="flex flex-col gap-4" [formGroup]="form" (ngSubmit)="save()">
          <base-form-field
            class="w-full"
            layout="floating"
            type="number"
            [label]="'modules.admin.adCommerce.prices.fields.price' | transloco"
            [control]="form.controls.price"
          />
          <base-form-field
            class="w-full"
            layout="floating"
            icon="event"
            [jalaliDate]="true"
            [label]="'modules.admin.adCommerce.prices.fields.validFrom' | transloco"
            [control]="form.controls.validFrom"
          />
        </form>

        @if (history.length) {
          <div class="border-t border-gray-200 pt-4">
            <h3 class="mb-3 text-sm font-bold text-gray-800">
              {{ 'modules.admin.adCommerce.prices.history' | transloco }}
            </h3>
            <div class="max-h-48 space-y-2 overflow-y-auto text-sm">
              @for (item of history; track item.id) {
                <div class="flex items-center justify-between rounded-lg bg-gray-50 px-3 py-2">
                  <span>{{ item.price | number:'1.0-0' }} {{ 'modules.admin.adCommerce.prices.currency' | transloco }}</span>
                  <span class="text-gray-500">{{ formatDate(item.validFrom) }} — {{ item.validTo ? formatDate(item.validTo) : '...' }}</span>
                </div>
              }
            </div>
          </div>
        }
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="soft" size="sm" (click)="close()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button type="button" color="blue" size="sm" [loading]="saving" (click)="save()">
          {{ 'shared.save' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class SetAdPriceDialogComponent implements OnInit {
  private _dialogRef = inject(MatDialogRef<SetAdPriceDialogComponent>);
  data = inject<SetAdPriceDialogData>(MAT_DIALOG_DATA);
  private _service = inject(AdPricesService);
  private _localization = inject(LocalizationService);

  saving = false;
  error = '';
  history: AdvertisementPriceDto[] = [];

  form = new FormGroup({
    price: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    validFrom: new FormControl<Moment | null>(null, Validators.required),
  });

  ngOnInit(): void {
    if (this.data.currentPrice != null) {
      this.form.controls.price.setValue(this.data.currentPrice);
    }
    this.form.controls.validFrom.setValue(jMoment().startOf('day'));
    void this.loadHistory();
  }

  async loadHistory(): Promise<void> {
    const result = await this._service.getHistory(this.data.positionId, this.data.durationId);
    if (result.success && result.data) {
      this.history = result.data;
    }
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('fa-IR', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value));
  }

  close(): void {
    this._dialogRef.close(false);
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();
    const validFrom = value.validFrom?.clone().startOf('day').toDate();

    if (!validFrom) {
      this.saving = false;
      return;
    }

    try {
      const result = await this._service.setPrice({
        advertisementPositionId: this.data.positionId,
        advertisementDurationId: this.data.durationId,
        price: value.price ?? 0,
        validFrom: validFrom.toISOString(),
      });

      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.adCommerce.prices.errors.saveFailed'),
        );
      }

      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.adCommerce.prices.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}

@Component({
  selector: 'app-ad-prices',
  standalone: true,
  imports: [DecimalPipe, TranslocoPipe, MatIconModule],
  templateUrl: './ad-prices.component.html',
  styles: `:host { display: block; }`,
})
export class AdPricesComponent implements OnInit {
  readonly entity = SystemEntity.ContentAds;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ContentAds);
  private _dialog = inject(MatDialog);

  loading = false;
  error = '';
  matrix: AdvertisementPriceMatrixDto | null = null;

  constructor(
    private _service: AdPricesService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    void this.load();
  }

  get positions(): AdvertisementPositionDto[] {
    return this.matrix?.positions ?? [];
  }

  get durations(): AdvertisementDurationDto[] {
    return this.matrix?.durations ?? [];
  }

  cell(positionId: number, durationId: number): AdvertisementPriceMatrixCellDto | undefined {
    return this.matrix?.cells.find(
      (c) => c.advertisementPositionId === positionId && c.advertisementDurationId === durationId,
    );
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getMatrix();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.adCommerce.prices.errors.loadFailed'),
        );
      }
      this.matrix = result.data;
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.adCommerce.prices.errors.loadFailed');
      this.matrix = null;
    } finally {
      this.loading = false;
    }
  }

  openSetPrice(position: AdvertisementPositionDto, duration: AdvertisementDurationDto): void {
    const cell = this.cell(position.id, duration.id);
    const dialogRef = this._dialog.open(SetAdPriceDialogComponent, {
      width: '480px',
      maxWidth: '92vw',
      data: {
        positionId: position.id,
        durationId: duration.id,
        positionTitle: position.title,
        durationTitle: duration.title,
        currentPrice: cell?.currentPrice,
      } satisfies SetAdPriceDialogData,
      panelClass: BASE_DIALOG_PANEL_CLASS,
    });

    dialogRef.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }
}
