import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { TestInfoListItemDto } from './test-infos.types';

export type TestInfoAction =
  | { type: 'detail'; row: TestInfoListItemDto }
  | { type: 'editPrice'; row: TestInfoListItemDto };

@Injectable({ providedIn: 'root' })
export class TestInfosColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<TestInfoAction>();

  get(
    formatPrice: (value: number | null | undefined) => string,
    canUpdate = true,
  ): ColDef[] {
    return [
      {
        headerName: this._localization.translate(
          'modules.profile.testInfos.columns.cpnCode'
        ),
        field: 'cpnCode',
        minWidth: 100,
        width: 100,
        valueGetter: (p) => p.data?.cpnCode || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.testInfos.columns.nationalCode'
        ),
        field: 'nationalCode',
        minWidth: 100,
        width: 100,
        valueGetter: (p) => p.data?.nationalCode || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.testInfos.columns.fullName'
        ),
        field: 'fullName',
        minWidth: 200,
        width: 200,
        flex: 1,
        cellClass: 'font-medium text-slate-800',
        valueGetter: (p) => p.data?.fullName || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.testInfos.columns.shortName'
        ),
        field: 'shortName',
        minWidth: 200,
        width: 200,
        flex: 1,
        valueGetter: (p) => p.data?.shortName || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.testInfos.columns.approvePrice'
        ),
        minWidth: 140,
        width: 140,
        valueGetter: (p) => formatPrice(p.data?.approvePrice),
        cellClass: 'font-semibold text-lab',
      },
      {
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 100,
        minWidth: 100,
        maxWidth: 100,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams<TestInfoListItemDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate(
                'modules.profile.testInfos.viewDetail'
              ),
              icon: 'heroicons_outline:eye',
              iconClass: 'text-blue-500',
              action: (x: ICellRendererParams<TestInfoListItemDto>) =>
                x.data
                  ? this.actionClicked.emit({ type: 'detail', row: x.data })
                  : undefined,
            },
            ...(canUpdate
              ? [
                  {
                    label: this._localization.translate(
                      'modules.profile.testInfos.editPrice'
                    ),
                    icon: 'heroicons_outline:pencil-square',
                    iconClass: 'text-blue-500',
                    action: (x: ICellRendererParams<TestInfoListItemDto>) =>
                      x.data
                        ? this.actionClicked.emit({ type: 'editPrice', row: x.data })
                        : undefined,
                  },
                ]
              : []),
          ],
        }),
      },
    ];
  }
}
