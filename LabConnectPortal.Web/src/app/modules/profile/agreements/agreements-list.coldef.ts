import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { LabAgreementDto } from './agreements.types';

export type AgreementListAction =
  | { type: 'detail'; row: LabAgreementDto }
  | { type: 'addendums'; row: LabAgreementDto }
  | { type: 'attachments'; row: LabAgreementDto }
  | { type: 'tests'; row: LabAgreementDto };

@Injectable({ providedIn: 'root' })
export class AgreementsListColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<AgreementListAction>();

  get(
    formatJalaliDate: (value?: string | null) => string,
    labName: (labCodeNew: number) => string
  ): ColDef[] {
    return [
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.contractNumber'
        ),
        minWidth: 120,
        width: 140,
        flex: 0,
        valueGetter: (p) => p.data?.contractNumber || '—',
        hide: true,
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.title'
        ),
        field: 'title',
        minWidth: 180,
        flex: 1,
        valueGetter: (p) => p.data?.title || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.primaryLab'
        ),
        minWidth: 180,
        flex: 1,
        valueGetter: (p) => labName(p.data?.primaryAgreementLabCodeNew ?? 0),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.receiverLab'
        ),
        minWidth: 180,
        flex: 1,
        valueGetter: (p) => labName(p.data?.receiverAgreementLabCodeNew ?? 0),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.startDate'
        ),
        width: 90,
        valueGetter: (p) => formatJalaliDate(p.data?.startDate),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.expDate'
        ),
        width: 90,
        valueGetter: (p) => formatJalaliDate(p.data?.expDate),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.childrenCount'
        ),
        minWidth: 140,
        valueGetter: (p) => p.data?.childrenCount ?? 0,
        hide: true,
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.attachmentCount'
        ),
        minWidth: 150,
        valueGetter: (p) => p.data?.attachmentCount ?? 0,
        hide: true,
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.actions'
        ),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        width: 100,
        minWidth: 100,
        maxWidth: 100,
        flex: 0,
        cellRenderer: MenuActionsCellRenderer,
        cellRendererParams: (p: ICellRendererParams<LabAgreementDto>) => ({
          ...p,
          triggerLabel: this._localization.translate(
            'modules.profile.agreements.columns.actions'
          ),
          actions: [
            {
              label: this._localization.translate(
                'modules.profile.agreements.viewDetail'
              ),
              icon: 'heroicons_outline:eye',
              action: (x: ICellRendererParams<LabAgreementDto>) =>
                x.data
                  ? this.actionClicked.emit({ type: 'detail', row: x.data })
                  : undefined,
            },
            {
              label: this._localization.translate(
                'modules.profile.agreements.viewAddendums'
              ),
              icon: 'heroicons_outline:book-open',
              hidden: (x: ICellRendererParams<LabAgreementDto>) =>
                !x.data?.id || !(x.data.childrenCount ?? 0),
              action: (x: ICellRendererParams<LabAgreementDto>) =>
                x.data
                  ? this.actionClicked.emit({ type: 'addendums', row: x.data })
                  : undefined,
            },
            {
              label: this._localization.translate(
                'modules.profile.agreements.viewAttachments'
              ),
              icon: 'heroicons_outline:paper-clip',
              hidden: (x: ICellRendererParams<LabAgreementDto>) =>
                !x.data?.id || !(x.data.attachmentCount ?? 0),
              action: (x: ICellRendererParams<LabAgreementDto>) =>
                x.data
                  ? this.actionClicked.emit({
                      type: 'attachments',
                      row: x.data,
                    })
                  : undefined,
            },
            {
              label: this._localization.translate(
                'modules.profile.agreements.viewTests'
              ),
              icon: 'heroicons_outline:beaker',
              action: (x: ICellRendererParams<LabAgreementDto>) =>
                x.data
                  ? this.actionClicked.emit({ type: 'tests', row: x.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }
}
