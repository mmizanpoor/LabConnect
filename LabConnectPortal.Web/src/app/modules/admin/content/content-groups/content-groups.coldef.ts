import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { ContentGroupDto } from '../content.types';

export type ContentGroupAction =
  | { type: 'edit'; row: ContentGroupDto }
  | { type: 'delete'; row: ContentGroupDto };

@Injectable({ providedIn: 'root' })
export class ContentGroupsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<ContentGroupAction>();

  get(): ColDef[] {
    return [
      {
        field: 'contentGroupId',
        headerName: this._localization.translate(
          'modules.admin.contentGroups.columns.id'
        ),
        width: 60,
        cellClass: 'ag-header-center',
        headerClass: 'ag-header-center',
      },
      {
        field: 'title',
        headerName: this._localization.translate(
          'modules.admin.contentGroups.columns.title'
        ),
        minWidth: 240,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
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
        cellRendererParams: (p: ICellRendererParams<ContentGroupDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.edit'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              action: (params: ICellRendererParams<ContentGroupDto>) =>
                params.data
                  ? this.actionClicked.emit({ type: 'edit', row: params.data })
                  : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              action: (params: ICellRendererParams<ContentGroupDto>) =>
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
