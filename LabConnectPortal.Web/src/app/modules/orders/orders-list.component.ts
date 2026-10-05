import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DecimalPipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { Subject, firstValueFrom, takeUntil } from 'rxjs';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import {
  BASE_DIALOG_PANEL_CLASS,
} from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseConfirmDialogComponent } from '@modules/base/components/base-dialog/base-confirm-dialog.component';
import { OrdersService } from './orders.service';
import {
  GetMyOrdersQuery,
  MY_ORDER_STATUSES,
  ProductOrderListItemDto,
  ProductOrderStatus,
} from './orders.types';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatDialogModule,
    MatIconModule,
    DecimalPipe,
    TranslocoPipe,
    BasePagingComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './orders-list.component.html',
  styleUrl: './orders-list.component.scss',
})
export class OrdersListComponent implements OnInit, OnDestroy {
  private readonly _dialog = inject(MatDialog);
  private readonly _destroy$ = new Subject<void>();

  loading = false;
  error = '';
  rows: ProductOrderListItemDto[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  statusFilter = new FormControl<ProductOrderStatus | ''>('', { nonNullable: true });
  statusFilterOptions: BaseFormSelectOption[] = [];

  constructor(
    private _ordersService: OrdersService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    this.statusFilterOptions = [
      { value: '', label: this._localization.translate('shared.all') },
      ...MY_ORDER_STATUSES.map((status) => ({
        value: status,
        label: this.statusLabel(status),
      })),
    ];

    this.statusFilter.valueChanges.pipe(takeUntil(this._destroy$)).subscribe(() => {
      this.page = 1;
      void this.load();
    });

    void this.load();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const status = this.statusFilter.value;
      const query: GetMyOrdersQuery = {
        page: this.page,
        pageSize: this.pageSize,
      };
      if (status) query.status = status;

      const result = await this._ordersService.getMyOrders(query);
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.orders.errors.loadFailed'));
      }
      this.rows = result.data.items;
      this.totalCount = result.data.totalCount;
    } catch (e: unknown) {
      this.error = this.resolveLoadError(e);
    } finally {
      this.loading = false;
    }
  }

  private resolveLoadError(e: unknown): string {
    const fallback = this._localization.translate('modules.orders.errors.loadFailed');
    if (e instanceof HttpErrorResponse) {
      const body = e.error as { message?: string; title?: string } | string | null;
      if (typeof body === 'string' && body.trim()) return body;
      if (body && typeof body === 'object') {
        const msg = body.message ?? body.title;
        if (typeof msg === 'string' && msg.trim()) return msg;
      }
      if (e.status === 0) return fallback;
      return fallback;
    }
    if (e instanceof Error && e.message) return e.message;
    return fallback;
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
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

  async cancel(row: ProductOrderListItemDto): Promise<void> {
    if (!confirm(this._localization.translate('modules.orders.confirmCancel'))) return;
    const result = await this._ordersService.cancel(row.id);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.orders.errors.cancelFailed');
      return;
    }
    await this.load();
  }

  async confirmDelivery(row: ProductOrderListItemDto): Promise<void> {
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

    const result = await this._ordersService.confirmDelivery(row.id);
    if (!result.success) {
      this.error =
        result.message ?? this._localization.translate('modules.orders.errors.confirmDeliveryFailed');
      return;
    }
    await this.load();
  }
}
