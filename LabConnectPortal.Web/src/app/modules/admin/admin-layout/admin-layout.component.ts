import { Component, OnInit, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { filter } from 'rxjs';
import { AuthService } from '@core/services/auth/auth.service';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { HeaderUserMenuComponent } from '@modules/base/components/header-user-menu/header-user-menu.component';
import {
  ADMIN_DASHBOARD_LINK,
  ADMIN_NAV_GROUPS,
  ADMIN_NAV_STANDALONE,
  ADMIN_ROUTE_TITLE_OVERRIDES,
  AdminNavGroup,
  AdminNavLink,
  AdminNavStandalone,
} from './admin-nav.config';

@Component({
  selector: 'app-admin-layout',
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
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss',
})
export class AdminLayoutComponent implements OnInit {
  private _authService = inject(AuthService);
  private _authUtils = inject(AuthUtils);
  private _sitePermission = inject(SitePermissionService);
  private _localization = inject(LocalizationService);
  private _router = inject(Router);

  readonly dashboardLink = ADMIN_DASHBOARD_LINK;
  readonly menuSearch = new FormControl('', { nonNullable: true });

  readonly sidebarWidth = '20rem';
  readonly sidebarCollapsedWidth = '4.5rem';
  sidebarCollapsed = false;
  expandedGroups = new Set<string>();

  get menuSearchTerm(): string {
    return this.menuSearch.value.trim().toLowerCase();
  }

  get isMenuSearching(): boolean {
    return this.menuSearchTerm.length > 0;
  }

  get showDashboardLink(): boolean {
    return this.labelMatches(this.dashboardLink.labelKey, this.menuSearchTerm);
  }

  get navGroups(): AdminNavGroup[] {
    const groups = ADMIN_NAV_GROUPS.map((group) => ({
      ...group,
      items: group.items.filter((item) => this.isNavLinkVisible(item)),
    })).filter((group) => group.items.length > 0);

    const term = this.menuSearchTerm;
    if (!term) {
      return groups;
    }

    return groups
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

  get standaloneLinks(): AdminNavStandalone[] {
    const links = ADMIN_NAV_STANDALONE.filter((link) => this.isNavLinkVisible(link));
    const term = this.menuSearchTerm;
    if (!term) {
      return links;
    }
    return links.filter((link) => this.labelMatches(link.labelKey, term));
  }

  get activePageLink(): AdminNavLink {
    const url = this._router.url.split('?')[0];
    const override = ADMIN_ROUTE_TITLE_OVERRIDES.find((item) => item.match(url));
    if (override) {
      return {
        route: url,
        icon: 'article',
        labelKey: override.labelKey,
      };
    }

    const candidates: AdminNavLink[] = [
      this.dashboardLink,
      ...ADMIN_NAV_GROUPS.flatMap((group) =>
        group.items.filter((item) => this.isNavLinkVisible(item)),
      ),
      ...ADMIN_NAV_STANDALONE.filter((link) => this.isNavLinkVisible(link)),
    ];

    let best: AdminNavLink | null = null;
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
        route: '/admin/dashboard',
        icon: 'dashboard',
        labelKey: 'modules.admin.layout.dashboard',
      }
    );
  }

  get activePageTitleKey(): string {
    return this.activePageLink.labelKey;
  }

  ngOnInit(): void {
    this.syncExpandedGroups();

    this._router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => this.syncExpandedGroups());
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
    if (this.sidebarCollapsed) {
      this.menuSearch.setValue('');
    }
  }

  clearMenuSearch(): void {
    this.menuSearch.setValue('');
  }

  toggleGroup(group: AdminNavGroup): void {
    if (this.sidebarCollapsed) {
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

  isGroupActive(group: AdminNavGroup): boolean {
    return group.items.some((item) => this.isLinkActive(item));
  }

  isLinkActive(link: AdminNavLink): boolean {
    const url = this._router.url.split('?')[0];
    if (link.exact) {
      return url === link.route;
    }
    return url === link.route || url.startsWith(`${link.route}/`);
  }

  logout(): void {
    this._authService.logout();
  }

  private labelMatches(labelKey: string, term: string): boolean {
    if (!term) {
      return true;
    }
    const label = this._localization.translate(labelKey).toLowerCase();
    return label.includes(term);
  }

  private isNavLinkVisible(link: AdminNavLink | AdminNavStandalone): boolean {
    if (link.administratorOnly && !this._authUtils.isAdministrator()) {
      return false;
    }
    if (!link.systemEntity) {
      return true;
    }
    if (this._authUtils.isAdministrator()) {
      return true;
    }
    return this._sitePermission.canView(link.systemEntity);
  }

  private syncExpandedGroups(): void {
    for (const group of this.navGroups) {
      if (group.items.some((item) => this.isLinkActive(item))) {
        this.expandedGroups.add(group.id);
      }
    }
  }
}
