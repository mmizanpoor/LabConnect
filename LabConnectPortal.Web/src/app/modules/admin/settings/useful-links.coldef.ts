import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { IconActionsCellRenderer } from '@modules/base/components/base-grid/renderer/icon-actions-cell/icon-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { SiteUsefulLinkDto } from './site-settings.types';

export type UsefulLinkAction = { type: 'delete'; row: SiteUsefulLinkDto };

@Injectable({ providedIn: 'root' })
export class UsefulLinksColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<UsefulLinkAction>();

  get(canDelete: boolean): ColDef[] {
    const cols: ColDef[] = [
      {
        field: 'title',
        headerName: this._localization.translate(
          'modules.admin.settings.columns.linkTitle'
        ),
        minWidth: 200,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        field: 'url',
        headerName: this._localization.translate(
          'modules.admin.settings.columns.linkUrl'
        ),
        minWidth: 280,
        flex: 1.4,
      },
    ];

    if (canDelete) {
      cols.push({
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 100,
        minWidth: 100,
        maxWidth: 100,
        flex: 0,
        cellClass: 'ag-header-center',
        cellRenderer: IconActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams<SiteUsefulLinkDto>) => ({
          ...p,
          actions: [
            {
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              action: (params: ICellRendererParams<SiteUsefulLinkDto>) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'delete',
                      row: params.data,
                    })
                  : undefined,
            },
          ],
        }),
      });
    }

    return cols;
  }
}
