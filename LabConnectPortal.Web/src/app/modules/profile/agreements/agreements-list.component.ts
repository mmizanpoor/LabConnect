import { AfterViewInit, Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { AuthService } from '@core/services/auth/auth.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { AgreementDetailDialogComponent } from './agreement-detail-dialog.component';
import { AgreementAddendumsDialogComponent } from './agreement-addendums-dialog.component';
import { AgreementTestPricesDialogComponent } from './agreement-test-prices-dialog.component';
import { AgreementAttachmentsDialogComponent } from './agreement-attachments-dialog.component';
import { AgreementsService } from './agreements.service';
import { AgreementDirection, LabAgreementDto } from './agreements.types';
import { AgreementsListColDef, AgreementListAction } from './agreements-list.coldef';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

@Component({
  selector: 'app-agreements-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    MatRadioModule,
    MatSidenavModule,
    MatDialogModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseGridComponent,
  ],
  templateUrl: './agreements-list.component.html',
  styleUrl: './agreements-list.component.scss',
})
export class AgreementsListComponent implements OnInit, AfterViewInit {
  readonly entity = SystemEntity.LabAgreement;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.LabAgreement);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  private _dialog = inject(MatDialog);
  private _router = inject(Router);
  private _route = inject(ActivatedRoute);
  private _destroyRef = inject(DestroyRef);

  loading = false;
  error = '';
  validationMessage = '';
  userLabCodeNew = 0;
  rows: LabAgreementDto[] = [];
  labNameMap: Record<number, string> = {};
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<LabAgreementDto[]>([]);

  readonly AgreementDirection = AgreementDirection;
  agreementDirectionFilter = new FormControl<AgreementDirection>(AgreementDirection.Received, {
    nonNullable: true,
  });
  startDateFilter = new FormControl<Moment | null>(null);

  constructor(
    private _authService: AuthService,
    private _agreementsService: AgreementsService,
    private _localization: LocalizationService,
    private _colDef: AgreementsListColDef,
    private _labPermission: LabPermissionService,
  ) {}

  get canCreate(): boolean {
    return this._labPermission.can(this.entity, 'create');
  }

  ngOnInit(): void {
    this.setDefaultStartDate();
    this.colDef = this._colDef.get(
      (v) => this.formatJalaliDate(v),
      (c) => this.labName(c),
    );
    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt: AgreementListAction) => {
        if (!evt?.row) return;
        if (evt.type === 'detail') this.openDetailDialog(evt.row);
        if (evt.type === 'addendums') this.openAddendumsDialog(evt.row);
        if (evt.type === 'attachments') this.openAttachmentsDialog(evt.row);
        if (evt.type === 'tests') void this.openTestPricesDialog(evt.row);
      });
    void this.initPage();
  }

  ngAfterViewInit(): void {
    queueMicrotask(() => this.filterDrawer?.open());
  }

  goToNewAgreement(): void {
    void this._router.navigate(['/profile/agreements/new']);
  }

  async initPage(): Promise<void> {
    this.error = '';
    try {
      const profile = await this._authService.getProfile();
      this.userLabCodeNew = profile.labCodeNew ?? profile.labCode ?? 0;
      if (!this.userLabCodeNew) {
        throw new Error(this._localization.translate('modules.profile.agreements.errors.noLabCode'));
      }

      // Deeplink to a specific agreement still loads data immediately.
      if (this._route.snapshot.queryParamMap.get('agreementId')) {
        await this.search();
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreements.errors.loadFailed');
    }
  }

  async applyFilters(): Promise<void> {
    await this.search();
    if (!this.validationMessage) {
      void this.filterDrawer?.close();
    }
  }

  async search(): Promise<void> {
    if (!this.userLabCodeNew) return;

    this.error = '';
    this.validationMessage = '';

    const startDateTime = this.toIsoDate(this.startDateFilter.value, false);
    if (!startDateTime) {
      this.validationMessage = this._localization.translate(
        'modules.profile.agreements.errors.dateRequired',
      );
      this.rows = [];
      this.list$.next([]);
      return;
    }

    this.loading = true;
    try {
      const agreements = await this._agreementsService.getLabAgreements({
        agreementDirection: this.agreementDirectionFilter.value,
        primaryLabCodeNew: this.userLabCodeNew,
        startDateTime,
      });
      try {
        this.labNameMap = await this._agreementsService.loadLabNamesForAgreements(agreements);
      } catch {
        this.labNameMap = {};
      }
      this.rows = agreements;
      this.list$.next(this.rows);
      await this.openDetailFromQueryParamIfNeeded();
    } catch (e: unknown) {
      this.rows = [];
      this.labNameMap = {};
      this.list$.next([]);
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreements.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  formatJalaliDate(value?: string | null): string {
    if (!value?.trim()) return '—';
    const parsed = jMoment(value);
    if (!parsed.isValid()) return value.trim();
    return parsed.locale('fa').format('jYYYY/jMM/jDD');
  }

  labName(labCodeNew: number): string {
    return this._agreementsService.resolveLabName(this.labNameMap, labCodeNew);
  }

  openDetailDialog(row: LabAgreementDto): void {
    if (!row.id) return;

    this._dialog.open(AgreementDetailDialogComponent, {
      width: '920px',
      maxWidth: '95vw',
      maxHeight: '92vh',
      autoFocus: false,
      panelClass: 'agreement-detail-dialog-panel',
      data: { agreementId: row.id, labNameMap: this.labNameMap },
    });
  }

  private async openDetailFromQueryParamIfNeeded(): Promise<void> {
    const agreementIdParam = this._route.snapshot.queryParamMap.get('agreementId');
    if (!agreementIdParam) return;

    const agreementId = Number(agreementIdParam);
    if (!Number.isFinite(agreementId) || agreementId <= 0) return;

    const row = this.rows.find((item) => item.id === agreementId);
    if (row) {
      this.openDetailDialog(row);
      return;
    }

    this._dialog.open(AgreementDetailDialogComponent, {
      width: '920px',
      maxWidth: '95vw',
      maxHeight: '92vh',
      autoFocus: false,
      panelClass: 'agreement-detail-dialog-panel',
      data: { agreementId, labNameMap: this.labNameMap },
    });
  }

  openAddendumsDialog(row: LabAgreementDto): void {
    if (!row.id) return;

    this._dialog.open(AgreementAddendumsDialogComponent, {
      width: 'min(95vw, 72rem)',
      maxWidth: '95vw',
      maxHeight: '92vh',
      autoFocus: false,
      panelClass: 'agreement-addendums-dialog-panel',
      data: {
        agreementId: row.id,
        parentTitle: row.title,
        labNameMap: this.labNameMap,
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

  async openTestPricesDialog(row: LabAgreementDto): Promise<void> {
    if (!row.id) return;

    try {
      const detail = await this._agreementsService.getLabAgreementById(row.id);
      const testPrices = detail.mergedTestPrices ?? detail.testPrices ?? [];

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
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreements.errors.loadTestPricesFailed');
    }
  }

  private setDefaultStartDate(): void {
    this.startDateFilter.setValue(jMoment().locale('fa').startOf('jMonth'));
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
