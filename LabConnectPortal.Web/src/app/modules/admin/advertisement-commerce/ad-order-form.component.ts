import { DecimalPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { UsersService } from '../users/users.service';
import { UserDto } from '../users/users.types';
import { AdDurationsService } from './ad-prices.service';
import { AdPositionsService } from './ad-positions.service';
import { AdOrdersService } from './ad-orders.service';
import {
  ADVERTISEMENT_ORDER_STATUS_OPTIONS,
  AdvertisementDurationDto,
  AdvertisementOrderDto,
  AdvertisementOrderStatus,
  AdvertisementPositionDto,
  advertisementOrderStatusLabelKey,
} from './advertisement-commerce.types';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

@Component({
  selector: 'app-ad-order-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    DecimalPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './ad-order-form.component.html',
})
export class AdOrderFormComponent implements OnInit {
  readonly entity = SystemEntity.ContentAds;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ContentAds);

  loading = false;
  saving = false;
  error = '';
  isEdit = false;
  orderId = '';
  positions: AdvertisementPositionDto[] = [];
  durations: AdvertisementDurationDto[] = [];
  users: UserDto[] = [];
  existing: AdvertisementOrderDto | null = null;

  form = new FormGroup({
    userId: new FormControl('', Validators.required),
    advertisementPositionId: new FormControl<number | null>(null, Validators.required),
    advertisementDurationId: new FormControl<number | null>(null, Validators.required),
    startAt: new FormControl<Moment | null>(null, Validators.required),
    status: new FormControl<AdvertisementOrderStatus>(AdvertisementOrderStatus.Draft, {
      nonNullable: true,
    }),
  });

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _ordersService: AdOrdersService,
    private _positionsService: AdPositionsService,
    private _durationsService: AdDurationsService,
    private _usersService: UsersService,
    private _localization: LocalizationService,
  ) {}

  get positionOptions(): BaseFormSelectOption[] {
    return this.positions.map((p) => ({ value: p.id, label: p.title }));
  }

  get durationOptions(): BaseFormSelectOption[] {
    return this.durations.map((d) => ({ value: d.id, label: d.title }));
  }

  get userOptions(): BaseFormSelectOption[] {
    return this.users.map((u) => ({
      value: u.id,
      label: `${u.firstName} ${u.lastName}`.trim() || u.mobileNumber,
    }));
  }

  get statusOptions(): BaseFormSelectOption[] {
    return ADVERTISEMENT_ORDER_STATUS_OPTIONS.map((status) => ({
      value: status,
      label: this._localization.translate(advertisementOrderStatusLabelKey(status)),
    }));
  }

  ngOnInit(): void {
    this.orderId = this._route.snapshot.paramMap.get('id') ?? '';
    this.isEdit = this.orderId !== '' && this.orderId !== 'new';
    void this.init();
  }

  async init(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const [positionsResult, durationsResult, usersResult] = await Promise.all([
        this._positionsService.getAll(true),
        this._durationsService.getAll(),
        this._usersService.getUsers({ page: 1, pageSize: 200 }),
      ]);

      if (!positionsResult.success || !positionsResult.data) {
        throw new Error(positionsResult.message ?? 'Failed to load positions');
      }
      if (!durationsResult.success || !durationsResult.data) {
        throw new Error(durationsResult.message ?? 'Failed to load durations');
      }
      if (!usersResult.success || !usersResult.data) {
        throw new Error(usersResult.message ?? 'Failed to load users');
      }

      this.positions = positionsResult.data;
      this.durations = durationsResult.data;
      this.users = usersResult.data.items;

      if (this.isEdit) {
        const orderResult = await this._ordersService.getById(this.orderId);
        if (!orderResult.success || !orderResult.data) {
          throw new Error(
            orderResult.message ??
              this._localization.translate('modules.admin.adCommerce.orders.errors.loadFailed'),
          );
        }
        this.existing = orderResult.data;
        this.patchForm(orderResult.data);
        this.form.controls.userId.disable();
        this.form.controls.advertisementPositionId.disable();
        this.form.controls.advertisementDurationId.disable();
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.adCommerce.orders.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  patchForm(order: AdvertisementOrderDto): void {
    this.form.patchValue({
      userId: order.userId,
      advertisementPositionId: order.advertisementPositionId,
      advertisementDurationId: order.advertisementDurationId,
      startAt: jMoment(order.startDate),
      status: order.status,
    });
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();
    const startDate = value.startAt?.toDate();
    if (!startDate) {
      this.saving = false;
      return;
    }

    try {
      if (this.isEdit && this.existing) {
        const result = await this._ordersService.updateStatus({
          id: this.orderId,
          status: value.status,
          startDate: startDate.toISOString(),
        });
        if (!result.success) {
          throw new Error(
            result.message ??
              this._localization.translate('modules.admin.adCommerce.orders.errors.saveFailed'),
          );
        }
      } else {
        const result = await this._ordersService.create({
          userId: value.userId ?? '',
          advertisementPositionId: value.advertisementPositionId ?? 0,
          advertisementDurationId: value.advertisementDurationId ?? 0,
          startDate: startDate.toISOString(),
          status: value.status,
        });
        if (!result.success) {
          throw new Error(
            result.message ??
              this._localization.translate('modules.admin.adCommerce.orders.errors.saveFailed'),
          );
        }
      }

      void this._router.navigate(['/admin/ad-orders']);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.adCommerce.orders.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}
