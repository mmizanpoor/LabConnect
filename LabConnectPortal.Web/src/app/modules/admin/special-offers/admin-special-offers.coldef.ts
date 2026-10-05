import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { PillStatusCellRenderer } from '@modules/base/components/base-grid/renderer/pill-status-cell/pill-status-cell.renderer';
import { TwoLineCellRenderer } from '@modules/base/components/base-grid/renderer/two-line-cell/two-line-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { AdminSpecialOfferListItem } from './admin-special-offers.types';

export type AdminSpecialOfferAction = {
  type: 'detail';
  row: AdminSpecialOfferListItem;
};

@Injectable({ providedIn: 'root' })
export class AdminSpecialOffersColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<AdminSpecialOfferAction>();

  get(
    formatDate: (value: string) => string,
    statusLabel: (row: AdminSpecialOfferListItem) => string
  ): ColDef[] {
    return [
      {
        field: 'labName',
        headerName: this._localization.translate(
          'modules.admin.specialOffers.columns.labName'
        ),
        minWidth: 200,
        headerClass: 'ag-header-center',
        cellClass: 'font-medium text-gray-900 ag-header-center',
        valueGetter: (p) => p.data?.labName || '—',
      },
      {
        field: 'title',
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.title'
        ),
        minWidth: 200,
        flex: 1,
        headerClass: 'ag-header-center',
        cellClass: 'font-medium text-gray-900 ag-header-center',
        valueGetter: (p) => p.data?.title || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.startDate'
        ),
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
        valueGetter: (p) => formatDate(p.data?.startDate ?? ''),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.endDate'
        ),
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
        valueGetter: (p) => formatDate(p.data?.endDate ?? ''),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.status'
        ),
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
        cellRenderer: PillStatusCellRenderer,
        cellRendererParams: (
          p: ICellRendererParams<AdminSpecialOfferListItem>
        ) => ({
          ...p,
          label: (x: ICellRendererParams<AdminSpecialOfferListItem>) =>
            x.data ? statusLabel(x.data) : '',
          className: (x: ICellRendererParams<AdminSpecialOfferListItem>) => {
            const row = x.data;
            if (!row) return 'bg-slate-100 text-slate-600';
            if (row.isExpired) return 'bg-amber-50 text-amber-700';
            if (row.isActive) return 'bg-emerald-50 text-emerald-700';
            return 'bg-slate-100 text-slate-600';
          },
        }),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.testCount'
        ),
        field: 'testCount',
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.requestCount'
        ),
        field: 'requestCount',
        width: 100,
        headerClass: 'ag-header-center',
        cellClass: 'font-semibold text-violet-700 ag-header-center',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.actions'
        ),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        cellClass: 'ag-header-center',
        width: 100,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (
          p: ICellRendererParams<AdminSpecialOfferListItem>
        ) => ({
          ...p,
          triggerLabel: this._localization.translate(
            'modules.profile.specialOffers.columns.actions'
          ),
          actions: [
            {
              label: this._localization.translate(
                'modules.admin.specialOffers.details'
              ),
              icon: 'heroicons_outline:eye',
              iconClass: 'text-blue-500',
              action: (
                params: ICellRendererParams<AdminSpecialOfferListItem>
              ) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'detail',
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
