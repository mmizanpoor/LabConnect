import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { JobApplicationDto, JobApplicationStatus } from '../../jobs/jobs.types';

export type JobApplicationAction =
  | { type: 'approve'; row: JobApplicationDto }
  | { type: 'reject'; row: JobApplicationDto };

@Injectable({ providedIn: 'root' })
export class JobApplicationsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<JobApplicationAction>();

  get(
    statusClass: (status: JobApplicationStatus) => string,
    statusLabel: (status: JobApplicationStatus) => string,
    canUpdate = true,
  ): ColDef[] {
    return [
      {
        headerName: this._localization.translate('modules.profile.jobApplications.columns.applicant'),
        minWidth: 220,
        flex: 1,
        valueGetter: (p) => {
          const r = p.data;
          if (!r) return '—';
          const name = `${r.applicantFirstName ?? ''} ${r.applicantLastName ?? ''}`.trim();
          return name || r.applicantMobileNumber || '—';
        },
        cellRenderer: (p: ICellRendererParams<JobApplicationDto>) => {
          const r = p.data;
          if (!r) return '—';
          const name = `${r.applicantFirstName ?? ''} ${r.applicantLastName ?? ''}`.trim();
          const mobile = r.applicantMobileNumber ?? '';
          return `
            <div>
              <div class="font-medium text-slate-800">${name || '—'}</div>
              <div class="text-xs text-slate-500">${mobile}</div>
            </div>
          `;
        },
      },
      {
        headerName: this._localization.translate('modules.profile.jobApplications.columns.submittedAt'),
        minWidth: 170,
        valueGetter: (p) =>
          p.data?.submittedAt ? new Date(p.data.submittedAt).toLocaleString('fa-IR') : '—',
      },
      {
        headerName: this._localization.translate('modules.profile.jobApplications.columns.status'),
        minWidth: 140,
        cellRenderer: (p: ICellRendererParams<JobApplicationDto>) => {
          const s = p.data?.status;
          if (!s) return '—';
          return `<span class="px-2 py-1 rounded text-xs font-medium ${statusClass(s)}">${statusLabel(s)}</span>`;
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
        cellRendererParams: (p: ICellRendererParams<JobApplicationDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('modules.profile.jobApplications.approve'),
              icon: 'heroicons_outline:check-circle',
              iconClass: 'text-green-600',
              hidden: (x: ICellRendererParams<JobApplicationDto>) =>
                !canUpdate || !x.data || x.data.status !== 'Pending',
              action: (x: ICellRendererParams<JobApplicationDto>) =>
                x.data ? this.actionClicked.emit({ type: 'approve', row: x.data }) : undefined,
            },
            {
              label: this._localization.translate('modules.profile.jobApplications.reject'),
              icon: 'heroicons_outline:x-circle',
              iconClass: 'text-red-500',
              danger: true,
              hidden: (x: ICellRendererParams<JobApplicationDto>) =>
                !canUpdate || !x.data || x.data.status !== 'Pending',
              action: (x: ICellRendererParams<JobApplicationDto>) =>
                x.data ? this.actionClicked.emit({ type: 'reject', row: x.data }) : undefined,
            },
          ],
        }),
      },
    ];
  }
}

