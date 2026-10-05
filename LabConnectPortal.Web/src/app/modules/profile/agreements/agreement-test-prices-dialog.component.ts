import { Component, Inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { LabAgreementTestPriceDto } from './agreements.types';

export interface AgreementTestPricesDialogData {
  addendumTitle?: string | null;
  testPrices: LabAgreementTestPriceDto[];
}

@Component({
  selector: 'app-agreement-test-prices-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    TranslocoPipe,
    BaseGridComponent,
  ],
  templateUrl: './agreement-test-prices-dialog.component.html',
})
export class AgreementTestPricesDialogComponent {
  readonly list$ = new BehaviorSubject<LabAgreementTestPriceDto[]>([]);
  readonly colDef: ColDef[];

  constructor(
    private _dialogRef: MatDialogRef<AgreementTestPricesDialogComponent>,
    private _localization: LocalizationService,
    @Inject(MAT_DIALOG_DATA) readonly data: AgreementTestPricesDialogData
  ) {
    this.list$.next(data.testPrices ?? []);
    this.colDef = this.buildColDef();
  }

  get showAddendumColumn(): boolean {
    return this.data.testPrices.some((row) => !!row.addendumTitle?.trim());
  }

  close(): void {
    this._dialogRef.close();
  }

  formatNumber(value?: number | null): string {
    if (value == null) return '—';
    return value.toLocaleString('fa-IR');
  }

  private buildColDef(): ColDef[] {
    const cols: ColDef[] = [
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.testPricesDialog.columns.testName'
        ),
        minWidth: 160,
        flex: 1,
        valueGetter: (p) => p.data?.testName?.trim() || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.testPricesDialog.columns.cpnCode'
        ),
        minWidth: 88,
        width: 96,
        maxWidth: 110,
        flex: 0,
        valueGetter: (p) => p.data?.cpnCode?.trim() || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.testPricesDialog.columns.nationalCode'
        ),
        minWidth: 88,
        width: 96,
        maxWidth: 110,
        flex: 0,
        valueGetter: (p) => p.data?.nationalCode?.trim() || '—',
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.testPricesDialog.columns.baseTariffApproved'
        ),
        minWidth: 100,
        width: 108,
        maxWidth: 120,
        flex: 0,
        valueGetter: (p) => this.formatNumber(p.data?.baseTariffApproved),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.testPricesDialog.columns.approved'
        ),
        minWidth: 100,
        width: 108,
        maxWidth: 120,
        flex: 0,
        valueGetter: (p) => this.formatNumber(p.data?.approved),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.testPricesDialog.columns.firstAdditions'
        ),
        minWidth: 110,
        width: 110,
        maxWidth: 110,
        flex: 0,
        valueGetter: (p) => this.formatNumber(p.data?.firstAdditions),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.testPricesDialog.columns.secondAdditions'
        ),
        minWidth: 110,
        width: 110,
        maxWidth: 110,
        flex: 0,
        valueGetter: (p) => this.formatNumber(p.data?.secondAdditions),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.testPricesDialog.columns.urgentAmount'
        ),
        minWidth: 110,
        width: 110,
        maxWidth: 110,
        flex: 0,
        valueGetter: (p) => this.formatNumber(p.data?.urgentAmount),
      },
    ];

    if (this.showAddendumColumn) {
      cols.push({
        headerName: this._localization.translate(
          'modules.profile.agreements.testPricesDialog.columns.addendumTitle'
        ),
        minWidth: 120,
        flex: 1,
        valueGetter: (p) => p.data?.addendumTitle?.trim() || '—',
      });
    }

    return cols;
  }
}
