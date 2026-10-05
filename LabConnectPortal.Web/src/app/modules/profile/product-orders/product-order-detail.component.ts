import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DecimalPipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BreadcrumbService } from '@core/services/breadcrumb/breadcrumb.service';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import {
  BASE_DIALOG_PANEL_CLASS,
} from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseConfirmDialogComponent } from '@modules/base/components/base-dialog/base-confirm-dialog.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { ProductOrdersService } from './product-orders.service';
import {
  ProductOrderDto,
  ProductOrderStatus,
} from './product-orders.types';
import { normalizeProductOrderStatus } from './product-orders.coldef';
import { DeliverOrderDialogComponent } from './deliver-order-dialog.component';
import { ShipOrderDialogComponent } from './ship-order-dialog.component';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

@Component({
  selector: 'app-product-order-detail',
  standalone: true,
  imports: [
    MatDialogModule,
    MatIconModule,
    DecimalPipe,
    TranslocoPipe,
    BaseBackButtonComponent,
  ],
  templateUrl: './product-order-detail.component.html',
  styleUrl: './product-order-detail.component.scss',
})
export class ProductOrderDetailComponent implements OnInit {
  readonly entity = SystemEntity.ProductOrder;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ProductOrder);
  private readonly _dialog = inject(MatDialog);

  loading = true;
  actionLoading = false;
  error = '';
  order: ProductOrderDto | null = null;

  constructor(
    private _route: ActivatedRoute,
    private _service: ProductOrdersService,
    private _localization: LocalizationService,
    private _breadcrumb: BreadcrumbService,
    private _labPermission: LabPermissionService,
  ) {}

  ngOnInit(): void {
    void this.load();
  }

  get canUpdate(): boolean {
    return this._labPermission.can(this.entity, 'update');
  }

  get buyerTitle(): string {
    if (!this.order) return '';
    return (
      this.order.buyerName?.trim() ||
      this.order.shippingRecipientName?.trim() ||
      this.order.centerName
    );
  }

  get normalizedStatus(): ProductOrderStatus | null {
    return normalizeProductOrderStatus(this.order?.status);
  }

  get canComplete(): boolean {
    return this.canUpdate && this.normalizedStatus === 'Paid';
  }

  get canShip(): boolean {
    return this.canUpdate && this.normalizedStatus === 'Completed';
  }

  get canDeliver(): boolean {
    return this.canUpdate && this.normalizedStatus === 'Shipped';
  }

  get showActions(): boolean {
    return this.canComplete || this.canShip || this.canDeliver;
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    const id = this._route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = this._localization.translate('modules.profile.productOrders.errors.notFound');
      this.loading = false;
      return;
    }

    try {
      const result = await this._service.getCenterOrderById(id);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.profile.productOrders.errors.notFound'),
        );
      }
      this.order = result.data;
      this._breadcrumb.setDynamicLabel(this.buyerTitle);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.productOrders.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  async complete(): Promise<void> {
    if (!this.order || !this.canComplete || this.actionLoading) return;

    const confirmed = await firstValueFrom(
      this._dialog
        .open(BaseConfirmDialogComponent, {
          width: '400px',
          panelClass: BASE_DIALOG_PANEL_CLASS,
          data: {
            title: this._localization.translate('modules.profile.productOrders.complete'),
            message: this._localization.translate('modules.profile.productOrders.confirmComplete'),
            confirmLabel: this._localization.translate('modules.profile.productOrders.complete'),
            warnConfirm: false,
          },
        })
        .afterClosed(),
    );
    if (confirmed !== true) return;

    this.actionLoading = true;
    this.error = '';
    try {
      const result = await this._service.complete(this.order.id);
      if (!result.success) {
        this.error =
          result.message ??
          this._localization.translate('modules.profile.productOrders.errors.completeFailed');
        return;
      }
      this.order = result.data ?? this.order;
      this._breadcrumb.setDynamicLabel(this.buyerTitle);
    } finally {
      this.actionLoading = false;
    }
  }

  async shipOrder(): Promise<void> {
    if (!this.order || !this.canShip || this.actionLoading) return;

    const payload = await firstValueFrom(
      this._dialog
        .open(ShipOrderDialogComponent, {
          width: '420px',
          panelClass: BASE_DIALOG_PANEL_CLASS,
        })
        .afterClosed(),
    );
    if (!payload) return;

    this.actionLoading = true;
    this.error = '';
    try {
      const result = await this._service.markOrderShipped({
        orderId: this.order.id,
        shippingCompany: payload.shippingCompany,
        trackingCode: payload.trackingCode,
      });
      if (!result.success) {
        this.error =
          result.message ??
          this._localization.translate('modules.profile.productOrders.errors.shipFailed');
        return;
      }
      this.order = result.data ?? this.order;
      this._breadcrumb.setDynamicLabel(this.buyerTitle);
    } finally {
      this.actionLoading = false;
    }
  }

  async markDelivered(): Promise<void> {
    if (!this.order || !this.canDeliver || this.actionLoading) return;

    const payload = await firstValueFrom(
      this._dialog
        .open(DeliverOrderDialogComponent, {
          width: '420px',
          panelClass: BASE_DIALOG_PANEL_CLASS,
        })
        .afterClosed(),
    );
    if (!payload) return;

    this.actionLoading = true;
    this.error = '';
    try {
      const result = await this._service.markOrderDelivered({
        orderId: this.order.id,
        notes: payload.notes,
      });
      if (!result.success) {
        this.error =
          result.message ??
          this._localization.translate('modules.profile.productOrders.errors.deliverFailed');
        return;
      }
      this.order = result.data ?? this.order;
      this._breadcrumb.setDynamicLabel(this.buyerTitle);
    } finally {
      this.actionLoading = false;
    }
  }

  statusLabel(status: ProductOrderStatus | number | string): string {
    const normalized = normalizeProductOrderStatus(status);
    if (!normalized) return '—';
    return this._localization.translate(`enum.productOrderStatus.${normalized}`);
  }

  formatDateTime(value: string | null | undefined): string {
    if (!value) return '—';
    const parsed = jMoment(value).locale('fa');
    if (!parsed.isValid()) return '—';
    // LTR isolate: date on the left, time on the right.
    return `\u2066${parsed.format('jYYYY/jMM/jDD HH:mm')}\u2069`;
  }

  statusClass(status: ProductOrderStatus | number | string): string {
    switch (normalizeProductOrderStatus(status)) {
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
    return this.normalizedStatus === 'Cancelled';
  }

  get trackingSteps(): Array<{
    key: string;
    label: string;
    done: boolean;
    current: boolean;
  }> {
    const order = this.order;
    const status = this.normalizedStatus;
    if (!order || !status) return [];

    const rank: Record<string, number> = {
      PendingPayment: 0,
      Paid: 1,
      Completed: 2,
      Shipped: 3,
      Delivered: 4,
      Cancelled: -1,
    };
    const currentRank = rank[status] ?? 0;

    if (this.isCancelled) {
      return [
        {
          key: 'created',
          label: this._localization.translate('modules.orders.trackingSteps.created'),
          done: true,
          current: false,
        },
        {
          key: 'cancelled',
          label: this._localization.translate('modules.orders.trackingSteps.cancelled'),
          done: true,
          current: true,
        },
      ];
    }

    return [
      {
        key: 'created',
        label: this._localization.translate('modules.orders.trackingSteps.created'),
        done: true,
        current: status === 'PendingPayment',
      },
      {
        key: 'paid',
        label: this._localization.translate('modules.orders.trackingSteps.paid'),
        done: currentRank >= 1 || !!order.paidAt,
        current: status === 'Paid',
      },
      {
        key: 'completed',
        label: this._localization.translate('modules.orders.trackingSteps.completed'),
        done: currentRank >= 2 || !!order.completedAt,
        current: status === 'Completed',
      },
      {
        key: 'shipped',
        label: this._localization.translate('modules.orders.trackingSteps.shipped'),
        done: currentRank >= 3 || !!order.shippedAt,
        current: status === 'Shipped',
      },
      {
        key: 'delivered',
        label: this._localization.translate('modules.orders.trackingSteps.delivered'),
        done: currentRank >= 4 || !!order.deliveredAt,
        current: status === 'Delivered',
      },
    ];
  }
}
