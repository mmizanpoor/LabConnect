import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import {
  ActivatedRoute,
  NavigationEnd,
  Router,
  RouterLink,
  RouterOutlet,
} from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import { filter } from 'rxjs';
import { AuthService } from '@core/services/auth/auth.service';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { NotificationService } from '@core/services/notification/notification.service';
import { AzmoonProContextService } from '@core/services/site/azmoon-pro-context.service';
import { PortalKind } from '@core/portal/portal.types';
import { HeaderUserMenuComponent } from '@modules/base/components/header-user-menu/header-user-menu.component';
import { CenterProfileService } from '../center-profile/center-profile.service';
import { CenterProfileDto } from '../center-profile/center-profile.types';
import { LAB_ACTIVITY_LOGS_LINK, LAB_CENTER_PROFILE_LINK, LAB_DASHBOARD_LINK, LAB_PRODUCT_LISTINGS_LINK, getLabNavGroups } from '@modules/lab/lab-nav.config';
import {
  STORE_ACTIVITY_LOGS_LINK,
  STORE_CENTER_PROFILE_LINK,
  STORE_DASHBOARD_LINK,
  STORE_PRODUCT_LISTINGS_LINK,
  getStoreNavGroups,
} from '@modules/store/store-nav.config';
import { getUserNavGroups } from '@modules/user/user-nav.config';
import {
  PROFILE_LOGIN_REPORT_LINK,
  PROFILE_MESSAGES_LINK,
  isProfileLinkVisible,
} from './profile-nav.config';
import { ProfileNavGroup, ProfileNavLink } from './profile-nav.types';

const MOBILE_BREAKPOINT = '(max-width: 767.98px)';

@Component({
  selector: 'app-profile-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    ReactiveFormsModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    TranslocoPipe,
    HeaderUserMenuComponent,
  ],
  templateUrl: './profile-layout.component.html',
  styleUrl: './profile-layout.component.scss',
})
export class ProfileLayoutComponent implements OnInit {
  private _authService = inject(AuthService);
  private _centerProfileService = inject(CenterProfileService);
  private _localization = inject(LocalizationService);
  private _notificationService = inject(NotificationService);
  private _breakpoint = inject(BreakpointObserver);
  private _router = inject(Router);
  private _route = inject(ActivatedRoute);
  private _azmoonPro = inject(AzmoonProContextService);
  private _destroyRef = inject(DestroyRef);
  readonly authUtils = inject(AuthUtils);
  readonly labPermission = inject(LabPermissionService);

  hideChrome = false;
  isMobile = false;
  mobileMenuOpen = false;
  unreadCount = 0;
  readonly portal: PortalKind = this.resolvePortal();
  readonly createAdRoute = '/profile/product-listings/new';

  readonly dashboardLink = this.portal === 'store' ? STORE_DASHBOARD_LINK : LAB_DASHBOARD_LINK;
  readonly productListingsLink =
    this.portal === 'store' ? STORE_PRODUCT_LISTINGS_LINK : LAB_PRODUCT_LISTINGS_LINK;
  readonly activityLogsLink =
    this.portal === 'store' ? STORE_ACTIVITY_LOGS_LINK : LAB_ACTIVITY_LOGS_LINK;
  readonly messagesLink = PROFILE_MESSAGES_LINK;
  readonly loginReportLink = PROFILE_LOGIN_REPORT_LINK;
  readonly centerProfileLink =
    this.portal === 'store' ? STORE_CENTER_PROFILE_LINK : LAB_CENTER_PROFILE_LINK;
  readonly menuSearch = new FormControl('', { nonNullable: true });
  readonly sidebarWidth = '20rem';
  readonly sidebarCollapsedWidth = '4.5rem';

  sidebarCollapsed = false;
  centerProfile: CenterProfileDto | null = null;
  private _navGroups: ProfileNavGroup[] = [];

