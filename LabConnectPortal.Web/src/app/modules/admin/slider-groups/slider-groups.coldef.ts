import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { SliderGroupListItemDto } from './slider-groups.types';

export type SliderGroupAction =
  | { type: 'edit'; row: SliderGroupListItemDto }
  | { type: 'delete'; row: SliderGroupListItemDto };

@Injectable({ providedIn: 'root' })
export class SliderGroupsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<SliderGroupAction>();

  get(): ColDef[] {
    return [
      {
        field: 'title',
        headerName: this._localization.translate(
          'modules.admin.sliderGroups.columns.title'
        ),
        minWidth: 220,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'datesText',
        headerName: this._localization.translate(
          'modules.admin.sliderGroups.columns.dates'
        ),
        minWidth: 200,
      },
      {
        field: 'statusText',
        headerName: this._localization.translate(
          'modules.admin.sliderGroups.columns.status'
        ),
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
      },
      {
        field: 'slideCount',
        headerName: this._localization.translate(
          'modules.admin.sliderGroups.columns.slides'
        ),
        width: 80,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
      },
      {
        headerName: this._localization.translate(
          'modules.admin.sliderGroups.columns.actions'
        ),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 160,
        minWidth: 160,
        maxWidth: 160,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (
          p: ICellRendererParams<SliderGroupListItemDto & any>
        ) => ({
          ...p,
          triggerLabel: this._localization.translate(
            'modules.admin.sliderGroups.columns.actions'
          ),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<any>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<any>) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'delete',
                      row: params.data,
                    })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }
}
