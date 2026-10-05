import { Component, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { NotificationService } from '@core/services/notification/notification.service';
import { NotificationDto, NotificationType } from '@core/services/notification/notification.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

type FilterTab = 'all' | 'unread';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [MatIconModule, TranslocoPipe],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss',
})
export class MessagesComponent implements OnInit {
  readonly entity = SystemEntity.Message;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Message);

  loading = false;
  markingAll = false;
  error = '';
  messages: NotificationDto[] = [];
  filter: FilterTab = 'all';
  page = 1;
  readonly pageSize = 12;

  constructor(
    private _notificationService: NotificationService,
    private _localization: LocalizationService,
  ) {}

  get unreadCount(): number {
    return this.messages.filter((m) => !m.isRead).length;
  }

  get filteredMessages(): NotificationDto[] {
    if (this.filter === 'unread') {
      return this.messages.filter((m) => !m.isRead);
    }
    return this.messages;
  }

  get pagedMessages(): NotificationDto[] {
    const end = this.page * this.pageSize;
    return this.filteredMessages.slice(0, end);
  }

  get hasMore(): boolean {
    return this.pagedMessages.length < this.filteredMessages.length;
  }

  get showMarkAll(): boolean {
    return this.unreadCount > 0;
  }

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._notificationService.getMyNotifications();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.profile.messages.errors.loadFailed'),
        );
      }
      this.messages = [...result.data].sort(
        (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
      );
      this.page = 1;
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.messages.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  setFilter(tab: FilterTab): void {
    this.filter = tab;
    this.page = 1;
  }

  loadMore(): void {
    if (!this.hasMore) return;
    this.page += 1;
  }

  async markRead(message: NotificationDto): Promise<void> {
    if (message.isRead) return;
    const result = await this._notificationService.markAsRead(message.id);
    if (result.success) {
      message.isRead = true;
    }
  }

  async markAllRead(): Promise<void> {
    if (this.unreadCount === 0 || this.markingAll) return;
    this.markingAll = true;
    try {
      const unread = this.messages.filter((m) => !m.isRead);
      await Promise.all(
        unread.map(async (m) => {
          const result = await this._notificationService.markAsRead(m.id);
          if (result.success) m.isRead = true;
        }),
      );
    } finally {
      this.markingAll = false;
    }
  }

  iconFor(msg: NotificationDto): string {
    const text = `${msg.title ?? ''} ${msg.message ?? ''}`;

    if (/آگهی|محصول|انتشار|منتشر/.test(text)) return 'campaign';
    if (
      /پروفایل|تأیید|تایید/.test(text) &&
      String(msg.type) !== '0' &&
      String(msg.type) !== 'Rejection'
    ) {
      return 'verified_user';
    }
    if (/هشدار|منقضی|انقضا|اخطار/.test(text)) return 'warning_amber';

    switch (String(msg.type)) {
      case 'Approval':
      case '1':
        return 'verified';
      case 'Rejection':
      case '0':
        return 'error_outline';
      default:
        return 'notifications';
    }
  }

  iconToneClass(type: NotificationType | string | number): string {
    switch (String(type)) {
      case 'Approval':
      case '1':
        return 'bg-emerald-50/80 text-emerald-600';
      case 'Rejection':
      case '0':
        return 'bg-rose-50/80 text-rose-500';
      default:
        return 'bg-slate-100/80 text-slate-500';
    }
  }

  formatRelativeTime(value: string): string {
    const date = jMoment(value);
    if (!date.isValid()) return '—';

    const now = jMoment();
    const diffMin = Math.max(0, now.diff(date, 'minutes'));
    if (diffMin < 1) {
      return this._localization.translate('modules.profile.messages.time.justNow');
    }
    if (diffMin < 60) {
      return this._localization.translate('modules.profile.messages.time.minutesAgo', {
        count: diffMin,
      });
    }

    const diffHours = now.diff(date, 'hours');
    if (diffHours < 24 && now.isSame(date, 'day')) {
      return this._localization.translate('modules.profile.messages.time.todayAt', {
        time: date.format('HH:mm'),
      });
    }

    if (diffHours < 48 && now.clone().subtract(1, 'day').isSame(date, 'day')) {
      return this._localization.translate('modules.profile.messages.time.yesterdayAt', {
        time: date.format('HH:mm'),
      });
    }

    const diffDays = now.diff(date, 'days');
    if (diffDays < 7) {
      return this._localization.translate('modules.profile.messages.time.daysAgo', {
        count: diffDays,
      });
    }

    return date.format('jYYYY/jMM/jDD');
  }
}
