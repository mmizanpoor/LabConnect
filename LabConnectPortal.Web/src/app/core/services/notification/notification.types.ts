export type NotificationType = 'Rejection' | 'Approval' | 'General';

export interface NotificationDto {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
}
