import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { LocalizationService } from '@core/services/localization/localization.service';
import { LoginAttemptService } from '@core/services/login-attempt/login-attempt.service';
import {
  GetLoginAttemptsQuery,
  LoginMethod,
  UserLoginLogDto,
} from '@core/services/login-attempt/login-attempt.types';
import { downloadTableAsExcel } from '@core/utils/excel-export.util';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { LoginReportColDef } from '@modules/shared/login-report/login-report.coldef';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

@Component({
  selector: 'app-profile-login-report',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './login-report.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }

    :host ::ng-deep .mat-drawer-container {
      height: 100%;
    }
  `,
})
export class ProfileLoginReportComponent implements OnInit {
  readonly entity = SystemEntity.Profile;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Profile);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<UserLoginLogDto[]>([]);
  totalCount = 0;
  page = 1;
  pageSize = 20;
  loading = false;
  exporting = false;
  hasSearched = false;
  error = '';

  fromDateControl = new FormControl<Moment | null>(null);
  toDateControl = new FormControl<Moment | null>(null);
  successControl = new FormControl<string>('');
  methodControl = new FormControl<string>('');

  constructor(
    private _loginAttempt: LoginAttemptService,
    private _localization: LocalizationService,
    private _colDef: LoginReportColDef,
  ) {}

  get successOptions(): BaseFormSelectOption[] {
    return [
      { value: '', label: this._localization.translate('shared.all') },
      {
        value: 'true',
        label: this._localization.translate('modules.loginReport.status.success'),
      },
      {
        value: 'false',
        label: this._localization.translate('modules.loginReport.status.failed'),
      },
    ];
  }

  get methodOptions(): BaseFormSelectOption[] {
    return [
      { value: '', label: this._localization.translate('shared.all') },
      {
        value: 'Mobile',
        label: this._localization.translate('modules.loginReport.methods.Mobile'),
      },
      {
        value: 'Username',
        label: this._localization.translate('modules.loginReport.methods.Username'),
      },
    ];
  }

  get noRowsMessage(): string {
    return this.hasSearched
      ? this._localization.translate('modules.loginReport.empty')
      : this._localization.translate('modules.loginReport.searchPrompt');
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get({ includeUser: false });
    const today = jMoment().locale('fa');
    this.fromDateControl.setValue(today.clone().startOf('jMonth'));
    this.toDateControl.setValue(today.clone());
  }

  applyFilters(): void {
    this.page = 1;
    this.hasSearched = true;
    void this.filterDrawer?.close();
    void this.load();
  }

  onPageChange(page: number): void {
    if (!this.hasSearched) return;
    this.page = page;
    void this.load();
  }

  async exportExcel(): Promise<void> {
    if (!this.hasSearched || this.exporting) return;
    this.exporting = true;
    this.error = '';
    try {
      const allRows: UserLoginLogDto[] = [];
      let page = 1;
      const pageSize = 100;
      let total = 0;

      do {
        const result = await this.fetchPage(page, pageSize);
        if (!result) {
          throw new Error(
            this._localization.translate('modules.loginReport.errors.loadFailed'),
          );
        }
        allRows.push(...result.items);
        total = result.totalCount;
        page += 1;
      } while (allRows.length < total);

      if (!allRows.length) {
        this.error = this._localization.translate('modules.loginReport.errors.exportEmpty');
        return;
      }

      const headers = [
        this._localization.translate('modules.loginReport.columns.createdAt'),
        this._localization.translate('modules.loginReport.columns.identity'),
        this._localization.translate('modules.loginReport.columns.method'),
        this._localization.translate('modules.loginReport.columns.status'),
        this._localization.translate('modules.loginReport.columns.ip'),
        this._localization.translate('modules.loginReport.columns.failureReason'),
      ];

      const excelRows = allRows.map((row) => [
        this.formatJalaliDateTime(row.createdAt),
        row.mobileNumber || row.username || '—',
        this.methodLabel(row.loginMethod),
        this.statusLabel(row.success),
        row.ipAddress ?? '—',
        row.failureReason ?? '',
      ]);

      downloadTableAsExcel(
        this._localization.translate('modules.loginReport.exportFileName'),
        headers,
        excelRows,
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.loginReport.errors.loadFailed');
    } finally {
      this.exporting = false;
    }
  }

  async load(): Promise<void> {
    if (!this.hasSearched) return;
    this.loading = true;
    this.error = '';
    try {
      const result = await this.fetchPage(this.page, this.pageSize);
      if (!result) {
        this.list$.next([]);
        this.totalCount = 0;
        this.error = this._localization.translate('modules.loginReport.errors.loadFailed');
        return;
      }
      this.list$.next(result.items);
      this.totalCount = result.totalCount;
    } catch {
      this.list$.next([]);
      this.totalCount = 0;
      this.error = this._localization.translate('modules.loginReport.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  private async fetchPage(page: number, pageSize: number) {
    const result = await this._loginAttempt.getMine(this.buildQuery(page, pageSize));
    if (result.success && result.data) return result.data;
    return null;
  }

  private buildQuery(page: number, pageSize: number): GetLoginAttemptsQuery {
    const successRaw = this.successControl.value;
    const methodRaw = this.methodControl.value;
    return {
      from: this.toIsoDate(this.fromDateControl.value, false) ?? undefined,
      to: this.toIsoDate(this.toDateControl.value, true) ?? undefined,
      success: successRaw === 'true' ? true : successRaw === 'false' ? false : undefined,
      loginMethod:
        methodRaw === 'Mobile' || methodRaw === 'Username'
          ? (methodRaw as LoginMethod)
          : undefined,
      page,
      pageSize,
    };
  }

  private toIsoDate(value: Moment | null, endOfDay: boolean): string | null {
    if (!value || !value.isValid()) return null;
    const m = value.clone().locale('en');
    if (endOfDay) m.endOf('day');
    else m.startOf('day');
    return m.toISOString();
  }

  private methodLabel(method: LoginMethod | number): string {
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
