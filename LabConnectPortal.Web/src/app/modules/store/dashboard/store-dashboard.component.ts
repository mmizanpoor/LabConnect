import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SystemEntity } from '@core/system-entity/system-entity';
import { dashboardThemeClasses, DashboardTheme } from '@core/utils/dashboard-theme.util';
import { ProductCatalogService } from '@modules/admin/products/product-catalog.service';
import { ProductStatus } from '@modules/admin/products/product-catalog.types';

interface DashboardShortcutCard {
  labelKey: string;
  icon: string;
  route: string;
  theme: DashboardTheme;
  primary?: boolean;
}

interface ProductListingStatCard {
  labelKey: string;
  icon: string;
  theme: DashboardTheme;
  value: keyof ProductListingDashboardStats;
  status: ProductStatus;
}

interface ProductListingDashboardStats {
  publishedCount: number;
  pendingCount: number;
  expiredCount: number;
  rejectedCount: number;
}

@Component({
  selector: 'app-store-dashboard',
  standalone: true,
  imports: [MatIconModule, RouterLink, TranslocoPipe],
  templateUrl: './store-dashboard.component.html',
  styleUrl: './store-dashboard.component.scss',
})
export class StoreDashboardComponent implements OnInit {
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Product);

  productStats: ProductListingDashboardStats = {
    publishedCount: 0,
    pendingCount: 0,
    expiredCount: 0,
    rejectedCount: 0,
  };

  jobListingStats: ProductListingDashboardStats = {
    publishedCount: 0,
    pendingCount: 0,
    expiredCount: 0,
    rejectedCount: 0,
  };

  loading = true;
  error = '';

  readonly productCards: ProductListingStatCard[] = [
    {
      labelKey: 'modules.profile.dashboard.publishedProducts',
      icon: 'check_circle',
      theme: 'green',
      value: 'publishedCount',
      status: 'Approved',
    },
    {
      labelKey: 'modules.profile.dashboard.pendingProducts',
      icon: 'hourglass_top',
      theme: 'amber',
      value: 'pendingCount',
      status: 'PendingApproval',
    },
    {
      labelKey: 'modules.profile.dashboard.expiredProducts',
      icon: 'event_busy',
      theme: 'slate',
      value: 'expiredCount',
      status: 'Unpublished',
    },
    {
      labelKey: 'modules.profile.dashboard.rejectedProducts',
      icon: 'cancel',
      theme: 'rose',
      value: 'rejectedCount',
      status: 'Rejected',
    },
  ];

  /** Same status cards as product listings, scoped to resume-accepting categories. */
  readonly jobListingCards: ProductListingStatCard[] = this.productCards;

  readonly shortcutCards: DashboardShortcutCard[] = [
    {
      labelKey: 'modules.profile.productListings.form.addTitle',
      icon: 'add_circle_outline',
      route: '/profile/product-listings/new',
      theme: 'amber',
      primary: true,
    },
    {
      labelKey: 'modules.profile.layout.products',
      icon: 'campaign',
      route: '/profile/product-listings',
      theme: 'blue',
    },
    {
      labelKey: 'modules.profile.layout.messages',
      icon: 'notifications_none',
      route: '/profile/messages',
      theme: 'violet',
    },
    {
      labelKey: 'modules.profile.layout.centerProfile',
      icon: 'person_outline',
      route: '/profile/center',
      theme: 'green',
    },
  ];

  constructor(
    private _catalogService: ProductCatalogService,
    private _router: Router,
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const [productStats, jobListingStats] = await Promise.all([
        this._catalogService.getMyProductDashboardStats({ acceptsResume: false }),
        this._catalogService.getMyProductDashboardStats({ acceptsResume: true }),
      ]);
      this.productStats = productStats;
      this.jobListingStats = jobListingStats;
    } catch (err) {
      this.error = err instanceof Error ? err.message : '';
    } finally {
      this.loading = false;
    }
  }

  productValue(card: ProductListingStatCard): number {
    return this.productStats[card.value] ?? 0;
  }

  jobListingValue(card: ProductListingStatCard): number {
    return this.jobListingStats[card.value] ?? 0;
  }

  openProductListings(status: ProductStatus, acceptsResume: boolean): void {
    void this._router.navigate(['/profile/product-listings'], {
      queryParams: {
        status,
        acceptsResume: acceptsResume ? 'true' : 'false',
      },
    });
  }

  theme(themeName: DashboardTheme) {
    return dashboardThemeClasses(themeName);
  }
}
