import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { AdvertisementOrderDto } from './advertisement-commerce.types';

export type AdOrderAction =
  | { type: 'edit'; row: AdvertisementOrderDto }
  | { type: 'delete'; row: AdvertisementOrderDto };

@Injectable({ providedIn: 'root' })
export class AdOrdersColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<AdOrderAction>();

  get(): ColDef[] {
    return [
      {
        field: 'positionTitle',
        headerName: this._localization.translate('modules.admin.adCommerce.orders.columns.position'),
        minWidth: 160,
      },
      {
        field: 'durationTitle',
        headerName: this._localization.translate('modules.admin.adCommerce.orders.columns.duration'),
        minWidth: 120,
      },
      {
        field: 'userDisplayName',
        headerName: this._localization.translate('modules.admin.adCommerce.orders.columns.user'),
        minWidth: 150,
      },
      {
        field: 'price',
        headerName: this._localization.translate('modules.admin.adCommerce.orders.columns.price'),
        minWidth: 120,
        valueGetter: (p) =>
          p.data?.price != null
            ? `${new Intl.NumberFormat('fa-IR').format(p.data.price)}`
            : '',
      },
      {
        field: 'startDate',
        headerName: this._localization.translate('modules.admin.adCommerce.orders.columns.startDate'),
        minWidth: 130,
        valueGetter: (p) => this.formatDate(p.data?.startDate),
      },
      {
        field: 'endDate',
        headerName: this._localization.translate('modules.admin.adCommerce.orders.columns.endDate'),
        minWidth: 130,
        valueGetter: (p) => this.formatDate(p.data?.endDate),
      },
      {
        field: 'statusTitle',
        headerName: this._localization.translate('modules.admin.adCommerce.orders.columns.status'),
        minWidth: 120,
      },
      {
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        width: 160,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams<AdvertisementOrderDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<AdvertisementOrderDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<AdvertisementOrderDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'delete', row: params.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }

  private formatDate(value?: string | null): string {
    if (!value) return '';
    return new Intl.DateTimeFormat('fa-IR', { dateStyle: 'medium' }).format(new Date(value));
  }
}
