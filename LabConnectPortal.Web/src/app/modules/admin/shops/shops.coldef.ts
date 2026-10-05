import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import {
  CellClassParams,
  ColDef,
  ICellRendererParams,
} from 'ag-grid-community';
import { CenterProfileListItemDto } from './shops.types';

export type ShopAction =
  | { type: 'detail'; row: CenterProfileListItemDto }
  | { type: 'enableApiKey'; row: CenterProfileListItemDto };

@Injectable({ providedIn: 'root' })
export class ShopsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<ShopAction>();

  get(): ColDef[] {
    return [
      {
        field: 'name',
        headerName: this._localization.translate(
          'modules.admin.shops.columns.name'
        ),
        minWidth: 180,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'mobileNumber',
        headerName: this._localization.translate('shared.mobile'),
        width: 140,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'completionText',
        headerName: this._localization.translate(
          'modules.admin.shops.columns.completion'
        ),
        width: 80,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
        cellClassRules: {
          'text-green-500': (p: CellClassParams) => !!p.data?.isComplete,
          'text-red-500': (p: CellClassParams) => !p.data?.isComplete,
        },
      },
      {
        field: 'approvalText',
        headerName: this._localization.translate(
          'modules.admin.shops.columns.approval'
        ),
        width: 80,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
        cellClassRules: {
          'text-green-500': (p: CellClassParams) => !!p.data?.isApproved,
          'text-red-500': (p: CellClassParams) => !p.data?.isApproved,
        },
      },
      {
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 180,
        minWidth: 180,
        maxWidth: 180,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.view'),
              icon: 'heroicons_outline:eye',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'detail',
                      row: params.data,
                    })
                  : undefined,
            },
            ...(p.data?.isApiKeyEnabled ? [] : [{
              label: this._localization.translate('shared.enableApiKey'),
              icon: 'heroicons_outline:key',
              iconClass: 'text-emerald-600',
              action: (params: ICellRendererParams) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'enableApiKey',
                      row: params.data,
                    })
                  : undefined,
            }]),
          ],
        }),
      },
    ];
  }
}
