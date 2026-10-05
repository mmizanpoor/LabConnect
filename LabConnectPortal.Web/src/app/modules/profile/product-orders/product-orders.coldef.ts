import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { PillStatusCellRenderer } from '@modules/base/components/base-grid/renderer/pill-status-cell/pill-status-cell.renderer';
import {
  ProductOrderListItemDto,
  ProductOrderShipmentItemDto,
  ProductOrderStatus,
} from './product-orders.types';

export type ProductOrderAction =
  | { type: 'track'; row: ProductOrderListItemDto }
  | { type: 'markPaid'; row: ProductOrderListItemDto }
  | { type: 'cancel'; row: ProductOrderListItemDto };

export type ProductShipmentAction = { type: 'markShipped'; row: ProductOrderShipmentItemDto };

const STATUS_BY_CODE: Record<number, ProductOrderStatus> = {
  0: 'PendingPayment',
  1: 'Paid',
  2: 'Completed',
  3: 'Cancelled',
  4: 'Shipped',
  5: 'Delivered',
};

export function normalizeProductOrderStatus(
  status: ProductOrderStatus | number | string | null | undefined,
): ProductOrderStatus | null {
  if (status == null || status === '') return null;
  if (typeof status === 'number') return STATUS_BY_CODE[status] ?? null;
  if (typeof status === 'string' && /^\d+$/.test(status)) {
    return STATUS_BY_CODE[Number(status)] ?? null;
  }
  const asString = String(status);
  if (
    asString === 'PendingPayment' ||
    asString === 'Paid' ||
    asString === 'Completed' ||
    asString === 'Cancelled' ||
    asString === 'Shipped' ||
    asString === 'Delivered'
  ) {
    return asString;
  }
  return null;
}

function isStatus(
  row: ProductOrderListItemDto | undefined,
  expected: ProductOrderStatus,
): boolean {
  return normalizeProductOrderStatus(row?.status) === expected;
}

@Injectable({ providedIn: 'root' })
export class ProductOrdersColDef {
  constructor(private _localization: LocalizationService) {}

  orderActionClicked = new EventEmitter<ProductOrderAction>();
  shipmentActionClicked = new EventEmitter<ProductShipmentAction>();

  orders(
    statusClass: (status: ProductOrderStatus) => string,
    statusLabel: (status: ProductOrderStatus) => string,
    canUpdate: () => boolean = () => true,
  ): ColDef[] {
    return [
      {
        headerName: this._localization.translate('modules.profile.productOrders.columns.center'),
        field: 'centerName',
        minWidth: 220,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        headerName: this._localization.translate('modules.profile.productOrders.columns.items'),
        field: 'itemCount',
        minWidth: 130,
      },
      {
        headerName: this._localization.translate('modules.profile.productOrders.columns.total'),
        minWidth: 190,
        valueGetter: (p) =>
          p.data?.totalAmount != null
            ? `${p.data.totalAmount.toLocaleString('fa-IR')} ${this._localization.translate('shared.currency')}`
            : '—',
      },
      {
        headerName: this._localization.translate('modules.profile.productOrders.columns.status'),
        minWidth: 160,
        cellRenderer: PillStatusCellRenderer,
        cellRendererParams: (p: ICellRendererParams<ProductOrderListItemDto>) => ({
          ...p,
          label: (x: ICellRendererParams<ProductOrderListItemDto>) => {
            const status = normalizeProductOrderStatus(x.data?.status);
            return status ? statusLabel(status) : '';
          },
          className: (x: ICellRendererParams<ProductOrderListItemDto>) => {
            const status = normalizeProductOrderStatus(x.data?.status);
            return status ? statusClass(status) : 'bg-slate-100 text-slate-600';
          },
        }),
      },
      {
        headerName: this._localization.translate('modules.profile.productOrders.columns.createdAt'),
        minWidth: 160,
        valueGetter: (p) =>
          p.data?.createdAt ? new Date(p.data.createdAt).toLocaleDateString('fa-IR') : '—',
      },
      {
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 160,
        minWidth: 160,
        maxWidth: 160,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams<ProductOrderListItemDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('modules.profile.productOrders.trackOrder'),
              icon: 'heroicons_outline:map',
              iconClass: 'text-indigo-600',
              action: () =>
                p.data ? this.orderActionClicked.emit({ type: 'track', row: p.data }) : undefined,
            },
            {
              label: this._localization.translate('modules.profile.productOrders.markPaid'),
              icon: 'heroicons_outline:check-circle',
              iconClass: 'text-green-600',
              hidden: () => !canUpdate() || !isStatus(p.data, 'PendingPayment'),
              action: () =>
                p.data ? this.orderActionClicked.emit({ type: 'markPaid', row: p.data }) : undefined,
            },
            {
              label: this._localization.translate('shared.cancel'),
              icon: 'heroicons_outline:x-circle',
              iconClass: 'text-red-500',
              danger: true,
              hidden: () =>
                !canUpdate() ||
                (!isStatus(p.data, 'PendingPayment') && !isStatus(p.data, 'Paid')),
              action: () =>
                p.data ? this.orderActionClicked.emit({ type: 'cancel', row: p.data }) : undefined,
            },
          ],
        }),
      },
    ];
  }
}
