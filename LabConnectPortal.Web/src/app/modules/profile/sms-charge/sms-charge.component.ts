import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { Subscription } from 'rxjs';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { CenterProfileService } from '../center-profile/center-profile.service';
import { SiteChargeServicePriceOptionDto } from '../center-profile/center-profile.types';
import {
  SiteChargePricingMode,
  toSiteChargePricingMode,
} from '../../admin/site-charge-services/site-charge-services.types';
import { InitPayResult, PaymentGatewayType } from './sms-charge.types';

@Component({
  selector: 'app-sms-charge',
  standalone: true,
  imports: [
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './sms-charge.component.html',
  styleUrl: './sms-charge.component.scss',
})
export class SmsChargeComponent implements OnInit, OnDestroy {
  private _centerProfileService = inject(CenterProfileService);
  private _localization = inject(LocalizationService);
  private _countSub?: Subscription;

  loading = false;
  paying = false;
  error = '';
  success = '';
  remainingCount = 0;
  pricingMode: SiteChargePricingMode = SiteChargePricingMode.Fixed;
  prices: SiteChargeServicePriceOptionDto[] = [];
  unitPrice = 0;
  payableTotal = 0;
  selectedPriceId: number | null = null;

  countControl = new FormControl<number | null>(100, {
    validators: [Validators.required, Validators.min(1)],
  });

  get isPackageMode(): boolean {
    return this.pricingMode === SiteChargePricingMode.Package;
  }

  get isRangeMode(): boolean {
    return this.pricingMode === SiteChargePricingMode.Range;
  }

  get canPay(): boolean {
    if (this.paying || this.loading || this.payableTotal <= 0) return false;
    if (this.isPackageMode) return this.selectedPriceId != null;
    return this.countControl.valid;
  }

  ngOnInit(): void {
    this._countSub = this.countControl.valueChanges.subscribe(() =>
      this.recalc(),
    );
    void this.load();
  }

  ngOnDestroy(): void {
    this._countSub?.unsubscribe();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._centerProfileService.getSmsChargeInfo();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.profile.center.sms.errors.loadFailed',
            ),
        );
      }
      this.remainingCount = result.data.remainingCount;
      this.pricingMode = toSiteChargePricingMode(result.data.pricingMode);
      this.prices = result.data.prices ?? [];
      this.unitPrice = result.data.unitPrice ?? 0;

      if (this.isPackageMode && this.prices.length > 0) {
        this.selectPackage(this.prices[0].siteChargeServicePriceId);
      } else {
        this.recalc();
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.center.sms.errors.loadFailed',
            );
    } finally {
      this.loading = false;
    }
  }

  selectPackage(priceId: number): void {
    this.selectedPriceId = priceId;
    this.recalc();
  }

  private recalc(): void {
    if (this.isPackageMode) {
      const pkg = this.prices.find(
        (p) => p.siteChargeServicePriceId === this.selectedPriceId,
      );
      this.unitPrice = 0;
      this.payableTotal = pkg?.price ?? 0;
      return;
    }

    const count = Number(this.countControl.value) || 0;
    if (count < 1) {
      this.payableTotal = 0;
      return;
    }

    if (this.isRangeMode) {
      const tier = this.prices.find(
        (p) =>
          count >= (p.minQuantity ?? 1) &&
          (p.maxQuantity == null || count <= p.maxQuantity),
      );
      this.unitPrice = tier?.price ?? 0;
    } else {
      this.unitPrice = this.prices[0]?.price ?? this.unitPrice;
    }

    this.payableTotal = this.unitPrice > 0 ? this.unitPrice * count : 0;
  }

  clearSuccess(): void {
    this.success = '';
  }

  clearError(): void {
    this.error = '';
  }

  async pay(): Promise<void> {
    if (this.isPackageMode) {
      if (!this.canPay || this.selectedPriceId == null) return;
    } else {
      this.countControl.markAsTouched();
      this.recalc();
      if (!this.canPay) return;
    }

    this.paying = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._centerProfileService.initPayCharge(
        this.isPackageMode
          ? { priceId: this.selectedPriceId }
          : { count: Number(this.countControl.value) },
      );
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.profile.center.sms.errors.payFailed',
            ),
        );
      }
      if (
        result.data.paymentGatewayType.toString() ==
        PaymentGatewayType[PaymentGatewayType.BehPardakht]
      ) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = result.data.redirectUrl!; // non-null assertion if you're sure it's set

        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'RefId';
        input.value = result.data.key;

        form.appendChild(input);
        document.body.appendChild(form);
        form.submit();
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.center.sms.errors.payFailed',
            );
    } finally {
      this.paying = false;
    }
  }
}
