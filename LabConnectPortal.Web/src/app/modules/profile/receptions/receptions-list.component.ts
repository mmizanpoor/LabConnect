import { AfterViewInit, Component, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { firstValueFrom, BehaviorSubject } from 'rxjs';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseConfirmDialogComponent } from '@modules/base/components/base-dialog/base-confirm-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { ReceptionTestsDialogComponent } from './reception-tests-dialog.component';
import { ReceptionsService } from './receptions.service';
import { ClearReceiverReceptionItem, ReceptionDto } from './receptions.types';
import { ColDef } from 'ag-grid-community';
import { ReceptionsColDef, ReceptionAction } from './receptions.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

@Component({
  selector: 'app-receptions-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    BaseCheckboxComponent,
    MatSidenavModule,
    MatDialogModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseGridComponent,
  ],
  templateUrl: './receptions-list.component.html',
  styleUrl: './receptions-list.component.scss',
})
export class ReceptionsListComponent implements OnInit, AfterViewInit {
  readonly entity = SystemEntity.Reception;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Reception);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  private _dialog = inject(MatDialog);

  loading = false;
  clearing = false;
  error = '';
  validationMessage = '';
  rows: ReceptionDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ReceptionDto[]>([]);

  ownLabFilter = new FormControl('', { nonNullable: true });
  partnerLabFilter = new FormControl('', { nonNullable: true });
  isRejectFilter = new FormControl<boolean>(false, { nonNullable: true });
  isReceptionFilter = new FormControl<boolean>(false, { nonNullable: true });
  fromDateFilter = new FormControl<Moment | null>(null);
  toDateFilter = new FormControl<Moment | null>(null);

  constructor(
    private _authUtils: AuthUtils,
    private _receptionsService: ReceptionsService,
    private _localization: LocalizationService,
    private _colDef: ReceptionsColDef,
  ) {}

  get canView(): boolean {
    return this._authUtils.isAdministrator();
  }

  get clearableItems(): ClearReceiverReceptionItem[] {
    return this.rows.flatMap((row) => this.clearableItemsForRow(row));
  }

  get canClearReceiverReceptions(): boolean {
    return this.canView && !this.loading && !this.clearing && this.clearableItems.length > 0;
  }

  clearableItemsForRow(row: ReceptionDto): ClearReceiverReceptionItem[] {
    const items: ClearReceiverReceptionItem[] = [];
    for (const test of row.receptTests ?? []) {
      if (test.result?.trim()) continue;
      if (!test.id || !row.sourceLabId || !row.targetLabId || !row.sourceReceptId?.trim()) {
        continue;
      }
      items.push({
        id: test.id,
        sourceLabId: row.sourceLabId,
        sourceReceptId: row.sourceReceptId.trim(),
        targetLabId: row.targetLabId,
      });
    }
    return items;
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get(
      (row) => row.sourceReceptId?.trim() || '—',
      (row) => row.targetReceptId?.trim() || '—',
      (row) => this.patientName(row),
      (value) => this.formatReceptionDate(value),
      (row) => this.testCount(row),
      (isUrgent) =>
        isUrgent
          ? this._localization.translate('shared.yes')
          : this._localization.translate('shared.no'),
      (row) => this.resultStatusLabel(row),
    );

    this._colDef.actionClicked.subscribe((evt: ReceptionAction) => {
      if (!evt?.row) return;
      if (evt.type === 'viewTests') this.openTestsDialog(evt.row);
      if (evt.type === 'clearReceiver') void this.clearReceiverReceptions(evt.row);
    });

    this.setDefaultDates();
  }

  ngAfterViewInit(): void {
    queueMicrotask(() => this.filterDrawer?.open());
  }

  async applyFilters(): Promise<void> {
    await this.search();
    if (!this.validationMessage) {
      void this.filterDrawer?.close();
    }
  }

  async search(): Promise<void> {
    this.error = '';
    this.validationMessage = '';

    const ownLabInput = this.parseLabCodeInput(this.ownLabFilter.value);
    if (ownLabInput == null) {
      this.validationMessage = this._localization.translate(
        'modules.profile.receptions.errors.ownLabRequired',
      );
      this.ownLabFilter.markAsTouched();
      this.clearRows();
      return;
    }

    const partnerLabInput = this.parseLabCodeInput(this.partnerLabFilter.value);
    if (partnerLabInput == null) {
      this.validationMessage = this._localization.translate(
        'modules.profile.receptions.errors.labRequired',
      );
      this.partnerLabFilter.markAsTouched();
      this.clearRows();
      return;
    }

    const fromDate = this.toIsoDate(this.fromDateFilter.value, false);
    const toDate = this.toIsoDate(this.toDateFilter.value, true);
    if (!fromDate || !toDate) {
      this.validationMessage = this._localization.translate(
        'modules.profile.receptions.errors.dateRequired',
      );
      this.clearRows();
      return;
    }

    this.loading = true;
    try {
      this.rows = await this._receptionsService.getReceptions({
        labCode: ownLabInput,
        sourceLabCodes: [partnerLabInput],
        fromDate,
        toDate,
        isReception: this.isReceptionFilter.value ? true : null,
        isReject: this.isRejectFilter.value,
      });
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.clearRows();
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.receptions.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  async clearReceiverReceptions(row?: ReceptionDto): Promise<void> {
    const items = row ? this.clearableItemsForRow(row) : this.clearableItems;
    if (!this.canView || this.loading || this.clearing || items.length === 0) return;

    const confirmed = await this.confirmClearReceiver();
    if (!confirmed) return;

    this.clearing = true;
    this.error = '';
    try {
      await this._receptionsService.clearReceiverReceptions({ items });
      await this.search();
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.receptions.errors.clearReceiverFailed');
    } finally {
      this.clearing = false;
    }
  }

  patientName(row: ReceptionDto): string {
    return [row.firstName, row.lastName].filter(Boolean).join(' ').trim() || '—';
  }

  testCount(row: ReceptionDto): number {
    return row.receptTests?.length ?? 0;
  }

  resultStatusLabel(row: ReceptionDto): string {
    const hasResult = (row.receptTests ?? []).some((test) => !!test.result?.trim());
    return hasResult
      ? this._localization.translate('modules.profile.receptions.columns.hasResult')
      : '—';
  }

  formatReceptionDate(value?: string | null): string {
    if (!value?.trim()) return '—';
    const trimmed = value.trim();
    if (trimmed.length <= 10) return trimmed;
    const date = trimmed.slice(0, 10);
    const time = trimmed.slice(10);
    return `${time} - ${date}`;
  }

  openTestsDialog(row: ReceptionDto): void {
    if (!this.canView) return;
    this._dialog.open(ReceptionTestsDialogComponent, {
      width: '960px',
      maxWidth: '95vw',
      maxHeight: '92vh',
      autoFocus: false,
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        patientName: this.patientName(row),
        sourceReceptId: row.sourceReceptId,
        receptionDate: this.formatReceptionDate(row.sourceSendReceptDate),
        tests: row.receptTests ?? [],
      },
    });
  }

  private async confirmClearReceiver(): Promise<boolean> {
    const ref = this._dialog.open(BaseConfirmDialogComponent, {
      width: '440px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        title: this._localization.translate(
          'modules.profile.receptions.clearReceiver.title',
        ),
        message: this._localization.translate(
          'modules.profile.receptions.clearReceiver.confirm',
        ),
        confirmLabel: this._localization.translate('shared.confirm'),
        warnConfirm: true,
      },
    });
    return (await firstValueFrom(ref.afterClosed())) === true;
  }

  private clearRows(): void {
    this.rows = [];
    this.list$.next([]);
  }

  private parseLabCodeInput(value: string): number | null {
    const digits = value
      .trim()
      .replace(/[۰-۹]/g, (digit) => String.fromCharCode(digit.charCodeAt(0) - 1728))
      .replace(/\D/g, '');
    if (digits.length !== 4 && digits.length !== 5) {
      return null;
    }
    const code = Number(digits);
    return Number.isFinite(code) && code > 0 ? code : null;
  }

  private setDefaultDates(): void {
    const today = jMoment().locale('fa');
    this.fromDateFilter.setValue(today.clone().startOf('jMonth'));
    this.toDateFilter.setValue(today.clone());
  }

  private toIsoDate(value: Moment | null, endOfDay: boolean): string | null {
    if (!value) return null;
    const date = value.clone().locale('fa');
    if (endOfDay) {
      date.hours(23).minutes(59).seconds(59).milliseconds(999);
    } else {
      date.hours(0).minutes(0).seconds(0).milliseconds(0);
    }
    return date.toISOString();
  }
}
