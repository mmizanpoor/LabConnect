import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { PillStatusCellRenderer } from '@modules/base/components/base-grid/renderer/pill-status-cell/pill-status-cell.renderer';
import {
  AdminProductOrderListItemDto,
  normalizeProductOrderStatus,
  ProductOrderStatus,
} from './admin-product-orders.types';

export type AdminProductOrderAction = {
  type: 'track';
  row: AdminProductOrderListItemDto;
};

@Injectable({ providedIn: 'root' })
export class AdminProductOrdersColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<AdminProductOrderAction>();

  get(
    statusClass: (status: ProductOrderStatus) => string,
    statusLabel: (status: ProductOrderStatus) => string
  ): ColDef[] {
    return [
      {
        headerName: this._localization.translate(
          'modules.admin.productOrders.columns.center'
        ),
        field: 'centerName',
        minWidth: 180,
        flex: 1,
      },
      {
        headerName: this._localization.translate(
          'modules.admin.productOrders.columns.buyer'
        ),
        field: 'buyerName',
        width: 120,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
      },
      {
        headerName: this._localization.translate(
          'modules.admin.productOrders.columns.phone'
        ),
        field: 'buyerPhone',
        width: 130,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
        valueGetter: (p) => p.data?.buyerPhone?.trim() || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.admin.productOrders.columns.items'
        ),
        field: 'itemCount',
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
      },
      {
        headerName: this._localization.translate(
          'modules.admin.productOrders.columns.total'
        ),
        width: 120,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
        valueGetter: (p) =>
          p.data?.totalAmount != null
            ? `${p.data.totalAmount.toLocaleString(
                'fa-IR'
              )} ${this._localization.translate('shared.currency')}`
            : '—',
      },
      {
        headerName: this._localization.translate(
          'modules.admin.productOrders.columns.tracking'
        ),
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
        valueGetter: (p) => p.data?.trackingCode?.trim() || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.admin.productOrders.columns.status'
        ),
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
        cellRenderer: PillStatusCellRenderer,
        cellRendererParams: (
          p: ICellRendererParams<AdminProductOrderListItemDto>
        ) => ({
          ...p,
          label: (x: ICellRendererParams<AdminProductOrderListItemDto>) => {
            const status = normalizeProductOrderStatus(x.data?.status);
            return status ? statusLabel(status) : '';
          },
          className: (x: ICellRendererParams<AdminProductOrderListItemDto>) => {
            const status = normalizeProductOrderStatus(x.data?.status);
            return status ? statusClass(status) : 'bg-slate-100 text-slate-600';
          },
        }),
      },
      {
        headerName: this._localization.translate(
          'modules.admin.productOrders.columns.createdAt'
        ),
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
        valueGetter: (p) =>
          p.data?.createdAt
            ? new Date(p.data.createdAt).toLocaleDateString('fa-IR')
            : '—',
      },
      {
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        width: 160,
        minWidth: 160,
        maxWidth: 160,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (
          p: ICellRendererParams<AdminProductOrderListItemDto>
        ) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate(
                'modules.admin.productOrders.trackOrder'
              ),
              icon: 'heroicons_outline:map',
              iconClass: 'text-indigo-600',
              action: () =>
                p.data
                  ? this.actionClicked.emit({ type: 'track', row: p.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }
}
