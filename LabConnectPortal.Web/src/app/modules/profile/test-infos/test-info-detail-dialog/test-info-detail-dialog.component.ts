import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { TestInfosService } from '../test-infos.service';
import { TestInfoDetailDto } from '../test-infos.types';

export interface TestInfoDetailDialogData {
  id: number;
}

@Component({
  selector: 'app-test-info-detail-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, TranslocoPipe],
  templateUrl: './test-info-detail-dialog.component.html',
})
export class TestInfoDetailDialogComponent implements OnInit {
  loading = false;
  error = '';
  testInfo: TestInfoDetailDto | null = null;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: TestInfoDetailDialogData,
    private _dialogRef: MatDialogRef<TestInfoDetailDialogComponent>,
    private _service: TestInfosService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getById(this.data.id);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.profile.testInfos.errors.loadFailed'),
        );
      }
      this.testInfo = result.data;
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.testInfos.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  close(): void {
    this._dialogRef.close();
  }

  display(value: string | number | null | undefined): string {
    if (value === null || value === undefined || value === '') return '—';
    return String(value);
  }

  formatPrice(value: number | null | undefined): string {
    if (value === null || value === undefined) return '—';
    return `${value.toLocaleString('fa-IR')} ${this._localization.translate('shared.currency')}`;
  }
}
