import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { JobPostingListItemDto, JobPostingStatus } from './job-postings.types';

export type JobPostingAction =
  | { type: 'edit'; row: JobPostingListItemDto }
  | { type: 'publish'; row: JobPostingListItemDto }
  | { type: 'delete'; row: JobPostingListItemDto }
  | { type: 'close'; row: JobPostingListItemDto }
  | { type: 'reopen'; row: JobPostingListItemDto }
  | { type: 'applications'; row: JobPostingListItemDto };

@Injectable({ providedIn: 'root' })
export class JobPostingsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<JobPostingAction>();

  get(
    statusClass: (status: JobPostingStatus) => string,
    statusLabel: (status: JobPostingStatus) => string,
    canView: () => boolean,
    canManage: () => boolean,
    canDelete: () => boolean = canManage,
  ): ColDef[] {
    return [
      {
        headerName: this._localization.translate('modules.profile.jobPostings.columns.category'),
        field: 'jobCategoryName',
        minWidth: 220,
        flex: 1,
        cellClass: 'font-medium text-gray-900',
      },
      {
        headerName: this._localization.translate('modules.profile.jobPostings.columns.location'),
        field: 'locationName',
        minWidth: 180,
        flex: 1,
      },
      {
        headerName: this._localization.translate('modules.profile.jobPostings.columns.status'),
        minWidth: 140,
        valueGetter: (p) => p.data?.status ? statusLabel(p.data.status) : '—',
        cellRenderer: (p: ICellRendererParams<JobPostingListItemDto>) => {
          const status = p.data?.status;
          if (!status) return '—';
          return `<span class="rounded px-2 py-1 text-xs font-medium ${statusClass(status)}">${statusLabel(status)}</span>`;
        },
      },
      {
        headerName: this._localization.translate('modules.profile.jobPostings.columns.createdAt'),
        minWidth: 160,
        valueGetter: (p) => p.data?.createdAt ? new Date(p.data.createdAt).toLocaleDateString('fa-IR') : '—',
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
        cellRendererParams: (p: ICellRendererParams<JobPostingListItemDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('shared.view'),
              icon: 'heroicons_outline:pencil-square',
              iconClass: 'text-blue-500',
              hidden: () => !canView(),
              action: (x: ICellRendererParams<JobPostingListItemDto>) =>
                x.data ? this.actionClicked.emit({ type: 'edit', row: x.data }) : undefined,
            },
            {
              label: this._localization.translate('modules.profile.jobPostings.publish'),
              icon: 'heroicons_outline:arrow-up-tray',
              iconClass: 'text-green-600',
              hidden: () => !canManage(),
              action: (x: ICellRendererParams<JobPostingListItemDto>) =>
                x.data && x.data.status === 'Draft'
                  ? this.actionClicked.emit({ type: 'publish', row: x.data })
                  : undefined,
            },
            {
              label: this._localization.translate('modules.profile.jobPostings.close'),
              icon: 'heroicons_outline:x-circle',
              iconClass: 'text-amber-600',
              hidden: () => !canManage(),
              action: (x: ICellRendererParams<JobPostingListItemDto>) =>
                x.data && x.data.status === 'Active'
                  ? this.actionClicked.emit({ type: 'close', row: x.data })
                  : undefined,
            },
            {
              label: this._localization.translate('modules.profile.jobPostings.reopen'),
              icon: 'heroicons_outline:arrow-path',
              iconClass: 'text-green-600',
              hidden: () => !canManage(),
              action: (x: ICellRendererParams<JobPostingListItemDto>) =>
                x.data && x.data.status === 'Closed'
                  ? this.actionClicked.emit({ type: 'reopen', row: x.data })
                  : undefined,
            },
            {
              label: this._localization.translate('modules.profile.jobApplications.view'),
              icon: 'heroicons_outline:clipboard-document-list',
              iconClass: 'text-teal-600',
              hidden: (x: ICellRendererParams<JobPostingListItemDto>) =>
                !canView() || !x.data || x.data.status !== 'Active',
              action: (x: ICellRendererParams<JobPostingListItemDto>) =>
                x.data ? this.actionClicked.emit({ type: 'applications', row: x.data }) : undefined,
            },
            {
              label: this._localization.translate('shared.delete'),
              icon: 'heroicons_outline:trash',
              iconClass: 'text-red-500',
              danger: true,
              hidden: () => !canDelete(),
              action: (x: ICellRendererParams<JobPostingListItemDto>) =>
                x.data && x.data.status === 'Draft'
                  ? this.actionClicked.emit({ type: 'delete', row: x.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }
}

