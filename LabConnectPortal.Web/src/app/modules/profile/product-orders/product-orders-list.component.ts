import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import { downloadTableAsExcel } from '@core/utils/excel-export.util';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { ProductOrdersService } from './product-orders.service';
import {
  PRODUCT_ORDER_STATUSES,
  ProductOrderListItemDto,
  ProductOrderStatus,
} from './product-orders.types';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import {
  ProductOrdersColDef,
  ProductOrderAction,
  normalizeProductOrderStatus,
} from './product-orders.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

@Component({
  selector: 'app-product-orders-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './product-orders-list.component.html',
  styleUrl: './product-orders-list.component.scss',
})
export class ProductOrdersListComponent implements OnInit, AfterViewInit {
  readonly entity = SystemEntity.ProductOrder;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ProductOrder);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  loading = false;
  exporting = false;
  error = '';
  rows: ProductOrderListItemDto[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  statusFilter = new FormControl<ProductOrderStatus | ''>('');
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ProductOrderListItemDto[]>([]);

  constructor(
    private _productOrdersService: ProductOrdersService,
    private _localization: LocalizationService,
    private _colDef: ProductOrdersColDef,
    private _labPermission: LabPermissionService,
    private _router: Router,
  ) {}

  get canUpdate(): boolean {
    return this._labPermission.can(this.entity, 'update');
  }

  get canExport(): boolean {
    return this._labPermission.canExport(this.entity);
  }

  get statusFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: '', label: this._localization.translate('shared.all') },
      ...PRODUCT_ORDER_STATUSES.map((status) => ({
        value: status,
        label: this.statusLabel(status),
      })),
    ];
  }

  ngOnInit(): void {
    this.colDef = this._colDef.orders(
      (s) => this.statusClass(s),
      (s) => this.statusLabel(s),
      () => this.canUpdate,
    );
    this._colDef.orderActionClicked.subscribe((evt: ProductOrderAction) => {
      if (!evt?.row) return;
      if (evt.type === 'track') {
        void this._router.navigate(['/profile/product-orders', evt.row.id]);
        return;
      }
      if (evt.type === 'markPaid') void this.markPaid(evt.row);
      if (evt.type === 'cancel') void this.cancel(evt.row);
    });
    void this.load();
  }

  ngAfterViewInit(): void {
    queueMicrotask(() => this.filterDrawer?.open());
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._productOrdersService.getCenterOrders({
        status: this.statusFilter.value || undefined,
        page: this.page,
        pageSize: this.pageSize,
      });
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.profile.productOrders.errors.loadFailed'),
        );
      }
      this.rows = result.data.items;
      this.totalCount = result.data.totalCount;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.productOrders.errors.loadFailed');
      this.rows = [];
      this.totalCount = 0;
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
  }

  applyFilters(): void {
    this.page = 1;
    void this.filterDrawer?.close();
    void this.load();
  }

  statusLabel(status: ProductOrderStatus | number | string): string {
    const normalized = normalizeProductOrderStatus(status);
    if (!normalized) return '—';
    return this._localization.translate(`enum.productOrderStatus.${normalized}`);
  }

  statusClass(status: ProductOrderStatus | number | string): string {
    switch (normalizeProductOrderStatus(status)) {
      case 'Paid':
        return 'bg-blue-100 text-blue-800';
      case 'Completed':
        return 'bg-green-100 text-green-800';
      case 'Shipped':
        return 'bg-indigo-100 text-indigo-800';
      case 'Delivered':
        return 'bg-teal-100 text-teal-800';
      case 'Cancelled':
        return 'bg-gray-100 text-gray-700';
      default:
        return 'bg-amber-100 text-amber-800';
    }
  }

  async markPaid(row: ProductOrderListItemDto): Promise<void> {
    const result = await this._productOrdersService.markPaid(row.id);
    if (!result.success) {
      this.error =
        result.message ??
        this._localization.translate('modules.profile.productOrders.errors.markPaidFailed');
      return;
    }
    await this.load();
  }

  async cancel(row: ProductOrderListItemDto): Promise<void> {
    if (!confirm(this._localization.translate('modules.profile.productOrders.confirmCancel'))) return;
    const result = await this._productOrdersService.cancelCenterOrder(row.id);
    if (!result.success) {
      this.error =
        result.message ??
        this._localization.translate('modules.profile.productOrders.errors.cancelFailed');
      return;
    }
    await this.load();
  }

  async exportOrdersToExcel(): Promise<void> {
    if (!this.canExport || this.exporting || !this.totalCount) return;

    this.exporting = true;
    this.error = '';
    try {
      const items = await this.fetchAllFilteredOrders();
      if (!items.length) {
        this.error = this._localization.translate('modules.profile.productOrders.errors.exportEmpty');
        return;
      }

      const headers = [
        this._localization.translate('modules.profile.productOrders.columns.center'),
        this._localization.translate('modules.profile.productOrders.columns.address'),
        this._localization.translate('modules.profile.productOrders.columns.phone'),
        this._localization.translate('modules.profile.productOrders.columns.items'),
        this._localization.translate('modules.profile.productOrders.columns.total'),
        this._localization.translate('modules.profile.productOrders.columns.status'),
        this._localization.translate('modules.profile.productOrders.columns.createdAt'),
      ];

      const currency = this._localization.translate('shared.currency');
      const rows = items.map((row) => [
        row.centerName,
        row.buyerAddress?.trim() || '—',
        row.buyerPhone?.trim() || '—',
        String(row.itemCount),
        row.totalAmount != null ? `${row.totalAmount.toLocaleString('fa-IR')} ${currency}` : '—',
        this.statusLabel(row.status),
        row.createdAt ? new Date(row.createdAt).toLocaleDateString('fa-IR') : '—',
      ]);

      downloadTableAsExcel('product-orders', headers, rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.productOrders.errors.exportFailed');
    } finally {
      this.exporting = false;
    }
  }

  private async fetchAllFilteredOrders(): Promise<ProductOrderListItemDto[]> {
    const pageSize = 200;
    let page = 1;
    let total = Number.POSITIVE_INFINITY;
    const all: ProductOrderListItemDto[] = [];
    const status = this.statusFilter.value || undefined;

    while (all.length < total) {
      const result = await this._productOrdersService.getCenterOrders({
        status,
        page,
        pageSize,
      });
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.profile.productOrders.errors.exportFailed'),
        );
      }
      all.push(...result.data.items);
      total = result.data.totalCount;
      if (!result.data.items.length) break;
      page += 1;
    }

    return all;
  }
}
