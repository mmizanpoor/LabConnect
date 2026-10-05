import { Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { UserLoginLogDto } from '@core/services/login-attempt/login-attempt.types';
import { PillStatusCellRenderer } from '@modules/base/components/base-grid/renderer/pill-status-cell/pill-status-cell.renderer';
import { ColDef, ICellRendererParams, ValueFormatterParams } from 'ag-grid-community';
import jMoment from 'moment-jalaali';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

@Injectable({ providedIn: 'root' })
export class LoginReportColDef {
  constructor(private _localization: LocalizationService) {}

  get(options?: { includeUser?: boolean }): ColDef[] {
    const includeUser = options?.includeUser === true;
    const cols: ColDef[] = [
      {
        headerName: this._localization.translate('modules.loginReport.columns.createdAt'),
        field: 'createdAt',
        minWidth: 160,
        width: 170,
        valueFormatter: (p: ValueFormatterParams<UserLoginLogDto>) =>
          this.formatJalaliDateTime(p.value),
      },
    ];

    if (includeUser) {
      cols.push({
        headerName: this._localization.translate('modules.loginReport.columns.user'),
        field: 'displayName',
        minWidth: 160,
        flex: 1,
        valueGetter: (p) => p.data?.displayName?.trim() || '—',
      });
    }

    cols.push(
      {
        headerName: this._localization.translate('modules.loginReport.columns.identity'),
        field: 'identity',
        minWidth: 140,
        width: 160,
        cellClass: 'dir-ltr',
        valueGetter: (p) =>
          p.data?.mobileNumber || p.data?.username || p.data?.displayName || '—',
      },
      {
        headerName: this._localization.translate('modules.loginReport.columns.method'),
        field: 'loginMethod',
        minWidth: 120,
        width: 130,
        valueFormatter: (p: ValueFormatterParams<UserLoginLogDto>) =>
          this.methodLabel(p.value),
      },
      {
        headerName: this._localization.translate('modules.loginReport.columns.status'),
        field: 'success',
        minWidth: 110,
        width: 120,
        headerClass: 'ag-header-center',
        cellClass: 'ag-header-center',
        cellRenderer: PillStatusCellRenderer,
        cellRendererParams: (p: ICellRendererParams<UserLoginLogDto>) => ({
          ...p,
          label: (x: ICellRendererParams<UserLoginLogDto>) =>
            this.statusLabel(!!x.data?.success),
          className: (x: ICellRendererParams<UserLoginLogDto>) =>
            x.data?.success
              ? 'bg-emerald-100 text-emerald-800'
              : 'bg-red-100 text-red-800',
        }),
      },
      {
        headerName: this._localization.translate('modules.loginReport.columns.ip'),
        field: 'ipAddress',
        minWidth: 130,
        width: 150,
        cellClass: 'dir-ltr font-mono text-xs',
        valueGetter: (p) => p.data?.ipAddress?.trim() || '—',
      },
      {
        headerName: this._localization.translate('modules.loginReport.columns.failureReason'),
        field: 'failureReason',
        minWidth: 180,
        flex: 1,
        valueGetter: (p) => p.data?.failureReason?.trim() || '—',
      },
    );

    return cols;
  }

  private methodLabel(method: string | number | null | undefined): string {
    if (method === 'Mobile' || method === 0) {
      return this._localization.translate('modules.loginReport.methods.Mobile');
    }
    return this._localization.translate('modules.loginReport.methods.Username');
  }

  private statusLabel(success: boolean): string {
    return this._localization.translate(
      success
        ? 'modules.loginReport.status.success'
        : 'modules.loginReport.status.failed',
    );
  }

  private formatJalaliDateTime(value: string | null | undefined): string {
    if (!value?.trim()) return '—';
    const parsed = jMoment(value).locale('fa');
    if (!parsed.isValid()) return value.trim();
    return `\u2066${parsed.format('jYYYY/jMM/jDD HH:mm')}\u2069`;
  }
}
