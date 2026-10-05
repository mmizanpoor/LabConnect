import { Component, Inject, OnInit, inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { BehaviorSubject } from 'rxjs';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { MenuActionsCellRenderer } from '@modules/base/components/base-grid/renderer/menu-actions-cell/menu-actions-cell.renderer';
import { AgreementDetailDialogComponent } from './agreement-detail-dialog.component';
import { AgreementTestPricesDialogComponent } from './agreement-test-prices-dialog.component';
import { AgreementAttachmentsDialogComponent } from './agreement-attachments-dialog.component';
import { AgreementsService } from './agreements.service';
import { LabAgreementDto } from './agreements.types';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

export interface AgreementAddendumsDialogData {
  agreementId: number;
  parentTitle?: string | null;
  labNameMap?: Record<number, string>;
}

@Component({
  selector: 'app-agreement-addendums-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    TranslocoPipe,
    BaseGridComponent,
  ],
  templateUrl: './agreement-addendums-dialog.component.html',
})
export class AgreementAddendumsDialogComponent implements OnInit {
  private _dialog = inject(MatDialog);
  private _agreementsService = inject(AgreementsService);
  private _localization = inject(LocalizationService);

  loading = true;
  error = '';
  rows: LabAgreementDto[] = [];
  labNameMap: Record<number, string> = {};
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<LabAgreementDto[]>([]);

  constructor(
    private _dialogRef: MatDialogRef<AgreementAddendumsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) readonly data: AgreementAddendumsDialogData
  ) {
    this.labNameMap = { ...(data.labNameMap ?? {}) };
    this.colDef = this.buildColDef();
  }

  ngOnInit(): void {
    void this.loadAddendums();
  }

  async loadAddendums(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const agreement = await this._agreementsService.getLabAgreementById(
        this.data.agreementId
      );
      this.rows = agreement.children ?? [];
      this.labNameMap = await this._agreementsService.loadLabNamesForAgreements(
        this.rows,
        this.labNameMap
      );
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.rows = [];
      this.list$.next([]);
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.agreements.errors.loadAddendumsFailed'
            );
    } finally {
      this.loading = false;
    }
  }

  close(): void {
    this._dialogRef.close();
  }

  labName(labCodeNew: number): string {
    return this._agreementsService.resolveLabName(this.labNameMap, labCodeNew);
  }

  formatJalaliDate(value?: string | null): string {
    if (!value?.trim()) return '—';
    const parsed = jMoment(value);
    if (!parsed.isValid()) return value.trim();
    return parsed.locale('fa').format('jYYYY/jMM/jDD');
  }

  hasAttachments(row: LabAgreementDto): boolean {
    return (row.attachmentCount ?? 0) > 0 || !!row.attachments?.length;
  }

  openDetailDialog(row: LabAgreementDto): void {
    if (!row.id) return;

    this._dialog.open(AgreementDetailDialogComponent, {
      width: '920px',
      maxWidth: '95vw',
      maxHeight: '92vh',
      autoFocus: false,
      panelClass: 'agreement-detail-dialog-panel',
      data: {
        agreementId: row.id,
        labNameMap: this.labNameMap,
        dialogTitleKey:
          'modules.profile.agreements.addendumsDialog.detailTitle',
      },
    });
  }

  async openTestPricesDialog(row: LabAgreementDto): Promise<void> {
    if (!row.id) return;

    let testPrices = row.testPrices ?? [];
    if (!testPrices.length) {
      try {
        const detail = await this._agreementsService.getLabAgreementById(
          row.id
        );
        testPrices = detail.testPrices ?? [];
      } catch (e: unknown) {
        this.error =
          e instanceof Error
            ? e.message
            : this._localization.translate(
                'modules.profile.agreements.errors.loadTestPricesFailed'
              );
        return;
      }
    }

    this._dialog.open(AgreementTestPricesDialogComponent, {
      width: 'min(95vw, 72rem)',
      maxWidth: '95vw',
      maxHeight: '92vh',
      autoFocus: false,
      panelClass: 'agreement-test-prices-dialog-panel',
      data: {
        addendumTitle: row.title,
        testPrices,
      },
    });
  }

  openAttachmentsDialog(row: LabAgreementDto): void {
    if (!row.id) return;

    this._dialog.open(AgreementAttachmentsDialogComponent, {
      width: 'min(95vw, 72rem)',
      maxWidth: '95vw',
      maxHeight: '92vh',
      autoFocus: false,
      panelClass: 'agreement-attachments-dialog-panel',
      data: {
        agreementId: row.id,
        agreementTitle: row.title,
      },
    });
  }

  private buildColDef(): ColDef[] {
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
        valueGetter: (p) =>
          this.labName(p.data?.primaryAgreementLabCodeNew ?? 0),
        hide: true,
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.receiverLab'
        ),
        minWidth: 180,
        flex: 1,
        valueGetter: (p) =>
          this.labName(p.data?.receiverAgreementLabCodeNew ?? 0),
        hide: true,
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.startDate'
        ),
        minWidth: 100,
        width: 100,
        valueGetter: (p) => this.formatJalaliDate(p.data?.startDate),
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.expDate'
        ),
        minWidth: 130,
        valueGetter: (p) => this.formatJalaliDate(p.data?.expDate),
        hide: true,
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.testPricesCount'
        ),
        headerClass: 'ag-header-center base-grid-header-cell',
        cellClass: 'base-grid-cell-center',
        cellStyle: {
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          textAlign: 'center',
        },
        minWidth: 120,
        width: 120,
        valueGetter: (p) => p.data?.testPrices?.length ?? 0,
      },
      {
        headerName: this._localization.translate(
          'modules.profile.agreements.columns.actions'
        ),
        suppressHeaderMenuButton: true,
        sortable: false,
        headerClass: 'ag-header-center base-grid-header-cell',
        cellClass: 'ag-header-center',
        width: 160,
        minWidth: 160,
        maxWidth: 160,
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
                x.data ? this.openDetailDialog(x.data) : undefined,
            },
            {
              label: this._localization.translate(
                'modules.profile.agreements.viewAttachments'
              ),
              icon: 'heroicons_outline:paper-clip',
              hidden: (x: ICellRendererParams<LabAgreementDto>) =>
                !x.data?.id || !this.hasAttachments(x.data),
              action: (x: ICellRendererParams<LabAgreementDto>) =>
                x.data ? this.openAttachmentsDialog(x.data) : undefined,
            },
            {
              label: this._localization.translate(
                'modules.profile.agreements.viewTests'
              ),
              icon: 'heroicons_outline:beaker',
              action: (x: ICellRendererParams<LabAgreementDto>) =>
                x.data ? void this.openTestPricesDialog(x.data) : undefined,
            },
          ],
        }),
      },
    ];
  }
}
