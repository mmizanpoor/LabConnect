import { Component, OnInit, inject } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DecimalPipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BreadcrumbService } from '@core/services/breadcrumb/breadcrumb.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BASE_DIALOG_PANEL_CLASS,
} from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseConfirmDialogComponent } from '@modules/base/components/base-dialog/base-confirm-dialog.component';
import { ProductReviewService } from '../products/products.service';
import { OrdersService } from './orders.service';
import {
  ProductOrderDto,
  ProductOrderItemDto,
  ProductOrderStatus,
} from './orders.types';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    DecimalPipe,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
  ],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.scss',
})
export class OrderDetailComponent implements OnInit {
  private readonly _dialog = inject(MatDialog);

  readonly ratingStars = [1, 2, 3, 4, 5];

  loading = true;
  error = '';
  order: ProductOrderDto | null = null;
  reviewItemId = '';
  reviewLoading = false;
  reviewError = '';
  reviewSuccess = '';

  reviewForm = new FormGroup({
    rating: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(1),
      Validators.max(5),
    ]),
    comment: new FormControl('', Validators.required),
  });

  confirmingDelivery = false;

  constructor(
    private _route: ActivatedRoute,
    private _ordersService: OrdersService,
    private _productReviewService: ProductReviewService,
    private _breadcrumb: BreadcrumbService,
    private _localization: LocalizationService
  ) {}

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    const id = this._route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = this._localization.translate(
        'modules.orders.errors.notFound'
      );
      this.loading = false;
      return;
    }

    try {
      const result = await this._ordersService.getMyOrderById(id);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.orders.errors.notFound')
        );
      }
      this.order = result.data;
      this._breadcrumb.setDynamicLabel(result.data.centerName);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.orders.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  statusLabel(status: ProductOrderStatus): string {
    return this._localization.translate(`enum.productOrderStatus.${status}`);
  }

  formatDateTime(value: string | null | undefined): string {
    if (!value) return '—';
    const parsed = jMoment(value).locale('fa');
    if (!parsed.isValid()) return '—';
    return `\u2066${parsed.format('jYYYY/jMM/jDD HH:mm')}\u2069`;
  }

  statusClass(status: ProductOrderStatus): string {
    switch (status) {
      case 'Paid':
        return 'is-paid';
      case 'Completed':
        return 'is-completed';
      case 'Shipped':
        return 'is-shipped';
      case 'Delivered':
        return 'is-delivered';
      case 'Cancelled':
        return 'is-cancelled';
      default:
        return 'is-pending';
    }
  }

  get isCancelled(): boolean {
    return this.order?.status === 'Cancelled';
  }

  get trackingSteps(): Array<{
    key: string;
    label: string;
    done: boolean;
    current: boolean;
    at?: string | null;
  }> {
    const order = this.order;
    if (!order) return [];

    const status = order.status;
    const rank: Record<string, number> = {
      PendingPayment: 0,
      Paid: 1,
      Completed: 2,
      Shipped: 3,
      Delivered: 4,
      Cancelled: -1,
    };
    const currentRank = rank[status] ?? 0;

    const steps = [
      {
        key: 'created',
        label: this._localization.translate('modules.orders.trackingSteps.created'),
        done: true,
        current: status === 'PendingPayment',
        at: order.createdAt,
      },
      {
        key: 'paid',
        label: this._localization.translate('modules.orders.trackingSteps.paid'),
        done: currentRank >= 1 || !!order.paidAt,
        current: status === 'Paid',
        at: order.paidAt,
      },
      {
        key: 'completed',
        label: this._localization.translate('modules.orders.trackingSteps.completed'),
        done: currentRank >= 2 || !!order.completedAt,
        current: status === 'Completed',
        at: order.completedAt,
      },
      {
        key: 'shipped',
        label: this._localization.translate('modules.orders.trackingSteps.shipped'),
        done: currentRank >= 3 || !!order.shippedAt,
        current: status === 'Shipped',
        at: order.shippedAt,
      },
      {
        key: 'delivered',
        label: this._localization.translate('modules.orders.trackingSteps.delivered'),
        done: currentRank >= 4 || !!order.deliveredAt,
        current: status === 'Delivered',
        at: order.deliveredAt,
      },
    ];

    if (this.isCancelled) {
      return [
        steps[0],
        {
          key: 'cancelled',
          label: this._localization.translate('modules.orders.trackingSteps.cancelled'),
          done: true,
          current: true,
          at: null,
        },
      ];
    }

    return steps;
  }

  async confirmDelivery(): Promise<void> {
    if (!this.order || this.confirmingDelivery) return;

    const confirmed = await firstValueFrom(
      this._dialog
        .open(BaseConfirmDialogComponent, {
          width: '400px',
          panelClass: BASE_DIALOG_PANEL_CLASS,
          data: {
            title: this._localization.translate('modules.orders.confirmDeliveryAction'),
            message: this._localization.translate('modules.orders.confirmDelivery'),
            confirmLabel: this._localization.translate('modules.orders.confirmDeliveryAction'),
            warnConfirm: false,
          },
        })
        .afterClosed(),
    );
    if (confirmed !== true) return;

    this.confirmingDelivery = true;
    this.error = '';
    try {
      const result = await this._ordersService.confirmDelivery(this.order.id);
      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.orders.errors.confirmDeliveryFailed')
        );
      }
      this.order = result.data ?? this.order;
      await this.load();
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.orders.errors.confirmDeliveryFailed');
    } finally {
      this.confirmingDelivery = false;
    }
  }

  startReview(item: ProductOrderItemDto): void {
    this.reviewItemId = item.id;
    this.reviewForm.reset();
    this.reviewError = '';
    this.reviewSuccess = '';
  }

  setRating(value: number): void {
    this.reviewForm.controls.rating.setValue(value);
    this.reviewForm.controls.rating.markAsTouched();
  }

  cancelReview(): void {
    this.reviewItemId = '';
    this.reviewForm.reset();
    this.reviewError = '';
  }

  async submitReview(): Promise<void> {
    if (!this.reviewItemId || this.reviewForm.invalid) return;
    this.reviewLoading = true;
    this.reviewError = '';
    this.reviewSuccess = '';
    try {
      const value = this.reviewForm.getRawValue();
      const result = await this._productReviewService.create({
        orderItemId: this.reviewItemId,
        rating: value.rating ?? 0,
        comment: value.comment ?? '',
      });
      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.orders.review.failed')
        );
      }
      this.reviewSuccess = this._localization.translate(
        'modules.orders.review.success'
      );
      this.cancelReview();
      await this.load();
    } catch (e: unknown) {
      this.reviewError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.orders.review.failed');
    } finally {
      this.reviewLoading = false;
    }
  }
}
