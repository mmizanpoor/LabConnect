import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { NotificationDto } from './notification.types';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  constructor(private _http: ApiHttpService) {}

  getMyNotifications() {
    return this._http.get<NotificationDto[]>('Notification', 'GetMyNotifications');
  }

  getUnreadCount() {
    return this._http.get<number>('Notification', 'GetUnreadCount');
  }

  markAsRead(id: string) {
    return this._http.post('Notification', 'MarkAsRead', { id });
  }
}
