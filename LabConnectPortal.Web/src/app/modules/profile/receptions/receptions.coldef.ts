import { EventEmitter, Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { CellClassParams, ColDef, ICellRendererParams } from 'ag-grid-community';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { ReceptionDto } from './receptions.types';

export type ReceptionAction =
  | { type: 'viewTests'; row: ReceptionDto }
  | { type: 'clearReceiver'; row: ReceptionDto };

@Injectable({ providedIn: 'root' })
export class ReceptionsColDef {
  constructor(private _localization: LocalizationService) {}

  actionClicked = new EventEmitter<ReceptionAction>();

  get(
    receptionNumber: (row: ReceptionDto) => string,
    ourReceptionNumber: (row: ReceptionDto) => string,
    patientName: (row: ReceptionDto) => string,
    formatReceptionDate: (value?: string | null) => string,
    testCount: (row: ReceptionDto) => number,
    urgentLabel: (isUrgent: boolean) => string,
    resultStatusLabel: (row: ReceptionDto) => string,
  ): ColDef[] {
    const hasClearableReceiverTests = (row: ReceptionDto): boolean =>
      (row.receptTests ?? []).some(
        (test) =>
          !test.result?.trim() &&
          !!test.id &&
          !!row.sourceLabId &&
          !!row.targetLabId &&
          !!row.sourceReceptId?.trim(),
      );

    return [
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.columns.sourceReceptId'
        ),
        minWidth: 150,
        valueGetter: (p) => (p.data ? receptionNumber(p.data) : '—'),
        headerClass: 'ag-header-center',
        cellClass: 'ag-cell-center justify-center',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.columns.ourReceptId'
        ),
        minWidth: 150,
        valueGetter: (p) => (p.data ? ourReceptionNumber(p.data) : '—'),
        headerClass: 'ag-header-center',
        cellClass: 'ag-cell-center justify-center',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.columns.patientName'
        ),
        minWidth: 200,
        flex: 1,
        valueGetter: (p) => (p.data ? patientName(p.data) : '—'),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.columns.sendDate'
        ),
        minWidth: 170,
        valueGetter: (p) => formatReceptionDate(p.data?.sourceSendReceptDate),
        headerClass: 'ag-header-center',
        cellClass: 'ag-cell-center justify-center',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.columns.testCount'
        ),
        minWidth: 120,
        valueGetter: (p) => (p.data ? testCount(p.data) : 0),
        headerClass: 'ag-header-center',
        cellClass: 'ag-cell-center justify-center',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.columns.urgent'
        ),
        minWidth: 120,
        valueGetter: (p) => urgentLabel(!!p.data?.isUrgent),
        headerClass: 'ag-header-center',
        cellClass: 'ag-cell-center justify-center',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.columns.resultStatus'
        ),
        minWidth: 130,
        valueGetter: (p) => (p.data ? resultStatusLabel(p.data) : '—'),
        headerClass: 'ag-header-center',
        cellClass: 'ag-cell-center justify-center',
        cellClassRules: {
          'font-semibold text-green-600': (params: CellClassParams<ReceptionDto>) =>
            !!params.data?.receptTests?.some((test) => !!test.result?.trim()),
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
        cellRendererParams: (p: ICellRendererParams<ReceptionDto>) => ({
          ...p,
          triggerLabel: this._localization.translate('shared.actions'),
          actions: [
            {
              label: this._localization.translate(
                'modules.profile.receptions.viewTests'
              ),
              icon: 'heroicons_outline:beaker',
              hidden: (x: ICellRendererParams<ReceptionDto>) =>
                !x.data || testCount(x.data) === 0,
              action: (x: ICellRendererParams<ReceptionDto>) =>
                x.data
                  ? this.actionClicked.emit({ type: 'viewTests', row: x.data })
                  : undefined,
            },
            {
              label: this._localization.translate(
                'modules.profile.receptions.clearReceiver.rowAction'
              ),
              icon: 'heroicons_outline:trash',
              danger: true,
              hidden: (x: ICellRendererParams<ReceptionDto>) =>
                !x.data || !hasClearableReceiverTests(x.data),
              action: (x: ICellRendererParams<ReceptionDto>) =>
                x.data
                  ? this.actionClicked.emit({ type: 'clearReceiver', row: x.data })
                  : undefined,
            },
          ],
        }),
      },
    ];
  }
}
