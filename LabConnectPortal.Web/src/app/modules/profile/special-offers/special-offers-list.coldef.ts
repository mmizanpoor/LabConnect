import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { PillStatusCellRenderer } from '@modules/base/components/base-grid/renderer/pill-status-cell/pill-status-cell.renderer';
import { TwoLineCellRenderer } from '@modules/base/components/base-grid/renderer/two-line-cell/two-line-cell.renderer';
import { SpecialOfferListItemDto } from './special-offers.types';

export type SpecialOfferListAction =
  | { type: 'edit'; row: SpecialOfferListItemDto }
  | { type: 'requests'; row: SpecialOfferListItemDto }
  | { type: 'delete'; row: SpecialOfferListItemDto };

@Injectable({ providedIn: 'root' })
export class SpecialOffersListColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<SpecialOfferListAction>();

  get(
    formatDate: (value: string) => string,
    statusLabel: (row: SpecialOfferListItemDto) => string,
    canUpdate = true,
    canDelete = true
  ): ColDef[] {
    return [
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.title'
        ),
        minWidth: 200,
        width: 200,
        flex: 1,
        cellRenderer: TwoLineCellRenderer,
        cellRendererParams: (
          p: ICellRendererParams<SpecialOfferListItemDto>
        ) => ({
          ...p,
          title: (x: ICellRendererParams<SpecialOfferListItemDto>) =>
            x.data?.title ?? '',
          subtitle: (x: ICellRendererParams<SpecialOfferListItemDto>) =>
            x.data?.summary ?? '',
        }),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.startDate'
        ),
        minWidth: 100,
        width: 100,
        valueGetter: (p) => formatDate(p.data?.startDate ?? ''),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.endDate'
        ),
        minWidth: 100,
        width: 100,
        valueGetter: (p) => formatDate(p.data?.endDate ?? ''),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.status'
        ),
        minWidth: 100,
        width: 100,
        cellRenderer: PillStatusCellRenderer,
        cellRendererParams: (
          p: ICellRendererParams<SpecialOfferListItemDto>
        ) => ({
          ...p,
          label: (x: ICellRendererParams<SpecialOfferListItemDto>) =>
            x.data ? statusLabel(x.data) : '',
          className: (x: ICellRendererParams<SpecialOfferListItemDto>) => {
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
        minWidth: 120,
        width: 120,
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.requestCount'
        ),
        field: 'requestCount',
        minWidth: 120,
        width: 120,
        cellClass: 'font-semibold text-violet-700',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.specialOffers.columns.actions'
        ),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        minWidth: 100,
        width: 100,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (
          p: ICellRendererParams<SpecialOfferListItemDto>
        ) => ({
          ...p,
          triggerLabel: this._localization.translate(
            'modules.profile.specialOffers.columns.actions'
          ),
          actions: [
            ...(canUpdate
              ? [
                  {
                    label: this._localization.translate(
                      'modules.profile.specialOffers.edit'
                    ),
                    icon: 'heroicons_outline:pencil-square',
                    iconClass: 'text-blue-500',
                    action: (params: ICellRendererParams<SpecialOfferListItemDto>) =>
                      params.data
                        ? this.actionClicked.emit({ type: 'edit', row: params.data })
                        : undefined,
                  },
                ]
              : []),
            {
              label: this._localization.translate(
                'modules.profile.specialOffers.requestList'
              ),
              icon: 'heroicons_outline:list-bullet',
              iconClass: 'text-slate-600',
              action: (params: ICellRendererParams<SpecialOfferListItemDto>) =>
                params.data
                  ? this.actionClicked.emit({
                      type: 'requests',
                      row: params.data,
                    })
                  : undefined,
            },
            ...(canDelete
              ? [
                  {
                    label: this._localization.translate('shared.delete'),
                    icon: 'heroicons_outline:trash',
                    iconClass: 'text-red-500',
                    danger: true,
                    action: (params: ICellRendererParams<SpecialOfferListItemDto>) =>
                      params.data
                        ? this.actionClicked.emit({
                            type: 'delete',
                            row: params.data,
                          })
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
