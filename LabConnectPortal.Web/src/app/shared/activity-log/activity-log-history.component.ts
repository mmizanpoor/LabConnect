import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { ActivityLogService } from '@core/services/activity-log/activity-log.service';
import { ActivityLogBatchDto } from '@core/services/activity-log/activity-log.types';
import { formatActivityLogDisplayValue } from '@core/services/activity-log/activity-log-value.util';
import { LocalizationService } from '@core/services/localization/localization.service';

@Component({
  selector: 'app-activity-log-history',
  standalone: true,
  imports: [TranslocoPipe],
  template: `
    <div class="activity-log-history" dir="rtl">
      @if (loading) {
        <p class="text-sm text-gray-500">{{ 'shared.loading' | transloco }}</p>
      } @else if (!batches.length) {
        <p class="text-sm text-gray-500">{{ 'modules.activityLog.empty' | transloco }}</p>
      } @else {
        <div class="flex flex-col gap-3">
          @for (batch of batches; track batch.batchId) {
            <article
              class="overflow-hidden rounded-[5px] border border-slate-200 bg-white p-4 shadow-[0_1px_3px_rgba(15,23,42,0.06)]"
            >
              <header class="mb-3 flex flex-wrap items-center gap-2 text-sm text-slate-600">
                <span class="font-semibold text-slate-800">{{
                  batch.userDisplayName || ('modules.activityLog.unknownUser' | transloco)
                }}</span>
                <span class="rounded bg-slate-100 px-2 py-0.5 text-xs">{{ actionLabel(batch.action) }}</span>
                @if (batch.recordTitle) {
                  <span class="rounded bg-amber-50 px-2 py-0.5 text-xs font-medium text-amber-900">{{
                    batch.recordTitle
                  }}</span>
                }
                <span class="text-xs text-slate-400">{{ formatJalaliDateTime(batch.createdAt) }}</span>
              </header>
              <ul class="m-0 flex list-none flex-col gap-2 p-0">
                @for (change of batch.changes; track change.id) {
                  <li class="flex flex-row flex-wrap items-center gap-2 rounded border border-slate-100 bg-slate-50 px-3 py-2 text-sm">
                    <div class="shrink-0 font-medium text-[#35507c]">{{ fieldLabel(change.fieldName) }}:</div>
                    <div class="flex min-w-0 flex-row flex-wrap items-center gap-2 text-slate-700">
                      @if (hasValue(change.oldValue)) {
                        <span class="max-w-full break-words text-red-700/90 line-through decoration-red-300">{{
                          displayValue(change.oldValue)
                        }}</span>
                        @if (hasValue(change.newValue)) {
                          <span class="text-slate-400">←</span>
                        }
                      }
                      @if (hasValue(change.newValue)) {
                        <span class="max-w-full break-words font-medium text-emerald-800">{{
                          displayValue(change.newValue)
                        }}</span>
                      }
                    </div>
                  </li>
                }
              </ul>
            </article>
          }
        </div>
      }
    </div>
  `,
})
export class ActivityLogHistoryComponent implements OnChanges {
  @Input({ required: true }) entityName!: string;
  @Input({ required: true }) recordKey!: string;

  loading = false;
  batches: ActivityLogBatchDto[] = [];

  constructor(
    private _activityLog: ActivityLogService,
    private _localization: LocalizationService,
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['entityName'] || changes['recordKey']) {
      void this.load();
    }
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

  displayValue(value: string | null): string {
    if (!this.hasValue(value)) {
      return '';
    }
    const formatted = formatActivityLogDisplayValue(value!);
    return formatted.length > 500 ? formatted.slice(0, 500) + '…' : formatted;
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

  private async load(): Promise<void> {
    if (!this.entityName || !this.recordKey) {
      this.batches = [];
      return;
    }
    this.loading = true;
    try {
      const result = await this._activityLog.getByRecord({
        entityName: this.entityName,
        recordKey: this.recordKey,
        page: 1,
        pageSize: 50,
      });
      this.batches = result.success && result.data ? result.data.items : [];
    } finally {
      this.loading = false;
    }
  }
}
