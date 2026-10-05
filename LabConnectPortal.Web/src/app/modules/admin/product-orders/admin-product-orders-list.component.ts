import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { AdminProductOrdersColDef } from './admin-product-orders.coldef';
import { AdminProductOrdersService } from './admin-product-orders.service';
import {
  ADMIN_ORDER_STATUSES,
  AdminProductOrderListItemDto,
  normalizeProductOrderStatus,
  ProductOrderStatus,
} from './admin-product-orders.types';

@Component({
  selector: 'app-admin-product-orders-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './admin-product-orders-list.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }
  `,
})
export class AdminProductOrdersListComponent implements OnInit {
  readonly entity = SystemEntity.ProductOrder;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ProductOrder);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  loading = false;
  error = '';
  rows: AdminProductOrderListItemDto[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  statusFilter = new FormControl<ProductOrderStatus | ''>('');
  searchFilter = new FormControl('', { nonNullable: true });
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<AdminProductOrderListItemDto[]>([]);

  constructor(
    private _service: AdminProductOrdersService,
    private _localization: LocalizationService,
    private _colDef: AdminProductOrdersColDef,
    private _router: Router,
  ) {}

  get statusFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: '', label: this._localization.translate('shared.all') },
      ...ADMIN_ORDER_STATUSES.map((status) => ({
        value: status,
        label: this.statusLabel(status),
      })),
    ];
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get(
      (s) => this.statusClass(s),
      (s) => this.statusLabel(s),
    );
    this._colDef.actionClicked.subscribe((evt) => {
      if (evt?.type === 'track' && evt.row) {
        void this._router.navigate(['/admin/product-orders', evt.row.id]);
      }
    });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getAdminOrders({
        status: this.statusFilter.value || undefined,
        search: this.searchFilter.value.trim() || undefined,
        page: this.page,
        pageSize: this.pageSize,
      });
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.productOrders.errors.loadFailed'),
        );
      }
      this.rows = result.data.items;
      this.totalCount = result.data.totalCount;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.productOrders.errors.loadFailed');
      this.rows = [];
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
}
