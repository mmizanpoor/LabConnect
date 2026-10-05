import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ActivityLogService } from '@core/services/activity-log/activity-log.service';
import {
  ActivityLogRecordGroupDto,
  ActivityLogUserOptionDto,
} from '@core/services/activity-log/activity-log.types';
import { ACTIVITY_LOG_PROFILE_ENTITIES } from '@core/services/activity-log/activity-log-entities';
import { formatActivityLogDisplayValue } from '@core/services/activity-log/activity-log-value.util';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

@Component({
  selector: 'app-profile-activity-logs',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
  ],
  templateUrl: './activity-logs.component.html',
  styleUrl: './activity-logs.component.scss',
})
export class ProfileActivityLogsComponent implements OnInit {
  readonly entity = SystemEntity.ActivityLog;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ActivityLog);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  groups: ActivityLogRecordGroupDto[] = [];
  centerUsers: ActivityLogUserOptionDto[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 12;
  loading = false;
  hasSearched = false;

  entityNameControl = new FormControl('');
  actionControl = new FormControl('');
  userIdControl = new FormControl('');
  fromDateControl = new FormControl<Moment | null>(null);
  toDateControl = new FormControl<Moment | null>(null);

  constructor(
    private _activityLog: ActivityLogService,
    private _localization: LocalizationService,
  ) {
    const today = jMoment().locale('fa');
    this.fromDateControl.setValue(today.clone().startOf('jMonth'));
    this.toDateControl.setValue(today.clone());
  }

  ngOnInit(): void {
    void this.loadCenterUsers();
  }

  get entityOptions(): BaseFormSelectOption[] {
    return [
      { value: '', label: this._localization.translate('shared.all') },
      ...ACTIVITY_LOG_PROFILE_ENTITIES.map((value) => ({
        value,
        label: this._localization.translate(`modules.activityLog.entities.${value}`),
      })),
    ];
  }

  get actionOptions(): BaseFormSelectOption[] {
    return [
      { value: '', label: this._localization.translate('shared.all') },
      { value: 'Create', label: this._localization.translate('modules.activityLog.actions.Create') },
      { value: 'Update', label: this._localization.translate('modules.activityLog.actions.Update') },
      { value: 'Delete', label: this._localization.translate('modules.activityLog.actions.Delete') },
    ];
  }

  get userOptions(): BaseFormSelectOption[] {
    return [
      { value: '', label: this._localization.translate('shared.all') },
      ...this.centerUsers.map((user) => ({
        value: user.id,
        label: user.mobileNumber
          ? `${user.displayName} (${user.mobileNumber})`
          : user.displayName,
      })),
    ];
  }

  applyFilters(): void {
    this.page = 1;
    this.hasSearched = true;
    void this.filterDrawer?.close();
    void this.load();
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
  }

  actionLabel(action: string): string {
    const key = `modules.activityLog.actions.${action}`;
    const translated = this._localization.translate(key);
    return translated === key ? action : translated;
  }

  fieldLabel(field: string): string {
    const key = `modules.activityLog.fields.${field}`;
    const translated = this._localization.translate(key);
    return translated === key ? field : translated;
  }

  entityLabel(name: string): string {
    const key = `modules.activityLog.entities.${name}`;
    const translated = this._localization.translate(key);
    return translated === key ? name : translated;
  }

  displayValue(value: string | null): string {
    if (!this.hasValue(value)) {
      return '';
    }
    const formatted = formatActivityLogDisplayValue(value!);
    return formatted.length > 300 ? formatted.slice(0, 300) + '…' : formatted;
  }

  hasValue(value: string | null | undefined): boolean {
    return value != null && value !== '';
  }

  formatJalaliDateTime(value: string | null | undefined): string {
    if (!value?.trim()) return '-';
    const parsed = jMoment(value).locale('fa');
    if (!parsed.isValid()) return value.trim();
    // LTR isolate keeps date on the left and time on the right inside RTL layouts.
    return `\u2066${parsed.format('jYYYY/jMM/jDD HH:mm')}\u2069`;
  }

  private async loadCenterUsers(): Promise<void> {
    try {
      const result = await this._activityLog.getCenterUsers();
      if (result.success && result.data) {
        this.centerUsers = result.data;
      }
    } catch {
      this.centerUsers = [];
    }
  }

  async load(): Promise<void> {
    if (!this.hasSearched) return;

    this.loading = true;
    try {
      const result = await this._activityLog.getAll({
        entityName: this.entityNameControl.value || undefined,
        action: this.actionControl.value || undefined,
        userId: this.userIdControl.value || undefined,
        from: this.toIsoDate(this.fromDateControl.value, false) ?? undefined,
        to: this.toIsoDate(this.toDateControl.value, true) ?? undefined,
        page: this.page,
        pageSize: this.pageSize,
      });
      if (result.success && result.data) {
        this.groups = result.data.items;
        this.totalCount = result.data.totalCount;
      } else {
        this.groups = [];
        this.totalCount = 0;
      }
    } finally {
      this.loading = false;
    }
  }

  private toIsoDate(value: Moment | null, endOfDay: boolean): string | null {
    if (!value?.isValid()) return null;
    const date = value.clone().locale('fa');
    if (endOfDay) {
      date.hours(23).minutes(59).seconds(59).milliseconds(999);
    } else {
      date.hours(0).minutes(0).seconds(0).milliseconds(0);
    }
    return date.toISOString();
  }
}
