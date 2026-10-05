import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { DegreeLevel, getEnumName } from '../resume/resume.types';
import { ProductResumeApplicationForOwnerDto } from '../../products/products.types';
import { resumeApplicationStatusKey } from './product-resume-application-status';

export type ProductResumeApplicationAction = {
  type: 'view';
  row: ProductResumeApplicationForOwnerDto;
};

@Injectable({ providedIn: 'root' })
export class ProductResumeApplicationsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<ProductResumeApplicationAction>();

  get(): ColDef[] {
    return [
      {
        headerName: this._localization.translate(
          'modules.profile.productResumeApplications.columns.applicant',
        ),
        minWidth: 220,
        flex: 1.2,
        cellRenderer: (p: ICellRendererParams<ProductResumeApplicationForOwnerDto>) => {
          const row = p.data;
          if (!row) return '—';
          const name = `${row.applicantFirstName ?? ''} ${row.applicantLastName ?? ''}`.trim();
          const mobile = row.applicantMobileNumber ?? '';
          return `
            <div>
              <div class="font-medium text-slate-800">${name || '—'}</div>
              <div class="text-xs text-slate-500">${mobile}</div>
            </div>
          `;
        },
      },
      {
        headerName: this._localization.translate(
          'modules.profile.productResumeApplications.columns.jobTitle',
        ),
        field: 'jobTitle',
        minWidth: 180,
        flex: 1,
        valueGetter: (p) => p.data?.jobTitle?.trim() || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.productResumeApplications.columns.degreeLevel',
        ),
        minWidth: 160,
        flex: 0.8,
        valueGetter: (p) => this.degreeLabel(p.data?.degreeLevel),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.productResumeApplications.columns.status',
        ),
        minWidth: 130,
        valueGetter: (p) => this.statusLabel(p.data?.status),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.productResumeApplications.columns.submittedAt',
        ),
        minWidth: 170,
        valueGetter: (p) =>
          p.data?.submittedAt ? new Date(p.data.submittedAt).toLocaleString('fa-IR') : '—',
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
        cellRendererParams: (p: ICellRendererParams<ProductResumeApplicationForOwnerDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate('modules.profile.productResumeApplications.view'),
              icon: 'heroicons_outline:document-text',
              iconClass: 'text-teal-600',
              action: (x: ICellRendererParams<ProductResumeApplicationForOwnerDto>) =>
                x.data ? this.actionClicked.emit({ type: 'view', row: x.data }) : undefined,
            },
          ],
        }),
      },
    ];
  }

  private statusLabel(status: ProductResumeApplicationForOwnerDto['status'] | undefined): string {
    const key = resumeApplicationStatusKey(status);
    return this._localization.translate(`modules.profile.productResumeApplications.status.${key}`);
  }

  private degreeLabel(value: DegreeLevel | string | number | null | undefined): string {
    const name = getEnumName(DegreeLevel, value);
    return name
      ? this._localization.translate(`modules.profile.resume.enums.degreeLevel.${name}`)
      : '—';
  }
}