  constructor() {
    this.syncHideChrome();
    this._azmoonPro.hideChrome$
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this.syncHideChrome());
    this._router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this._destroyRef),
      )
      .subscribe((event) => {
        this.syncExpandedGroups();
        this.syncHideChrome(event.urlAfterRedirects);
        if (this.isMobile) {
          this.closeMobileMenu();
        }
      });
    this._breakpoint
      .observe(MOBILE_BREAKPOINT)
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((state) => {
        const wasMobile = this.isMobile;
        this.isMobile = state.matches;
        if (this.isMobile) {
          this.sidebarCollapsed = false;
          this.mobileMenuOpen = false;
          this.menuSearch.setValue('');
        } else if (wasMobile) {
          this.mobileMenuOpen = false;
        }
      });
  }
  expandedGroups = new Set<string>();

  get menuSearchTerm(): string {
    return this.menuSearch.value.trim().toLowerCase();
  }

  get isMenuSearching(): boolean {
    return this.menuSearchTerm.length > 0;
  }

  get showRejectionBanner(): boolean {
    return (
      !!this.centerProfile?.rejectionReason &&
      !this.centerProfile?.isComplete &&
      !this.centerProfile?.isApproved &&
      (this.portal === 'lab' || this.portal === 'store')
    );
  }

  get canShowDashboardLink(): boolean {
    return this.portal !== 'user' && !this.isPendingLab;
  }

  get canShowProductListingsLink(): boolean {
    if (this.portal === 'user' || this.isPendingLab) {
      return false;
    }
    return isProfileLinkVisible(this.productListingsLink, this.authUtils, this.labPermission);
  }

  get canShowMessagesLink(): boolean {
    if (this.isPendingLab) {
      return false;
    }
    if (this.portal === 'user') {
      return true;
    }
    return isProfileLinkVisible(this.messagesLink, this.authUtils, this.labPermission);
  }

  get showDashboardLink(): boolean {
    if (!this.canShowDashboardLink) {
      return false;
    }
    return this.labelMatches(this.dashboardLink.labelKey, this.menuSearchTerm);
  }

  get showProductListingsLink(): boolean {
    if (!this.canShowProductListingsLink) {
      return false;
    }
    return this.labelMatches(this.productListingsLink.labelKey, this.menuSearchTerm);
  }

  get showCreateAdLink(): boolean {
    return this.canShowProductListingsLink;
  }

  get showMessagesLink(): boolean {
    if (!this.canShowMessagesLink) {
      return false;
    }
    return this.labelMatches(this.messagesLink.labelKey, this.menuSearchTerm);
  }

  get showActivityLogsLink(): boolean {
    if (this.portal === 'user' || this.isPendingLab) {
      return false;
    }
    if (this.portal === 'store') {
      return this.labelMatches(this.activityLogsLink.labelKey, this.menuSearchTerm);
    }
    if (!isProfileLinkVisible(this.activityLogsLink, this.authUtils, this.labPermission)) {
      return false;
    }
    return this.labelMatches(this.activityLogsLink.labelKey, this.menuSearchTerm);
  }

  get showLoginReportLink(): boolean {
    if (this.isPendingLab) {
      return false;
    }
    return this.labelMatches(this.loginReportLink.labelKey, this.menuSearchTerm);
  }

  get canShowCenterProfileLink(): boolean {
    if (this.portal === 'user') {
      return false;
    }
    if (this.isPendingLab) {
      return true;
    }
    return isProfileLinkVisible(this.centerProfileLink, this.authUtils, this.labPermission);
  }

  get showCenterProfileLink(): boolean {
    if (!this.canShowCenterProfileLink) {
      return false;
    }
    return this.labelMatches(this.centerProfileLink.labelKey, this.menuSearchTerm);
  }

  get showTopNavDivider(): boolean {
    return (
      (this.showDashboardLink ||
        this.showProductListingsLink ||
        this.showMessagesLink ||
        this.showActivityLogsLink ||
        this.showLoginReportLink ||
        this.showCenterProfileLink) &&
      this.navGroups.length > 0
    );
  }

  get showBottomNav(): boolean {
    return this.isMobile && !this.hideChrome;
  }

  /** On mobile, only mount the drawer while the menu is open so it never reserves layout space. */
  get showSidenav(): boolean {
    return !this.isMobile || this.mobileMenuOpen;
  }

  get sidenavOpened(): boolean {
    return this.isMobile ? this.mobileMenuOpen : true;
  }

  get sidenavMode(): 'over' | 'side' {
    return this.isMobile ? 'over' : 'side';
  }

  get navGroups(): ProfileNavGroup[] {
    const term = this.menuSearchTerm;
    if (!term) {
      return this._navGroups;
    }

    return this._navGroups
      .map((group) => {
        const groupMatches = this.labelMatches(group.labelKey, term);
        return {
          ...group,
          items: groupMatches
            ? group.items
            : group.items.filter((item) => this.labelMatches(item.labelKey, term)),
        };
      })
      .filter((group) => group.items.length > 0);
  }

  get isPendingLab(): boolean {
    if (this.portal !== 'lab' || !this.authUtils.isAdminLab() || !this.centerProfile) {
      return false;
    }
    const status = this.centerProfile.status;
    return (
      status === 'Pending' ||
      status === 'Suspended' ||
      status === 0 ||
      status === 2
    );
  }

  get activePageLink(): ProfileNavLink {
    const url = this._router.url.split('?')[0];
    const topLinks: ProfileNavLink[] = [];
    if (this.showDashboardLink) topLinks.push(this.dashboardLink);
    if (this.showProductListingsLink) topLinks.push(this.productListingsLink);
    if (this.showMessagesLink) topLinks.push(this.messagesLink);
    if (this.showActivityLogsLink) topLinks.push(this.activityLogsLink);
    if (this.showLoginReportLink) topLinks.push(this.loginReportLink);
    if (this.showCenterProfileLink) topLinks.push(this.centerProfileLink);

    const candidates: ProfileNavLink[] = [...topLinks, ...this._navGroups.flatMap((group) => group.items)];

    let best: ProfileNavLink | null = null;
    let bestLength = -1;

    for (const link of candidates) {
      const matches = link.exact
        ? url === link.route
        : url === link.route || url.startsWith(`${link.route}/`);

      if (matches && link.route.length > bestLength) {
        best = link;
        bestLength = link.route.length;
      }
    }

    return (
      best ?? {
        route: '/profile',
        icon: 'person',
        labelKey: 'modules.profile.layout.account',
        exact: true,
      }
    );
  }

  get activePageTitleKey(): string {
    return this.activePageLink.labelKey;
  }

  isCreateAdActive(): boolean {
    const url = this._router.url.split('?')[0];
    return url === this.createAdRoute || url.endsWith('/product-listings/new');
  }

  isAccountHomeActive(): boolean {
    const url = this._router.url.split('?')[0];
    return url === '/profile' || url === '/profile/';
  }

  get unreadBadgeLabel(): string {
    return this.unreadCount > 99 ? '99+' : String(this.unreadCount);
  }

  async ngOnInit(): Promise<void> {
    await this._authService.loadLabPermissionsIfNeeded();

    if (this.portal === 'lab' || this.portal === 'store') {
      await this.loadCenterProfile();
    }

    this.refreshNavGroups();
    this.syncExpandedGroups();
    this.syncHideChrome();
    await this.loadUnreadCount();
  }

  toggleSidebar(): void {
    if (this.isMobile) {
      this.toggleMobileMenu();
      return;
    }
    this.sidebarCollapsed = !this.sidebarCollapsed;
    if (this.sidebarCollapsed) {
      this.menuSearch.setValue('');
    }
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  openMobileMenu(): void {
    this.mobileMenuOpen = true;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }

  onSidenavOpenedChange(opened: boolean): void {
    if (this.isMobile) {
      this.mobileMenuOpen = opened;
    }
  }

  clearMenuSearch(): void {
    this.menuSearch.setValue('');
  }

  toggleGroup(group: ProfileNavGroup): void {
    if (this.sidebarCollapsed && !this.isMobile) {
      this.sidebarCollapsed = false;
      this.expandedGroups.add(group.id);
      return;
    }

    if (this.isMenuSearching) {
      return;
    }

    if (this.expandedGroups.has(group.id)) {
      this.expandedGroups.delete(group.id);
    } else {
      this.expandedGroups.add(group.id);
    }
  }

  isGroupExpanded(groupId: string): boolean {
    if (this.isMenuSearching) {
      return true;
    }
    return this.expandedGroups.has(groupId);
  }

  isGroupActive(group: ProfileNavGroup): boolean {
    return group.items.some((item) => this.isLinkActive(item));
  }

  isLinkActive(link: ProfileNavLink): boolean {
    const url = this._router.url.split('?')[0];
    if (link.exact) {
      return url === link.route;
    }
    return url === link.route || url.startsWith(`${link.route}/`);
  }

  isBottomNavLinkActive(link: ProfileNavLink): boolean {
    if (link.route === this.productListingsLink.route && this.isCreateAdActive()) {
      return false;
    }
    return this.isLinkActive(link);
  }

  logout(): void {
    this._authService.logout();
  }

  goToEditProfile(): void {
    void this._router.navigate(['/profile/center'], {
      queryParamsHandling: 'preserve',
    });
  }

  private resolvePortal(): PortalKind {
    const data = this._route.snapshot.data['portal'] as PortalKind | undefined;
    if (data === 'lab' || data === 'store' || data === 'user') {
      return data;
    }
    return 'user';
  }

  private labelMatches(labelKey: string, term: string): boolean {
    if (!term) {
      return true;
    }
    const label = this._localization.translate(labelKey).toLowerCase();
    return label.includes(term);
  }

  private refreshNavGroups(): void {
    if (this.isPendingLab) {
      // Center profile is shown as a top link for pending labs.
      this._navGroups = [];
      return;
    }

    if (this.portal === 'store') {
      this._navGroups = this.filterApiKeyGroups(
        getStoreNavGroups(this.authUtils, this.labPermission),
      );
      return;
    }
    if (this.portal === 'lab') {
      this._navGroups = this.filterApiKeyGroups(
        getLabNavGroups(this.authUtils, this.labPermission),
      );
      return;
    }
    this._navGroups = getUserNavGroups(this.authUtils, this.labPermission);
  }

  private filterApiKeyGroups(groups: ProfileNavGroup[]): ProfileNavGroup[] {
    return this.centerProfile?.isApiKeyEnabled
      ? groups
      : groups.filter((group) => group.id !== 'apiKeys');
  }

  private syncHideChrome(url?: string): void {
    this.hideChrome = this._azmoonPro.shouldHideProfileChrome(url);
  }

  private syncExpandedGroups(): void {
    for (const group of this._navGroups) {
      if (group.items.some((item) => this.isLinkActive(item))) {
        this.expandedGroups.add(group.id);
      }
    }
  }

  private async loadCenterProfile(): Promise<void> {
    const result = await this._centerProfileService.getMyProfile();
    if (result.success && result.data) {
      this.centerProfile = result.data;
      this.refreshNavGroups();
      if (this.isPendingLab) {
        const url = this._router.url.split('?')[0];
        if (url !== '/profile/center' && !url.startsWith('/profile/center/')) {
          void this._router.navigate(['/profile/center'], {
            queryParamsHandling: 'preserve',
          });
        }
      }
    }
  }

  private async loadUnreadCount(): Promise<void> {
    const result = await this._notificationService.getUnreadCount();
    if (result.success && result.data != null) {
      this.unreadCount = result.data;
    }
  }
}
