import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import {
  CellClassParams,
  ColDef,
  ICellRendererParams,
} from 'ag-grid-community';
import { CenterProfileListItemDto } from './laboratories.types';

export type LaboratoryAction =
  | { type: 'detail'; row: CenterProfileListItemDto }
  | { type: 'editMobile'; row: CenterProfileListItemDto }
  | { type: 'enableApiKey'; row: CenterProfileListItemDto };

@Injectable({ providedIn: 'root' })
export class LaboratoriesColDef {
  constructor(private _localizationService: LocalizationService) {}

  actionClicked = new EventEmitter<LaboratoryAction>();

  public get(): ColDef[] {
    return [
      {
        field: 'name',
        headerName: this._localizationService.translate(
          'modules.admin.laboratories.columns.name'
        ),
        minWidth: 160,
        flex: 1,
        cellClass: 'font-medium text-gray-900 ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'labCode',
        headerName: this._localizationService.translate(
          'modules.admin.laboratories.columns.labCode'
        ),
        width: 100,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'mobileNumber',
        headerName: this._localizationService.translate('shared.mobile'),
        width: 120,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'statusText',
        headerName: this._localizationService.translate(
          'modules.admin.laboratories.columns.status'
        ),
        width: 110,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
        cellClassRules: {
          'text-amber-600': (params: CellClassParams) =>
            params.data?.status === 'Pending' || params.data?.status === 0,
          'text-green-600': (params: CellClassParams) =>
            params.data?.status === 'Active' || params.data?.status === 1,
          'text-red-500': (params: CellClassParams) =>
            params.data?.status === 'Suspended' || params.data?.status === 2,
        },
      },
      {
        field: 'completionText',
        headerName: this._localizationService.translate(
          'modules.admin.laboratories.columns.completion'
        ),
        width: 110,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
        cellClassRules: {
          'text-green-500': (params: CellClassParams) =>
            !!params.data?.isComplete,
          'text-red-500': (params: CellClassParams) => !params.data?.isComplete,
        },
      },
      {
        field: 'approvalText',
        headerName: this._localizationService.translate(
          'modules.admin.laboratories.columns.approval'
        ),
        width: 110,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
        cellClassRules: {
          'text-green-500': (params: CellClassParams) =>
            !!params.data?.isApproved,
          'text-red-500': (params: CellClassParams) => !params.data?.isApproved,
        },
      },
      {
        headerName: this._localizationService.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 220,
        minWidth: 220,
        maxWidth: 220,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams) => ({
          ...p,
          triggerLabel: this._localizationService.translate('shared.actions'),
          actions: [
            {
              label: this._localizationService.translate('shared.view'),
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
            {
              label: this._localizationService.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-amber-500',
              action: (params: ICellRendererParams) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'editMobile',
                      row: params.data,
                    })
                  : undefined,
            },
            ...(p.data?.isApiKeyEnabled ? [] : [{
              label: this._localizationService.translate('shared.enableApiKey'),
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
