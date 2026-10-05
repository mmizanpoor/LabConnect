import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { ProductResumeApplicationDto } from '../../products/products.types';
import { resumeApplicationStatusKey } from '../product-resume-applications/product-resume-application-status';

export type MyResumeApplicationAction = {
  type: 'status';
  row: ProductResumeApplicationDto;
};

@Injectable({ providedIn: 'root' })
export class MyResumeApplicationsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<MyResumeApplicationAction>();

  get(): ColDef[] {
    return [
      {
        headerName: this._localization.translate(
          'modules.profile.myResumeApplications.columns.listing',
        ),
        field: 'productTitle',
        minWidth: 240,
        flex: 1.4,
        valueGetter: (p) => p.data?.productTitle?.trim() || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.myResumeApplications.columns.submittedAt',
        ),
        minWidth: 170,
        valueGetter: (p) =>
          p.data?.submittedAt ? new Date(p.data.submittedAt).toLocaleString('fa-IR') : '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.myResumeApplications.columns.status',
        ),
        minWidth: 140,
        valueGetter: (p) => this.statusLabel(p.data?.status),
      },
      {
        headerName: this._localization.translate('shared.actions'),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 180,
        minWidth: 180,
        maxWidth: 180,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams<ProductResumeApplicationDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate(
                'modules.profile.myResumeApplications.viewStatus',
              ),
              icon: 'heroicons_outline:eye',
              iconClass: 'text-teal-600',
              action: (x: ICellRendererParams<ProductResumeApplicationDto>) =>
                x.data ? this.actionClicked.emit({ type: 'status', row: x.data }) : undefined,
            },
          ],
        }),
      },
    ];
  }

  private statusLabel(status: ProductResumeApplicationDto['status'] | undefined): string {
    const key = resumeApplicationStatusKey(status);
    return this._localization.translate(`modules.profile.productResumeApplications.status.${key}`);
  }
}
