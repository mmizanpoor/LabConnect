import { Component, Inject, OnInit, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { AgreementsService } from './agreements.service';
import { LabAgreementDto } from './agreements.types';
import { sfdtToPlainText } from './sfdt-to-text.util';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

export interface AgreementDetailDialogData {
  agreementId: number;
  labNameMap?: Record<number, string>;
  dialogTitleKey?: string;
}

@Component({
  selector: 'app-agreement-detail-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, TranslocoPipe],
  templateUrl: './agreement-detail-dialog.component.html',
  styleUrl: './agreement-detail-dialog.component.scss',
})
export class AgreementDetailDialogComponent implements OnInit {
  private _agreementsService = inject(AgreementsService);
  private _localization = inject(LocalizationService);

  loading = true;
  error = '';
  agreement: LabAgreementDto | null = null;
  labNameMap: Record<number, string> = {};

  constructor(
    private _dialogRef: MatDialogRef<AgreementDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) readonly data: AgreementDetailDialogData,
  ) {
    this.labNameMap = { ...(data.labNameMap ?? {}) };
  }

  get dialogTitleKey(): string {
    return this.data.dialogTitleKey ?? 'modules.profile.agreements.detailDialog.title';
  }

  get testCount(): number {
    const tests = this.agreement?.mergedTestPrices ?? this.agreement?.testPrices;
    return tests?.length ?? 0;
  }

  ngOnInit(): void {
    void this.loadDetail();
  }

  async loadDetail(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      this.agreement = await this._agreementsService.getLabAgreementById(this.data.agreementId);
      this.labNameMap = await this._agreementsService.loadLabNamesForAgreements(
        [this.agreement],
        this.labNameMap,
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreements.errors.loadDetailFailed');
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

  countOrNone(count?: number | null): string {
    if (count && count > 0) return String(count);
    return this._localization.translate('modules.profile.agreements.detailDialog.none');
  }

  plainText(content?: string | null): string {
    return sfdtToPlainText(content);
  }

  hasSign(value?: string | null): boolean {
    return !!value?.trim();
  }

  signImageSrc(value?: string | null): string | null {
    const trimmed = value?.trim();
    if (!trimmed) return null;
    if (trimmed.startsWith('data:') || trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
      return trimmed;
    }
    if (trimmed.startsWith('/9j/')) {
      return `data:image/jpeg;base64,${trimmed}`;
    }
    return `data:image/png;base64,${trimmed}`;
  }

  hasContractTextSection(agreement: LabAgreementDto): boolean {
    return (
      !!this.plainText(agreement.text) ||
      this.hasSign(agreement.primaryAgreementSign) ||
      this.hasSign(agreement.receiverAgreementSign)
    );
  }
}
