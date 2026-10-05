import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { AdvertisementPositionDto } from './advertisement-commerce.types';

export type AdPositionAction =
  | { type: 'edit'; row: AdvertisementPositionDto }
  | { type: 'delete'; row: AdvertisementPositionDto };

@Injectable({ providedIn: 'root' })
export class AdPositionsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<AdPositionAction>();

  get(): ColDef[] {
    return [
      {
        field: 'code',
        headerName: this._localization.translate('modules.admin.adCommerce.positions.columns.code'),
        minWidth: 140,
      },
      {
        field: 'title',
        headerName: this._localization.translate('modules.admin.adCommerce.positions.columns.title'),
        minWidth: 180,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'maxConcurrentSlots',
        headerName: this._localization.translate('modules.admin.adCommerce.positions.columns.maxSlots'),
        minWidth: 120,
      },
      {
        field: 'maxDisplayCount',
        headerName: this._localization.translate('modules.admin.adCommerce.positions.columns.maxDisplay'),
        minWidth: 120,
        valueGetter: (p) => p.data?.maxDisplayCount ?? '—',
      },
      {
        field: 'isActive',
        headerName: this._localization.translate('modules.admin.adCommerce.positions.columns.isActive'),
        minWidth: 100,
        valueGetter: (p) =>
          p.data?.isActive
            ? this._localization.translate('shared.yes')
            : this._localization.translate('shared.no'),
      },
      {
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        width: 160,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams<AdvertisementPositionDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<AdvertisementPositionDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<AdvertisementPositionDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'delete', row: params.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }
}
