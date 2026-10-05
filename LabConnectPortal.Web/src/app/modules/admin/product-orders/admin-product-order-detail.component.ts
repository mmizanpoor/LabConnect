import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { DatePipe, DecimalPipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BreadcrumbService } from '@core/services/breadcrumb/breadcrumb.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { AdminProductOrdersService } from './admin-product-orders.service';
import {
  ProductOrderDto,
  ProductOrderStatus,
  ShippingMethod,
} from './admin-product-orders.types';

@Component({
  selector: 'app-admin-product-order-detail',
  standalone: true,
  imports: [
    MatIconModule,
    DatePipe,
    DecimalPipe,
    TranslocoPipe,
    BaseBackButtonComponent,
  ],
  templateUrl: './admin-product-order-detail.component.html',
  styleUrl: './admin-product-order-detail.component.scss',
})
export class AdminProductOrderDetailComponent implements OnInit {
  readonly entity = SystemEntity.ProductOrder;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ProductOrder);

  loading = true;
  error = '';
  order: ProductOrderDto | null = null;

  constructor(
    private _route: ActivatedRoute,
    private _service: AdminProductOrdersService,
    private _localization: LocalizationService,
    private _breadcrumb: BreadcrumbService,
  ) {}

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    const id = this._route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = this._localization.translate('modules.admin.productOrders.errors.notFound');
      this.loading = false;
      return;
    }

    try {
      const result = await this._service.getAdminOrderById(id);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.productOrders.errors.notFound'),
        );
      }
      this.order = result.data;
      this._breadcrumb.setDynamicLabel(result.data.centerName);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.productOrders.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  statusLabel(status: ProductOrderStatus): string {
    return this._localization.translate(`enum.productOrderStatus.${status}`);
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

  shippingMethodLabel(method: ShippingMethod | number | string | null | undefined): string {
    const map: Record<number, ShippingMethod> = { 0: 'Post', 1: 'Courier', 2: 'Pickup' };
    let normalized: ShippingMethod | null = null;
    if (typeof method === 'number') normalized = map[method] ?? null;
    else if (method === 'Post' || method === 'Courier' || method === 'Pickup') normalized = method;
    else if (typeof method === 'string' && /^\d+$/.test(method)) normalized = map[Number(method)] ?? null;
    if (!normalized) return '—';
    return this._localization.translate(`enum.shippingMethod.${normalized}`);
  }

  get isCancelled(): boolean {
    return this.order?.status === 'Cancelled';
  }

  get trackingSteps(): Array<{
    key: string;
    label: string;
    done: boolean;
    current: boolean;
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
