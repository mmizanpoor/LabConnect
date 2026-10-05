import { Injectable } from '@angular/core';
import { LocalizationService } from '@core/services/localization/localization.service';
import { CellClassParams, ColDef } from 'ag-grid-community';
import { ReceptTestDto } from './receptions.types';

@Injectable({ providedIn: 'root' })
export class ReceptionTestsColDef {
  constructor(private _localization: LocalizationService) {}

  get(): ColDef[] {
    return [
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.testsDialog.columns.cpn',
        ),
        minWidth: 140,
        valueGetter: (p) => p.data?.sourceCPN?.trim() || '—',
        headerClass: 'ag-header-center',
        cellClass: 'ag-cell-center justify-center',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.testsDialog.columns.testName',
        ),
        minWidth: 180,
        flex: 1,
        valueGetter: (p) => p.data?.sourceTestName?.trim() || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.testsDialog.columns.result',
        ),
        minWidth: 140,
        valueGetter: (p) => p.data?.result?.trim() || '—',
        headerClass: 'ag-header-center',
        cellClass: 'ag-cell-center justify-center',
        cellClassRules: {
          'font-semibold': (params: CellClassParams<ReceptTestDto>) =>
            !!params.data?.result?.trim(),
          'text-green-600': (params: CellClassParams<ReceptTestDto>) =>
            !!params.data?.result?.trim(),
        },
      },
      {
        headerName: this._localization.translate(
          'modules.profile.receptions.testsDialog.columns.normalRange',
        ),
        minWidth: 180,
        flex: 1,
        valueGetter: (p) => this.formatNormalRange(p.data),
        headerClass: 'ag-header-center',
        cellClass: 'ag-cell-center justify-center',
      },
    ];
  }

  private formatNormalRange(test?: ReceptTestDto | null): string {
    const range = test?.rangeDetail;
    if (!range) return '—';

    if (range.normalText?.trim()) return range.normalText.trim();

    const hasMin = range.minNormalValue != null && range.minNormalValue !== 0;
    const hasMax = range.maxNormalValue != null && range.maxNormalValue !== 0;
    if (hasMin || hasMax) {
      const unit = range.unitDesc?.trim() ? ` ${range.unitDesc.trim()}` : '';
      return `${range.minNormalValue} - ${range.maxNormalValue}${unit}`;
    }

    if (range.borderLineText?.trim()) return range.borderLineText.trim();

    return '—';
  }
}
