import { Component, Input, OnInit, ViewEncapsulation, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthService } from '@core/services/auth/auth.service';
import { ProfileDto } from '@core/services/auth/auth.types';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { NotificationService } from '@core/services/notification/notification.service';
import { PublicSiteService } from '@core/services/site/public-site.service';
import { SiteContextService } from '@core/services/site/site-context.service';

@Component({
  selector: 'app-header-user-menu',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, MatMenuModule, TranslocoPipe],
  templateUrl: './header-user-menu.component.html',
  encapsulation: ViewEncapsulation.None,
})
export class HeaderUserMenuComponent implements OnInit {
  /** Hide the notifications bell (e.g. when shown in mobile bottom nav). */
  @Input() hideNotifications = false;
  /** Compact avatar-only trigger for dense mobile top bars. */
  @Input() compact = false;

  unreadCount = 0;
  profile: ProfileDto | null = null;

  private _authService = inject(AuthService);
  private _notificationService = inject(NotificationService);
  private _authUtils = inject(AuthUtils);
  private _siteContext = inject(SiteContextService);

  get profileViewLink(): string {
    if (this._authUtils.canAccessAdminPortal()) return '/admin/account';
    if (this._authUtils.canManageCenterProfile()) return '/profile/center/view';
    return '/profile';
  }

  get messagesLink(): string {
    return this._authUtils.canAccessAdminPortal()
      ? '/admin/messages'
      : '/profile/messages';
  }

  get changeUsernameLink(): string {
    return this._authUtils.canAccessAdminPortal()
      ? '/admin/change-username'
      : '/profile/change-username';
  }

  get changePasswordLink(): string {
    return this._authUtils.canAccessAdminPortal()
      ? '/admin/change-password'
      : '/profile/change-password';
  }

  get loginReportLink(): string {
    return this._authUtils.canAccessAdminPortal()
      ? '/admin/login-report'
      : '/profile/login-report';
  }

  /** Site logo from /admin/settings — used as admin avatar when available. */
  get avatarUrl(): string | null {
    if (!this._authUtils.canAccessAdminPortal()) {
      return null;
    }
    return this._siteContext.settings?.hasLogo ? PublicSiteService.logoUrl() : null;
  }

  get profileButtonLabel(): string {
    const firstName = this.profile?.firstName?.trim();
    if (firstName) return firstName;

    const username = this.shortUsername;
    if (username) return username;

    return this._authUtils.mobileNumber.trim();
  }

  get displayName(): string {
    if (this.profile) {
      const fullName = `${this.profile.firstName ?? ''} ${this.profile.lastName ?? ''}`.trim();
      if (fullName) return fullName;
    }

    return this._authUtils.displayIdentity;
  }

  get handleLabel(): string {
    const username = this._authUtils.username.trim();
    if (username) return `@${username}`;

    const mobile = this._authUtils.mobileNumber.trim();
    return mobile || '';
  }

  private get shortUsername(): string {
    const username = this._authUtils.username.trim();
    if (!username) return '';

    const separatorIndex = username.indexOf('_');
    return separatorIndex > 0 ? username.slice(0, separatorIndex) : username;
  }

  async ngOnInit(): Promise<void> {
    await Promise.all([
      this.loadUnreadCount(),
      this.loadProfile(),
      this._siteContext.ensureLoaded(),
    ]);
  }

  logout(): void {
    this._authService.logout();
  }

  private async loadProfile(): Promise<void> {
    try {
      this.profile = await this._authService.getProfile();
    } catch {
      this.profile = null;
    }
  }

  private async loadUnreadCount(): Promise<void> {
    const result = await this._notificationService.getUnreadCount();
    if (result.success && result.data != null) {
      this.unreadCount = result.data;
    }
  }
}
