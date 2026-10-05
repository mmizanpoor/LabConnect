import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { SpecialOfferRequestDto, SpecialOfferRequestStatus } from '../special-offers.types';

export type SpecialOfferRequestAction =
  | { type: 'approve'; row: SpecialOfferRequestDto }
  | { type: 'reject'; row: SpecialOfferRequestDto }
  | { type: 'createContract'; row: SpecialOfferRequestDto };

@Injectable({ providedIn: 'root' })
export class SpecialOfferRequestsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<SpecialOfferRequestAction>();

  get(
    formatDateOnly: (value: string) => string,
    formatTimeOnly: (value: string) => string,
    statusKey: (status: SpecialOfferRequestStatus) => string,
    statusClass: (status: SpecialOfferRequestStatus) => string,
    canApprove: (row: SpecialOfferRequestDto) => boolean,
    canReject: (row: SpecialOfferRequestDto) => boolean,
    canCreateContract: (row: SpecialOfferRequestDto) => boolean,
  ): ColDef[] {
    return [
      {
        headerName: this._localization.translate('modules.profile.specialOffers.requestsDialog.user'),
        field: 'userDisplayName',
        minWidth: 180,
        flex: 1,
        cellClass: 'font-bold text-gray-900',
        valueGetter: (p) => p.data?.userDisplayName || '—',
      },
      {
        headerName: this._localization.translate('modules.profile.specialOffers.requestsDialog.requesterLab'),
        field: 'requesterLabName',
        minWidth: 180,
        flex: 1,
        cellClass: 'font-semibold text-slate-600',
        valueGetter: (p) => p.data?.requesterLabName || '—',
      },
      {
        headerName: this._localization.translate('modules.profile.specialOffers.requestsDialog.description'),
        field: 'description',
        minWidth: 260,
        flex: 2,
        cellClass: 'whitespace-pre-wrap',
        valueGetter: (p) => p.data?.description || '—',
      },
      {
        headerName: this._localization.translate('modules.profile.specialOffers.requestsDialog.status'),
        minWidth: 180,
        cellRenderer: (p: ICellRendererParams<SpecialOfferRequestDto>) => {
          const s = p.data?.status;
          if (s == null) return '—';
          return `
            <span class="inline-flex items-center rounded-full px-3 py-1.5 text-[0.8125rem] font-bold leading-tight ${statusClass(s)}">
              ${this._localization.translate(statusKey(s))}
            </span>
          `;
        },
      },
      {
        headerName: this._localization.translate('modules.profile.specialOffers.requestsDialog.date'),
        minWidth: 170,
        valueGetter: (p) => {
          const v = p.data?.createdAt ?? '';
          const d = formatDateOnly(v);
          const t = formatTimeOnly(v);
          return t ? `${d} ${t}` : d;
        },
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
        cellRendererParams: (p: ICellRendererParams<SpecialOfferRequestDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('modules.profile.specialOffers.requestsDialog.approve'),
              icon: 'heroicons_outline:check-circle',
              iconClass: 'text-green-600',
              hidden: (x: ICellRendererParams<SpecialOfferRequestDto>) => !x.data || !canApprove(x.data),
              action: (x: ICellRendererParams<SpecialOfferRequestDto>) =>
                x.data ? this.actionClicked.emit({ type: 'approve', row: x.data }) : undefined,
            },
            {
              label: this._localization.translate('modules.profile.specialOffers.requestsDialog.reject'),
              icon: 'heroicons_outline:x-circle',
              iconClass: 'text-red-500',
              danger: true,
              hidden: (x: ICellRendererParams<SpecialOfferRequestDto>) => !x.data || !canReject(x.data),
              action: (x: ICellRendererParams<SpecialOfferRequestDto>) =>
                x.data ? this.actionClicked.emit({ type: 'reject', row: x.data }) : undefined,
            },
            {
              label: this._localization.translate('modules.profile.specialOffers.requestsDialog.createContract'),
              icon: 'heroicons_outline:document-text',
              iconClass: 'text-violet-600',
              hidden: (x: ICellRendererParams<SpecialOfferRequestDto>) => !x.data || !canCreateContract(x.data),
              action: (x: ICellRendererParams<SpecialOfferRequestDto>) =>
                x.data ? this.actionClicked.emit({ type: 'createContract', row: x.data }) : undefined,
            },
          ],
        }),
      },
    ];
  }
}

